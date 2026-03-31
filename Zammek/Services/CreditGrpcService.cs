using Google.Type;
using Grpc.Core;
using Zammek.Bank.Proto.Credits;
using Zammek.Metrics;

namespace Zammek.Services;

public class CreditGrpcService(ILogger<CreditGrpcService> logger, MetricsSet metricsSet)
    : CreditService.CreditServiceBase
{
    public override Task<IncreaseBalanceResponse> IncreaseBalance(IncreaseBalanceRequest request,
        ServerCallContext context)
    {
        metricsSet.OnIncreaseBalance(ToDecimal(request.Amount));
        logger.LogInformation("User {userId} increased balance", request.UserId);

        return Task.FromResult(new IncreaseBalanceResponse());
    }

    public override Task<DecreaseBalanceResponse> DecreaseBalance(DecreaseBalanceRequest request,
        ServerCallContext context)
    {
        if (ToDecimal(request.Amount) <= 0)
        {
            logger.LogInformation("Suspicious decrease transaction from user {userId}", request.UserId);
            return Task.FromResult(new DecreaseBalanceResponse());
        }

        metricsSet.OnDecreaseBalance(ToDecimal(request.Amount));
        logger.LogInformation("User {userId} decreased balance", request.UserId);


        return Task.FromResult(new DecreaseBalanceResponse());
    }

    private static decimal ToDecimal(Money money)
        => money.Units + (decimal)money.Nanos / 1_000_000_000;
}