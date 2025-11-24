using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Infra.Persistance;
using UrlShortener.Tests.Integration.Helpers;

namespace UrlShortener.Tests.Integration.Fixtures;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{   
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {   
        builder.ConfigureTestServices(services =>
        {
            // auth

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>("Test", _ => {});

            // db

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UrlShortenerContext>));

            if(descriptor != null) services.Remove(descriptor);

            _connection = new SqliteConnection("DataSource=:memory:");

            _connection.Open();

            services.AddDbContext<UrlShortenerContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });

        base.ConfigureWebHost(builder);
    }

    public async Task InitializeAsync() => await Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        if(_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
