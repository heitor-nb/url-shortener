using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Domain.Entities;

public class User : BaseEntity
{
    public User(
        Name name,
        Email email,
        string password
    )
    {
        Name = name;
        Email = email;
        Password = password;
    }

    protected User() { } // For EF

    private readonly List<Url> _urls = [];

    public Name Name { get; private set; }
    public Email Email { get; private set; }
    public string Password { get; set; }
    public IReadOnlyCollection<Url> Urls => _urls;
}
