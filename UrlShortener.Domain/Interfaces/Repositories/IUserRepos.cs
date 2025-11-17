using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Repositories;

public interface IUserRepos : IBaseRepos<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
}
