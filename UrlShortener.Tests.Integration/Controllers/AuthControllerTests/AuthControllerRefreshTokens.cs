using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Domain.Entities;
using UrlShortener.Infra.Persistance;
using UrlShortener.Tests.Integration.Fixtures;

namespace UrlShortener.Tests.Integration.Controllers.AuthControllerTests;

public class AuthControllerRefreshTokens : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;
    private readonly string _url = "/api/v1/Auth/RefreshTokens";

    public AuthControllerRefreshTokens(WebApplicationFactoryFixture factory)
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
    public async Task ShouldRefreshTokensGivenValid()
    {   
        AsyncServiceScope scope;

        var user = new User(
            new("test"),
            new("test@mail.com"),
            ""
        );

        var refreshTokenToken = Guid.NewGuid().ToString();

        await using (scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();
            
            await using var transaction = await context.Database.BeginTransactionAsync();

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var refreshToken = new RefreshToken(
                refreshTokenToken,
                user,
                7
            );

            await context.RefreshTokens.AddAsync(refreshToken);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, _url);
        request.Headers.Add("Cookie", $"refresh-token={refreshTokenToken}");

        var response = await _client.SendAsync(request);

        // status code + cookies

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cookies = response.Headers.GetValues("Set-Cookie").ToList();

        var jwtTokenCookie = cookies.FirstOrDefault(c => c.StartsWith("jwt-token="));
        var refreshTokenCookie = cookies.FirstOrDefault(c => c.StartsWith("refresh-token="));

        Assert.NotNull(jwtTokenCookie);
        Assert.NotNull(refreshTokenCookie);

        static void CheckCookie(string cookie)
        {
            Assert.Contains("httponly", cookie);
            Assert.Contains("samesite=none", cookie);
            Assert.Contains("secure", cookie);
            // Assert of the expires
        }

        CheckCookie(jwtTokenCookie);
        CheckCookie(jwtTokenCookie);

        // body

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.NotNull(body);
        Assert.Equal("Tokens refreshed successfully.", body["message"]);

        // database

        /*

        The informed refresh token must be revoked (RevokedAt != null)
        and the returned one must be persisted.

        */

        await using (scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

            var refreshTokens = await context.RefreshTokens.OrderBy(t => t.CreateAt).ToListAsync();

            Assert.Equal(2, refreshTokens.Count);

            var informedRefreshToken = refreshTokens[0];
            var returnedRefreshToken = refreshTokens[1];

            Assert.NotNull(informedRefreshToken!.RevokedAt);

            Assert.NotNull(returnedRefreshToken);
            Assert.Equal(user.Id, returnedRefreshToken.UserId);
        }
    }

    [Fact]
    public async Task ShouldReturnUnauthorizedGivenNoRefreshToken()
    {
        var response = await _client.PostAsync(_url, null);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.NotNull(body);
        Assert.Equal("Missing refresh token.", body["message"]);
    }
}
