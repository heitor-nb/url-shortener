using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Infra.Persistance.Repositories;

public class BaseRepos<T> : IBaseRepos<T> where T : BaseEntity
{
    protected readonly UrlShortenerContext _context;
    protected readonly int _pageSize;

    public BaseRepos(
        UrlShortenerContext context,
        IConfiguration cfg
    )
    {
        _context = context;
        _pageSize = cfg.GetValue<int>("PageSize");
    }

    public async Task CreateAsync(T t, CancellationToken ct) => await _context.Set<T>().AddAsync(t, ct);

    public void Delete(T t) => _context.Set<T>().Remove(t);

    public async Task<List<T>> GetAllAsync(
        int pageNumber,
        CancellationToken ct
    ) => await _context.Set<T>()
        .OrderBy(t => t.CreateAt)
        .Skip((pageNumber - 1) * _pageSize)
        .Take(_pageSize)
        .ToListAsync(ct);

    public async Task<T?> GetByIdAsync(
        int id, 
        CancellationToken ct
    ) => await _context.Set<T>()
        .FirstOrDefaultAsync(t => t.Id == id, ct);

    public void Update(T t) => _context.Set<T>().Update(t);
}
