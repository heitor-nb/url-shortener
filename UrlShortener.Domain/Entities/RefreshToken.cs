using UrlShortener.Domain.Exceptions;

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
        if(substituteToken != null)
        {
            if(substituteToken.UserId != UserId) throw new DomainException("Can not replace by other user's token.");

            if(substituteToken.ExpiresAt < DateTime.UtcNow) throw new DomainException("Can not replace by an expired token.");

            if(substituteToken.RevokedAt != null) throw new DomainException("Can not replace by an revoked token.");

            if(ReplacedByToken != null) throw new DomainException("Can not change the replacement token.");
        }

        ReplacedByToken = substituteToken?.Token;

        RevokedAt = DateTime.UtcNow;
    }
}
