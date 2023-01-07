﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WindowsServicify.Domain;

public class WindowsBackgroundService : BackgroundService
{
    private readonly ProcessManager _processManager;
    private readonly ILogger<WindowsBackgroundService> _logger;

    public WindowsBackgroundService(
        ProcessManager processManager,
        ILogger<WindowsBackgroundService> logger)
    {
        _processManager = processManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _processManager.Start();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_processManager.IsCorrectlyRunning())
                {
                    _logger.LogWarning("Restarting Process...");
                    _processManager.Start();
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _processManager.Stop();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }
}