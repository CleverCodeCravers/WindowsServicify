using CSharp_CommandLineArgumentsParser;

namespace WindowsServicify.Domain;
public class ConsoleCommandLineParser
{
    public Result<ConsoleCommandLineParameters> Parse(string[] args)
    {
        var parser = new Parser(
            new ICommandLineOption[] {
                new BoolCommandLineOption("--configure"),
                new BoolCommandLineOption("--install"),
                new BoolCommandLineOption("--uninstall"),
                new BoolCommandLineOption("--testrun")
            });

        if (!parser.TryParse(args, true) || args.Length == 0)
        {
            return Result<ConsoleCommandLineParameters>.Failure("Invalid command line arguments");
        }

        return Result<ConsoleCommandLineParameters>.Success(
            new ConsoleCommandLineParameters(
                parser.GetBoolOption("--configure"),
                parser.GetBoolOption("--install"),
                parser.GetBoolOption("--uninstall"),
                parser.GetBoolOption("--testrun")
            ));
    }
}