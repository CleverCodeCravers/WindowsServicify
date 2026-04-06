using System.Text.Json;

namespace WindowsServicify.Domain;

public class ServiceConfigurationFileHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions DeserializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static Result<ServiceConfiguration> Load(string filePath)
    {
        string configFile;
        try
        {
            configFile = File.ReadAllText(filePath);
        }
        catch (FileNotFoundException)
        {
            return Result<ServiceConfiguration>.Failure($"Configuration file not found: {filePath}");
        }
        catch (IOException ex)
        {
            return Result<ServiceConfiguration>.Failure($"Could not read configuration file: {ex.Message}");
        }

        ServiceConfiguration? configuration;
        try
        {
            configuration = JsonSerializer.Deserialize<ServiceConfiguration>(configFile, DeserializerOptions);
        }
        catch (JsonException ex)
        {
            return Result<ServiceConfiguration>.Failure($"Configuration file contains invalid JSON: {ex.Message}");
        }

        if (configuration is null)
        {
            return Result<ServiceConfiguration>.Failure("Configuration file deserialized to null.");
        }

        return ServiceConfigurationValidator.Validate(configuration);
    }

    public static void Save(string filePath, ServiceConfiguration serviceConfiguration)
    {
        var json = JsonSerializer.Serialize(serviceConfiguration, SerializerOptions);
        File.WriteAllText(filePath, json);
    }
}