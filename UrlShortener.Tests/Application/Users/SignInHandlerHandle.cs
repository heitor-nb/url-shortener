using NSubstitute;
using NSubstitute.ReturnsExtensions;
using UrlShortener.Application.Exceptions;
using UrlShortener.Application.UseCases.Users.Commands.Queries.SignIn;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;
using UrlShortener.Tests.Helpers;

namespace UrlShortener.Tests.Application.Users;

public class SignInHandlerHandle
{
    private readonly IUserRepos _userRepos = Substitute.For<IUserRepos>();
    private readonly IRefreshTokenRepos _refreshTokenRepos = Substitute.For<IRefreshTokenRepos>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IAuthService _authService = Substitute.For<IAuthService>();

    private SignInHandler CreateHandler() => new(
        _userRepos,
        _passwordHasher,
        _authService,
        _refreshTokenRepos,
        _unitOfWork
    );

    [Fact]
    public async Task ShouldThrowGivenEmailDoesNotExist()
    {
        var handler = CreateHandler();
        var request = new SignInRequest("teste@email.com", "Senha123!");

        _userRepos
            .GetByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var ex = await Assert.ThrowsAsync<AppException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal("Email or password invalids.", ex.Message);
    }

    [Fact]
    public async Task ShouldThrowGivenPasswordDoesNotMatch()
    {
        var handler = CreateHandler();
        var request = new SignInRequest("email@mail.com", "wrongpass");

        var user = EntityFactory.CreateUser(email: request.Email);

        _userRepos
            .GetByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher
            .Verify(request.Password, user.Password)
            .Returns(false);

        var ex = await Assert.ThrowsAsync<AppException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal("Email or password invalids.", ex.Message);
    }

    [Fact]
    public async Task ShouldGenerateTokensGivenValidCredentials()
    {
        var handler = CreateHandler();
        var request = new SignInRequest("teste@email.com", "Senha123!");

        var user = EntityFactory.CreateUser(email: request.Email);

        _userRepos
            .GetByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher
            .Verify(request.Password, user.Password)
            .Returns(true);

        var refreshToken = EntityFactory.CreateRefreshToken(user);

        _authService.GenerateToken(user).Returns("access-token");
        _authService.GenerateRefreshToken(user).Returns(refreshToken);

        _refreshTokenRepos
            .GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("access-token", result.token);
        Assert.Equal(refreshToken.Token, result.refreshToken);

        await _refreshTokenRepos.Received(1).CreateAsync(refreshToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldRevokeActiveRefreshTokenGivenItExists()
    {
        var handler = CreateHandler();
        var request = new SignInRequest("teste@email.com", "Senha123!");

        var user = EntityFactory.CreateUser(email: request.Email);

        _userRepos
            .GetByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify(request.Password, user.Password).Returns(true);

        var newRefreshToken = EntityFactory.CreateRefreshToken(user);
        var activeRefreshToken = EntityFactory.CreateRefreshToken(user);

        _authService.GenerateRefreshToken(user).Returns(newRefreshToken);

        _refreshTokenRepos
            .GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>())              
            .Returns(activeRefreshToken);

        await handler.Handle(request, CancellationToken.None);

        await _refreshTokenRepos.Received(1).GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>());
        await _refreshTokenRepos.Received(1).CreateAsync(newRefreshToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());

        Assert.Equal(newRefreshToken.Token, activeRefreshToken.ReplacedByToken);
        Assert.NotNull(activeRefreshToken.RevokedAt);
    }

    [Fact]
    public async Task ShouldNotRevokeGivenNoActiveRefreshToken()
    {
        var handler = CreateHandler();
        var request = new SignInRequest("teste@email.com", "Senha123!");

        var user = EntityFactory.CreateUser(email: request.Email);

        _userRepos
            .GetByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify(request.Password, user.Password).Returns(true);

        var refreshToken = EntityFactory.CreateRefreshToken(user);

        _authService.GenerateRefreshToken(user).Returns(refreshToken);

        _refreshTokenRepos
            .GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        await handler.Handle(request, CancellationToken.None);

        await _refreshTokenRepos.Received(1).GetActiveByUserIdAsync(user.Id, Arg.Any<CancellationToken>());
        await _refreshTokenRepos.Received(1).CreateAsync(refreshToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}

