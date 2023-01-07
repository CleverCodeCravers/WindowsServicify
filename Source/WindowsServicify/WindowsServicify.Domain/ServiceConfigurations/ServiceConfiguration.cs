namespace WindowsServicify.Domain;

public record ServiceConfiguration(
    string ServiceName,
    string DisplayName,
    string Description,
    string Command,
    string WorkingDirectory,
    string Arguments);