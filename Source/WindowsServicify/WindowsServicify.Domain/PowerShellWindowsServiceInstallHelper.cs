using System.Diagnostics;

namespace WindowsServicify.Domain;

public class PowerShellWindowsServiceInstallHelper : IWindowsServiceInstallHelper
{
    private static string SanitizeForPowershell(string value)
    {
        return value.Replace("\"", "").Replace(Environment.NewLine, "").Replace("&", "");
    }

    private string GetPowershellCommand(string serviceName, string displayName, string description, string filePath)
    {
        serviceName = SanitizeForPowershell(serviceName);
        description = SanitizeForPowershell(description);
        displayName = SanitizeForPowershell(displayName);

        var command = $@"New-Service -Name ""{serviceName}"" -DisplayName ""{displayName}"" -Description ""{description}"" -BinaryPathName ""{filePath}""";
        return command;
    }

    public void InstallService(string serviceName, string displayName, string description, string filePath)
    {
        var command = GetPowershellCommand(serviceName, displayName, description, filePath);
        var (exitCode, stdout) = ExecutePowerShellCommand(command);

        if (exitCode == 0)
        {
            Console.WriteLine(stdout);
            Console.WriteLine("Thank you for installing your service.\r\nNow launch services.msc and configure your new service. You will probably need to adjust the start mode, maybe the user under which it will be executing...");
        }
        else
        {
            Console.WriteLine(stdout);
        }
    }

    private static string EncodeCommand(string command)
    {
        byte[] plainTextBytes = System.Text.Encoding.Unicode.GetBytes(command);
        string encodedText = Convert.ToBase64String(plainTextBytes); 
        return encodedText;
    }

    private static (int exitCode, string stdout) ExecutePowerShellCommand(string command)
    {
        var process = new Process();
        process.StartInfo.FileName = "powershell.exe";
        process.StartInfo.Arguments = "-EncodedCommand  \"" + EncodeCommand(command) + "\" ";
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();

        var stdout = process.StandardOutput.ReadToEnd().Trim();

        int exitCode = process.ExitCode;

        return (exitCode, stdout);
    }

    public void RemoveService(string serviceName)
    {
        var (exitCode, stdout) = ExecutePowerShellCommand("Get-Service -Name " + SanitizeForPowershell(serviceName) + " | Remove-Service");

        if (exitCode == 0)
        {
            Console.WriteLine("The Service has been removed successfully!");
        }
        else
        {
            Console.WriteLine(stdout);
        }

    }
}
