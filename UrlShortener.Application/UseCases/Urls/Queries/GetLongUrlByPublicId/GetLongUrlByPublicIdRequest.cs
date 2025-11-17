using NetDevPack.SimpleMediator;

namespace UrlShortener.Application.UseCases.Urls.Queries.GetLongUrlByPublicId;

public record GetLongUrlByPublicIdRequest(
    string PublicId
) : IRequest<Uri>;
