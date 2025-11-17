using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetDevPack.SimpleMediator;
using UrlShortener.Application.UseCases.UrlsAccessesLogs.Commands.Create;

namespace UrlShortener.Infra;

public class CreateUrlAccessLogProcessor : BackgroundService
{
    private readonly ILogger<CreateUrlAccessLogProcessor> _logger;
    private readonly ChannelReader<CreateUrlAccessLogRequest> _reader;
    private readonly IMediator _mediator;

    public CreateUrlAccessLogProcessor(
        ILogger<CreateUrlAccessLogProcessor> logger,
        Channel<CreateUrlAccessLogRequest> channel,
        IMediator mediator
    )
    {
        _logger = logger;
        _reader = channel.Reader;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach(var request in _reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("|{datetime}| Sending create url access log request. Url public id: {publicId}", DateTime.UtcNow, request.UrlPublicId);

            await _mediator.Send(request, stoppingToken);

            _logger.LogInformation("|{datetime}| Url access log successfully created. Url public id: {publicId}", DateTime.UtcNow, request.UrlPublicId);
        }
    }
}
