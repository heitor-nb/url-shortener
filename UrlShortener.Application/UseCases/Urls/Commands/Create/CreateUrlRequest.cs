using NetDevPack.SimpleMediator;

namespace UrlShortener.Application.UseCases.Urls.Commands.Create;

public record CreateUrlRequest(
    string CreatorEmail,
    string LongUrl
) : IRequest<string>;
