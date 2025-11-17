namespace UrlShortener.Domain.Entities;

public abstract class BaseEntity
{
    public BaseEntity()
    {
        CreateAt = DateTime.UtcNow;
    }
    
    public int Id { get; private set; }
    public DateTime CreateAt { get; private set; }
}
