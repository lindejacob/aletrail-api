using aletrail_api.Models;

namespace aletrail_api.Services.Jwt;

public class JwtService : IJwtService
{
    public string GenerateJWTToken(User user)
    {
        throw new NotImplementedException();
    }

    public string RetrieveIdFromJwtToken(string token)
    {
        throw new NotImplementedException();
    }

    public string RetrieveIdFromJwtTokenNoBearer(string token)
    {
        throw new NotImplementedException();
    }

    public string RetrieveRoleFromJwtToken(string token)
    {
        throw new NotImplementedException();
    }
}