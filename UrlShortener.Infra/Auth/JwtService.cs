using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;

namespace UrlShortener.Infra.Auth;

public class JwtService : IAuthService
{
    private readonly IConfigurationSection _jwtSettings;

    public JwtService(
        IConfiguration cfg
    )
    {
        _jwtSettings = cfg.GetSection("JwtSettings");
    }

    public string GenerateToken(User user)
    {

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings["SecretKey"] ?? string.Empty));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new(ClaimTypes.Email, user.Email.Address),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = _jwtSettings.GetValue<int>("ExpirationTimeInMinutes");

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expires),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        var randomNumber = new byte[64]; // 512 bits

        RandomNumberGenerator.Fill(randomNumber);
        
        var token = Convert.ToBase64String(randomNumber);

        return new(
            token,
            user,
            _jwtSettings.GetValue<int>("RefreshTokenExpirationTimeInDays")
        );
    }
}
