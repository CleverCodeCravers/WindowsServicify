using System.Diagnostics;

namespace WindowsServicify.Domain;

public static class WindowsServiceInstallHelper
{
    public static void InstallService(string serviceName, string filePath)
    {
        Process process = new();
        process.StartInfo.FileName = "sc.exe";
        process.StartInfo.Arguments = $" create \"{serviceName}\" binpath=\"{filePath}\"";
        process.Start();
        process.WaitForExit();
    }

    public static void RemoveService(string serviceName)
    {
        Process process = new();
        process.StartInfo.FileName = "sc.exe";
        process.StartInfo.Arguments = $" delete \"{serviceName}\"";
        process.Start();
        process.WaitForExit();
    }
}