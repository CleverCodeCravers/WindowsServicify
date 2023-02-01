using System.Diagnostics;

namespace WindowsServicify.Domain;

public class ProcessManager
{
    private readonly string _command;
    public readonly string _workingDirectory;
    private Process process;
    private bool shouldRun;
    private string _arguments;

    public ProcessManager(string command, string workingDirectory, string arguments)
    {
        _command = command;
        _workingDirectory = workingDirectory;
        _arguments = arguments;
    }

    public void Start()
    {
        shouldRun = true;
        StartProcess();
    }

    public void Stop()
    {
        shouldRun = false;
        StopProcess();
    }

    public bool IsCorrectlyRunning()
    {
        return !process.HasExited;
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
        if (process != null && !process.HasExited)
        {
            process.Kill();
        }
    }
}