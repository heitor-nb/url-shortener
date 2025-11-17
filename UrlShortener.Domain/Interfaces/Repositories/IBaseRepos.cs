using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Interfaces.Repositories;

public interface IBaseRepos<T> where T : BaseEntity
{
    Task CreateAsync(T t, CancellationToken ct);
    Task<List<T>> GetAllAsync(int pageNumber, CancellationToken ct);
    Task<T?> GetByIdAsync(int id, CancellationToken ct);
    void Update(T t);
    void Delete(T t);
}
