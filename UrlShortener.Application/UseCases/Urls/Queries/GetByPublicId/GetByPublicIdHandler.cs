using NetDevPack.SimpleMediator;
using UrlShortener.Application.Exceptions;
using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Application.Shared.Mappings;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.Urls.Queries.GetByPublicId;

public class GetByPublicIdHandler : IRequestHandler<GetByPublicIdRequest, UrlDto>
{
    private readonly IHashids _hashids;
    private readonly IUrlRepos _urlRepos;

    public GetByPublicIdHandler(
        IHashids hashids,
        IUrlRepos urlRepos
    )
    {   
        _hashids = hashids;
        _urlRepos = urlRepos;
    }

    public async Task<UrlDto> Handle(GetByPublicIdRequest request, CancellationToken cancellationToken)
    {   
        var id = _hashids.Decode(request.PublicId);

        var url = await _urlRepos.GetByIdAsync(
            id, 
            cancellationToken,
            includeCreator: true,
            includeAccessLogs: true
        ) ?? throw new NotFoundException("The informed public ID is not associated with any URL.");

        if (!url.Creator.Email.Address.Equals(request.UserEmail)) throw new UnauthorizedException("Only the creator can inspect the URL metrics.");

        return url.ToDto();
    }
}
