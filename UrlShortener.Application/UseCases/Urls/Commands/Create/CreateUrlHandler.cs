using NetDevPack.SimpleMediator;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.Urls.Commands.Create;

public class CreateUrlHandler : IRequestHandler<CreateUrlRequest, string>
{
    private readonly IUserRepos _userRepos;
    private readonly IUrlRepos _urlRepos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashids _hahsids;

    public CreateUrlHandler(
        IUserRepos userRepos,
        IUrlRepos urlRepos,
        IUnitOfWork unitOfWork,
        IHashids hashids
    )
    {
        _userRepos = userRepos;
        _urlRepos = urlRepos;
        _unitOfWork = unitOfWork;
        _hahsids = hashids;
    }

    public async Task<string> Handle(
        CreateUrlRequest request,
        CancellationToken cancellationToken)
    {
        var creator = await _userRepos.GetByEmailAsync(request.CreatorEmail, cancellationToken);

        var url = new Url(
            creator!, // The email is extracted from the token, thus it belongs to an user.
            request.LongUrl
        );

        await _urlRepos.CreateAsync(url, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // SET THE PUBLIC ID:
        // (analyze this approach)

        url.SetPublicId(_hahsids);

        await _unitOfWork.CommitAsync(cancellationToken);

        return url.PublicId!;
    }
}
