namespace WindowsServicify.Domain;

public static class WindowsServiceInstallHelperFactory {
    public static IWindowsServiceInstallHelper Create(bool legacy)
    {
        if ( legacy )
            return new LegacyWindowsServiceInstallHelper();

        return new PowerShellWindowsServiceInstallHelper();
    }
}
