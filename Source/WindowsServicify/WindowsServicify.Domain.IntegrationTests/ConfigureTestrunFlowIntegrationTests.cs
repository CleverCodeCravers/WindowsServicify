using NUnit.Framework;

namespace WindowsServicify.Domain.IntegrationTests;

/// <summary>
/// Integration tests for the configure/testrun flow:
/// Save config -> Load config -> Start process -> Verify output -> Stop.
/// Tests the end-to-end interaction of ServiceConfigurationFileHandler,
/// ProcessManager, and ProcessLogger.
/// </summary>
[TestFixture]
public class ConfigureTestrunFlowIntegrationTests
{
    private TempDirectory _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = new TempDirectory("CfgFlowIT");
    }

    [TearDown]
    public void TearDown()
    {
        Thread.Sleep(200);
        _tempDir.Dispose();
    }

    // --- Szenario 1: Configure-Flow schreibt gueltige Konfiguration ---

    [Test]
    public void SaveAndLoad_RoundTrip_AllFieldsMatch()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");
        var original = new ServiceConfiguration(
            ServiceName: "TestService",
            DisplayName: "Test Service Display",
            Description: "A test service description",
            Command: "cmd.exe",
            WorkingDirectory: _tempDir.Path,
            Arguments: "/c echo hello");

        ServiceConfigurationFileHandler.Save(configPath, original);

        var loadResult = ServiceConfigurationFileHandler.Load(configPath);

        Assert.That(loadResult.IsSuccess, Is.True,
            $"Load should succeed. Error: {loadResult.ErrorMessage}");
        Assert.That(loadResult.Value.ServiceName, Is.EqualTo(original.ServiceName));
        Assert.That(loadResult.Value.DisplayName, Is.EqualTo(original.DisplayName));
        Assert.That(loadResult.Value.Description, Is.EqualTo(original.Description));
        Assert.That(loadResult.Value.Command, Is.EqualTo(original.Command));
        Assert.That(loadResult.Value.WorkingDirectory, Is.EqualTo(original.WorkingDirectory));
        Assert.That(loadResult.Value.Arguments, Is.EqualTo(original.Arguments));
    }

    [Test]
    public void SaveAndLoad_ProducesValidJsonFile()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");
        var config = new ServiceConfiguration(
            ServiceName: "JsonTest",
            DisplayName: "Json Test",
            Description: "Checking JSON validity",
            Command: "cmd.exe",
            WorkingDirectory: _tempDir.Path,
            Arguments: "/c echo test");

        ServiceConfigurationFileHandler.Save(configPath, config);

        var jsonContent = File.ReadAllText(configPath);
        Assert.That(jsonContent, Is.Not.Empty,
            "Config file should not be empty");
        Assert.That(jsonContent, Does.Contain("JsonTest"),
            "JSON should contain the service name");

        // Verify it's valid JSON by loading it back
        var loadResult = ServiceConfigurationFileHandler.Load(configPath);
        Assert.That(loadResult.IsSuccess, Is.True,
            "File should be loadable as valid JSON");
    }

    // --- Szenario 2: Testrun-Flow startet Prozess und erfasst Output ---

    [Test]
    public void TestrunFlow_SaveConfig_StartProcess_VerifyLogOutput()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");
        var logDir = Path.Combine(_tempDir.Path, "logs");
        Directory.CreateDirectory(logDir);

        var config = new ServiceConfiguration(
            ServiceName: "TestrunService",
            DisplayName: "Testrun Service",
            Description: "Flow test",
            Command: "cmd.exe",
            WorkingDirectory: _tempDir.Path,
            Arguments: "/c echo TestrunOutput");

        // Step 1: Save config (simulates --configure)
        ServiceConfigurationFileHandler.Save(configPath, config);

        // Step 2: Load config (simulates testrun startup)
        var loadResult = ServiceConfigurationFileHandler.Load(configPath);
        Assert.That(loadResult.IsSuccess, Is.True);

        var loaded = loadResult.Value;

        // Step 3: Start process with loaded config (simulates --testrun)
        using var processLogger = new ProcessLogger(logDir);
        var processManager = new ProcessManager(
            loaded.Command,
            loaded.WorkingDirectory,
            loaded.Arguments,
            processLogger);

        processManager.Start();

        // Step 4: Verify output appears in log
        var found = LogFileReader.WaitFor(
            logDir,
            content => content.Contains("TestrunOutput"),
            timeoutMs: 5000);

        Assert.That(found, Is.True,
            "Process output 'TestrunOutput' should appear in log files");
    }

    [Test]
    public void TestrunFlow_ProcessExitsNormally_AfterEcho()
    {
        var logDir = Path.Combine(_tempDir.Path, "logs");
        Directory.CreateDirectory(logDir);

        using var processLogger = new ProcessLogger(logDir);
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c echo CompletionTest",
            processLogger: processLogger);

        processManager.Start();

        LogFileReader.WaitFor(logDir, c => c.Contains("CompletionTest"));
        Thread.Sleep(500);

        Assert.That(processManager.IsCorrectlyRunning(), Is.False,
            "Echo process should have exited after completion");
    }

    // --- Szenario: Load fails for non-existent config ---

    [Test]
    public void LoadConfig_NonExistentFile_ReturnsFailure()
    {
        var configPath = Path.Combine(_tempDir.Path, "nonexistent.json");

        var result = ServiceConfigurationFileHandler.Load(configPath);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("not found"));
    }

    // --- Szenario: Full flow with long-running process ---

    [Test]
    public void TestrunFlow_LongRunningProcess_StartMonitorStop()
    {
        var configPath = Path.Combine(_tempDir.Path, "config.json");
        var logDir = Path.Combine(_tempDir.Path, "logs");
        Directory.CreateDirectory(logDir);

        var config = new ServiceConfiguration(
            ServiceName: "LongRunService",
            DisplayName: "Long Run Service",
            Description: "Tests long-running process flow",
            Command: "cmd.exe",
            WorkingDirectory: Path.GetTempPath(),
            Arguments: "/c ping -n 30 127.0.0.1 > nul");

        ServiceConfigurationFileHandler.Save(configPath, config);
        var loaded = ServiceConfigurationFileHandler.Load(configPath).Value;

        using var processLogger = new ProcessLogger(logDir);
        var processManager = new ProcessManager(
            loaded.Command,
            loaded.WorkingDirectory,
            loaded.Arguments,
            processLogger,
            shutdownTimeoutMs: 2000);

        processManager.Start();
        Thread.Sleep(1000);

        Assert.That(processManager.IsCorrectlyRunning(), Is.True,
            "Long-running process should be running");

        processManager.Stop();
        Thread.Sleep(500);

        Assert.That(processManager.IsCorrectlyRunning(), Is.False,
            "Process should be stopped after Stop call");
    }
}