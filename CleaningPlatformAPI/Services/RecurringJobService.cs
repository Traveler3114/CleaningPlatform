using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Services;

public class RecurringJobService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RecurringJobService> _logger;
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public RecurringJobService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<RecurringJobService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = RunLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
            await _cts.CancelAsync();

        if (_loopTask is not null)
            await _loopTask.WaitAsync(cancellationToken);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var runHour = _configuration.GetValue<int>("RecurringJob:RunHour", 6);
                var nextRun = now.Date.AddHours(runHour);

                if (nextRun <= now)
                {
                    _logger.LogInformation("RecurringJobService starting catch-up run (restarted after RunHour={RunHour}).", runHour);
                    using var catchupScope = _scopeFactory.CreateScope();
                    var catchupManager = catchupScope.ServiceProvider.GetRequiredService<RecurringScheduleManager>();
                    await catchupManager.RunAutoGenerateAsync(ct);
                    nextRun = now.Date.AddDays(1).AddHours(runHour);
                }

                var delay = nextRun - now;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, ct);

                if (ct.IsCancellationRequested) break;

                using var scope = _scopeFactory.CreateScope();
                var manager = scope.ServiceProvider.GetRequiredService<RecurringScheduleManager>();
                var results = await manager.RunAutoGenerateAsync(ct);

                _logger.LogInformation("RecurringJobService completed. Generated {Count} schedule result(s).", results.Count);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RecurringJobService encountered an error.");
            }
        }
    }
}
