namespace WindowsServicify.Domain
{
    public class ServiceConfigurationRequester
    {

        public static ServiceConfiguration GetServiceConfiguration()
        {
            var serviceName = ConsoleInput.ReadInput("Enter the service name you'd like to setup: ", true);
            var displayName = ConsoleInput.ReadInput("Enter the display name you'd like: ", true);
            var description = ConsoleInput.ReadInput("Enter the service description you'd like: ", false);
            var command = ConsoleInput.ReadInput("Enter the command you'd like to execute: ", true);
            var arguments = ConsoleInput.ReadInput("Enter command arguments: ", false);
            var directory = ConsoleInput.ReadInput("Working directory for the command: ", true);
            var healthCheckPort = ReadOptionalPort("Enter health check port (leave empty to disable): ");

            return new ServiceConfiguration(serviceName, displayName, description, command, directory, arguments, healthCheckPort);
        }

        private static int? ReadOptionalPort(string prompt)
        {
            Console.WriteLine(prompt);

            while (true)
            {
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }

                if (int.TryParse(input, out var port) && port >= 1024 && port <= 65535)
                {
                    return port;
                }

                Console.WriteLine("Invalid port. Please enter a number between 1024 and 65535, or leave empty to disable.");
            }
        }

    }
}