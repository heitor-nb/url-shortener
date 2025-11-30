namespace UrlShortener.Application.Shared.QueryObjects;

public record BaseQueryObject
{
    public bool Decresc { get; init; } = false;

    private int _pageNumber = 1;
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }
}

