using NSubstitute;
using NSubstitute.ReturnsExtensions;
using UrlShortener.Application.Exceptions;
using UrlShortener.Application.Shared;
using UrlShortener.Application.UseCases.UrlsAccessesLogs.Commands.Create;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;
using UrlShortener.Tests.Helpers;

namespace UrlShortener.Tests.Application.UrlAccessLogs;

public class CreateUrlAccessLogHandlerHandle
{
    private readonly IUrlRepos _urlRepos = Substitute.For<IUrlRepos>();
    private readonly IUrlAccessLogRepos _urlAccessLogRepos = Substitute.For<IUrlAccessLogRepos>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IHashids _hashids = Substitute.For<IHashids>();

    private CreateUrlAccessLogHandler CreateHandler() => new(
        _hashids,
        _urlRepos,
        _urlAccessLogRepos,
        _unitOfWork
    );

    [Fact]
    public async Task ShouldCreateAccessLogGivenValid()
    {
        var handler = CreateHandler();
        var request = new CreateUrlAccessLogRequest(
            "public-id",
            Guid.NewGuid(),
            ""
        );
        var url = EntityFactory.CreateUrl();

        var type = typeof(BaseEntity);
        var propInfo = type.GetProperty("Id");
        propInfo?.SetValue(url, 10);

        UrlAccessLog? capturedLog = null;

        _hashids.Decode(request.UrlPublicId).Returns(url.Id);

        _urlRepos
            .GetByIdAsync(url.Id, Arg.Any<CancellationToken>(), includeAccessLogs: true)
            .Returns(url);

        _urlAccessLogRepos
            .When(x => x.CreateAsync(Arg.Any<UrlAccessLog>(), Arg.Any<CancellationToken>()))
            .Do(ci => capturedLog = ci.Arg<UrlAccessLog>());

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(capturedLog);
        Assert.Equal(url.Id, capturedLog.UrlId);
        Assert.Equal(request.VisitorId, capturedLog.VisitorId);
        Assert.Equal(request.Referrer, capturedLog.Referrer);
        Assert.Equal(typeof(VoidResult), result.GetType());
        Assert.Contains(capturedLog, url.AccessLogs);

        await _urlRepos.Received(1).GetByIdAsync(url.Id, Arg.Any<CancellationToken>(), includeAccessLogs: true);
        await _urlAccessLogRepos.Received(1).CreateAsync(capturedLog, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldThrowGivenUrlDoesNotExist()
    {
        var handler = CreateHandler();
        var request = new CreateUrlAccessLogRequest(
            "public-id",
            Guid.NewGuid(),
            ""
        );
        var nonExistentId = 10;

        _hashids.Decode(request.UrlPublicId).Returns(nonExistentId);

        _urlRepos
            .GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>(), includeAccessLogs: true)
            .ReturnsNull();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal("The informed public ID is not associated with any URL.", ex.Message);
    }

    [Fact]
    public async Task ShouldCallAddAccessLogOnEntityGivenValid()
    {
        var handler = CreateHandler();
        var request = new CreateUrlAccessLogRequest(
            "public-id",
            Guid.NewGuid(),
            ""
        );
        var existingId = 10;

        _hashids.Decode(request.UrlPublicId).Returns(existingId);

        var url = Substitute.For<Url>();

        _urlRepos
            .GetByIdAsync(existingId, Arg.Any<CancellationToken>(), includeAccessLogs: true)
            .Returns(url);

        await handler.Handle(request, CancellationToken.None);

        url.Received(1).AddAccessLog(Arg.Any<UrlAccessLog>());
    }
}

