namespace TimesheetService.Services
{
    public class TimesheetAutoApproveWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TimesheetAutoApproveWorker> _logger;

        public TimesheetAutoApproveWorker(IServiceScopeFactory serviceScopeFactory, ILogger<TimesheetAutoApproveWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunOnceAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunOnceAsync(stoppingToken);
            }
        }

        private async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ITimesheetEntryService>();
                var count = await service.AutoApproveExpiredSubmittedAsync(cancellationToken);

                if (count > 0)
                {
                    _logger.LogInformation("Auto-approved {Count} timesheet summary items.", count);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Timesheet auto-approve worker failed.");
            }
        }
    }
}
