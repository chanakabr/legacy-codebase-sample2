using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    class SQLServerHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                string _connectionString = ODBCWrapper.Connection.GetConnectionString("MAIN_CONNECTION_STRING", false, null);

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT GETDATE()";
                        var exectueResult = await command.ExecuteScalarAsync(cancellationToken);
                    }

                    return HealthCheckResult.Healthy("SQL Server is healthy");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(description: ex.Message, exception: ex);
            }
        }
    }
}
