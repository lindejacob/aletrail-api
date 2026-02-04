using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using aletrail_api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace aletrail_api.Services.Jwt;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string GenerateJWTToken(User user)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var keyString = _configuration["Jwt:Key"] ?? "ChangeThis_Default_ReplaceInProduction!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        int expirationMinutes = 60;
        if (!string.IsNullOrWhiteSpace(_configuration["Jwt:ExpirationMinutes"]))
        {
            int.TryParse(_configuration["Jwt:ExpirationMinutes"], out expirationMinutes);
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string RetrieveIdFromJwtToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return string.Empty;

        const string bearerPrefix = "Bearer ";
        if (token.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring(bearerPrefix.Length).Trim();
        }

        return RetrieveIdFromJwtTokenNoBearer(token);
    }

    public string RetrieveIdFromJwtTokenNoBearer(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return string.Empty;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var idClaim = jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == JwtRegisteredClaimNames.Sub ||
                c.Type == "id"
            );

            return idClaim?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public string RetrieveRoleFromJwtToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return string.Empty;

        const string bearerPrefix = "Bearer ";
        if (token.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring(bearerPrefix.Length).Trim();
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var roleClaim = jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type == "roles"
            );

            return roleClaim?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}