using BLL.Interfaces;

namespace NewsArchive.Publishing
{
    public class ScheduledPublisher : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<ScheduledPublisher> logger;

        public ScheduledPublisher(IServiceScopeFactory scopeFactory, ILogger<ScheduledPublisher> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var published = scope.ServiceProvider.GetRequiredService<IArticleService>().PublishDue();
                    if (published > 0)
                        logger.LogInformation("Published {Count} scheduled article(s)", published);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Publishing scheduled articles failed");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
