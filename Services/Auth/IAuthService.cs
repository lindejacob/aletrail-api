using aletrail_api.Dtos.User;
using aletrail_api.Models;

namespace aletrail_api.Services.Auth;

public interface IAuthService
{
    UserDto? Login(UserLoginDto loginDto);
    UserDto Register(UserCreateDto userCreateDto);
}