using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Talabat.HealthChecks
{
    // Pings Redis through the IConnectionMultiplexer that's already registered as a
    // singleton in Program.cs — no extra package or duplicate connection needed.
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var latency = await _connectionMultiplexer.GetDatabase().PingAsync();
                return HealthCheckResult.Healthy($"Redis responded in {latency.TotalMilliseconds}ms.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Could not reach Redis.", ex);
            }
        }
    }
}
