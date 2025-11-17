namespace UrlShortener.Application.Shared.QueryObjects;

public record UrlQueryObject(
    bool OrderByUniqueVisitors
) : BaseQueryObject;
