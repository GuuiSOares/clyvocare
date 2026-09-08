using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClyvoCare.API.Diagnostics;

public static class TelemetryConfig
{
    public const string ServiceName = "ClyvoCare.API";

    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);

    public static readonly Counter<long> LogsSaudeProcessadosCounter =
        Meter.CreateCounter<long>("clyvocare_logs_saude_criados_total", "Logs", "Total de logs de saude recebidos");
}