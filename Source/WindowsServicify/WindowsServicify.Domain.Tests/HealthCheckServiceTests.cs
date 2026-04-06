using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class HealthCheckServiceTests
{
    private string _logDirectory = null!;
    private ProcessLogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _logDirectory = Path.Combine(Path.GetTempPath(), "HealthCheckTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_logDirectory);
        _logger = new ProcessLogger(_logDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _logger?.Dispose();

        Thread.Sleep(200);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                if (Directory.Exists(_logDirectory))
                    Directory.Delete(_logDirectory, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 4)
            {
                Thread.Sleep(300);
            }
        }
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    // --- FormatUptime ---

    [Test]
    public void FormatUptime_ZeroTimeSpan_ReturnsZeroFormat()
    {
        var result = HealthCheckService.FormatUptime(TimeSpan.Zero);

        Assert.That(result, Is.EqualTo("00:00:00"));
    }

    [Test]
    public void FormatUptime_OneHourThirtyMinutesTwentySeconds_ReturnsCorrectFormat()
    {
        var uptime = new TimeSpan(1, 30, 20);

        var result = HealthCheckService.FormatUptime(uptime);

        Assert.That(result, Is.EqualTo("01:30:20"));
    }

    [Test]
    public void FormatUptime_OverTwentyThreeHours_WrapsAround()
    {
        // TimeSpan of 25 hours -- hh format wraps to 01
        var uptime = new TimeSpan(25, 0, 0);

        var result = HealthCheckService.FormatUptime(uptime);

        Assert.That(result, Is.EqualTo("01:00:00"));
    }

    // --- Health endpoint with running process ---

    [Test]
    public void HealthEndpoint_WithRunningProcess_ReturnsHttp200()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping 127.0.0.1 -n 30 > nul",
            processLogger: _logger);

        processManager.Start();
        Thread.Sleep(500);

        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;

            Assert.That((int)response.StatusCode, Is.EqualTo(200));
        }
        finally
        {
            healthCheck.Stop();
            processManager.Stop();
            Thread.Sleep(500);
        }
    }

    [Test]
    public void HealthEndpoint_WithRunningProcess_ReturnsHealthyJson()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping 127.0.0.1 -n 30 > nul",
            processLogger: _logger);

        processManager.Start();
        Thread.Sleep(500);

        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var json = JsonDocument.Parse(content);

            Assert.That(json.RootElement.GetProperty("status").GetString(), Is.EqualTo("healthy"));
            Assert.That(json.RootElement.GetProperty("process").GetString(), Is.EqualTo("running"));
            Assert.That(json.RootElement.TryGetProperty("uptime", out _), Is.True);
        }
        finally
        {
            healthCheck.Stop();
            processManager.Stop();
            Thread.Sleep(500);
        }
    }

    // --- Health endpoint with stopped process ---

    [Test]
    public void HealthEndpoint_WithStoppedProcess_ReturnsHttp503()
    {
        var port = GetFreePort();
        // Process that exits immediately
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c echo done",
            processLogger: _logger);

        processManager.Start();
        Thread.Sleep(1500); // Wait for process to exit

        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;

            Assert.That((int)response.StatusCode, Is.EqualTo(503));
        }
        finally
        {
            healthCheck.Stop();
            Thread.Sleep(300);
        }
    }

    [Test]
    public void HealthEndpoint_WithStoppedProcess_ReturnsUnhealthyJson()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c echo done",
            processLogger: _logger);

        processManager.Start();
        Thread.Sleep(1500);

        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var json = JsonDocument.Parse(content);

            Assert.That(json.RootElement.GetProperty("status").GetString(), Is.EqualTo("unhealthy"));
            Assert.That(json.RootElement.GetProperty("process").GetString(), Is.EqualTo("stopped"));
        }
        finally
        {
            healthCheck.Stop();
            Thread.Sleep(300);
        }
    }

    // --- Non-health paths ---

    [Test]
    public void HealthEndpoint_WithNonHealthPath_Returns404()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c echo done",
            processLogger: _logger);

        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/other").Result;

            Assert.That((int)response.StatusCode, Is.EqualTo(404));
        }
        finally
        {
            healthCheck.Stop();
            Thread.Sleep(300);
        }
    }

    // --- Response time ---

    [Test]
    public void HealthEndpoint_ResponseTime_IsUnder100Ms()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping 127.0.0.1 -n 30 > nul",
            processLogger: _logger);

        processManager.Start();
        Thread.Sleep(500);

        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            // Warm-up request
            client.GetAsync($"http://localhost:{port}/health").Wait();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            client.GetAsync($"http://localhost:{port}/health").Wait();
            stopwatch.Stop();

            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100),
                "Health check response should be under 100ms");
        }
        finally
        {
            healthCheck.Stop();
            processManager.Stop();
            Thread.Sleep(500);
        }
    }

    // --- JSON format uses camelCase ---

    [Test]
    public void HealthEndpoint_JsonProperties_AreCamelCase()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping 127.0.0.1 -n 30 > nul",
            processLogger: _logger);

        processManager.Start();
        Thread.Sleep(500);

        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;
            var content = response.Content.ReadAsStringAsync().Result;

            Assert.That(content, Does.Contain("\"status\""));
            Assert.That(content, Does.Contain("\"process\""));
            Assert.That(content, Does.Contain("\"uptime\""));
            // Verify no PascalCase properties
            Assert.That(content, Does.Not.Contain("\"Status\""));
            Assert.That(content, Does.Not.Contain("\"Process\""));
            Assert.That(content, Does.Not.Contain("\"Uptime\""));
        }
        finally
        {
            healthCheck.Stop();
            processManager.Stop();
            Thread.Sleep(500);
        }
    }

    // --- Content-Type ---

    [Test]
    public void HealthEndpoint_ContentType_IsApplicationJson()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping 127.0.0.1 -n 30 > nul",
            processLogger: _logger);

        processManager.Start();
        Thread.Sleep(500);

        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;

            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        }
        finally
        {
            healthCheck.Stop();
            processManager.Stop();
            Thread.Sleep(500);
        }
    }

    // --- Dispose ---

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c echo done",
            processLogger: _logger);

        var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        Assert.DoesNotThrow(() =>
        {
            healthCheck.Dispose();
            healthCheck.Dispose();
        });
    }

    // --- Process not started (never started) ---

    [Test]
    public void HealthEndpoint_WithProcessNeverStarted_ReturnsUnhealthy()
    {
        var port = GetFreePort();
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c echo done",
            processLogger: _logger);

        // Do NOT start processManager
        using var healthCheck = new HealthCheckService(port, processManager);
        healthCheck.Start();

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync($"http://localhost:{port}/health").Result;

            Assert.That((int)response.StatusCode, Is.EqualTo(503));
        }
        finally
        {
            healthCheck.Stop();
            Thread.Sleep(300);
        }
    }
}
