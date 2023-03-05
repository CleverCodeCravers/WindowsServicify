namespace WindowsServicify.Domain;

public interface IWindowsServiceInstallHelper
{
    void InstallService(string serviceName, string displayName, string description, string filePath);
    void RemoveService(string serviceName);
}
