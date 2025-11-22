using Microsoft.AspNetCore.Mvc;
using NetDevPack.SimpleMediator;
using UrlShortener.Application.UseCases.Users.Commands.Queries.SignIn;
using UrlShortener.Application.UseCases.Users.Commands.SignUp;
using UrlShortener.Application.UseCases.Users.Queries.RefreshTokens;

namespace UrlShortener.Api.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _cfg;

    public AuthController(
        IMediator mediator,
        IConfiguration cfg
    )
    {
        _mediator = mediator;
        _cfg = cfg;
    }

    [HttpPost]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        var user = await _mediator.Send(request);

        return Created("/SignIn", user);
    }

    [HttpPost]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        var (token, refreshToken) = await _mediator.Send(request);

        AppendAuthCookies(Response, token, refreshToken);

        return Ok(new { message = "Sign in successful" });
    }

    [HttpGet]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refresh-token"];

        var request = new RefreshTokenRequest(refreshToken ?? string.Empty);

        var (token, newRefreshToken) = await _mediator.Send(request);

        AppendAuthCookies(Response, token, newRefreshToken);

        return Ok(new { message = "Tokens refreshed successfully." });
    }

    // * implement logout

    private void AppendAuthCookies(
        HttpResponse response,
        string token,
        string refreshToken)
    {
        var jwtSettings = _cfg.GetSection("JwtSettings");

        response.Cookies.Append("jwt-token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpirationTimeInMinutes"))
        });

        response.Cookies.Append("refresh-token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(jwtSettings.GetValue<int>("RefreshTokenExpirationTimeInDays"))
        });
    }
}

