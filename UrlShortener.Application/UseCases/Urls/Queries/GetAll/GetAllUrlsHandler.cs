using NetDevPack.SimpleMediator;
using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Application.Shared.Mappings;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.Urls.Queries.GetAll;

public class GetAllUrlsHandler : IRequestHandler<GetAllUrlsRequest, List<MinimalUrlDto>>
{
    private readonly IUserRepos _userRepos;
    private readonly IUrlRepos _urlRepos;

    public GetAllUrlsHandler(
        IUserRepos userRepos,
        IUrlRepos urlRepos
    )
    {
        _userRepos = userRepos;
        _urlRepos = urlRepos;
    }

    public async Task<List<MinimalUrlDto>> Handle(GetAllUrlsRequest request, CancellationToken cancellationToken)
    {
        var creator = await _userRepos.GetByEmailAsync(request.CreatorEmail, cancellationToken);

        var qO = request.QueryObject;

        var urls = await _urlRepos.GetByCreatorEmailAsync(
            creator!.Email.Address,
            qO.OrderByUniqueVisitors,
            qO.Decresc,
            qO.PageNumber,
            cancellationToken
        );

        return [.. urls.Select(u => u.ToMinimalDto())]; // ** Select(u => u.ToMinimalDto()).OrderBy(u => u.UniqueVisitors) in repos method?
    }
}
