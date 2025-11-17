using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetDevPack.SimpleMediator;
using UrlShortener.Application.UseCases.UrlsAccessesLogs.Commands.Create;
using UrlShortener.Domain.Exceptions;

namespace UrlShortener.Infra;

public class CreateUrlAccessLogProcessor : BackgroundService
{
    private readonly ILogger<CreateUrlAccessLogProcessor> _logger;
    private readonly ChannelReader<CreateUrlAccessLogRequest> _reader;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateUrlAccessLogProcessor(
        ILogger<CreateUrlAccessLogProcessor> logger,
        Channel<CreateUrlAccessLogRequest> channel,
        IServiceScopeFactory scopeFactory
    )
    {
        _logger = logger;
        _reader = channel.Reader;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach(var request in _reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("|{datetime}| Sending create url access log request. Url public id: {publicId}", DateTime.UtcNow, request.UrlPublicId);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(request, stoppingToken);

                _logger.LogInformation("|{datetime}| Url access log successfully created. Url public id: {publicId}", DateTime.UtcNow, request.UrlPublicId);
            }
            catch(BaseException ex)
            {
                _logger.LogInformation("Handled exception: {message}", ex.Message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
            }
        }
    }
}
