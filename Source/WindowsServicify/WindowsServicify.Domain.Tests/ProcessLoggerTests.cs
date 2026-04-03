using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class ProcessLoggerTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "ProcessLoggerTests_" + Guid.NewGuid().ToString("N"));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    /// <summary>
    /// Reads file content even when the file is held open by a StreamWriter.
    /// On Windows, File.ReadAllText fails because it opens with FileShare.Read,
    /// which conflicts with our write lock. This helper uses FileShare.ReadWrite.
    /// </summary>
    private static string ReadFileContent(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // --- Szenario 1: Verzeichnis-Existenz wird einmalig im Konstruktor geprueft ---

    [Test]
    public void Constructor_CreatesDirectoryIfNotExists()
    {
        Assert.That(Directory.Exists(_testDirectory), Is.False);

        using var logger = new ProcessLogger(_testDirectory);

        Assert.That(Directory.Exists(_testDirectory), Is.True);
    }

    [Test]
    public void Constructor_DoesNotThrowIfDirectoryAlreadyExists()
    {
        Directory.CreateDirectory(_testDirectory);

        Assert.DoesNotThrow(() =>
        {
            using var logger = new ProcessLogger(_testDirectory);
        });
    }

    // --- Szenario 2 & 3: Log-Bereinigung (maximal einmal pro Tag) ---

    [Test]
    public void RemoveOldLogs_DeletesFilesOlderThanSevenDays()
    {
        Directory.CreateDirectory(_testDirectory);

        var oldFile = Path.Combine(_testDirectory, "old.log");
        File.WriteAllText(oldFile, "old content");
        File.SetCreationTime(oldFile, DateTime.Now.AddDays(-8));

        var recentFile = Path.Combine(_testDirectory, "recent.log");
        File.WriteAllText(recentFile, "recent content");

        using var logger = new ProcessLogger(_testDirectory);
        logger.RemoveOldLogs();

        Assert.That(File.Exists(oldFile), Is.False, "Old log file should be deleted");
        Assert.That(File.Exists(recentFile), Is.True, "Recent log file should remain");
    }

    [Test]
    public void RemoveOldLogs_KeepsFilesExactlySevenDaysOld()
    {
        Directory.CreateDirectory(_testDirectory);

        var borderlineFile = Path.Combine(_testDirectory, "borderline.log");
        File.WriteAllText(borderlineFile, "borderline content");
        File.SetCreationTime(borderlineFile, DateTime.Now.AddDays(-7).AddMinutes(1));

        using var logger = new ProcessLogger(_testDirectory);
        logger.RemoveOldLogs();

        Assert.That(File.Exists(borderlineFile), Is.True, "File exactly 7 days old should remain");
    }

    [Test]
    public void RemoveOldLogs_DoesNotThrowOnEmptyDirectory()
    {
        Directory.CreateDirectory(_testDirectory);

        using var logger = new ProcessLogger(_testDirectory);

        Assert.DoesNotThrow(() => logger.RemoveOldLogs());
    }

    [Test]
    public void Log_TriggersCleanupOnFirstCall()
    {
        Directory.CreateDirectory(_testDirectory);

        var oldFile = Path.Combine(_testDirectory, "2020-01-01.log");
        File.WriteAllText(oldFile, "old");
        File.SetCreationTime(oldFile, DateTime.Now.AddDays(-10));

        using var logger = new ProcessLogger(_testDirectory);
        logger.Log("test");

        Assert.That(File.Exists(oldFile), Is.False, "Old log should be cleaned up on first Log() call");
    }

    [Test]
    public void Log_DoesNotCleanupOnSubsequentCallsWithin24Hours()
    {
        Directory.CreateDirectory(_testDirectory);

        using var logger = new ProcessLogger(_testDirectory);

        // First call triggers cleanup
        logger.Log("first");

        // Create an old file after first cleanup
        var oldFile = Path.Combine(_testDirectory, "2020-01-02.log");
        File.WriteAllText(oldFile, "old");
        File.SetCreationTime(oldFile, DateTime.Now.AddDays(-10));

        // Second call should NOT trigger cleanup (within 24h)
        logger.Log("second");

        Assert.That(File.Exists(oldFile), Is.True, "Old log should remain because cleanup was already done recently");
    }

    // --- Szenario 4: StreamWriter statt File.AppendAllText ---

    [Test]
    public void Log_WritesMessageToLogFile()
    {
        using var logger = new ProcessLogger(_testDirectory);
        logger.Log("Hello World");

        var logFile = Directory.GetFiles(_testDirectory, "*.log").Single();
        var content = ReadFileContent(logFile);

        Assert.That(content, Does.Contain("Hello World"));
    }

    [Test]
    public void Log_MultipleCallsAppendToSameFile()
    {
        using var logger = new ProcessLogger(_testDirectory);
        logger.Log("First message");
        logger.Log("Second message");

        var logFile = Directory.GetFiles(_testDirectory, "*.log").Single();
        var content = ReadFileContent(logFile);

        Assert.That(content, Does.Contain("First message"));
        Assert.That(content, Does.Contain("Second message"));
    }

    // --- Szenario 5: Tageswechsel (indirekt testbar ueber Dateinamen) ---

    [Test]
    public void Log_CreatesLogFileWithTodaysDate()
    {
        using var logger = new ProcessLogger(_testDirectory);
        logger.Log("test");

        var expectedFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        var logFiles = Directory.GetFiles(_testDirectory, "*.log").Select(Path.GetFileName).ToList();

        Assert.That(logFiles, Does.Contain(expectedFileName));
    }

    // --- Szenario 6: IDisposable ---

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var logger = new ProcessLogger(_testDirectory);
        logger.Log("test");

        Assert.DoesNotThrow(() =>
        {
            logger.Dispose();
            logger.Dispose();
        });
    }

    [Test]
    public void Log_AfterDispose_ThrowsObjectDisposedException()
    {
        var logger = new ProcessLogger(_testDirectory);
        logger.Dispose();

        Assert.Throws<ObjectDisposedException>(() => logger.Log("test"));
    }

    [Test]
    public void Dispose_FlushesBufferedContent()
    {
        var logger = new ProcessLogger(_testDirectory);
        logger.Log("buffered content");
        logger.Dispose();

        var logFile = Directory.GetFiles(_testDirectory, "*.log").Single();
        var content = File.ReadAllText(logFile);

        Assert.That(content, Does.Contain("buffered content"));
    }

    // --- Szenario 7: Thread-Safety ---

    [Test]
    public void Log_ConcurrentAccess_DoesNotThrow()
    {
        using var logger = new ProcessLogger(_testDirectory);

        var tasks = Enumerable.Range(0, 50)
            .Select(i => Task.Run(() => logger.Log($"Thread message {i}")))
            .ToArray();

        Assert.DoesNotThrowAsync(async () => await Task.WhenAll(tasks));
    }

    [Test]
    public void Log_ConcurrentAccess_AllMessagesWritten()
    {
        var logger = new ProcessLogger(_testDirectory);
        const int messageCount = 50;

        var tasks = Enumerable.Range(0, messageCount)
            .Select(i => Task.Run(() => logger.Log($"MSG-{i:D4}")))
            .ToArray();

        Task.WaitAll(tasks);
        logger.Dispose();

        var logFile = Directory.GetFiles(_testDirectory, "*.log").Single();
        var content = File.ReadAllText(logFile);

        for (var i = 0; i < messageCount; i++)
        {
            Assert.That(content, Does.Contain($"MSG-{i:D4}"),
                $"Message MSG-{i:D4} should be present in log file");
        }
    }

    // --- Szenario 8: Log-Format bleibt identisch ---

    [Test]
    public void Log_WritesCorrectFormat()
    {
        using var logger = new ProcessLogger(_testDirectory);
        logger.Log("Test message");

        var logFile = Directory.GetFiles(_testDirectory, "*.log").Single();
        var content = ReadFileContent(logFile);

        // Format: \n[yyyy-MM-dd HH:mm:ss] message
        var datePrefix = DateTime.Now.ToString("yyyy-MM-dd");
        Assert.That(content, Does.Match(@"\n\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] Test message"));
        Assert.That(content, Does.Contain($"[{datePrefix}"));
    }

    [Test]
    public void Log_PrependsNewline()
    {
        var logger = new ProcessLogger(_testDirectory);
        logger.Log("first");
        logger.Log("second");
        logger.Dispose();

        var logFile = Directory.GetFiles(_testDirectory, "*.log").Single();
        var content = File.ReadAllText(logFile);

        // Each message starts with \n (consistent with original behavior)
        Assert.That(content, Does.StartWith("\n["));
        Assert.That(content, Does.Contain("first\n["));
    }

    // --- Edge Cases ---

    [Test]
    public void Log_EmptyMessage_WritesTimestampOnly()
    {
        var logger = new ProcessLogger(_testDirectory);
        logger.Log("");
        logger.Dispose();

        var logFile = Directory.GetFiles(_testDirectory, "*.log").Single();
        var content = File.ReadAllText(logFile);

        Assert.That(content, Does.Match(@"\n\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] $"));
    }

    [Test]
    public void EnsureLogFilePathExists_DoesNotThrowIfDirectoryAlreadyExists()
    {
        Directory.CreateDirectory(_testDirectory);
        using var logger = new ProcessLogger(_testDirectory);

        Assert.DoesNotThrow(() => logger.EnsureLogFilePathExists());
    }

    [Test]
    public void RemoveOldLogs_IgnoresNonLogFiles()
    {
        Directory.CreateDirectory(_testDirectory);

        var txtFile = Path.Combine(_testDirectory, "keep-me.txt");
        File.WriteAllText(txtFile, "not a log");
        File.SetCreationTime(txtFile, DateTime.Now.AddDays(-10));

        using var logger = new ProcessLogger(_testDirectory);
        logger.RemoveOldLogs();

        Assert.That(File.Exists(txtFile), Is.True, "Non-.log files should not be deleted");
    }

    [Test]
    public void RemoveOldLogs_DoesNotThrowIfDirectoryDoesNotExist()
    {
        var nonExistentDir = Path.Combine(Path.GetTempPath(), "NonExistent_" + Guid.NewGuid().ToString("N"));
        using var logger = new ProcessLogger(_testDirectory);

        // Delete the directory that was created by the constructor to simulate non-existent directory
        // We need to manipulate the logger's internal state, so instead we call RemoveOldLogs
        // on a logger whose log directory was deleted after construction
        Directory.Delete(_testDirectory, recursive: true);

        Assert.DoesNotThrow(() => logger.RemoveOldLogs());
    }
}
