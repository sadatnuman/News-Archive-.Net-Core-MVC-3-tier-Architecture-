using BLL;

namespace NewsArchive.Publishing
{
    public partial class MaintenanceJob : BackgroundService
    {
        public static readonly TimeSpan Interval = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<MaintenanceJob> logger;

        public MaintenanceJob(IServiceScopeFactory scopeFactory, ILogger<MaintenanceJob> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
                RunOnce();
        }

        public void RunOnce()
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                scope.ServiceProvider.PurgeExpiredData();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogFailed(ex);
            }
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Removing expired tokens, old audit entries and stale sign-ups failed")]
        private partial void LogFailed(Exception exception);
    }
}
