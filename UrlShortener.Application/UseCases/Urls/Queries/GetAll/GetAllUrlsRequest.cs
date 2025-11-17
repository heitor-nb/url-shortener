using NetDevPack.SimpleMediator;
using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Application.Shared.QueryObjects;

namespace UrlShortener.Application.UseCases.Urls.Queries.GetAll;

public record GetAllUrlsRequest(
    string CreatorEmail,
    UrlQueryObject QueryObject
) : IRequest<List<MinimalUrlDto>>;

// * test not sending QueryObject
