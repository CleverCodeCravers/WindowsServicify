using System.Text.Json;

namespace WindowsServicify.Domain
{
    public class ServiceConfiguration
    {
        private Dictionary<string, string> _service = new();
        public void ConfigureService()
        {
            Console.WriteLine("Enter the service name you'd like to setup: ");
            var serviceName = Console.ReadLine();
            Console.WriteLine("Enter the display name you'd like: ");
            var displayName = Console.ReadLine();
            Console.WriteLine("Enter the service description you'd like:: ");
            var description = Console.ReadLine();
            Console.WriteLine("Enter the command you'd like to execute: ");
            var command = Console.ReadLine();
            Console.WriteLine("Working directory for the command: ");
            var direcrory = Console.ReadLine();

            _service.Add("serviceName", serviceName);
            _service.Add("displayName", displayName);
            _service.Add("description", description);
            _service.Add("command", command);
            _service.Add("directory", direcrory);

            SaveJSONFile(_service);
        }

        private void SaveJSONFile(Dictionary<string, string> directory)
        {
            var serializeDict = JsonSerializer.Serialize(directory);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), serializeDict.ToString());
        }
    }
}
