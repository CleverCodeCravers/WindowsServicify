using System.Diagnostics;

namespace WindowsServicify.Domain;

public static class WindowsServiceInstallHelper
{
    public static void InstallService(string serviceName, string filePath)
    {
        Process process = new();
        process.StartInfo.FileName = "sc.exe";
        process.StartInfo.Arguments = $" create \"{serviceName}\" binpath=\"{filePath}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();
        var stdout = process.StandardOutput.ReadToEnd().Trim(); 
        Logger.Log(stdout);
        
        if (process.ExitCode == 0)
        {
            Console.WriteLine("Thank you for installing your service.\r\nNow launch services.msc and configure your new service. You will probably need to adjust the start mode, maybe the user under which it will be executing...");
            Logger.Log($"Service {serviceName} has been installed successfully");
        } else
        {
            Console.WriteLine(stdout);
        }
    }

    public static void RemoveService(string serviceName)
    {
        Process process = new();
        process.StartInfo.FileName = "sc.exe";
        process.StartInfo.Arguments = $" delete \"{serviceName}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();
        var stdout = process.StandardOutput.ReadToEnd().Trim();
        Logger.Log(stdout);

        if (process.ExitCode == 0)
        {
            Console.WriteLine("The Service has been removed successfully!");
            Logger.Log($"Service {serviceName} has been removed successfully");
        } else
        {
            Console.WriteLine(stdout);
        }
    }
}