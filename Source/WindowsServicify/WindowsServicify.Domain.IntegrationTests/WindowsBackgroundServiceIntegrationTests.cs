using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace WindowsServicify.Domain.IntegrationTests;

/// <summary>
/// Integration tests for WindowsBackgroundService lifecycle.
///
/// Uses TestProcessExitHandler to prevent Environment.Exit(1) from
/// terminating the test process when the CancellationToken is cancelled.
/// </summary>
[TestFixture]
public class WindowsBackgroundServiceIntegrationTests
{
    private TempDirectory _tempDir = null!;
    private TestProcessExitHandler _exitHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = new TempDirectory("BgSvcIT");
        _exitHandler = new TestProcessExitHandler();
    }

    [TearDown]
    public void TearDown()
    {
        Thread.Sleep(300);
        _tempDir.Dispose();
    }

    private WindowsBackgroundService CreateService(
        ProcessManager processManager,
        ProcessLogger processLogger)
    {
        var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Warning));
        var logger = loggerFactory.CreateLogger<WindowsBackgroundService>();
        return new WindowsBackgroundService(processManager, logger, processLogger, _exitHandler);
    }

    // --- Szenario 4: WindowsBackgroundService starts configured process ---

    [Test]
    public async Task HostStart_StartsConfiguredProcess()
    {
        using var processLogger = new ProcessLogger(_tempDir.Path);
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c echo HostStartTest",
            processLogger: processLogger);

        var service = CreateService(processManager, processLogger);

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);

        var found = LogFileReader.WaitFor(
            _tempDir.Path,
            content => content.Contains("HostStartTest"),
            timeoutMs: 5000);

        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        Assert.That(found, Is.True,
            "Process output 'HostStartTest' should appear in logs after service start");
    }

    // --- Szenario 4: StopAsync terminates process and logs message ---

    [Test]
    public async Task StopAsync_TerminatesProcessAndLogs()
    {
        using var processLogger = new ProcessLogger(_tempDir.Path);
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping -n 60 127.0.0.1 > nul",
            processLogger: processLogger,
            shutdownTimeoutMs: 2000);

        var service = CreateService(processManager, processLogger);

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);

        // Wait for process to actually start
        var processStarted = false;
        for (var i = 0; i < 20; i++)
        {
            if (processManager.IsCorrectlyRunning())
            {
                processStarted = true;
                break;
            }
            await Task.Delay(250);
        }

        Assert.That(processStarted, Is.True,
            "Process should be running after service start");

        cts.Cancel();
        await service.StopAsync(CancellationToken.None);
        await Task.Delay(500);

        Assert.That(processManager.IsCorrectlyRunning(), Is.False,
            "Process should be stopped after StopAsync");

        var logContent = LogFileReader.ReadAll(_tempDir.Path);
        Assert.That(logContent, Does.Contain("Stopped Background Service"),
            "StopAsync should log 'Stopped Background Service'");
    }

    // --- Szenario 5: BackgroundService detects exited process and restarts it ---

    [Test]
    public async Task ExecuteAsync_RestartsProcessWhenItExits()
    {
        using var processLogger = new ProcessLogger(_tempDir.Path);
        // Short-lived process that exits immediately
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c echo ShortLived",
            processLogger: processLogger);

        var service = CreateService(processManager, processLogger);

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);

        // The BackgroundService checks every 5 seconds. Wait long enough for
        // the process to exit and the restart check to trigger.
        var restartLogged = LogFileReader.WaitFor(
            _tempDir.Path,
            content => content.Contains("Restarting Process..."),
            timeoutMs: 15000);

        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        Assert.That(restartLogged, Is.True,
            "BackgroundService should log 'Restarting Process...' when process exits");
    }

    // --- Szenario 4: Host integration with real DI container ---

    [Test]
    public void HostIntegration_ProcessManagerAndLoggerResolvedViaDI()
    {
        using var processLogger = new ProcessLogger(_tempDir.Path);
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _tempDir.Path,
            arguments: "/c echo DITest",
            processLogger: processLogger);

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(processLogger);
                services.AddSingleton(processManager);
                services.AddSingleton<IProcessExitHandler>(_exitHandler);
                services.AddHostedService<WindowsBackgroundService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Warning);
            })
            .Build();

        var resolvedManager = host.Services.GetRequiredService<ProcessManager>();
        var resolvedLogger = host.Services.GetRequiredService<ProcessLogger>();
        var resolvedExit = host.Services.GetRequiredService<IProcessExitHandler>();

        Assert.That(resolvedManager, Is.SameAs(processManager));
        Assert.That(resolvedLogger, Is.SameAs(processLogger));
        Assert.That(resolvedExit, Is.SameAs(_exitHandler));
    }

    // --- Szenario: ExitHandler is called on unhandled exception path ---

    [Test]
    public async Task ExecuteAsync_CallsExitHandler_OnCancellation()
    {
        using var processLogger = new ProcessLogger(_tempDir.Path);
        var processManager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping -n 60 127.0.0.1 > nul",
            processLogger: processLogger,
            shutdownTimeoutMs: 1000);

        var service = CreateService(processManager, processLogger);

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);

        // Wait for process to start
        LogFileReader.WaitFor(_tempDir.Path, _ => processManager.IsCorrectlyRunning(), timeoutMs: 5000);

        // Cancel the token - this triggers OperationCanceledException in ExecuteAsync
        cts.Cancel();

        // Give ExecuteAsync time to hit the catch block
        await Task.Delay(2000);

        await service.StopAsync(CancellationToken.None);

        // The TestProcessExitHandler should have recorded the exit call
        Assert.That(_exitHandler.ExitWasCalled, Is.True,
            "Exit handler should be called when cancellation causes exception");
        Assert.That(_exitHandler.LastExitCode, Is.EqualTo(1),
            "Exit code should be 1");
    }
}
