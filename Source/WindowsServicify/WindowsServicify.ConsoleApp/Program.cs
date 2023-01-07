using WindowsServicify.Domain;

var parser = new ConsoleCommandLineParser();

var commandLineParametersResult = parser.Parse(args);

if (!commandLineParametersResult.IsSuccess)
{
    Console.WriteLine("Unfortunately there have been problems with the command line arguments.");
    return;
}

var parameters = commandLineParametersResult.Value;

if (parameters.Configure)
{
    Console.WriteLine("Hey There, Let's Configure Windows Servicify Together!");
    new ServiceConfiguration().ConfigureService();
}