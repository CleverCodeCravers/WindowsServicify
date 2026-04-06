namespace WindowsServicify.Domain;

/// <summary>
/// Abstraction for application-level exit operations.
/// Allows testing code that would otherwise call Environment.Exit directly.
/// </summary>
public interface IProcessExitHandler
{
    void Exit(int exitCode);
}

/// <summary>
/// Default implementation that delegates to Environment.Exit.
/// Used in production.
/// </summary>
public class DefaultProcessExitHandler : IProcessExitHandler
{
    public void Exit(int exitCode)
    {
        Environment.Exit(exitCode);
    }
}
