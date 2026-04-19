using System.Diagnostics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Zammek;
using Zammek.Jobs;
using Zammek.Metrics;
using Zammek.Services;

const string serviceName = "zammek";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services
    .AddSingleton<IMetricFactory>(Metrics.DefaultFactory)
    .AddSingleton<IMetricsSet, MetricsSet>();

var lokiUrl = builder.Configuration["Loki:Url"] ?? throw new InvalidOperationException("Loki:Url is not set");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Information()
    .Enrich.WithEnvironmentName()
    .Enrich.FromLogContext()
    .Enrich.With<TraceEnricher>()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.GrafanaLoki(
        uri: lokiUrl,
        labels: new List<LokiLabel>
        {
            new() { Key = "app", Value = "aspnet-app" },
            new() { Key = "service", Value = serviceName },
            new() { Key = "env", Value = builder.Environment.EnvironmentName.ToLower() }
        },
        propertiesAsLabels: ["level", "service", "Environment"],
        credentials: null
    )
    .CreateLogger();

builder.Host.UseSerilog();


var otlpUrl = builder.Configuration["Tempo:Url"] ?? throw new InvalidOperationException("Tempo:Url is not set");
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(serviceName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: "lab4"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options => { options.Endpoint = new Uri(otlpUrl); }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());

builder.Services.AddSingleton(new ActivitySource(serviceName));

builder.Services.AddHostedService<LoanExporterJob>();

var app = builder.Build();

app.UseGrpcMetrics();
app.MapGrpcService<CreditGrpcService>();
app.MapGrpcService<DebtGrpcService>();
app.MapGrpcService<UserGrpcService>();
app.MapGrpcReflectionService();

app.MapMetrics();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();