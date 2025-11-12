using System.Security.Cryptography;
using aletrail_api.Dtos.User;
using aletrail_api.Interfaces;
using aletrail_api.Models;

namespace aletrail_api.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<int> RegisterUserAsync(UserCreateDto userCreateDto)
    {
        var existing = await _userRepository.GetByEmailAsync(userCreateDto.Email);
        if (existing != null)
            throw new InvalidOperationException("A user with that email already exists.");

        var passwordHash = HashPassword(userCreateDto.Password);

        var user = new User
        {
            Username = userCreateDto.Username,
            Email = userCreateDto.Email,
            PasswordHash = passwordHash
        };

        var created = await _userRepository.CreateAsync(user);

        return created;
    }

    public async Task<UserDto?> AuthenticateUserAsync(UserLoginDto userLoginDto)
    {
        var user = await _userRepository.GetByEmailAsync(userLoginDto.Email);
        if (user == null) return null;

        if (!VerifyPassword(userLoginDto.Password, user.PasswordHash)) return null;

        return new UserDto
        {
            Username = user.Username,
            Email = user.Email
        };
    }

    private static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        // Use the static PBKDF2 helper to avoid obsolete constructors
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        var output = new byte[1 + SaltSize + KeySize];
        output[0] = 0; // version
        Buffer.BlockCopy(salt, 0, output, 1, SaltSize);
        Buffer.BlockCopy(key, 0, output, 1 + SaltSize, KeySize);

        return Convert.ToBase64String(output);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var bytes = Convert.FromBase64String(storedHash);
        if (bytes.Length != 1 + SaltSize + KeySize || bytes[0] != 0) return false;

        var salt = new byte[SaltSize];
        Buffer.BlockCopy(bytes, 1, salt, 0, SaltSize);

        var key = new byte[KeySize];
        Buffer.BlockCopy(bytes, 1 + SaltSize, key, 0, KeySize);

        var keyToCheck = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
    }
}
