namespace WindowsServicify.Domain.IntegrationTests;

/// <summary>
/// Helper to read log files that may still be held open by a ProcessLogger.
/// Uses FileShare.ReadWrite to avoid locking conflicts.
/// </summary>
internal static class LogFileReader
{
    /// <summary>
    /// Reads the content of all .log files in the given directory.
    /// </summary>
    public static string ReadAll(string logDirectory)
    {
        var logFiles = Directory.GetFiles(logDirectory, "*.log");
        if (logFiles.Length == 0)
            return string.Empty;

        var combined = new System.Text.StringBuilder();
        foreach (var logFile in logFiles)
        {
            using var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            combined.Append(reader.ReadToEnd());
        }

        return combined.ToString();
    }

    /// <summary>
    /// Polls the log directory until the predicate is satisfied or the timeout expires.
    /// </summary>
    public static bool WaitFor(
        string logDirectory,
        Func<string, bool> predicate,
        int timeoutMs = 10000,
        int pollIntervalMs = 200)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            var content = ReadAll(logDirectory);
            if (predicate(content))
                return true;
            Thread.Sleep(pollIntervalMs);
        }

        return false;
    }
}
