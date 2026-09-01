namespace AuditService.Services
{
    public class AuditRetentionWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AuditRetentionWorker(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                await service.CleanupExpiredAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
