using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Infra.Persistance.Repositories;

public class UrlRepos : BaseRepos<Url>, IUrlRepos
{   
    private readonly IHashids _hashids;
    
    public UrlRepos(
        UrlShortenerContext context, 
        IConfiguration cfg,
        IHashids hashids
    ) : base(context, cfg)
    {
        _hashids = hashids;
    }

    public async Task CreateAndCommitAsync(
        Url url, 
        CancellationToken ct
    )
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        await _context.AddAsync(url, ct);
        await _context.SaveChangesAsync(ct);

        url.SetPublicId(_hashids);

        await _context.SaveChangesAsync(ct);

        /*

        Commit transaction if all commands succeed, transaction will auto-rollback
        when disposed if either commands fails

        */

        await transaction.CommitAsync(ct);
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

        var urls = _context.Urls
            .Where(u => u.Creator.Email.Address.Equals(creatorEmail))
            .Include(u => u.AccessLogs)
            .AsQueryable();

        urls = decresc ? urls.OrderByDescending(u => u.CreateAt) : urls.OrderBy(u => u.CreateAt);

        if(orderByUniqueVisitors) urls = decresc ? urls.OrderByDescending(u => u.UniqueVisitorsCount) : urls.OrderBy(u => u.UniqueVisitorsCount);

        return urls
            .Skip((pageNumber - 1) * _pageSize)
            .Take(_pageSize)
            .ToListAsync(ct);
    }

    public Task<Url?> GetByIdAsync(
        int id, 
        CancellationToken ct,
        bool includeCreator = false,
        bool includeAccessLogs = false
    )
    {
        var urls = _context.Urls.AsQueryable();

        if(includeCreator) urls = urls.Include(u => u.Creator);

        if(includeAccessLogs) urls = urls.Include(u => u.AccessLogs);

        return urls.FirstOrDefaultAsync(u => u.Id == id, ct);
    }
}
