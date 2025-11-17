using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Infra.Persistance.Repositories;

public class RefreshTokenRepos : BaseRepos<RefreshToken>, IRefreshTokenRepos
{
    public RefreshTokenRepos(
        UrlShortenerContext context, 
        IConfiguration cfg
    ) : base(context, cfg)
    {
    }

    public async Task<RefreshToken?> GetActiveByUserIdAsync(
        int userId, 
        CancellationToken ct
    ) => await _context.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == userId && r.RevokedAt == null, ct);

    public async Task<RefreshToken?> GetByTokenAsync(
        string token, 
        CancellationToken ct
    ) => await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token.Equals(token), ct);
}
