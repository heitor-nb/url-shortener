using NetDevPack.SimpleMediator;

namespace UrlShortener.Application.UseCases.UrlsAccessesLogs.Commands.Create;

public record CreateUrlAccessLogRequest(
    string UrlPublicId,
    Guid VisitorId,
    string Referrer
) : IRequest<Uri>;
