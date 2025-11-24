using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace UrlShortener.Tests.Integration.Helpers;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public IEnumerable<Claim> Claims { get; set; } = [];
}
