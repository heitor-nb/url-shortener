using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Middlewares;
using UrlShortener.Infra;
using UrlShortener.Infra.Persistance;

namespace UrlShortener.Api;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy
                    .WithOrigins([
                        "http://localhost:5173",
                    "https://urlshortener.hnbraga.net"
                    ])
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );
        });

        builder.Services
            .AddPersistanceServices(builder.Configuration)
            .AddInfraServices(builder.Configuration);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpsRedirection();

        app.MapControllers();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

            dbContext.Database.Migrate();
        }

        app.Run();
    }
}
