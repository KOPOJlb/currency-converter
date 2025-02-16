using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CurrencyConverter.Infrastructure.HealthChecks
{
    public class ReadinessHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
