using WindowsServicify.Domain;

var parser = new ConsoleCommandLineParser();

var commandLineParametersResult = parser.Parse(args);

if (!commandLineParametersResult.IsSuccess)
{
    Console.WriteLine("Unfortunately there have been problems with the command line arguments.");
    return;
}
var configurationFilePath = Path.Combine(ExecutablePathHelper.GetExecutablePath(), "config.json");

var parameters = commandLineParametersResult.Value;

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