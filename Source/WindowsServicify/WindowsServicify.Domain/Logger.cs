namespace WindowsServicify.Domain;

public static class Logger
{
    public static void Log(string message)
    {
        string logFileName = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        string logDirectory = @"C:\WindowsServicify\Logs";

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        string[] logFiles = Directory.GetFiles(logDirectory, "*.log");
        foreach (string logFile in logFiles)
        {
            FileInfo fileInfo = new(logFile);
            if (fileInfo.CreationTime < DateTime.Now.AddDays(-7))
            {
                fileInfo.Delete();
            }
        }

        string logMessage = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message;
        string logPath = Path.Combine(logDirectory, logFileName);
        using StreamWriter logWriter = File.AppendText(logPath);
        logWriter.WriteLine(logMessage);
    }
}