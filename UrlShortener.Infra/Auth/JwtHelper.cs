using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace UrlShortener.Infra.Auth;

public static class JwtHelper
{
    public static TokenValidationParameters GetTokenValidationParameters(IConfiguration cfg)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["JwtSettings:SecretKey"] ?? string.Empty));

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ValidateIssuer = false,
            ValidateAudience = false
        };
    }
}
