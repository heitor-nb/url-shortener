namespace UrlShortener.Domain.Entities;

public class UrlAccessLog : BaseEntity
{
    public UrlAccessLog(
        Url url,
        Guid visitorId,
        string referrer
    )
    {
        Url = url;
        UrlId = url.Id;
        VisitorId = visitorId;
        Referrer = referrer;
    }

    protected UrlAccessLog() { } // For EF

    public Url Url { get; private set; }
    public int UrlId { get; private set; }
    public Guid VisitorId { get; private set; }
    public string Referrer { get; private set; }
}
