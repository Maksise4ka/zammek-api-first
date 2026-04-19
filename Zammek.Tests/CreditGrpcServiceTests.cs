using System.Diagnostics;
using FluentAssertions;
using Google.Type;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Zammek.Bank.Proto.Credits;
using Zammek.Metrics;
using Zammek.Services;

namespace Zammek.Tests;

public class CreditGrpcServiceTests
{
    private readonly Mock<ILogger<CreditGrpcService>> _logger = new();
    private readonly Mock<IMetricsSet> _metricsSet = new(MockBehavior.Strict);
    private readonly ActivitySource _activitySource = new("TestActivitySource");
    private readonly CreditGrpcService _service;

    public CreditGrpcServiceTests()
    {
        _service = new CreditGrpcService(_logger.Object, _metricsSet.Object, _activitySource);
    }

    [Fact]
    public async Task IncreaseBalance_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new IncreaseBalanceRequest
        {
            UserId = 1,
            Amount = new Money { Units = 100, Nanos = 0, CurrencyCode = "RU" }
        };

        _metricsSet.Setup(m => m.OnIncreaseBalance(It.IsAny<decimal>()));

        // Act
        var response = await _service.IncreaseBalance(request, Mock.Of<ServerCallContext>());

        // Assert
        response.Should().NotBeNull();
        _metricsSet.Verify(m => m.OnIncreaseBalance(100), Times.Once);
    }

    [Fact]
    public async Task DecreaseBalance_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new DecreaseBalanceRequest
        {
            UserId = 1,
            Amount = new Money { Units = 50, Nanos = 0, CurrencyCode = "RU" }
        };

        _metricsSet.Setup(m => m.OnDecreaseBalance(It.IsAny<decimal>()));

        // Act
        var response = await _service.DecreaseBalance(request, Mock.Of<ServerCallContext>());

        // Assert
        response.Should().NotBeNull();
        _metricsSet.Verify(m => m.OnDecreaseBalance(50), Times.Once);
    }

    [Fact]
    public async Task DecreaseBalance_InvalidAmount_ThrowsRpcException()
    {
        // Arrange
        var request = new DecreaseBalanceRequest
        {
            UserId = 1,
            Amount = new Money { Units = -1, Nanos = 0, CurrencyCode = "RU" }
        };

        // Act
        var act = () => _service.DecreaseBalance(request, Mock.Of<ServerCallContext>());

        // Assert
        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.InvalidArgument);
    }
}