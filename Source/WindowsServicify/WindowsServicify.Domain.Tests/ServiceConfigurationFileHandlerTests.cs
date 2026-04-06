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
        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ServiceName, Is.EqualTo(original.ServiceName));
        Assert.That(result.Value.DisplayName, Is.EqualTo(original.DisplayName));
        Assert.That(result.Value.Description, Is.EqualTo(original.Description));
        Assert.That(result.Value.Command, Is.EqualTo(original.Command));
        Assert.That(result.Value.WorkingDirectory, Is.EqualTo(original.WorkingDirectory));
        Assert.That(result.Value.Arguments, Is.EqualTo(original.Arguments));
    }

    [Test]
    public void SaveAndLoad_WithValidConfiguration_ProducesEqualRecord()
    {
        var original = CreateValidConfiguration();
        var filePath = GetTempFilePath();

        ServiceConfigurationFileHandler.Save(filePath, original);
        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(original));
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
        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Description, Is.EqualTo(""));
        Assert.That(result.Value.WorkingDirectory, Is.EqualTo(""));
        Assert.That(result.Value.Arguments, Is.EqualTo(""));
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
        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Command, Is.EqualTo(original.Command));
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
        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Description, Is.EqualTo(original.Description));
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
        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ServiceName, Is.EqualTo("UpdatedService"));
    }

    // --- Load error cases ---

    [Test]
    public void Load_WithNonExistentFile_ReturnsFailure()
    {
        var filePath = GetTempFilePath("nonexistent.json");

        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("not found"));
    }

    [Test]
    public void Load_WithInvalidJson_ReturnsFailure()
    {
        var filePath = GetTempFilePath();
        File.WriteAllText(filePath, "{ this is not valid json }");

        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("invalid JSON"));
    }

    [Test]
    public void Load_WithEmptyFile_ReturnsFailure()
    {
        var filePath = GetTempFilePath();
        File.WriteAllText(filePath, "");

        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Load_WithInvalidServiceName_ReturnsFailure()
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

        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    [Test]
    public void Load_WithPathTraversalInCommand_ReturnsFailure()
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

        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("path traversal"));
    }

    [Test]
    public void Load_WithEmptyServiceName_ReturnsFailure()
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

        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    // --- New tests for Result pattern ---

    [Test]
    public void Load_WithNullJsonContent_ReturnsFailure()
    {
        var filePath = GetTempFilePath();
        File.WriteAllText(filePath, "null");

        var result = ServiceConfigurationFileHandler.Load(filePath);

        Assert.That(result.IsSuccess, Is.False);
    }
}