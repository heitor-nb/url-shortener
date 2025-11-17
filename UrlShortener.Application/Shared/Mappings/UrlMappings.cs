using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Shared.Mappings;

public static class UrlMappings
{   
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
        url.UniqueVisitorsCount
    );

    private static AccessDateTime? GetPeakAccessDateTime(
        IReadOnlyCollection<UrlAccessLog> accessLogs
    )
    {
        var accessDateTime = accessLogs
           .GroupBy(a => new AccessDateTime(a.CreateAt.DayOfWeek.ToString(), a.CreateAt.Hour))
           .OrderBy(g => g.ToList().Count)
           .FirstOrDefault()?
           .Key;

        return accessDateTime;
    }
}