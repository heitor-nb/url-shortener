namespace UrlShortener.Application.Shared.Validators;

public static class UrlValidator
{
    public static Uri? Validate(
        string url
    )
    {   
        if(url.Length > 2048) return null;

        if(!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;

        if(uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return null;

        return uri;
    }
}
