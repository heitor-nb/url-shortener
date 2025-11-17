using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infra.Persistance;

public class UrlShortenerContext : DbContext
{
    public UrlShortenerContext(
        DbContextOptions<UrlShortenerContext> options
    ) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Url> Urls { get; set; }
    public DbSet<UrlAccessLog> UrlsAccessesLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.OwnsOne(u => u.Name);
            builder.OwnsOne(u => u.Email);

            builder
                .HasMany(u => u.Urls)
                .WithOne(u => u.Creator)
                .HasForeignKey(u => u.CreatorId);
        });

        modelBuilder.Entity<Url>()
            .HasMany(u => u.AccessLogs)
            .WithOne(u => u.Url)
            .HasForeignKey(u => u.UrlId);

        base.OnModelCreating(modelBuilder);
    }
}
