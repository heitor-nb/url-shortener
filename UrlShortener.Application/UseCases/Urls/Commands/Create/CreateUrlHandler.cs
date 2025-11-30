using NetDevPack.SimpleMediator;
using UrlShortener.Application.Exceptions;
using UrlShortener.Application.Shared.Validators;
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
        var creator = await _userRepos.GetByEmailAsync(request.CreatorEmail, cancellationToken) ?? throw new NotFoundException("The informed creator email is not associated with any user.");

        var uri = UrlValidator.Validate(request.LongUrl) ?? throw new AppException("The informed long URL is not valid.");

        var url = new Url(
            creator,
            uri
        );

        await _urlRepos.CreateAndCommitAsync(url, cancellationToken);

        return url.PublicId!;
    }
}
