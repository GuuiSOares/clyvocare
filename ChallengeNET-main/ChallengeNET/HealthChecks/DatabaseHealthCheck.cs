using ClyvoCare.API.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClyvoCare.API.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DatabaseHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                return HealthCheckResult.Healthy("Conexao com o Oracle estabelecida com sucesso.");
            }

            return HealthCheckResult.Unhealthy("Nao foi possivel conectar ao Oracle.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao validar conexao com o Oracle.", ex);
        }
    }
}
