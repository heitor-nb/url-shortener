namespace UrlShortener.Domain.Interfaces.Repositories;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct);
}
