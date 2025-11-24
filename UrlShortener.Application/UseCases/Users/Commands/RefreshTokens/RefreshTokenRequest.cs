using NetDevPack.SimpleMediator;

namespace UrlShortener.Application.UseCases.Users.Commands.RefreshTokens;

public record RefreshTokenRequest(
    string Token
) : IRequest<(string token, string refreshToken)>;
