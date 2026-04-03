using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class ProcessManagerTests
{
    private string _logDirectory = null!;
    private ProcessLogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _logDirectory = Path.Combine(Path.GetTempPath(), "ProcessManagerTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_logDirectory);
        _logger = new ProcessLogger(_logDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        // Dispose the logger first to release file handles
        _logger?.Dispose();

        // Allow processes to fully release file handles
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

    private string GetLogContent()
    {
        var logFiles = Directory.GetFiles(_logDirectory, "*.log");
        if (logFiles.Length == 0)
            return string.Empty;

        // Use FileShare.ReadWrite because ProcessLogger keeps the file open with a StreamWriter
        using var fs = new FileStream(logFiles[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fs);
        return reader.ReadToEnd();
    }

    private static void WaitForLogContent(Func<string> getContent, Func<string, bool> predicate, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            var content = getContent();
            if (predicate(content))
                return;
            Thread.Sleep(100);
        }
    }

    // --- Szenario 1: StdOut-Ausgaben werden vollstaendig erfasst ---

    [Test]
    public void Start_CapturesStdOutFromProcess()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _logDirectory,
            arguments: "/c echo HelloFromStdOut",
            processLogger: _logger);

        manager.Start();

        WaitForLogContent(GetLogContent, c => c.Contains("HelloFromStdOut"));

        var logContent = GetLogContent();
        Assert.That(logContent, Does.Contain("HelloFromStdOut"));
    }

    // --- Szenario 2: StdErr-Ausgaben werden erfasst und gekennzeichnet ---

    [Test]
    public void Start_CapturesStdErrWithErrorPrefix()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _logDirectory,
            arguments: "/c echo HelloFromStdErr 1>&2",
            processLogger: _logger);

        manager.Start();

        WaitForLogContent(GetLogContent, c => c.Contains("[ERROR]"));

        var logContent = GetLogContent();
        Assert.That(logContent, Does.Contain("[ERROR]"));
        Assert.That(logContent, Does.Contain("HelloFromStdErr"));
    }

    // --- Szenario 3: Event-Handler vor asynchronem Lesen (implizit durch Szenario 1+2) ---
    // Die Tatsache, dass StdOut und StdErr erfasst werden, beweist die korrekte Reihenfolge.
    // Ein expliziter Test prueft, dass auch die allererste Zeile erfasst wird.

    [Test]
    public void Start_CapturesFirstLineOfOutput()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _logDirectory,
            arguments: "/c echo VeryFirstLine",
            processLogger: _logger);

        manager.Start();

        WaitForLogContent(GetLogContent, c => c.Contains("VeryFirstLine"));

        var logContent = GetLogContent();
        Assert.That(logContent, Does.Contain("VeryFirstLine"));
    }

    // --- Szenario 4: StdOut und StdErr gleichzeitig ---

    [Test]
    public void Start_CapturesBothStdOutAndStdErr()
    {
        // cmd /c with parentheses to output to both streams
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _logDirectory,
            arguments: "/c \"echo StdOutLine & echo StdErrLine 1>&2\"",
            processLogger: _logger);

        manager.Start();

        WaitForLogContent(GetLogContent, c =>
            c.Contains("StdOutLine") && c.Contains("[ERROR]") && c.Contains("StdErrLine"));

        var logContent = GetLogContent();
        Assert.That(logContent, Does.Contain("StdOutLine"));
        Assert.That(logContent, Does.Contain("[ERROR]"));
        Assert.That(logContent, Does.Contain("StdErrLine"));
    }

    // --- StdOut hat kein [ERROR]-Prefix ---

    [Test]
    public void Start_StdOutDoesNotHaveErrorPrefix()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _logDirectory,
            arguments: "/c echo OnlyStdOutHere",
            processLogger: _logger);

        manager.Start();

        WaitForLogContent(GetLogContent, c => c.Contains("OnlyStdOutHere"));

        var logContent = GetLogContent();
        // StdOut sollte ohne [ERROR]-Prefix geloggt werden
        Assert.That(logContent, Does.Contain("OnlyStdOutHere"));
        Assert.That(logContent, Does.Not.Contain("[ERROR]"));
    }

    // --- IsCorrectlyRunning ---

    [Test]
    public void IsCorrectlyRunning_BeforeStart_ReturnsFalse()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _logDirectory,
            arguments: "/c echo test",
            processLogger: _logger);

        Assert.That(manager.IsCorrectlyRunning(), Is.False);
    }

    [Test]
    public void IsCorrectlyRunning_WhileRunning_ReturnsTrue()
    {
        // Use system temp as working directory to avoid locking _logDirectory
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping 127.0.0.1 -n 10 > nul",
            processLogger: _logger);

        manager.Start();
        Thread.Sleep(500);

        try
        {
            Assert.That(manager.IsCorrectlyRunning(), Is.True);
        }
        finally
        {
            manager.Stop();
            Thread.Sleep(500);
        }
    }

    // --- Stop ---

    [Test]
    public void Stop_TerminatesRunningProcess()
    {
        // Use system temp as working directory to avoid locking _logDirectory
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: Path.GetTempPath(),
            arguments: "/c ping 127.0.0.1 -n 30 > nul",
            processLogger: _logger);

        manager.Start();
        Thread.Sleep(500);
        Assert.That(manager.IsCorrectlyRunning(), Is.True);

        manager.Stop();
        Thread.Sleep(500);

        Assert.That(manager.IsCorrectlyRunning(), Is.False);
    }

    // --- Mehrere Zeilen StdOut ---

    [Test]
    public void Start_CapturesMultipleStdOutLines()
    {
        var manager = new ProcessManager(
            command: "cmd.exe",
            workingDirectory: _logDirectory,
            arguments: "/c \"echo Line1 & echo Line2 & echo Line3\"",
            processLogger: _logger);

        manager.Start();

        WaitForLogContent(GetLogContent, c =>
            c.Contains("Line1") && c.Contains("Line2") && c.Contains("Line3"));

        var logContent = GetLogContent();
        Assert.That(logContent, Does.Contain("Line1"));
        Assert.That(logContent, Does.Contain("Line2"));
        Assert.That(logContent, Does.Contain("Line3"));
    }
}
