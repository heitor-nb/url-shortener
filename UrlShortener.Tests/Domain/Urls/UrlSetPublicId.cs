using NSubstitute;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Tests.Helpers;

namespace UrlShortener.Tests.Domain.Urls;

public class UrlSetPublicId
{   
    private readonly IHashids _hashids = Substitute.For<IHashids>();

    [Fact]
    public void ShouldSetGivenPublicIdIsNull()
    {
        var url = EntityFactory.CreateUrl();
        var publicId = _hashids.Encode(url.Id);

        url.SetPublicId(_hashids);

        Assert.NotNull(url.PublicId);
        Assert.Equal(publicId, url.PublicId);
    }

    [Fact]
    public void ShouldNotSetGivenPublicIdIsAlreadySet()
    {
        var url = EntityFactory.CreateUrl();
        url.SetPublicId(_hashids);

        url.SetPublicId(_hashids);

        _hashids.Received(1).Encode(url.Id);
    }
}
