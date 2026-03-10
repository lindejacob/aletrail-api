using aletrail_api.Dtos.User;
using aletrail_api.Models;

namespace aletrail_api.Mappers.UserMapper;

public class UserCreateDtoMapper
{
    public static UserCreateDto ToDto(User user)
    {
        return new UserCreateDto
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.PasswordHash
        };
    }
    
    public static User ToUser(UserCreateDto dto)
    {
        
        return new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = dto.Password //Remember to hash it
        };
    }

    
}

