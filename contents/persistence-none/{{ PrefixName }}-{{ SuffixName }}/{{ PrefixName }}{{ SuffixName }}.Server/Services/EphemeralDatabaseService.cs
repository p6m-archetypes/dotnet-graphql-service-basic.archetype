using Microsoft.Extensions.Logging;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Services;

public class EphemeralDatabaseService
{
    private readonly ILogger<EphemeralDatabaseService> _logger;
    public bool IsRunning { get; private set; }

    public EphemeralDatabaseService(ILogger<EphemeralDatabaseService> logger)
    {
        _logger = logger;
    }

    public Task StartDatabaseAsync()
    {
        _logger.LogInformation("Starting ephemeral database (in-memory mode)");
        IsRunning = true;
        return Task.CompletedTask;
    }

    public Task StopDatabaseAsync()
    {
        _logger.LogInformation("Stopping ephemeral database");
        IsRunning = false;
        return Task.CompletedTask;
    }
}
