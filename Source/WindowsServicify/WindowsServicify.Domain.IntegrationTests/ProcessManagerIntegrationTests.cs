using NUnit.Framework;

namespace WindowsServicify.Domain.IntegrationTests;

/// <summary>
/// Integration tests for ProcessManager that verify end-to-end process lifecycle:
/// starting real processes, capturing their output via ProcessLogger, and stopping them.
/// </summary>
[TestFixture]
public class ProcessManagerIntegrationTests
{
    private TempDirectory _tempDir = null!;
    private ProcessLogger _processLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = new TempDirectory("ProcMgrIT");
        _processLogger = new ProcessLogger(_tempDir.Path);
    }

    [TearDown]
    public void TearDown()
    {
        _processLogger.Dispose();
        // Small delay to let process handles be released
        Thread.Sleep(200);
        _tempDir.Dispose();
    }

    // --- Szenario 2: Testrun-Flow startet Prozess und erfasst Output ---

    [Test]
    public void StartProcess_WithEchoCommand_CapturesOutputInLogFile()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c echo HelloIntegration",
            processLogger: _processLogger);

        manager.Start();

        var found = LogFileReader.WaitFor(
            _tempDir.Path,
            content => content.Contains("HelloIntegration"));

        Assert.That(found, Is.True, "Expected 'HelloIntegration' in log output");
    }

    [Test]
    public void StartProcess_WithEchoCommand_ProcessExitsNormally()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c echo TestExit",
            processLogger: _processLogger);

        manager.Start();

        // Wait for the short-lived process to finish
        LogFileReader.WaitFor(_tempDir.Path, content => content.Contains("TestExit"));
        Thread.Sleep(500);

        Assert.That(manager.IsCorrectlyRunning(), Is.False,
            "Short-lived echo process should have exited");
    }

    // --- Szenario 3: ProcessManager startet, ueberwacht und stoppt Prozess ---

    [Test]
    public void StartAndStop_LongRunningProcess_LifecycleIsCorrect()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping -n 30 127.0.0.1 > nul",
            processLogger: _processLogger,
            shutdownTimeoutMs: 2000);

        manager.Start();
        Thread.Sleep(1000);

        Assert.That(manager.IsCorrectlyRunning(), Is.True,
            "Process should be running after Start");

        manager.Stop();
        Thread.Sleep(500);

        Assert.That(manager.IsCorrectlyRunning(), Is.False,
            "Process should not be running after Stop");
    }

    [Test]
    public void Stop_LongRunningProcess_LogsShutdownMessages()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping -n 30 127.0.0.1 > nul",
            processLogger: _processLogger,
            shutdownTimeoutMs: 2000);

        manager.Start();
        Thread.Sleep(1000);

        manager.Stop();
        Thread.Sleep(500);

        var logContent = LogFileReader.ReadAll(_tempDir.Path);
        Assert.That(logContent, Does.Contain("Sending shutdown signal..."),
            "Shutdown signal message should be logged");
    }

    // --- Szenario: StdErr wird mit [ERROR]-Prefix erfasst ---

    [Test]
    public void StartProcess_WithStdErr_CapturesErrorPrefixedOutput()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c echo ErrorMessage 1>&2",
            processLogger: _processLogger);

        manager.Start();

        var found = LogFileReader.WaitFor(
            _tempDir.Path,
            content => content.Contains("[ERROR]") && content.Contains("ErrorMessage"));

        Assert.That(found, Is.True,
            "StdErr output should be logged with [ERROR] prefix");
    }

    // --- Szenario: Mehrere Zeilen Output werden erfasst ---

    [Test]
    public void StartProcess_WithMultipleOutputLines_CapturesAll()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c \"echo First & echo Second & echo Third\"",
            processLogger: _processLogger);

        manager.Start();

        var found = LogFileReader.WaitFor(
            _tempDir.Path,
            content => content.Contains("First")
                       && content.Contains("Second")
                       && content.Contains("Third"));

        Assert.That(found, Is.True,
            "All output lines should be captured in log");
    }

    // --- Szenario: Restart nach Prozess-Beendigung ---

    [Test]
    public void Restart_AfterProcessExits_NewProcessRuns()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c echo FirstRun",
            processLogger: _processLogger);

        manager.Start();

        LogFileReader.WaitFor(_tempDir.Path, c => c.Contains("FirstRun"));
        Thread.Sleep(500);

        Assert.That(manager.IsCorrectlyRunning(), Is.False,
            "First process should have exited");

        // Start a new process (simulating restart behavior)
        manager.Start();

        var found = LogFileReader.WaitFor(
            _tempDir.Path,
            content => content.Contains("FirstRun"));

        Assert.That(found, Is.True,
            "Second run output should be captured");
        // After the second echo, process exits again
        Thread.Sleep(500);
        Assert.That(manager.IsCorrectlyRunning(), Is.False);
    }
}