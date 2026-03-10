namespace aletrail_api.Services.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string providedPassword, string hashedPassword);
}