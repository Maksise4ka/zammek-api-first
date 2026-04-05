using System.Diagnostics;
using Google.Type;
using Grpc.Core;
using Zammek.Bank.Proto.Credits;
using Zammek.Metrics;

namespace Zammek.Services;

public class CreditGrpcService(ILogger<CreditGrpcService> logger, MetricsSet metricsSet, ActivitySource activitySource)
    : CreditService.CreditServiceBase
{
    public override Task<IncreaseBalanceResponse> IncreaseBalance(IncreaseBalanceRequest request,
        ServerCallContext context)
    {
        ValidateUser(request.UserId);
        UpdateBalance(request.UserId);

        metricsSet.OnIncreaseBalance(ToDecimal(request.Amount));
        logger.LogInformation("User {userId} increased balance", request.UserId);

        return Task.FromResult(new IncreaseBalanceResponse());
    }

    public override Task<DecreaseBalanceResponse> DecreaseBalance(DecreaseBalanceRequest request,
        ServerCallContext context)
    {
        ValidateUser(request.UserId);

        if (ToDecimal(request.Amount) <= 0)
        {
            using var activity = activitySource.StartActivity("Handle bad request");

            logger.LogInformation("Suspicious decrease transaction from user {userId}", request.UserId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid amount"));
        }

        UpdateBalance(request.UserId);
        metricsSet.OnDecreaseBalance(ToDecimal(request.Amount));
        logger.LogInformation("User {userId} decreased balance", request.UserId);

        return Task.FromResult(new DecreaseBalanceResponse());
    }

    private static decimal ToDecimal(Money money)
        => money.Units + (decimal)money.Nanos / 1_000_000_000;

    private void ValidateUser(long userId)
    {
        using var _ = activitySource.StartActivity();

        using (var activity = activitySource.StartActivity("Sql.GetUserById"))
        {
            activity!.SetTag("userId", userId);
        }

        using (var activity = activitySource.StartActivity("Sql.GetCreditsByUserId"))
        {
            activity!.SetTag("userId", userId);
        }
    }

    private void UpdateBalance(long userId)
    {
        using var _ = activitySource.StartActivity();

        using (var activity = activitySource.StartActivity("CreateTransaction"))
        {
            activity!.SetTag("userId", userId);
        }

        using (var activity = activitySource.StartActivity("Sql.UpdateUserBalance"))
        {
            activity!.SetTag("userId", userId);
        }

        using (var activity = activitySource.StartActivity("Sql.CreatePayment"))
        {
            activity!.SetTag("paymentId", Guid.NewGuid());
        }

        using (var activity = activitySource.StartActivity("Sql.PublishOutbox"))
        {
            activity!.SetTag("paymentId", Guid.NewGuid());
        }

        using (var activity = activitySource.StartActivity("CommitTransaction"))
        {
            activity!.SetTag("userId", userId);
        }
    }
}