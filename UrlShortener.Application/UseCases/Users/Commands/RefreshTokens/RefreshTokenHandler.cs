using NetDevPack.SimpleMediator;
using UrlShortener.Application.Exceptions;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.Users.Queries.RefreshTokens;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, (string token, string refreshToken)>
{
    private readonly IRefreshTokenRepos _refreshTokenRepos;
    private readonly IAuthService _authService;
    private readonly IUserRepos _userRepos;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(
        IRefreshTokenRepos refreshTokenRepos,
        IAuthService authService,
        IUserRepos userRepos,
        IUnitOfWork unitOfWork
    )
    {
        _refreshTokenRepos = refreshTokenRepos;
        _authService = authService;
        _userRepos = userRepos;
        _unitOfWork = unitOfWork;
    }

    public async Task<(string token, string refreshToken)> Handle(
        RefreshTokenRequest request, 
        CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepos.GetByTokenAsync(
            request.Token,
            cancellationToken
        ) ?? throw new NotFoundException("The informed token is not associated with any refresh token.");

        var user = await _userRepos.GetByIdAsync(
            refreshToken.UserId,
            cancellationToken
        );

        var token = _authService.GenerateToken(user!);
        var newRefreshToken = _authService.GenerateRefreshToken(user!);

        refreshToken.Revoke(newRefreshToken);

        await _refreshTokenRepos.CreateAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return (token, newRefreshToken.Token);
    }
}
