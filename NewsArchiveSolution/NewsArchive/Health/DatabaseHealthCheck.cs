using BLL.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NewsArchive.Health
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly SystemService systemService;

        public DatabaseHealthCheck(SystemService systemService)
        {
            this.systemService = systemService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(systemService.IsDatabaseReachable()
                ? HealthCheckResult.Healthy("Database reachable")
                : HealthCheckResult.Unhealthy("Database unreachable"));
        }
    }
}
