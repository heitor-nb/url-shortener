using System.Text.RegularExpressions;
using UrlShortener.Domain.Exceptions;

namespace UrlShortener.Domain.ValueObjects;

public partial class Email
{
    public Email(
        string address
    )
    {
        if (!ValidarEmail().IsMatch(address)) throw new DomainException("The email address informed is invalid.");

        Address = address;
    }

    public string Address { get; private set; }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex ValidarEmail();
}
