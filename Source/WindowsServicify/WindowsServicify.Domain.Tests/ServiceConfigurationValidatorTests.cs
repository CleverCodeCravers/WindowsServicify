using NUnit.Framework;

namespace WindowsServicify.Domain.Tests;

[TestFixture]
public class ServiceConfigurationValidatorTests
{
    private static ServiceConfiguration CreateValid()
    {
        return new ServiceConfiguration(
            ServiceName: "MyService",
            DisplayName: "My Service",
            Description: "A test service",
            Command: "C:\\Program Files\\myapp\\app.exe",
            WorkingDirectory: "C:\\Program Files\\myapp",
            Arguments: "--run"
        );
    }

    // --- Valid configurations ---

    [Test]
    public void Validate_WithValidConfiguration_ReturnsSuccess()
    {
        var config = CreateValid();

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Validate_WithValidConfiguration_ReturnsOriginalConfiguration()
    {
        var config = CreateValid();

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.Value, Is.EqualTo(config));
    }

    [Test]
    public void Validate_WithServiceNameContainingHyphensUnderscoresDotsSpaces_ReturnsSuccess()
    {
        var config = CreateValid() with { ServiceName = "My_Service-Name 2.0" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Validate_WithEmptyDescription_ReturnsSuccess()
    {
        var config = CreateValid() with { Description = "" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Validate_WithNullDescription_ReturnsSuccess()
    {
        var config = CreateValid() with { Description = null! };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.True);
    }

    // --- ServiceName injection attempts ---

    [Test]
    public void Validate_WithServiceNameContainingSemicolon_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "MyService; rm -rf /" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingPipe_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc|net user hacker pass /add" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingAmpersand_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc&calc" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingDollarParenthesis_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc$(whoami)" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingBacktick_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc`calc`" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingDoubleQuote_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc\"injected" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingSingleQuote_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc'injected" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithEmptyServiceName_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
        Assert.That(result.ErrorMessage, Does.Contain("must not be empty"));
    }

    [Test]
    public void Validate_WithNullServiceName_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = null! };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
    }

    // --- DisplayName injection attempts ---

    [Test]
    public void Validate_WithDisplayNameContainingPowerShellInjection_ReturnsFailure()
    {
        var config = CreateValid() with { DisplayName = "Test$(calc.exe)" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("DisplayName"));
    }

    [Test]
    public void Validate_WithDisplayNameContainingSemicolon_ReturnsFailure()
    {
        var config = CreateValid() with { DisplayName = "Display; whoami" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("DisplayName"));
    }

    [Test]
    public void Validate_WithEmptyDisplayName_ReturnsFailure()
    {
        var config = CreateValid() with { DisplayName = "" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("DisplayName"));
        Assert.That(result.ErrorMessage, Does.Contain("must not be empty"));
    }

    // --- Description injection attempts ---

    [Test]
    public void Validate_WithDescriptionContainingBacktickInjection_ReturnsFailure()
    {
        var config = CreateValid() with { Description = "Desc`whoami`" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Description"));
    }

    [Test]
    public void Validate_WithDescriptionContainingPowerShellSubexpression_ReturnsFailure()
    {
        var config = CreateValid() with { Description = "Desc$(evil)" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Description"));
    }

    // --- Command path traversal ---

    [Test]
    public void Validate_WithCommandContainingPathTraversal_ReturnsFailure()
    {
        var config = CreateValid() with { Command = @"..\..\..\..\Windows\System32\cmd.exe" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Command"));
        Assert.That(result.ErrorMessage, Does.Contain("path traversal"));
    }

    [Test]
    public void Validate_WithCommandContainingUnixPathTraversal_ReturnsFailure()
    {
        var config = CreateValid() with { Command = "../../../../etc/passwd" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Command"));
    }

    [Test]
    public void Validate_WithEmptyCommand_ReturnsFailure()
    {
        var config = CreateValid() with { Command = "" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Command"));
        Assert.That(result.ErrorMessage, Does.Contain("must not be empty"));
    }

    // --- Multiple validation errors ---

    [Test]
    public void Validate_WithMultipleInvalidFields_ReportsAllErrors()
    {
        var config = CreateValid() with
        {
            ServiceName = "Svc;bad",
            Description = "Desc$(evil)"
        };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
        Assert.That(result.ErrorMessage, Does.Contain("Description"));
    }

    [Test]
    public void Validate_WithAllFieldsInvalid_ReportsAllErrors()
    {
        var config = new ServiceConfiguration(
            ServiceName: "",
            DisplayName: "$(evil)",
            Description: "`inject`",
            Command: "",
            WorkingDirectory: "",
            Arguments: ""
        );

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
        // At least ServiceName (empty), DisplayName (invalid chars), Command (empty)
        Assert.That(result.ErrorMessage, Does.Contain("ServiceName"));
        Assert.That(result.ErrorMessage, Does.Contain("DisplayName"));
        Assert.That(result.ErrorMessage, Does.Contain("Command"));
    }

    // --- Edge cases for allowed characters ---

    [Test]
    public void Validate_WithServiceNameOnlyDigits_ReturnsSuccess()
    {
        var config = CreateValid() with { ServiceName = "12345" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Validate_WithServiceNameOnlyLetters_ReturnsSuccess()
    {
        var config = CreateValid() with { ServiceName = "MyService" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Validate_WithServiceNameContainingNewline_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc\ninjected" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Validate_WithServiceNameContainingTab_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc\tinjected" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Validate_WithServiceNameContainingRedirectOperator_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc>output.txt" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Validate_WithServiceNameContainingSlash_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = "Svc/injected" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Validate_WithServiceNameContainingBackslash_ReturnsFailure()
    {
        var config = CreateValid() with { ServiceName = @"Svc\injected" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public void Validate_WithValidCommandAbsolutePath_ReturnsSuccess()
    {
        var config = CreateValid() with { Command = @"C:\Program Files\app\service.exe" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void Validate_WithCommandContainingSingleDot_ReturnsSuccess()
    {
        var config = CreateValid() with { Command = @"C:\folder\app.exe" };

        var result = ServiceConfigurationValidator.Validate(config);

        Assert.That(result.IsSuccess, Is.True);
    }
}
