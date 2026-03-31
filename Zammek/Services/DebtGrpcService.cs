using Grpc.Core;
using Zammek.Bank.Proto.Credits;

namespace Zammek.Services;

public class DebtGrpcService(ILogger<DebtGrpcService> logger) : DebtService.DebtServiceBase
{
    public override Task<BuyDebtResponse> BuyDebt(BuyDebtRequest request, ServerCallContext context)
    {
        logger.LogInformation("User {UserId} bought debt with Id {DebtId}", request.UserId, request.DebtId);
        return base.BuyDebt(request, context);
    }
}