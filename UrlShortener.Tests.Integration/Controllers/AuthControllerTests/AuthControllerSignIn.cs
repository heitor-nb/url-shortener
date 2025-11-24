using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.UseCases.Users.Commands.Queries.SignIn;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Infra.Persistance;
using UrlShortener.Tests.Integration.Fixtures;

namespace UrlShortener.Tests.Integration.Controllers.AuthControllerTests;

public class AuthControllerSignIn : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;
    private readonly string _url = "/api/v1/Auth/SignIn";

    public AuthControllerSignIn(
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
    public async Task ShouldSignInGivenValid()
    {
        var email = "test@mail.com";
        var password = "Password123!";

        AsyncServiceScope scope;
        User user;

        await using (scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            user = new User(
                new("test"),
                new(email),
                passwordHasher.Hash(password)
            );

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        var request = new SignInRequest(email, password);

        var response = await _client.PostAsJsonAsync(_url, request);

        // status code + cookies

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        /*

        Cookies are sent as string inside the HTTP header.
        ex.:
        Set-Cookie: auth_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...; 
                    path=/; 
                    httponly; 
                    samesite=none; 
                    secure;
                    expires=Tue, 25 Nov 2025 12:00:00 GMT

        */

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
        Assert.Equal("Sign in successful", body["message"]);

        // database

        /*

        Should persist the new generated refresh token and revoke the older, if applicable.
        (Since there is no older refresh token, just the INSERT will be tested)

        */

        await using(scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

            var persistedRefreshToken = await context.RefreshTokens.FirstOrDefaultAsync();

            Assert.NotNull(persistedRefreshToken);
            Assert.Equal(user.Id, persistedRefreshToken.UserId);
        }
    }

    [Fact]
    public async Task ShouldReturnUnsupportedMediaTypeGivenNullBody()
    {
        var response = await _client.PostAsync(_url, null);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnBadRequestGivenInvalidModel()
    {
        var invalidBody = new { email = "test@mail.com" };

        var response = await _client.PostAsJsonAsync(_url, invalidBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
