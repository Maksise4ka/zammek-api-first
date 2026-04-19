using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Zammek.Jobs;
using Zammek.Metrics;

namespace Zammek.Tests;

public class LoanExporterJobTests
{
    private readonly Mock<IMetricsSet> _metricsSet = new(MockBehavior.Strict);
    private readonly Mock<ILogger<LoanExporterJob>> _logger = new(MockBehavior.Strict);
    private readonly LoanExporterJob _job;

    public LoanExporterJobTests()
    {
        _logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _job = new LoanExporterJob(_metricsSet.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SetsLoanPercentWithinValidRange()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        int? capturedValue = null;

        _metricsSet
            .Setup(m => m.SetLoanPercent(It.IsAny<int>()))
            .Callback<int>(value => capturedValue = value);

        // Act
        _ = _job.StartAsync(cts.Token);
        await Task.Delay(100, cts.Token);
        await _job.StopAsync(cts.Token);

        // Assert
        capturedValue.Should().NotBeNull();
        capturedValue.Should().BeInRange(20, 32);
    }

    [Fact]
    public async Task ExecuteAsync_ClampsLoanPercentWhenExceedingMax()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var values = new List<int>();

        _metricsSet
            .Setup(m => m.SetLoanPercent(It.IsAny<int>()))
            .Callback<int>(value => values.Add(value));

        // Act
        _ = _job.StartAsync(cts.Token);

        // Wait for multiple iterations to ensure we get values at boundaries
        await Task.Delay(500, cts.Token);
        await _job.StopAsync(cts.Token);

        // Assert - all values should be within valid range
        values.Should().NotBeEmpty();
        foreach (var value in values)
        {
            value.Should().BeInRange(20, 32);
        }
    }
}