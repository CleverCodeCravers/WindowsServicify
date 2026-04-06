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
        Console.WriteLine("Use --help to get help information.");
        return 1;
    }

    var parameters = commandLineParametersResult.Value;

    if (parameters.Help)
    {
        var commands = parser.GetCommandsList();

        var commandsWithDescription = commands
            .Select(c => $"{c.Name}\r\n\t{c.Description}")
            .ToList();

        Console.WriteLine($"\nWindows Servicify helps you setup, start and remove windows services." +
                    $"\n\n [Usage]\n\n" +
                    $"\n{string.Join("\r\n", commandsWithDescription)}");

        return 0;
    }

    if (parameters.Configure)
    {
        Console.WriteLine("Please enter the necessary configuration data:");
        var configData = ServiceConfigurationRequester.GetServiceConfiguration();
        ServiceConfigurationFileHandler.Save(configurationFilePath, configData);
        return 0;
    }

    if (parameters.Testrun)
    {
        var configResult = ServiceConfigurationFileHandler.Load(configurationFilePath);
        if (!configResult.IsSuccess)
        {
            Console.WriteLine(configResult.ErrorMessage);
            return 1;
        }

        var configData = configResult.Value;
        using var processLogger = new ProcessLogger(ExecutablePathHelper.GetExecutablePath());
        var processManager = new ProcessManager(configData.Command, configData.WorkingDirectory, configData.Arguments,
            processLogger);
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
        return 0;
    }

    if (parameters.Install)
    {
        if (!InstallService(configurationFilePath, parameters.Legacy))
        {
            return 1;
        }
    }

    if (parameters.Uninstall)
    {
        if (!RemoveService(configurationFilePath, parameters.Legacy))
        {
            return 1;
        }
    }

    return 0;
}

var configurationResult = ServiceConfigurationFileHandler.Load(configurationFilePath);
if (!configurationResult.IsSuccess)
{
    Console.Error.WriteLine(configurationResult.ErrorMessage);
    Environment.Exit(1);
}

var configuration = configurationResult.Value;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = configuration.ServiceName;
    })
    .ConfigureServices(services =>
    {
#pragma warning disable CA1416
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);
#pragma warning restore CA1416
        var processLogger = new ProcessLogger(ExecutablePathHelper.GetExecutablePath());
        services.AddSingleton(processLogger);

        services.AddSingleton(new ProcessManager(
            configuration.Command,
            configuration.WorkingDirectory,
            configuration.Arguments,
            processLogger));
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

return 0;

static bool InstallService(string configurationFilePath, bool legacy)
{
    var installHelper = WindowsServiceInstallHelperFactory.Create(legacy);

    var configResult = ServiceConfigurationFileHandler.Load(configurationFilePath);
    if (!configResult.IsSuccess)
    {
        Console.WriteLine(configResult.ErrorMessage);
        return false;
    }

    var configData = configResult.Value;
    installHelper.InstallService(configData.ServiceName, configData.DisplayName, configData.Description, ExecutablePathHelper.GetExecutableFilePath()!);
    return true;
}

static bool RemoveService(string configurationFilePath, bool legacy)
{
    var installHelper = WindowsServiceInstallHelperFactory.Create(legacy);

    var configResult = ServiceConfigurationFileHandler.Load(configurationFilePath);
    if (!configResult.IsSuccess)
    {
        Console.WriteLine(configResult.ErrorMessage);
        return false;
    }

    var configData = configResult.Value;
    installHelper.RemoveService(configData.ServiceName);
    return true;
}