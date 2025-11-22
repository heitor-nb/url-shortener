using NSubstitute;
using UrlShortener.Application.UseCases.Urls.Commands.Create;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;
using UrlShortener.Tests.Helpers;

namespace UrlShortener.Tests.Application.Urls;

public class CreateUrlHandlerTests
{
    private readonly IUserRepos _userRepos = Substitute.For<IUserRepos>();
    private readonly IUrlRepos _urlRepos = Substitute.For<IUrlRepos>();

    private CreateUrlHandler CreateHandler() => new(
        _userRepos,
        _urlRepos
    );

    [Fact]
    public async Task ShouldCreateUrlGivenValid()
    {   
        var handler = CreateHandler();
        var creator = EntityFactory.CreateUser();
        var request = new CreateUrlRequest(creator.Email.Address, new("http://teste.com"));

        _userRepos
            .GetByEmailAsync(creator.Email.Address, Arg.Any<CancellationToken>())
            .Returns(creator);

        Url? capturedUrl = null;

        /*

        When(...) can be used for methods that return Task or void.
        Do(...) triggers a callback function before continue the flow.
        ci is a CallInfo. It provides the real arguments passed into the call.

        */

        _urlRepos
            .When(x => x.CreateAndCommitAsync(Arg.Any<Url>(), Arg.Any<CancellationToken>()))
            .Do(ci =>
            {
                capturedUrl = ci.Arg<Url>();
                
                capturedUrl.SetPublicId(Substitute.For<IHashids>());
            });

        var publicId = await handler.Handle(request, CancellationToken.None);

        await _userRepos.Received(1).GetByEmailAsync(creator.Email.Address, Arg.Any<CancellationToken>());
        await _urlRepos.Received(1).CreateAndCommitAsync(Arg.Any<Url>(), Arg.Any<CancellationToken>());

        Assert.NotNull(publicId);
        Assert.Equal(capturedUrl!.PublicId, publicId);
    }
}

