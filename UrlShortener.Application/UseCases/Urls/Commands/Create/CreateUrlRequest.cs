using NetDevPack.SimpleMediator;

namespace UrlShortener.Application.UseCases.Urls.Commands.Create;

public record CreateUrlRequest(
    string CreatorEmail,
    Uri LongUrl
) : IRequest<string>;
