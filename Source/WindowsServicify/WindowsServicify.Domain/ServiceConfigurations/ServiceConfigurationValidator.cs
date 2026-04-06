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
        ValidateWorkingDirectory(errors, nameof(configuration.WorkingDirectory), configuration.WorkingDirectory);
        ValidateArguments(errors, nameof(configuration.Arguments), configuration.Arguments);

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

    private static void ValidateWorkingDirectory(List<string> errors, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // WorkingDirectory is optional — empty is allowed
            return;
        }

        if (ContainsPathTraversal(value))
        {
            errors.Add($"{fieldName} contains path traversal sequences ('..'), which are not allowed. Value: '{value}'");
        }

        if (ContainsInjectionPattern(value))
        {
            errors.Add($"{fieldName} contains potentially dangerous characters. Value: '{value}'");
        }
    }

    private static void ValidateArguments(List<string> errors, string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // Arguments is optional — empty is allowed
            return;
        }

        if (ContainsInjectionPattern(value))
        {
            errors.Add($"{fieldName} contains potentially dangerous characters. Value: '{value}'");
        }
    }

    private static bool ContainsPathTraversal(string value)
    {
        return value.Contains("..");
    }

    private static bool ContainsInjectionPattern(string value)
    {
        return value.Contains("$(") ||
               value.Contains('`') ||
               value.Contains('|') ||
               value.Contains(';');
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_\-\. ]+$")]
    private static partial Regex CreateSafeNameRegex();
}
