using Zammek.Metrics;

namespace Zammek.Jobs;

public partial class LoanExporterJob(IMetricsSet metricsSet, ILogger<LoanExporterJob> logger) : BackgroundService
{
    private const int MinLoan = 20;
    private const int MaxLoan = 32;
    private int _currentValue = 26;
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var change = _random.Next(-3, 4);
            _currentValue += change;

            _currentValue = Math.Clamp(_currentValue, MinLoan, MaxLoan);

            metricsSet.SetLoanPercent(_currentValue);
            LogLoanChange(logger, _currentValue, change);

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    [LoggerMessage(LogLevel.Information, "Loan percent set to: {CurrentLoan} (change: {Change})")]
    static partial void LogLoanChange(ILogger<LoanExporterJob> logger, int currentLoan, int change);
}