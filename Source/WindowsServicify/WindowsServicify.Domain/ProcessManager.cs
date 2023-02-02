using System.Diagnostics;

namespace WindowsServicify.Domain;

public class ProcessManager
{
    private readonly string _command;
    private readonly string _workingDirectory;
    private Process? _process;
    private readonly string _arguments;

    public ProcessManager(string command, string workingDirectory, string arguments)
    {
        _command = command;
        _workingDirectory = workingDirectory;
        _arguments = arguments;
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
        process = new Process();
        process.StartInfo.FileName = _command;
        process.StartInfo.Arguments = _arguments;
        process.StartInfo.WorkingDirectory = _workingDirectory;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.BeginOutputReadLine();
        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                ProcessLogger.Log(args.Data, _workingDirectory);
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