using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace WindowsServicify.Domain.IntegrationTests;

/// <summary>
/// Integration tests for the Health-Check HTTP endpoint.
/// Tests the full lifecycle: configure with port, start service,
/// query health endpoint, verify response, stop service.
/// </summary>
[TestFixture]
public class HealthCheckIntegrationTests
{
    private TempDirectory _tempDir = null!;
    private TestProcessExitHandler _exitHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = new TempDirectory("HealthIT");
        _exitHandler = new TestProcessExitHandler();
    }

    [TearDown]
    public void TearDown()
    {
        Thread.Sleep(300);
        _tempDir.Dispose();
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    // --- Szenario: Full configure-and-testrun flow with health check ---

    [Test]
    public void ConfigureAndTestrun_WithHealthCheckPort_EndpointReturnsHealthy()
    {
        var port = GetFreePort();
        var configPath = Path.Combine(_tempDir.Path, "config.json");

        // Step 1: Save config with HealthCheckPort
        var config = new ServiceConfiguration(
            ServiceName: "HealthTestService",
            DisplayName: "Health Test Service",
            Description: "Integration test for health check",
            Command: "cmd.exe",
            WorkingDirectory: Path.GetTempPath(),
            Arguments: "/c ping 127.0.0.1 -n 60 > nul",
            HealthCheckPort: port);

        ServiceConfigurationFileHandler.Save(configPath, config);

        // Step 2: Load config and verify HealthCheckPort persisted
        var loadResult = ServiceConfigurationFileHandler.Load(configPath);
        Assert.That(loadResult.IsSuccess, Is.True,
            $"Config should load successfully. Error: {loadResult.ErrorMessage}");
        Assert.That(loadResult.Value.HealthCheckPort, Is.EqualTo(port));

        // Step 3: Start process and health check
        using var processLogger = new ProcessLogger(_tempDir.Path);
        var processManager = new ProcessManager(
            loadResult.Value.Command,
            loadResult.Value.WorkingDirectory,
            loadResult.Value.Arguments,
            processLogger,
            shutdownTimeoutMs: 2000);

        using var healthCheck = new HealthCheckService(port, processManager);

        try
        {
            processManager.Start();
            healthCheck.Start();
            Thread.Sleep(1000);

            // Step 4: Query health endpoint
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var json = JsonDocument.Parse(content);

            Assert.That((int)response.StatusCode, Is.EqualTo(200));
            Assert.That(json.RootElement.GetProperty("status").GetString(), Is.EqualTo("healthy"));
            Assert.That(json.RootElement.GetProperty("process").GetString(), Is.EqualTo("running"));
        }
        finally
        {
            healthCheck.Stop();
            processManager.Stop();
            Thread.Sleep(500);
        }
    }

    // --- Szenario: Config without HealthCheckPort loads successfully ---

    [Test]
    public void LoadConfig_WithoutHealthCheckPort_ReturnsNullPort()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");

        // Write a JSON file without healthCheckPort (simulates legacy config)
        var legacyJson = JsonSerializer.Serialize(new
        {
            serviceName = "LegacyService",
            displayName = "Legacy Service",
            description = "No health check",
            command = "cmd.exe",
            workingDirectory = "C:\\Temp",
            arguments = "/c echo hello"
        }, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(configPath, legacyJson);

        var result = ServiceConfigurationFileHandler.Load(configPath);

        Assert.That(result.IsSuccess, Is.True,
            $"Legacy config should load. Error: {result.ErrorMessage}");
        Assert.That(result.Value.HealthCheckPort, Is.Null,
            "Missing healthCheckPort should default to null");
    }

    // --- Szenario: Config with PascalCase properties (backward compatibility) ---

    [Test]
    public void LoadConfig_WithPascalCaseProperties_LoadsSuccessfully()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");

        // Old-format config with PascalCase
        var oldFormatJson = JsonSerializer.Serialize(new
        {
            ServiceName = "OldFormatService",
            DisplayName = "Old Format Service",
            Description = "PascalCase config",
            Command = "cmd.exe",
            WorkingDirectory = "C:\\Temp",
            Arguments = "/c echo test"
        }, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(configPath, oldFormatJson);

        var result = ServiceConfigurationFileHandler.Load(configPath);

        Assert.That(result.IsSuccess, Is.True,
            $"PascalCase config should load. Error: {result.ErrorMessage}");
        Assert.That(result.Value.ServiceName, Is.EqualTo("OldFormatService"));
        Assert.That(result.Value.HealthCheckPort, Is.Null);
    }

    // --- Szenario: Health endpoint reports unhealthy when process stops ---

    [Test]
    public void HealthEndpoint_WhenProcessStops_ReturnsUnhealthy()
    {
        var port = GetFreePort();

        using var processLogger = new ProcessLogger(_tempDir.Path);
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c echo done",
            processLogger: processLogger);

        using var healthCheck = new HealthCheckService(port, processManager);

        try
        {
            processManager.Start();
            healthCheck.Start();
            Thread.Sleep(2000); // Wait for short-lived process to exit

            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var json = JsonDocument.Parse(content);

            Assert.That((int)response.StatusCode, Is.EqualTo(503));
            Assert.That(json.RootElement.GetProperty("status").GetString(), Is.EqualTo("unhealthy"));
            Assert.That(json.RootElement.GetProperty("process").GetString(), Is.EqualTo("stopped"));
        }
        finally
        {
            healthCheck.Stop();
            Thread.Sleep(300);
        }
    }

    // --- Szenario: WindowsBackgroundService with health check via DI ---

    [Test]
    public async Task BackgroundService_WithHealthCheck_EndpointAvailableDuringLifecycle()
    {
        var port = GetFreePort();

        using var processLogger = new ProcessLogger(_tempDir.Path);
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping 127.0.0.1 -n 60 > nul",
            processLogger: processLogger,
            shutdownTimeoutMs: 2000);

        var healthCheckService = new HealthCheckService(port, processManager);

        var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Warning));
        var logger = loggerFactory.CreateLogger<WindowsBackgroundService>();
        var service = new WindowsBackgroundService(
            processManager, logger, processLogger, _exitHandler, healthCheckService);

        using var cts = new CancellationTokenSource();

        try
        {
            await service.StartAsync(cts.Token);
            await Task.Delay(2000); // Wait for process and health check to start

            // Verify health endpoint is available
            using var client = new HttpClient();
            var response = await client.GetAsync($"http://localhost:{port}/health");

            Assert.That((int)response.StatusCode, Is.EqualTo(200));
        }
        finally
        {
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);
            healthCheckService.Dispose();
            await Task.Delay(500);
        }
    }

    // --- Szenario: Health check with invalid port in config ---

    [Test]
    public void LoadConfig_WithInvalidHealthCheckPort_ReturnsFailure()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");

        var invalidConfig = new
        {
            serviceName = "InvalidPortService",
            displayName = "Invalid Port Service",
            description = "Port out of range",
            command = "cmd.exe",
            workingDirectory = "C:\\Temp",
            arguments = "/c echo test",
            healthCheckPort = 80 // Below minimum of 1024
        };

        var json = JsonSerializer.Serialize(invalidConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configPath, json);

        var result = ServiceConfigurationFileHandler.Load(configPath);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("HealthCheckPort"));
    }

    // --- Szenario: Config round-trip preserves HealthCheckPort ---

    [Test]
    public void SaveAndLoad_WithHealthCheckPort_PreservesPort()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");
        var config = new ServiceConfiguration(
            ServiceName: "RoundTripService",
            DisplayName: "Round Trip Service",
            Description: "Tests round-trip with health check",
            Command: "cmd.exe",
            WorkingDirectory: "C:\\Temp",
            Arguments: "/c echo test",
            HealthCheckPort: 9090);

        ServiceConfigurationFileHandler.Save(configPath, config);
        var result = ServiceConfigurationFileHandler.Load(configPath);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.HealthCheckPort, Is.EqualTo(9090));
        Assert.That(result.Value, Is.EqualTo(config));
    }

    // --- Szenario: Saved config uses camelCase JSON ---

    [Test]
    public void Save_WritesHealthCheckPortInCamelCase()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");
        var config = new ServiceConfiguration(
            ServiceName: "CamelService",
            DisplayName: "Camel Service",
            Description: "Tests camelCase",
            Command: "cmd.exe",
            WorkingDirectory: "C:\\Temp",
            Arguments: "",
            HealthCheckPort: 8080);

        ServiceConfigurationFileHandler.Save(configPath, config);

        var json = File.ReadAllText(configPath);
        Assert.That(json, Does.Contain("\"healthCheckPort\""));
        Assert.That(json, Does.Contain("\"serviceName\""));
        Assert.That(json, Does.Not.Contain("\"HealthCheckPort\""));
        Assert.That(json, Does.Not.Contain("\"ServiceName\""));
    }
}