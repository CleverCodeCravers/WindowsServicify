using System.Text.Json;
using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class ServiceConfigurationFileHandlerTests
{
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "ServiceConfigTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private static ServiceConfiguration CreateValidConfiguration()
    {
        return new ServiceConfiguration(
            ServiceName: "TestService",
            DisplayName: "Test Service",
            Description: "A test service for unit tests",
            Command: "C:\\Program Files\\TestApp\\test.exe",
            WorkingDirectory: "C:\\Program Files\\TestApp",
            Arguments: "--run --verbose"
        );
    }

    private string GetTempFilePath(string fileName = "config.json")
    {
        return Path.Combine(_tempDirectory, fileName);
    }

    // --- Round-Trip tests ---

    [Test]
    public void SaveAndLoad_WithValidConfiguration_PreservesAllFields()
    {
        var original = CreateValidConfiguration();
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, original);
        var loaded = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(loaded.ServiceName, Is.EqualTo(original.ServiceName));
        Assert.That(loaded.DisplayName, Is.EqualTo(original.DisplayName));
        Assert.That(loaded.Description, Is.EqualTo(original.Description));
        Assert.That(loaded.Command, Is.EqualTo(original.Command));
        Assert.That(loaded.WorkingDirectory, Is.EqualTo(original.WorkingDirectory));
        Assert.That(loaded.Arguments, Is.EqualTo(original.Arguments));
    }

    [Test]
    public void SaveAndLoad_WithValidConfiguration_ProducesEqualRecord()
    {
        var original = CreateValidConfiguration();
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, original);
        var loaded = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(loaded, Is.EqualTo(original));
    }

    [Test]
    public void SaveAndLoad_WithEmptyOptionalFields_PreservesEmptyStrings()
    {
        var original = new ServiceConfiguration(
            ServiceName: "MinimalService",
            DisplayName: "Minimal Service",
            Description: "",
            Command: "C:\\app.exe",
            WorkingDirectory: "",
            Arguments: ""
        );
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, original);
        var loaded = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(loaded.Description, Is.EqualTo(""));
        Assert.That(loaded.WorkingDirectory, Is.EqualTo(""));
        Assert.That(loaded.Arguments, Is.EqualTo(""));
    }

    [Test]
    public void SaveAndLoad_WithSpecialCharactersInCommand_PreservesPath()
    {
        var original = CreateValidConfiguration() with
        {
            Command = "C:\\Program Files (x86)\\My App\\service.exe"
        };
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, original);
        var loaded = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(loaded.Command, Is.EqualTo(original.Command));
    }

    [Test]
    public void SaveAndLoad_WithUnicodeInDescription_PreservesUnicode()
    {
        var original = CreateValidConfiguration() with
        {
            Description = "Ein Testdienst mit Umlauten"
        };
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, original);
        var loaded = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(loaded.Description, Is.EqualTo(original.Description));
    }

    // --- Save tests ---

    [Test]
    public void Save_CreatesFileOnDisk()
    {
        var config = CreateValidConfiguration();
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, config);

        Assert.That(File.Exists(filePath), Is.True);
    }

    [Test]
    public void Save_WritesValidJson()
    {
        var config = CreateValidConfiguration();
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, config);

        var json = File.ReadAllText(filePath);
        Assert.DoesNotThrow(() => JsonDocument.Parse(json));
    }

    [Test]
    public void Save_WritesIndentedJson()
    {
        var config = CreateValidConfiguration();
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, config);

        var json = File.ReadAllText(filePath);
        Assert.That(json, Does.Contain(Environment.NewLine));
    }

    [Test]
    public void Save_OverwritesExistingFile()
    {
        var filePath = GetTempFilePath();
        var first = CreateValidConfiguration();
        var second = CreateValidConfiguration() with { ServiceName = "UpdatedService" };

        ServiceConfigurationFileHandler.Save(filePath, first);
        ServiceConfigurationFileHandler.Save(filePath, second);
        var loaded = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(loaded.ServiceName, Is.EqualTo("UpdatedService"));
    }

    // --- Load error cases ---

    [Test]
    public void Load_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var filePath = GetTempFilePath("nonexistent.json");

        Assert.Throws<FileNotFoundException>(() => ServiceConfigurationFileHandler.Load(filePath));
    }

    [Test]
    public void Load_WithInvalidJson_ThrowsJsonException()
    {
        var filePath = GetTempFilePath();
        File.WriteAllText(filePath, "{ this is not valid json }");

        Assert.Throws<JsonException>(() => ServiceConfigurationFileHandler.Load(filePath));
    }

    [Test]
    public void Load_WithEmptyFile_ThrowsJsonException()
    {
        var filePath = GetTempFilePath();
        File.WriteAllText(filePath, "");

        Assert.Throws<JsonException>(() => ServiceConfigurationFileHandler.Load(filePath));
    }

    [Test]
    public void Load_WithInvalidServiceName_ThrowsValidationException()
    {
        var filePath = GetTempFilePath();
        var invalidConfig = new
        {
            ServiceName = "Bad;Name",
            DisplayName = "Valid Display",
            Description = "Valid description",
            Command = "C:\\app.exe",
            WorkingDirectory = "C:\\",
            Arguments = ""
        };
        var json = JsonSerializer.Serialize(invalidConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);

        Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationFileHandler.Load(filePath));
    }

    [Test]
    public void Load_WithPathTraversalInCommand_ThrowsValidationException()
    {
        var filePath = GetTempFilePath();
        var invalidConfig = new
        {
            ServiceName = "TestService",
            DisplayName = "Test Service",
            Description = "A test",
            Command = @"..\..\..\..\Windows\System32\cmd.exe",
            WorkingDirectory = "C:\\",
            Arguments = ""
        };
        var json = JsonSerializer.Serialize(invalidConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);

        Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationFileHandler.Load(filePath));
    }

    [Test]
    public void Load_WithEmptyServiceName_ThrowsValidationException()
    {
        var filePath = GetTempFilePath();
        var invalidConfig = new
        {
            ServiceName = "",
            DisplayName = "Valid Display",
            Description = "Valid description",
            Command = "C:\\app.exe",
            WorkingDirectory = "C:\\",
            Arguments = ""
        };
        var json = JsonSerializer.Serialize(invalidConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);

        Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationFileHandler.Load(filePath));
    }
}
