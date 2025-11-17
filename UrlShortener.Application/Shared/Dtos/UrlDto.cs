namespace UrlShortener.Application.Shared.Dtos;

public record UrlDto(
    string PublicId,
    DateTime CreatedAt,
    Uri LongUrl,
    int UniqueVisitors,
    (string Day, int Hour)? PeakAccessDateTime,
    Dictionary<string, int> ReferrersCount
);
