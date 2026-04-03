namespace WindowsServicify.Domain;

public class ProcessLogger
{
    private readonly string _logToDirectory;
    private readonly object _writeLock = new();

    public ProcessLogger(string logToDirectory)
    {
        _logToDirectory = logToDirectory;
    }

    public void Log(string message)
    {
        string logFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        EnsureLogFilePathExists();
        RemoveOldLogs();

        string logMessage = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message;
        string logPath = Path.Combine(_logToDirectory, logFileName);

        lock (_writeLock)
        {
            File.AppendAllText(logPath, "\n" + logMessage);
        }
    }

    public void RemoveOldLogs()
    {
        string[] logFiles = Directory.GetFiles(_logToDirectory, "*.log");
        foreach (string logFile in logFiles)
        {
            FileInfo fileInfo = new(logFile);
            if (fileInfo.CreationTime < DateTime.Now.AddDays(-7))
            {
                fileInfo.Delete();
            }
        }
    }

    public void EnsureLogFilePathExists()
    {
        if (!Directory.Exists(_logToDirectory))
        {
            Directory.CreateDirectory(_logToDirectory);
        }
    }
}