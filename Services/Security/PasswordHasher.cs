namespace aletrail_api.Services.Jwt;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        throw new NotImplementedException();
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        throw new NotImplementedException();
    }
}