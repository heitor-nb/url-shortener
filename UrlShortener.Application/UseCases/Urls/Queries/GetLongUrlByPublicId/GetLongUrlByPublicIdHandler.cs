using NetDevPack.SimpleMediator;
using UrlShortener.Application.Exceptions;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.Urls.Queries.GetLongUrlByPublicId;

public class GetLongUrlByPublicIdHandler : IRequestHandler<GetLongUrlByPublicIdRequest, Uri>
{
    private readonly IHashids _hashids;
    private readonly IUrlRepos _urlRepos;

    public GetLongUrlByPublicIdHandler(
        IHashids hashids,
        IUrlRepos urlRepos
    )
    {
        _hashids = hashids;
        _urlRepos = urlRepos;
    }

    public async Task<Uri> Handle(GetLongUrlByPublicIdRequest request, CancellationToken cancellationToken)
    {
        var id = _hashids.Decode(request.PublicId);

        var url = await _urlRepos.GetByIdAsync(
            id,
            cancellationToken
        ) ?? throw new NotFoundException("The informed public ID is not associated with any URL.");

        return url.LongUrl;
    }
}
