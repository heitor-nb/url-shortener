using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces;

public interface IAuthService
{
    string GenerateToken(User user);
    RefreshToken GenerateRefreshToken(User user);
}
