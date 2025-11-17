using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Shared.Mappings;

public static class UrlMappings
{   
    private record AccessDateTime(DayOfWeek Day, int Hour);

    public static UrlDto ToDto(this Url url) => new(
        url.PublicId!,
        url.CreateAt,
        url.LongUrl,
        url.UniqueVisitorsCount,
        GetPeakAccessDateTime(url.AccessLogs),
        url.AccessLogs.GroupBy(a => a.Referrer).ToDictionary(g => g.Key, g => g.ToList().Count)
    );

    public static MinimalUrlDto ToMinimalDto(this Url url) => new(
        url.PublicId!,
        url.CreateAt,
        url.LongUrl,
        url.AccessLogs.GroupBy(a => a.VisitorId).Select(g => g.Key).ToList().Count
    );

    private static (string Day, int Hour)? GetPeakAccessDateTime(
        IReadOnlyCollection<UrlAccessLog> accessLogs
    )
    {      
        if(accessLogs.Count == 0) return null;

        var accessDateTime = accessLogs
           .GroupBy(a => new AccessDateTime(a.CreateAt.DayOfWeek, a.CreateAt.Hour))
           .OrderBy(g => g.ToList().Count)
           .First()
           .Key;

        return (accessDateTime.Day.ToString(), accessDateTime.Hour);
    }
}