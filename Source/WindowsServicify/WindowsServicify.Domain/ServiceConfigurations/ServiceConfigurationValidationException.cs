namespace WindowsServicify.Domain;

public class ServiceConfigurationValidationException : Exception
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public ServiceConfigurationValidationException(IReadOnlyList<string> validationErrors)
        : base(FormatMessage(validationErrors))
    {
        ValidationErrors = validationErrors;
    }

    private static string FormatMessage(IReadOnlyList<string> errors)
    {
        return "Service configuration is invalid:\n" + string.Join("\n", errors);
    }
}
