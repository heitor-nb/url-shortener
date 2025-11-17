using NetDevPack.SimpleMediator;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.Users.Commands.Queries.SignIn;

public class SignInHandler : IRequestHandler<SignInRequest, (string token, string refreshToken)>
{
    private readonly IUserRepos _userRepos;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthService _authService;
    private readonly IRefreshTokenRepos _refreshTokenRepos;
    private readonly IUnitOfWork _unitOfWork;

    public SignInHandler(
        IUserRepos userRepos,
        IPasswordHasher passwordHasher,
        IAuthService authService,
        IRefreshTokenRepos refreshTokenRepos,
        IUnitOfWork unitOfWork
    )
    {
        _userRepos = userRepos;
        _passwordHasher = passwordHasher;
        _authService = authService;
        _refreshTokenRepos = refreshTokenRepos;
        _unitOfWork = unitOfWork;
    }

    public async Task<(string token, string refreshToken)> Handle(SignInRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepos.GetByEmailAsync(request.Email, cancellationToken) ?? throw new ApplicationException("Email or password invalids.");

        if (!_passwordHasher.Verify(request.Password, user.Password)) throw new ApplicationException("Email or password invalids.");

        var token = _authService.GenerateToken(user);
        var refreshToken = _authService.GenerateRefreshToken(user);

        var activeRefreshToken = await _refreshTokenRepos.GetActiveByUserIdAsync(user.Id, cancellationToken);

        activeRefreshToken?.Revoke(refreshToken);

        await _refreshTokenRepos.CreateAsync(refreshToken, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return (token, refreshToken.Token);
    }
}
