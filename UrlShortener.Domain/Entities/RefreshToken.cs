namespace UrlShortener.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public RefreshToken(
        string token,
        User user,
        int expirationTimeInDays
    )
    {
        Token = token;
        UserId = user.Id;

        ExpiresAt = DateTime.UtcNow.AddDays(expirationTimeInDays);
    }

    protected RefreshToken() { }

    public string Token { get; private set; }
    public int UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public void Revoke(
        RefreshToken? substituteToken = null
    )
    {
        ReplacedByToken = substituteToken?.Token;

        RevokedAt = DateTime.UtcNow;
    }
}
