namespace WindowsServicify.Domain;

public class ProcessLogger : IDisposable
{
    private readonly string _logToDirectory;
    private readonly object _lock = new();
    private DateTime _lastCleanup = DateTime.MinValue;
    private StreamWriter? _currentWriter;
    private string? _currentLogFileName;
    private bool _disposed;

    public ProcessLogger(string logToDirectory)
    {
        _logToDirectory = logToDirectory;
        EnsureLogFilePathExists();
    }

    public void Log(string message)
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            CleanupIfNeeded();

            var logFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            EnsureCorrectWriter(logFileName);

            var logMessage = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message;
            _currentWriter!.Write("\n" + logMessage);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            lock (_lock)
            {
                _currentWriter?.Dispose();
                _currentWriter = null;
                _currentLogFileName = null;
                _disposed = true;
            }
        }
    }

    internal void RemoveOldLogs()
    {
        if (!Directory.Exists(_logToDirectory))
            return;

        var logFiles = Directory.GetFiles(_logToDirectory, "*.log");
        foreach (var logFile in logFiles)
        {
            var fileInfo = new FileInfo(logFile);
            if (fileInfo.CreationTime < DateTime.Now.AddDays(-7))
            {
                fileInfo.Delete();
            }
        }
    }

    internal void EnsureLogFilePathExists()
    {
        if (!Directory.Exists(_logToDirectory))
        {
            Directory.CreateDirectory(_logToDirectory);
        }
    }

    private void CleanupIfNeeded()
    {
        if ((DateTime.Now - _lastCleanup).TotalHours >= 24)
        {
            RemoveOldLogs();
            _lastCleanup = DateTime.Now;
        }
    }

    private void EnsureCorrectWriter(string logFileName)
    {
        if (_currentLogFileName == logFileName && _currentWriter != null)
            return;

        _currentWriter?.Dispose();

        var logPath = Path.Combine(_logToDirectory, logFileName);
        var fileStream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read);
        _currentWriter = new StreamWriter(fileStream) { AutoFlush = true };
        _currentLogFileName = logFileName;
    }
}
