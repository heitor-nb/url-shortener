using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Domain.Interfaces.Repositories;
using UrlShortener.Infra.Persistance.Repositories;

namespace UrlShortener.Infra.Persistance;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistanceServices(
        this IServiceCollection services,
        IConfiguration cfg
    )
    {
        services.AddDbContext<UrlShortenerContext>(options => options.UseNpgsql(cfg.GetConnectionString("Default")));

        services.AddScoped<IUserRepos, UserRepos>();
        services.AddScoped<IRefreshTokenRepos, RefreshTokenRepos>();
        services.AddScoped<IUrlRepos, UrlRepos>();
        services.AddScoped<IUrlAccessLogRepos, UrlAccessLogRepos>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
