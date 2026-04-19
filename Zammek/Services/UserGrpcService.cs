using Grpc.Core;
using Zammek.Bank.Proto.Users;

namespace Zammek.Services;

public class UserGrpcService : UserService.UserServiceBase
{
    public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CreateUserResponse());
    }
}