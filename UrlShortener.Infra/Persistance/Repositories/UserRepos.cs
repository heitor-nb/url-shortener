using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Infra.Persistance.Repositories;

public class UserRepos : BaseRepos<User>, IUserRepos
{
    public UserRepos(
        UrlShortenerContext context, 
        IConfiguration cfg
    ) : base(context, cfg)
    {
    }

    public async Task<User?> GetByEmailAsync(
        string email, 
        CancellationToken ct
    ) => await _context.Users
        .FirstOrDefaultAsync(u => u.Email.Address.Equals(email), ct);
}
