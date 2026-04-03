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
    public void Validate_WithValidConfiguration_DoesNotThrow()
    {
        var config = CreateValid();

        Assert.DoesNotThrow(() => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithServiceNameContainingHyphensUnderscoresDotsSpaces_DoesNotThrow()
    {
        var config = CreateValid() with { ServiceName = "My_Service-Name 2.0" };

        Assert.DoesNotThrow(() => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithEmptyDescription_DoesNotThrow()
    {
        var config = CreateValid() with { Description = "" };

        Assert.DoesNotThrow(() => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithNullDescription_DoesNotThrow()
    {
        var config = CreateValid() with { Description = null! };

        Assert.DoesNotThrow(() => ServiceConfigurationValidator.Validate(config));
    }

    // --- ServiceName injection attempts ---

    [Test]
    public void Validate_WithServiceNameContainingSemicolon_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "MyService; rm -rf /" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingPipe_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc|net user hacker pass /add" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingAmpersand_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc&calc" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingDollarParenthesis_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc$(whoami)" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingBacktick_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc`calc`" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingDoubleQuote_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc\"injected" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithServiceNameContainingSingleQuote_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc'injected" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
    }

    [Test]
    public void Validate_WithEmptyServiceName_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
        Assert.That(ex.Message, Does.Contain("must not be empty"));
    }

    [Test]
    public void Validate_WithNullServiceName_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = null! };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("ServiceName"));
    }

    // --- DisplayName injection attempts ---

    [Test]
    public void Validate_WithDisplayNameContainingPowerShellInjection_ThrowsValidationException()
    {
        var config = CreateValid() with { DisplayName = "Test$(calc.exe)" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("DisplayName"));
    }

    [Test]
    public void Validate_WithDisplayNameContainingSemicolon_ThrowsValidationException()
    {
        var config = CreateValid() with { DisplayName = "Display; whoami" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("DisplayName"));
    }

    [Test]
    public void Validate_WithEmptyDisplayName_ThrowsValidationException()
    {
        var config = CreateValid() with { DisplayName = "" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("DisplayName"));
        Assert.That(ex.Message, Does.Contain("must not be empty"));
    }

    // --- Description injection attempts ---

    [Test]
    public void Validate_WithDescriptionContainingBacktickInjection_ThrowsValidationException()
    {
        var config = CreateValid() with { Description = "Desc`whoami`" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("Description"));
    }

    [Test]
    public void Validate_WithDescriptionContainingPowerShellSubexpression_ThrowsValidationException()
    {
        var config = CreateValid() with { Description = "Desc$(evil)" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("Description"));
    }

    // --- Command path traversal ---

    [Test]
    public void Validate_WithCommandContainingPathTraversal_ThrowsValidationException()
    {
        var config = CreateValid() with { Command = @"..\..\..\..\Windows\System32\cmd.exe" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("Command"));
        Assert.That(ex.Message, Does.Contain("path traversal"));
    }

    [Test]
    public void Validate_WithCommandContainingUnixPathTraversal_ThrowsValidationException()
    {
        var config = CreateValid() with { Command = "../../../../etc/passwd" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("Command"));
    }

    [Test]
    public void Validate_WithEmptyCommand_ThrowsValidationException()
    {
        var config = CreateValid() with { Command = "" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.Message, Does.Contain("Command"));
        Assert.That(ex.Message, Does.Contain("must not be empty"));
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

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.ValidationErrors, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(ex.Message, Does.Contain("ServiceName"));
        Assert.That(ex.Message, Does.Contain("Description"));
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

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.ValidationErrors, Has.Count.GreaterThanOrEqualTo(3));
    }

    // --- Exception properties ---

    [Test]
    public void ValidationException_HasReadableErrorList()
    {
        var config = CreateValid() with { ServiceName = "Svc;bad" };

        var ex = Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));

        Assert.That(ex!.ValidationErrors, Is.Not.Empty);
        Assert.That(ex.ValidationErrors[0], Does.Contain("ServiceName"));
    }

    // --- Edge cases for allowed characters ---

    [Test]
    public void Validate_WithServiceNameOnlyDigits_DoesNotThrow()
    {
        var config = CreateValid() with { ServiceName = "12345" };

        Assert.DoesNotThrow(() => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithServiceNameOnlyLetters_DoesNotThrow()
    {
        var config = CreateValid() with { ServiceName = "MyService" };

        Assert.DoesNotThrow(() => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithServiceNameContainingNewline_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc\ninjected" };

        Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithServiceNameContainingTab_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc\tinjected" };

        Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithServiceNameContainingRedirectOperator_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc>output.txt" };

        Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithServiceNameContainingSlash_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = "Svc/injected" };

        Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithServiceNameContainingBackslash_ThrowsValidationException()
    {
        var config = CreateValid() with { ServiceName = @"Svc\injected" };

        Assert.Throws<ServiceConfigurationValidationException>(
            () => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithValidCommandAbsolutePath_DoesNotThrow()
    {
        var config = CreateValid() with { Command = @"C:\Program Files\app\service.exe" };

        Assert.DoesNotThrow(() => ServiceConfigurationValidator.Validate(config));
    }

    [Test]
    public void Validate_WithCommandContainingSingleDot_DoesNotThrow()
    {
        var config = CreateValid() with { Command = @"C:\folder\app.exe" };

        Assert.DoesNotThrow(() => ServiceConfigurationValidator.Validate(config));
    }
}
