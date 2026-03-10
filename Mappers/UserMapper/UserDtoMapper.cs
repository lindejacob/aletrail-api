using aletrail_api.Dtos.User;
using aletrail_api.Models;

namespace aletrail_api.Mappers.UserMapper;

public class UserDtoMapper
{
    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Username = user.Username,
            Email = user.Email
        };
    }
    
    public static User ToUser(UserDto dto)
    {
        return new User
        {
            Username = dto.Username,
            Email = dto.Email
        };
    }
}