using System;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Tests.Helpers;

public static class EntityFactory
{
    public static User CreateUser(
        string? email = null
    ) => new(
        new("teste"),
        new(email ?? "teste@email.com"),
        "Senha123!"
    );

    public static RefreshToken CreateRefreshToken(
        User? user = null
    ) => new(
        "refresh-token",
        user ?? CreateUser(),
        7
    );

    public static Url CreateUrl() => new(
        CreateUser(),
        new("http://teste.com")
    );

    public static UrlAccessLog CreateUrlAccessLog(
        Url? url = null,
        Guid? visitorId = null
    ) => new(
        url ?? CreateUrl(),
        visitorId ?? Guid.NewGuid(),
        ""
    );
}
