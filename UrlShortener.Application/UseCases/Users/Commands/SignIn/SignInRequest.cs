using NetDevPack.SimpleMediator;

namespace UrlShortener.Application.UseCases.Users.Commands.Queries.SignIn;

public record SignInRequest(
    string Email,
    string Password
) : IRequest<(string token, string refreshToken)>;
