namespace WindowsServicify.Domain;

public record ConsoleCommandLineParameters(bool Configure, bool Install, bool Uninstall, bool Testrun, bool Legacy, bool Help);