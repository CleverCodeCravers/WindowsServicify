namespace WindowsServicify.Domain;

public static class ProcessLogger
{
    public static void Log(string message, string logFilePath)
    {
        string logFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        EnsureLogFileExists(logFilePath);
        RemoveOldLogs(logFilePath);

        string logMessage = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message;
        string logPath = Path.Combine(logFilePath, logFileName);
        using StreamWriter logWriter = File.AppendText(logPath);
        logWriter.WriteLine(logMessage);
    }

    public static void RemoveOldLogs(string logFilePath)
    {
        string[] logFiles = Directory.GetFiles(logFilePath, "*.log");
        foreach (string logFile in logFiles)
        {
            FileInfo fileInfo = new(logFile);
            if (fileInfo.CreationTime < DateTime.Now.AddDays(-7))
            {
                fileInfo.Delete();
            }
        }
    }

    public static void EnsureLogFileExists(string logFilePath)
    {

        if (!Directory.Exists(logFilePath))
        {
            Directory.CreateDirectory(logFilePath);
        }

        return;
    }
}