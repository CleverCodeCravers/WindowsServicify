using System.Diagnostics;

namespace WindowsServicify.Domain;

public class ExecutablePathHelper
{
    public static string GetExecutablePath()
    {
        var exeLocation = GetExecutableFilePath();
        var exeDirectory = Path.GetDirectoryName(exeLocation)!;
        
        return exeDirectory;
    }

    public static string GetExecutableFilePath()
    {
        return Process.GetCurrentProcess().MainModule.FileName;
    }
}