using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Interfaces;

namespace UrlShortener.Domain.Entities;

public class Url : BaseEntity
{
    public Url(
        User creator,
        Uri longUrl
    )
    {
        Creator = creator;
        CreatorId = creator.Id;

        LongUrl = longUrl;
    }
    
    protected Url() { } // For EF

    private readonly List<UrlAccessLog> _accessLogs = [];

    public User Creator { get; private set; }
    public int CreatorId { get; private set; }
    public Uri LongUrl { get; private set; }
    public string? PublicId { get; private set; } = null;
    public IReadOnlyCollection<UrlAccessLog> AccessLogs => _accessLogs;

    public int UniqueVisitorsCount { get; private set; }

    public void SetPublicId(
        IHashids hashids
    )
    {
        if (PublicId != null) return;

        PublicId = hashids.Encode(Id);
    }

    public virtual void AddAccessLog(UrlAccessLog accessLog)
    {
        if(accessLog.UrlId != Id) throw new DomainException("Can not add other url's access log.");

        if(!_accessLogs.Any(a => a.VisitorId == accessLog.VisitorId)) UniqueVisitorsCount++;

        _accessLogs.Add(accessLog);
    }
}
