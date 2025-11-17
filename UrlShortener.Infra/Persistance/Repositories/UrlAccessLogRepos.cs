using Microsoft.Extensions.Configuration;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Infra.Persistance.Repositories;

public class UrlAccessLogRepos : BaseRepos<UrlAccessLog>, IUrlAccessLogRepos
{
    public UrlAccessLogRepos(
        UrlShortenerContext 
        context, IConfiguration cfg
    ) : base(context, cfg)
    {
    }
}
