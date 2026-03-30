using Prometheus;

namespace Zammek.Metrics;

public class MetricsSet(IMetricFactory metricFactory)
{
    private readonly Histogram _balanceChange =
        metricFactory.CreateHistogram(
            "zammek_balance_change",
            "Изменения баланса",
            ["type"],
            new HistogramConfiguration
            {
                Buckets = [100, 200, 300, 400, 500, 600, 700, 1000, 1500, 2000, 3000, 4000, 5000, 6000, 6500]
            }
        );

    private readonly Gauge _currentLoan = metricFactory.CreateGauge("zammek_current_loan_percent", "Текущая ставка");

    public void OnIncreaseBalance(decimal amount)
        => _balanceChange.WithLabels("increase").Observe((double)amount);

    public void OnDecreaseBalance(decimal amount)
        => _balanceChange.WithLabels("decrease").Observe((double)amount);

    public void SetLoanPercent(int value)
        => _currentLoan.Set(value);
}