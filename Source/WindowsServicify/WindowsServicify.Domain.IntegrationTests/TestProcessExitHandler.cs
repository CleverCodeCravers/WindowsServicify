namespace WindowsServicify.Domain.IntegrationTests;

/// <summary>
/// Test double for IProcessExitHandler that records exit calls
/// instead of terminating the process.
/// </summary>
internal class TestProcessExitHandler : IProcessExitHandler
{
    public int? LastExitCode { get; private set; }
    public bool ExitWasCalled => LastExitCode.HasValue;

    public void Exit(int exitCode)
    {
        LastExitCode = exitCode;
    }
}