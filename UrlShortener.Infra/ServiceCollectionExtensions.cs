using System.Reflection;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetDevPack.SimpleMediator;
using UrlShortener.Application;
using UrlShortener.Application.UseCases.UrlsAccessesLogs.Commands.Create;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Infra.Auth;

namespace UrlShortener.Infra;

public static class Extensions
{
    public static IServiceCollection AddInfraServices(
        this IServiceCollection services,
        IConfiguration cfg
    )
    {   
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = JwtHelper.GetTokenValidationParameters(cfg);

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["jwt-token"];

                        if(!string.IsNullOrWhiteSpace(token)) context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthService, JwtService>();

        services.AddTransient<IHashids, Hashids>();

        // Garante que o assembly da camada Application esteja carregado antes de chamar o AddSimpleMediator().
        _ = typeof(ApplicationAssemblyReference).Assembly;

        services.AddSimpleMediator();

        services.AddSingleton<Channel<CreateUrlAccessLogRequest>>();
        services.AddHostedService<CreateUrlAccessLogProcessor>();

        return services;
    }

    /*

    It was throwing exceptions due to dependencies between services with different lifetimes.
    So I override the AddSimpleMediator method only changing from AddTransient to AddScoped.

    */
    
    private static IServiceCollection AddSimpleMediator(
        this IServiceCollection services,
        params object[] args
    )
    {
        var assemblies = ResolveAssemblies(args);

        services.AddScoped<IMediator, Mediator>();

        RegisterHandlers(services, assemblies, typeof(INotificationHandler<>));
        RegisterHandlers(services, assemblies, typeof(IRequestHandler<,>));

        return services;
    }

    private static Assembly[] ResolveAssemblies(object[] args)
    {
        // Return ALL
        if (args == null || args.Length == 0) return [.. AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.FullName))
        ];

        // Return all informed (same behavior as above)
        if (args.All(a => a is Assembly)) return [.. args.Cast<Assembly>()];

        // Return filtered by namespace (most performatic)
        if (args.All(a => a is string))
        {
            var prefixes = args.Cast<string>().ToArray();

            return [.. AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a =>
                    !a.IsDynamic &&
                    !string.IsNullOrWhiteSpace(a.FullName) &&
                    prefixes.Any(p => a.FullName!.StartsWith(p))
                )
            ];
        }

        throw new ArgumentException("Invalid parameters for AddSimpleMediator(). Use: no arguments, Assembly[], or prefix strings.");
    }

    private static void RegisterHandlers(
        IServiceCollection services, 
        Assembly[] assemblies, 
        Type handlerInterface
    )
    {
        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var type in types)
        {
            var interfaces = type
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == handlerInterface
                );

            foreach (var iface in interfaces) services.AddScoped(iface, type);
        }
    }
}
