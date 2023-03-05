using System.Diagnostics;

namespace WindowsServicify.Domain;


public class LegacyWindowsServiceInstallHelper : IWindowsServiceInstallHelper
{
    public void InstallService(string serviceName, string displayName, string description, string filePath)
    {
        Process process = new();
        process.StartInfo.FileName = "sc.exe";
        process.StartInfo.Arguments = $" create \"{serviceName}\" binpath=\"{filePath}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();
        var stdout = process.StandardOutput.ReadToEnd().Trim();
        
        if (process.ExitCode == 0)
        {
            Console.WriteLine("Thank you for installing your service.\r\nNow launch services.msc and configure your new service. You will probably need to adjust the start mode, maybe the user under which it will be executing...");
        } 
        else
        {
            Console.WriteLine(stdout);
        }
    }

    public void RemoveService(string serviceName)
    {
        Process process = new();
        process.StartInfo.FileName = "sc.exe";
        process.StartInfo.Arguments = $" delete \"{serviceName}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();
        var stdout = process.StandardOutput.ReadToEnd().Trim();

        if (process.ExitCode == 0)
        {
            Console.WriteLine("The Service has been removed successfully!");
        } 
        else
        {
            Console.WriteLine(stdout);
        }
    }
}