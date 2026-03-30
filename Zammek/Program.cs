using Prometheus;
using Zammek.Jobs;
using Zammek.Metrics;
using Zammek.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services
    .AddSingleton<IMetricFactory>(Metrics.DefaultFactory)
    .AddSingleton<MetricsSet>();

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