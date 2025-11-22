using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Repositories;

public interface IUrlRepos : IBaseRepos<Url>
{   
    Task CreateAndCommitAsync(
        Url url,
        CancellationToken ct
    );

    Task<List<Url>> GetByCreatorEmailAsync(
        string creatorEmail,
        bool orderByUniqueVisitors,
        bool decresc,
        int pageNumber,
        CancellationToken ct
    );
    
    Task<Url?> GetByIdAsync(
        int id, 
        CancellationToken ct,
        bool includeCreator = false,
        bool includeAccessLogs = false
    );
}
