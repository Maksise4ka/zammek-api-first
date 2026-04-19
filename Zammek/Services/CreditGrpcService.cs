using System.Diagnostics;
using Google.Type;
using Grpc.Core;
using Zammek.Bank.Proto.Credits;
using Zammek.Metrics;

namespace Zammek.Services;

public class CreditGrpcService(ILogger<CreditGrpcService> logger, IMetricsSet metricsSet, ActivitySource activitySource)
    : CreditService.CreditServiceBase
{
    public override async Task<IncreaseBalanceResponse> IncreaseBalance(IncreaseBalanceRequest request,
        ServerCallContext context)
    {
        ValidateUser(request.UserId);
        await UpdateBalance(ToDecimal(request.Amount), request.UserId);

        metricsSet.OnIncreaseBalance(ToDecimal(request.Amount));
        logger.LogInformation("User {userId} increased balance", request.UserId);

        return new IncreaseBalanceResponse();
    }

    public override async Task<DecreaseBalanceResponse> DecreaseBalance(DecreaseBalanceRequest request,
        ServerCallContext context)
    {
        ValidateUser(request.UserId);

        if (ToDecimal(request.Amount) <= 0)
        {
            using var activity = activitySource.StartActivity("Handle bad request");

            logger.LogInformation("Suspicious decrease transaction from user {userId}", request.UserId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid amount"));
        }

        await UpdateBalance(ToDecimal(request.Amount), request.UserId);
        metricsSet.OnDecreaseBalance(ToDecimal(request.Amount));
        logger.LogInformation("User {userId} decreased balance", request.UserId);

        return new DecreaseBalanceResponse();
    }

    private static decimal ToDecimal(Money money)
        => money.Units + (decimal)money.Nanos / 1_000_000_000;

    private void ValidateUser(long userId)
    {
        using var _ = activitySource.StartActivity();

        using (var activity = activitySource.StartActivity("Sql.GetUserById"))
        {
            activity?.SetTag("userId", userId);
        }

        using (var activity = activitySource.StartActivity("Sql.GetCreditsByUserId"))
        {
            activity?.SetTag("userId", userId);
        }
    }

    private async Task UpdateBalance(decimal amount, long userId)
    {
        using var _ = activitySource.StartActivity();

        using (var activity = activitySource.StartActivity("CreateTransaction"))
        {
            activity?.SetTag("userId", userId);
        }

        using (var activity = activitySource.StartActivity("Sql.UpdateUserBalance"))
        {
            activity?.SetTag("userId", userId);
        }

        using (var activity = activitySource.StartActivity("Sql.CreatePayment"))
        {
            activity?.SetTag("paymentId", Guid.NewGuid());
        }

        using (var activity = activitySource.StartActivity("Sql.PublishOutbox"))
        {
            activity?.SetTag("paymentId", Guid.NewGuid());
        }

        using (var activity = activitySource.StartActivity("CommitTransaction"))
        {
            await Task.Delay((int)amount % 10_000);
            activity?.SetTag("userId", userId);
        }
    }
}