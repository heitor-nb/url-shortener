using NetDevPack.SimpleMediator;
using UrlShortener.Application.Shared;

namespace UrlShortener.Application.UseCases.UrlsAccessesLogs.Commands.Create;

public record CreateUrlAccessLogRequest(
    string UrlPublicId,
    Guid VisitorId,
    string Referrer
) : IRequest<VoidResult>;
