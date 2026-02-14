using aletrail_api.DAL.Account;
using aletrail_api.Dtos.User;
using aletrail_api.Mappers.UserMapper;
using aletrail_api.Services.Jwt;
using aletrail_api.Services.Security;

namespace aletrail_api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    
    public AuthService(IConfiguration configuration, IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<string?> Login(UserLoginDto loginDto)
    {
        var hashedUser = await _userRepository.getUserByEmailAsync(loginDto.Email);
        if (hashedUser == null) return null;
        if (_passwordHasher.VerifyPassword(hashedUser.PasswordHash, loginDto.Password))
        {
            var token = _jwtService.GenerateJWTToken(hashedUser);
            return token;
        }
        return null;
    }

    public async Task<int?> Register(UserCreateDto userCreateDto)
    {
        // if (await _userRepository.UsernameExistsAsync(userCreateDto.Username))
        // {
        //     throw new Exception("Username already exists");
        // }
        // if (await _userRepository.EmailExistsAsync(userCreateDto.Email))
        // {
        //     throw new Exception("Email already exists");
        // }
        var user = UserCreateDtoMapper.ToUser(userCreateDto);
        var userId = await _userRepository.insertUserAsync(user);
        return userId;
    }
}