using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Tests.Helpers;

namespace UrlShortener.Tests.Domain.Urls;

public class UrlAddAccessLog
{
    [Fact]
    public void ShouldThrowGivenAccessLogBelongsToAnotherUrl()
    {
        var url1 = EntityFactory.CreateUrl();

        // Change url1 Id

        var type = typeof(BaseEntity);
        var propInfo = type.GetProperty("Id");
        propInfo?.SetValue(url1, 1);

        var url2 = EntityFactory.CreateUrl();
        var url2AccessLog = EntityFactory.CreateUrlAccessLog(url2);

        var ex = Assert.Throws<DomainException>(() => url1.AddAccessLog(url2AccessLog));

        Assert.Equal("Can not add other url's access log.", ex.Message);
    }

    [Fact]
    public void ShouldNotIncrementUniqueVisitorsCountGivenThereIsAlreadyAccessLogWithTheSameVisitorId()
    {
        var url = EntityFactory.CreateUrl();
        
        var visitorId = Guid.NewGuid();
        var urlAccessLog1 = EntityFactory.CreateUrlAccessLog(url, visitorId);
        var urlAccessLog2 = EntityFactory.CreateUrlAccessLog(url, visitorId);

        url.AddAccessLog(urlAccessLog1);

        var previousUniqueVisitorsCount = url.UniqueVisitorsCount;

        url.AddAccessLog(urlAccessLog2);

        Assert.Equal(previousUniqueVisitorsCount, url.UniqueVisitorsCount);
    }

    [Fact]
    public void ShouldIncrementUniqueVisitorsCountGivenThereIsNoAccessLogWithTheSameVisitorId()
    {
        var url = EntityFactory.CreateUrl();
        var urlAccessLog = EntityFactory.CreateUrlAccessLog(url);

        var previousUniqueVisitorsCount = url.UniqueVisitorsCount;

        url.AddAccessLog(urlAccessLog);

        Assert.Equal(previousUniqueVisitorsCount + 1, url.UniqueVisitorsCount);
    }

    [Fact]
    public void ShouldAddGivenValid()
    {
        var url = EntityFactory.CreateUrl();
        var urlAccessLog = EntityFactory.CreateUrlAccessLog(url);

        url.AddAccessLog(urlAccessLog);

        Assert.NotEmpty(url.AccessLogs);
        Assert.Contains(urlAccessLog, url.AccessLogs);
    }
}
