namespace WindowsServicify.Domain.IntegrationTests;

/// <summary>
/// Creates a unique temporary directory that is automatically cleaned up on disposal.
/// Used by integration tests to provide isolated file system environments.
/// </summary>
internal sealed class TempDirectory : IDisposable
{
    public string Path { get; }

    public TempDirectory(string prefix = "IntTest")
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"{prefix}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        // Retry cleanup because processes may still hold file handles briefly after stopping
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                if (Directory.Exists(Path))
                    Directory.Delete(Path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 4)
            {
                Thread.Sleep(300);
            }
            catch (UnauthorizedAccessException) when (attempt < 4)
            {
                Thread.Sleep(300);
            }
        }
    }
}