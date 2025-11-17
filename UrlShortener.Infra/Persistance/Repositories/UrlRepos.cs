using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Infra.Persistance.Repositories;

public class UrlRepos : BaseRepos<Url>, IUrlRepos
{
    public UrlRepos(
        UrlShortenerContext context, 
        IConfiguration cfg
    ) : base(context, cfg)
    {
    }

    public Task<List<Url>> GetByCreatorEmailAsync(
        string creatorEmail, 
        bool orderByUniqueVisitors, 
        bool decresc, 
        int pageNumber, 
        CancellationToken ct
    )
    {
        // Filter inside the query -> EF can generate the JOIN automatically.
        // (no need to include Creator)

        var urls = _context.Urls.Where(u => u.Creator.Email.Address.Equals(creatorEmail)).AsQueryable();

        urls = decresc ? urls.OrderByDescending(u => u.CreateAt) : urls.OrderBy(u => u.CreateAt);

        if(orderByUniqueVisitors) urls = decresc ? urls.OrderByDescending(u => u.UniqueVisitorsCount) : urls.OrderBy(u => u.UniqueVisitorsCount);

        return urls
            .Skip((pageNumber - 1) * _pageSize)
            .Take(pageNumber)
            .ToListAsync(ct);
    }

    public Task<Url?> GetByIdAsync(
        int id, 
        CancellationToken ct,
        bool includeCreator = false
    )
    {
        var urls = _context.Urls.AsQueryable();

        if(includeCreator) urls = urls.Include(u => u.Creator);

        return urls.FirstOrDefaultAsync(u => u.Id == id, ct);
    }
}
