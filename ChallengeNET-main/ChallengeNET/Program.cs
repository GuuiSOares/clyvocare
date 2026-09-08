using ClyvoCare.API.Data;
using ClyvoCare.API.Diagnostics;
using ClyvoCare.API.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Host.UseSerilog();
}

builder.Services.AddControllers();

if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection nao configurada. Use a variavel de ambiente ConnectionStrings__DefaultConnection.");
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseOracle(connectionString));
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ClyvoCare API",
        Version = "v1",
        Description = "Monitoramento preditivo de saude animal - Sprint 3 DevOps (ACR + ACI)"
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "liveness" })
    .AddCheck<DatabaseHealthCheck>("oracle_db", tags: new[] { "readiness" });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(TelemetryConfig.ServiceName))
    .WithTracing(tracing => tracing
        .AddSource(TelemetryConfig.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddMeter(TelemetryConfig.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClyvoCare API v1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseSerilogRequestLogging();
}
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("liveness")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("readiness")
});

app.MapPrometheusScrapingEndpoint();
app.MapControllers();

app.Run();

public partial class Program { }
