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
            var directory = ConsoleInput.ReadInput("Working directory for the command: ", true);

            return new ServiceConfiguration(serviceName, displayName, description, command, directory);
        }
        
    }
}
