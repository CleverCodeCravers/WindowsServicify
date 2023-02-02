using System.Diagnostics;

namespace WindowsServicify.Domain;

public class ProcessManager
{
    private readonly string _command;
    private readonly string _workingDirectory;
    private Process? _process;
    private readonly string _arguments;
    private readonly ProcessLogger _processLogger;

    public ProcessManager(
        string command,
        string workingDirectory,
        string arguments,
        ProcessLogger processLogger)
    {
        _command = command;
        _workingDirectory = workingDirectory;
        _arguments = arguments;
        _processLogger = processLogger;
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
        _process.Start();
        _process.BeginOutputReadLine();
        _process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                _processLogger.Log(args.Data);
            }
        };
    }
    
    private void StopProcess()
    {
        if (_process != null && !_process.HasExited)
        {
            _process.Kill();
        }
    }
}