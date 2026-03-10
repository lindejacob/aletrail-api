using aletrail_api.Dtos.User;
using aletrail_api.Models;

namespace aletrail_api.Services.Auth;

public interface IAuthService
{
    Task<string?> Login(UserLoginDto loginDto);
    Task<int?> Register(UserCreateDto userCreateDto);
}