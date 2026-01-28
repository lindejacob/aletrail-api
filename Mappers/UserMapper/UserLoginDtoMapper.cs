using aletrail_api.Dtos.User;
using aletrail_api.Models;

namespace aletrail_api.Mappers.UserMapper;

public class UserLoginDtoMapper
{
    public static UserLoginDto ToDto(User user)
    {
        return new UserLoginDto
        {
            Email = user.Email,
            Password = "" // Password is not stored in User model; return empty or handle accordingly
        };
    }
    
    public static UserLoginDto ToDto(string email, string password)
    {
        return new UserLoginDto
        {
            Email = email,
            Password = password
        };
    }
}