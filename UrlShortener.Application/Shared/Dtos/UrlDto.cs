namespace UrlShortener.Application.Shared.Dtos;

public record UrlDto(
    string PublicId,
    DateTime CreatedAt,
    Uri LongUrl,
    int UniqueVisitors,
    AccessDateTime? PeakAccessDateTime,
    Dictionary<string, int> ReferrersCount
);
