using System.Text.Json;

namespace WindowsServicify.Domain;

public class ServiceConfigurationFileHandler
{
    public static ServiceConfiguration Load(string filePath)
    {
        var configFile = File.ReadAllText(filePath);
        var configuration = JsonSerializer.Deserialize<ServiceConfiguration>(configFile)!;

        ServiceConfigurationValidator.Validate(configuration);

        return configuration;
    }

    public static void Save(string filePath, ServiceConfiguration serviceConfiguration)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        var json = JsonSerializer.Serialize(serviceConfiguration, options);
        File.WriteAllText(filePath, json);
    }
}