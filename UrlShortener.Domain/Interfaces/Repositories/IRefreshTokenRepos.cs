using System;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepos : IBaseRepos<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken ct
    );

    Task<RefreshToken?> GetActiveByUserIdAsync(
        int userId,
        CancellationToken ct
    );
}
