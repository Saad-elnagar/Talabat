using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Talabat.HealthChecks
{
    // Simply tries to open a SQL connection — no external health-check package needed,
    // which sidesteps version-to-version API changes in AspNetCore.HealthChecks.SqlServer.
    public class SqlConnectionHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public SqlConnectionHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Could not open SQL Server connection.", ex);
            }
        }
    }
}
