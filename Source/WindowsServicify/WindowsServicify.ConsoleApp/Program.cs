using CommandLineArgumentsParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using WindowsServicify.Domain;

var runningInConsole = Environment.UserInteractive;
var configurationFilePath = Path.Combine(ExecutablePathHelper.GetExecutablePath(), "config.json");

if (runningInConsole)
{
    var parser = new ConsoleCommandLineParser();

    var commandLineParametersResult = parser.Parse(args);

    if (!commandLineParametersResult.IsSuccess)
    {
        Console.WriteLine("Unfortunately there have been problems with the command line arguments.");
        return;
    }

    var parameters = commandLineParametersResult.Value;

    if (parameters.Help)
    {
        ICommandLineOption[] commands = parser.GetCommandsList();
        string[] commandsDescription =
        {
                    "Prompts questions to configure the service",
                    "Installs the windows service",
                    "Removes the windows service",
                    "Performs a test run for the service and outputs the result to the console",
                    "Prints out the commands and their corresponding description",
                };

        List<string> commandsWithDescription = new();

        for (int i = 0; i < commands.Length; i++)
        {
            commandsWithDescription.Add($"{commands[i].Name}\r\n\t{commandsDescription[i]}");
        }

        Console.WriteLine($"\nWindows Servicify helps you setup, start and remove windows services." +
                    $"\n\n [Usage]\n\n" +
                    $"\n{string.Join("\r\n", commandsWithDescription)}");

        return;
    }

    if (parameters.Configure)
    {
        Console.WriteLine("Please enter the necessary configuration data:");
        var configData = ServiceConfigurationRequester.GetServiceConfiguration();
        ServiceConfigurationFileHandler.Save(configurationFilePath,configData);
        return;
    }

    if (parameters.Testrun)
    {
        var configData = ServiceConfigurationFileHandler.Load(configurationFilePath);
        var processManager = new ProcessManager(configData.Command, configData.WorkingDirectory, configData.Arguments);
        Console.WriteLine("Process starting...");
        processManager.Start();
        while (!Console.KeyAvailable)
        {
            Console.Write(".");
            if (!processManager.IsCorrectlyRunning())
            {
                Console.WriteLine("Restarting process...");
                processManager.Start();
            }
            Thread.Sleep(1000);
        }
        Console.WriteLine("Good bye!");
        processManager.Stop();
    }

    if (parameters.Install)
    {
        var configData = ServiceConfigurationFileHandler.Load(configurationFilePath);
        WindowsServiceInstallHelper.InstallService(configData.ServiceName, 
            ExecutablePathHelper.GetExecutableFilePath()
            );
    }

    if (parameters.Uninstall)
    {
        var configData = ServiceConfigurationFileHandler.Load(configurationFilePath);
        WindowsServiceInstallHelper.RemoveService(configData.ServiceName);
    }

    return;
}

var configuration = ServiceConfigurationFileHandler.Load(configurationFilePath);

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = configuration.ServiceName;
    })
    .ConfigureServices(services =>
    {
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);

        services.AddSingleton(new ProcessManager(configuration.Command, configuration.WorkingDirectory, configuration.Arguments));
        services.AddHostedService<WindowsBackgroundService>();
    })
    .ConfigureLogging((context, logging) =>
    {
        // See: https://github.com/dotnet/runtime/issues/47303
        logging.AddConfiguration(
            context.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();

