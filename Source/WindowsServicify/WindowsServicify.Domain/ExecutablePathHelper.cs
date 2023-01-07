using System.Reflection;

namespace WindowsServicify.Domain;

public class ExecutablePathHelper
{
    public static string GetExecutablePath()
    {
        string exeLocation = Assembly.GetExecutingAssembly().Location;
        string exeDirectory = Path.GetDirectoryName(exeLocation)!;
        return exeDirectory;
    }
}