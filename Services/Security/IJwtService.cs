using aletrail_api.Models;

namespace aletrail_api.Services.Jwt;

public interface IJwtService
{
    string GenerateJWTToken(User user);
    string RetrieveIdFromJwtToken(string token);
    string RetrieveIdFromJwtTokenNoBearer(string token);
    string RetrieveRoleFromJwtToken(string token);
}