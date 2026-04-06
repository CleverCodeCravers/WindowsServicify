using System.Text.RegularExpressions;

namespace WindowsServicify.Domain;

public static partial class ServiceConfigurationValidator
{
    private static readonly Regex SafeNamePattern = CreateSafeNameRegex();

    /// <summary>
    /// Validates all fields of a ServiceConfiguration.
    /// Returns a Result indicating success or failure with error details.
    /// </summary>
    public static Result<ServiceConfiguration> Validate(ServiceConfiguration configuration)
    {
        var errors = new List<string>();

        ValidateRequiredSafeName(errors, nameof(configuration.ServiceName), configuration.ServiceName);
        ValidateRequiredSafeName(errors, nameof(configuration.DisplayName), configuration.DisplayName);
        ValidateOptionalSafeName(errors, nameof(configuration.Description), configuration.Description);
        ValidateCommand(errors, nameof(configuration.Command), configuration.Command);

        if (errors.Count > 0)
        {
            var message = "Service configuration is invalid:\n" + string.Join("\n", errors);
            return Result<ServiceConfiguration>.Failure(message);
        }

        return Result<ServiceConfiguration>.Success(configuration);
    }

    private static void ValidateRequiredSafeName(List<string> errors, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} must not be empty.");
            return;
        }

        if (!SafeNamePattern.IsMatch(value))
        {
            errors.Add($"{fieldName} contains invalid characters. Only letters, digits, spaces, underscores, hyphens, and dots are allowed. Value: '{value}'");
        }
    }

    private static void ValidateOptionalSafeName(List<string> errors, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!SafeNamePattern.IsMatch(value))
        {
            errors.Add($"{fieldName} contains invalid characters. Only letters, digits, spaces, underscores, hyphens, and dots are allowed. Value: '{value}'");
        }
    }

    private static void ValidateCommand(List<string> errors, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} must not be empty.");
            return;
        }

        if (ContainsPathTraversal(value))
        {
            errors.Add($"{fieldName} contains path traversal sequences ('..'), which are not allowed. Value: '{value}'");
        }
    }

    private static bool ContainsPathTraversal(string value)
    {
        return value.Contains("..");
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_\-\. ]+$")]
    private static partial Regex CreateSafeNameRegex();
}
