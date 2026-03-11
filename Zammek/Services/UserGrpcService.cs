using Zammek.Bank.Proto.Users;

namespace Zammek.Services;

public class UserGrpcService(ILogger<UserGrpcService> logger) : UserService.UserServiceBase
{
}