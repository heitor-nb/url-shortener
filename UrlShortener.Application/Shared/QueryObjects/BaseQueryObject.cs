namespace UrlShortener.Application.Shared.QueryObjects;

public record BaseQueryObject(
    // Order by creation date by default
    bool Decresc = false,
    int PageNumber = 1
);
