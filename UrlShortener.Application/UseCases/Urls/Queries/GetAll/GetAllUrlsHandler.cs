using NetDevPack.SimpleMediator;
using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Application.Shared.Mappings;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.Urls.Queries.GetAll;

public class GetAllUrlsHandler : IRequestHandler<GetAllUrlsRequest, List<MinimalUrlDto>>
{
    private readonly IUrlRepos _urlRepos;

    public GetAllUrlsHandler(
        IUrlRepos urlRepos
    )
    {
        _urlRepos = urlRepos;
    }

    public async Task<List<MinimalUrlDto>> Handle(GetAllUrlsRequest request, CancellationToken cancellationToken)
    {
        var qO = request.QueryObject;

        var urls = await _urlRepos.GetByCreatorEmailAsync(
            request.CreatorEmail,
            qO.OrderByUniqueVisitors,
            qO.Decresc,
            qO.PageNumber,
            cancellationToken
        );

        return [.. urls.Select(u => u.ToMinimalDto())];
    }
}
