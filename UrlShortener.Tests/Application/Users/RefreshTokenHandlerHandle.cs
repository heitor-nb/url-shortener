using NSubstitute;
using NSubstitute.ReturnsExtensions;
using UrlShortener.Application.Exceptions;
using UrlShortener.Application.UseCases.Users.Queries.RefreshTokens;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;
using UrlShortener.Tests.Helpers;

namespace UrlShortener.Tests.Application.Users;

public class RefreshTokenHandlerHandle
{
    private readonly IUserRepos _userRepos = Substitute.For<IUserRepos>();
    private readonly IRefreshTokenRepos _refreshTokenRepos = Substitute.For<IRefreshTokenRepos>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IAuthService _authService = Substitute.For<IAuthService>();

    private RefreshTokenHandler CreateHandler() => new(
        _refreshTokenRepos,
        _authService,
        _userRepos,
        _unitOfWork
    );

    [Fact]
    public async Task ShouldThrowGivenRefreshTokenDoesNotExist()
    {
        var handler = CreateHandler();
        var request = new RefreshTokenRequest("refresh-token");

        _refreshTokenRepos
            .GetByTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal("The informed token is not associated with any refresh token.", ex.Message);
    }

    [Fact]
    public async Task ShouldGenerateNewTokensAndRevokeOldRefreshTokenGivenValidRefreshToken()
    {
        var handler = CreateHandler();
        var request = new RefreshTokenRequest("old-refresh-token");

        var user = EntityFactory.CreateUser();
        var oldRefreshToken = EntityFactory.CreateRefreshToken(user);

        _refreshTokenRepos
            .GetByTokenAsync(request.Token, Arg.Any<CancellationToken>())
            .Returns(oldRefreshToken);

        _userRepos
            .GetByIdAsync(oldRefreshToken.UserId, Arg.Any<CancellationToken>())
            .Returns(user);

        var newRefreshToken = EntityFactory.CreateRefreshToken(user);

        _authService.GenerateToken(user).Returns("new-access-token");
        _authService.GenerateRefreshToken(user).Returns(newRefreshToken);

        var (token, refreshToken) = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(newRefreshToken.Token, oldRefreshToken.ReplacedByToken);
        Assert.NotNull(oldRefreshToken.RevokedAt);

        Assert.Equal("new-access-token", token);
        Assert.Equal(newRefreshToken.Token, refreshToken);

        await _refreshTokenRepos.Received(1).CreateAsync(newRefreshToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}

