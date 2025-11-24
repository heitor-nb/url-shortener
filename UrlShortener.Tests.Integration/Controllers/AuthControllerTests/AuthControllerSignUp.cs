using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Application.UseCases.Users.Commands.SignUp;
using UrlShortener.Infra.Persistance;
using UrlShortener.Tests.Integration.Fixtures;

namespace UrlShortener.Tests.Integration.Controllers.AuthControllerTests;

public class AuthControllerSignUp : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;

    private readonly string _url = "/api/v1/Auth/SignUp";

    public AuthControllerSignUp(
        WebApplicationFactoryFixture factory
    )
    {
        _factory = factory;
        _client = _factory.CreateClient(); // * here method ConfigureWebHost(...) is called
    }

    // InitializeAsync() and DisposeAsync() are called foreach [Fact]

    /*

    IAsyncLifetime methods are being used to reset the database between tests
    - since the constructor do not support async calls.

    */

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();
        await context.Database.EnsureDeletedAsync();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();
        await context.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task ShouldCreateUserGivenValid()
    {
        var request = new SignUpRequest(
            "test",
            "test@mail.com",
            "Password123!"
        );

        var response = await _client.PostAsJsonAsync(_url, request);

        // headers

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/v1/Auth/SignIn", response.Headers.Location?.ToString());

        // body

        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();

        Assert.NotNull(createdUser);
        Assert.Equal(request.Name, createdUser.Name);
        Assert.Equal(request.Email, createdUser.Email);

        // database

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

        var persistedUser = await context.Users.FirstOrDefaultAsync(u => u.Email.Address == createdUser.Email);

        Assert.NotNull(persistedUser);
        Assert.Equal(createdUser.Name, persistedUser.Name.Value);
    }

    [Fact]
    public async Task ShouldReturnUnsupportedMediaTypeGivenNullBody()
    {
        var response = await _client.PostAsync(_url, null);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

        var users = await context.Users.ToListAsync();

        Assert.Empty(users);
    }

    /*
    
    In case one or more request's fields are null, the model binding 
    will not be able to map the body to the SignUpRequest record.
    Thus it will return BadRequest.

    */

    [Fact]
    public async Task ShouldReturnBadRequestGivenInvalidModel()
    {
        var invalidBody = new
        {
            name = "test",
            email = "test@email.com"
        };

        var response = await _client.PostAsJsonAsync(_url, invalidBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

        var users = await context.Users.ToListAsync();

        Assert.Empty(users);
    }
}
