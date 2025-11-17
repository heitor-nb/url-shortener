using NetDevPack.SimpleMediator;
using UrlShortener.Application.Shared.Dtos;

namespace UrlShortener.Application.UseCases.Urls.Queries.GetByPublicId;

public record GetByPublicIdRequest(
    string UserEmail,
    string PublicId
) : IRequest<UrlDto>;
