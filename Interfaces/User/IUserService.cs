using aletrail_api.Dtos;
using aletrail_api.Dtos.User;

namespace aletrail_api.Services;

public interface IUserService
{
    public Task<int> RegisterUserAsync(UserCreateDto userCreateDto);
    public Task<UserDto?> AuthenticateUserAsync(UserLoginDto userLoginDto);
}