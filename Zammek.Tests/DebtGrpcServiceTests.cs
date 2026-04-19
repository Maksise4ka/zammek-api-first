using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Zammek.Bank.Proto.Credits;
using Zammek.Services;

namespace Zammek.Tests;

public class DebtGrpcServiceTests
{
    private readonly Mock<ILogger<DebtGrpcService>> _logger = new(MockBehavior.Strict);
    private readonly DebtGrpcService _service;

    public DebtGrpcServiceTests()
    {
        _service = new DebtGrpcService(_logger.Object);
    }

    [Fact]
    public async Task BuyDebt_ValidRequest_CallsBaseMethod()
    {
        // Arrange
        var request = new BuyDebtRequest
        {
            UserId = 1,
            DebtId = 1
        };
        
        _logger.Setup(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Act
        var response = await _service.BuyDebt(request, Mock.Of<ServerCallContext>());

        // Assert
        response.Should().NotBeNull();
    }
}