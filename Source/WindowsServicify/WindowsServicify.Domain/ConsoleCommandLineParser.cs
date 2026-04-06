namespace WindowsServicify.Domain;

public class ConsoleCommandLineParser
{
    private static readonly CommandDefinition[] Commands =
    {
        new("--configure", "Prompts questions to configure the service"),
        new("--install", "Installs the windows service"),
        new("--uninstall", "Removes the windows service"),
        new("--testrun", "Performs a test run for the service and outputs the result to the console"),
        new("--legacy", "Uses legacy sc.exe instead of PowerShell for service management"),
        new("--help", "Prints out the commands and their corresponding description"),
    };

    public CommandDefinition[] GetCommandsList()
    {
        return Commands;
    }

    public Result<ConsoleCommandLineParameters> Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return Result<ConsoleCommandLineParameters>.Failure("No arguments provided.");
        }

        var configure = false;
        var install = false;
        var uninstall = false;
        var testrun = false;
        var legacy = false;
        var help = false;

        foreach (var arg in args)
        {
            switch (arg.ToLowerInvariant())
            {
                case "--configure":
                    configure = true;
                    break;
                case "--install":
                    install = true;
                    break;
                case "--uninstall":
                    uninstall = true;
                    break;
                case "--testrun":
                    testrun = true;
                    break;
                case "--legacy":
                    legacy = true;
                    break;
                case "--help":
                    help = true;
                    break;
                default:
                    return Result<ConsoleCommandLineParameters>.Failure(
                        $"Unknown argument: {arg}");
            }
        }

        return Result<ConsoleCommandLineParameters>.Success(
            new ConsoleCommandLineParameters(configure, install, uninstall, testrun, legacy, help));
    }
}

public record CommandDefinition(string Name, string Description);