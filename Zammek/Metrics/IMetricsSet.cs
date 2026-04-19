namespace Zammek.Metrics;

public interface IMetricsSet
{
    void OnIncreaseBalance(decimal amount);
    void OnDecreaseBalance(decimal amount);
    void SetLoanPercent(int value);
}