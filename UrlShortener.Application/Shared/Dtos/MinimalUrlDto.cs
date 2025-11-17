namespace UrlShortener.Application.Shared.Dtos;

public record MinimalUrlDto(
    string PublicId,
    DateTime CreatedAt,
    Uri LongUrl,
    int UniqueVisitors
);
