using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Infra.Persistance.Repositories;

public class UnitOfWork : IUnitOfWork
{   
    private readonly UrlShortenerContext _context;

    public UnitOfWork(
        UrlShortenerContext context
    )
    {
        _context = context;
    }
    
    public async Task CommitAsync(CancellationToken ct) => await _context.SaveChangesAsync(ct);
}
