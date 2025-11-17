using UrlShortener.Domain.Exceptions;

namespace UrlShortener.Domain.ValueObjects;

public class Name
{
    public Name(
        string value
    )
    {
        if (value.Length > 64) throw new DomainException("Name size must be less or equal to 64 characters.");

        Value = value;
    }

    public string Value { get; private set; }
}
