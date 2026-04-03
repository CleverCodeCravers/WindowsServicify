using System.Diagnostics;

namespace WindowsServicify.Domain;

public class ProcessManager
{
    private readonly string _command;
    private readonly string _workingDirectory;
    private Process? _process;
    private readonly string _arguments;
    private readonly ProcessLogger _processLogger;
    private readonly int _shutdownTimeoutMs;

    public const int DefaultShutdownTimeoutMs = 10_000;

    public ProcessManager(
        string command,
        string workingDirectory,
        string arguments,
        ProcessLogger processLogger,
        int shutdownTimeoutMs = DefaultShutdownTimeoutMs)
    {
        _command = command;
        _workingDirectory = workingDirectory;
        _arguments = arguments;
        _processLogger = processLogger;
        _shutdownTimeoutMs = shutdownTimeoutMs;
    }

    public void Start()
    {
        StartProcess();
    }

    public void Stop()
    {
        StopProcess();
    }

    public bool IsCorrectlyRunning()
    {
        if (_process == null)
            return false;

        return !_process.HasExited;
    }

    private void StartProcess()
    {
        _process = new Process();
        _process.StartInfo.FileName = _command;
        _process.StartInfo.Arguments = _arguments;
        _process.StartInfo.WorkingDirectory = _workingDirectory;
        _process.StartInfo.RedirectStandardOutput = true;
        _process.StartInfo.RedirectStandardError = true;

        _process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                _processLogger.Log(args.Data);
            }
        };

        _process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                _processLogger.Log("[ERROR] " + args.Data);
            }
        };

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    private void StopProcess()
    {
        if (_process == null || _process.HasExited)
            return;

        _processLogger.Log("Sending shutdown signal...");

        _process.CloseMainWindow();

        if (_process.WaitForExit(_shutdownTimeoutMs))
        {
            _processLogger.Log("Process exited gracefully.");
            return;
        }

        _processLogger.Log("Force-killing after timeout");
        _process.Kill(true);
    }
}
