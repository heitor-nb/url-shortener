using NetDevPack.SimpleMediator;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.Urls.Commands.Create;

public class CreateUrlHandler : IRequestHandler<CreateUrlRequest, string>
{
    private readonly IUserRepos _userRepos;
    private readonly IUrlRepos _urlRepos;

    public CreateUrlHandler(
        IUserRepos userRepos,
        IUrlRepos urlRepos
    )
    {
        _userRepos = userRepos;
        _urlRepos = urlRepos;
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

        await _urlRepos.CreateAndCommitAsync(url, cancellationToken);

        return url.PublicId!;
    }
}
