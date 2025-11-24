using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Infra.Persistance;
using UrlShortener.Tests.Integration.Fixtures;

namespace UrlShortener.Tests.Integration.Controllers;

public class UrlControllerCreate : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;
    private readonly string _url = "/api/v1/Url/Create";

    public UrlControllerCreate(
        WebApplicationFactoryFixture factory
    )
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();
        await context.Database.EnsureDeletedAsync();
    }
    
    [Fact]
    public async Task ShouldCreateGivenValid()
    {   
        AsyncServiceScope scope;
        User user; 

        await using (scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            user = new User(
                new("test"),
                new("test@mail.com"),
                passwordHasher.Hash("Password123!")
            );

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        var longUrl = "http://test/com";

        var response = await _client.PostAsJsonAsync(_url, longUrl);

        // status code + cookies

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/v1/GetById", response.Headers.Location?.ToString());
        
        // body

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string,string>>();

        Assert.NotNull(body);

        var shortUrl = body["shortUrl"];

        // database

        await using(scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

            var persistedUrl = await context.Urls.FirstOrDefaultAsync(u => u.PublicId == shortUrl);

            Assert.NotNull(persistedUrl);
            Assert.Equal(user.Id, persistedUrl.CreatorId);
            Assert.Equal(longUrl, persistedUrl.LongUrl.ToString());
        }
    }

    [Fact]
    public async Task ShouldReturnUnsupportedMediaTypeGivenNullBody()
    {
        var response = await _client.PostAsync(_url, null);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

        var urls = await context.Urls.ToListAsync();

        Assert.Empty(urls);
    }

    [Fact]
    public async Task ShouldReturnBadRequestGivenInvalidModel()
    {
        var invalidBody = new { };

        var response = await _client.PostAsJsonAsync(_url, invalidBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

        var urls = await context.Urls.ToListAsync();

        Assert.Empty(urls);
    }
}
