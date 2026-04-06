using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace WindowsServicify.Domain;

/// <summary>
/// Provides an HTTP health-check endpoint on a configurable port.
/// Reports the status of the monitored process as JSON.
/// Uses HttpListener to avoid heavy ASP.NET dependencies.
/// </summary>
public sealed class HealthCheckService : IDisposable
{
    private readonly int _port;
    private readonly ProcessManager _processManager;
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts;
    private readonly Stopwatch _uptimeStopwatch;
    private Task? _listenerTask;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HealthCheckService(int port, ProcessManager processManager)
    {
        _port = port;
        _processManager = processManager;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{_port}/");
        _cts = new CancellationTokenSource();
        _uptimeStopwatch = new Stopwatch();
    }

    /// <summary>
    /// Starts the HTTP listener on a background task.
    /// </summary>
    public void Start()
    {
        _listener.Start();
        _uptimeStopwatch.Start();
        _listenerTask = Task.Run(() => ListenerLoopAsync(_cts.Token));
    }

    /// <summary>
    /// Stops the HTTP listener and waits for the background task to complete.
    /// </summary>
    public void Stop()
    {
        if (_disposed)
            return;

        _cts.Cancel();
        _listener.Stop();
        _uptimeStopwatch.Stop();

        try
        {
            _listenerTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Expected when cancellation occurs during GetContextAsync
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Stop();
        _cts.Dispose();
        _listener.Close();
    }

    private async Task ListenerLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException)
            {
                // Listener was stopped
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            try
            {
                await HandleRequestAsync(context).ConfigureAwait(false);
            }
            catch
            {
                // Swallow errors from individual request handling to keep the listener alive
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            if (request.HttpMethod == "GET" && IsHealthPath(request.Url?.AbsolutePath))
            {
                var isRunning = _processManager.IsCorrectlyRunning();

                object responseBody;
                if (isRunning)
                {
                    var uptime = _uptimeStopwatch.Elapsed;
                    response.StatusCode = 200;
                    responseBody = new HealthyResponse("healthy", "running", FormatUptime(uptime));
                }
                else
                {
                    response.StatusCode = 503;
                    responseBody = new UnhealthyResponse("unhealthy", "stopped");
                }

                var json = JsonSerializer.Serialize(responseBody, JsonOptions);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);

                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false);
            }
            else
            {
                response.StatusCode = 404;
            }
        }
        finally
        {
            response.Close();
        }
    }

    private static bool IsHealthPath(string? path)
    {
        return string.Equals(path, "/health", StringComparison.OrdinalIgnoreCase);
    }

    internal static string FormatUptime(TimeSpan uptime)
    {
        return uptime.ToString(@"hh\:mm\:ss");
    }

    private record HealthyResponse(string Status, string Process, string Uptime);

    private record UnhealthyResponse(string Status, string Process);
}
