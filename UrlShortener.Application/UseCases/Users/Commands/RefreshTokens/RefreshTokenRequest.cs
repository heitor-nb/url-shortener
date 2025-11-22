using NetDevPack.SimpleMediator;

namespace UrlShortener.Application.UseCases.Users.Queries.RefreshTokens;

public record RefreshTokenRequest(
    string Token
) : IRequest<(string token, string refreshToken)>;
