using Google.Type;
using Grpc.Core;
using Zammek.Bank.Proto.Credits;
using Zammek.Metrics;

namespace Zammek.Services;

public class CreditGrpcService(MetricsSet metricsSet) : CreditService.CreditServiceBase
{
    public override Task<IncreaseBalanceResponse> IncreaseBalance(IncreaseBalanceRequest request,
        ServerCallContext context)
    {
        metricsSet.OnIncreaseBalance(ToDecimal(request.Amount));
        return Task.FromResult(new IncreaseBalanceResponse());
    }

    public override Task<DecreaseBalanceResponse> DecreaseBalance(DecreaseBalanceRequest request,
        ServerCallContext context)
    {
        metricsSet.OnDecreaseBalance(ToDecimal(request.Amount));

        return Task.FromResult(new DecreaseBalanceResponse());
    }

    private static decimal ToDecimal(Money money)
        => money.Units + (decimal)money.Nanos / 1_000_000_000;
}