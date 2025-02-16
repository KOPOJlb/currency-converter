using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CurrencyConverter.Infrastructure.HealthChecks
{
    public class LivenessHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
