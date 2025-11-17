using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetDevPack.SimpleMediator;
using UrlShortener.Application.Shared.QueryObjects;
using UrlShortener.Application.UseCases.Urls.Commands.Create;
using UrlShortener.Application.UseCases.Urls.Queries.GetAll;
using UrlShortener.Application.UseCases.Urls.Queries.GetByPublicId;

namespace UrlShortener.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class UrlController : ControllerBase
{
    private readonly ILogger<UrlController> _logger;
    private readonly IMediator _mediator;

    public UrlController(
        ILogger<UrlController> logger,
        IMediator mediator
    )
    {   
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(
        [FromBody] Uri longUrl
    )
    {   
        var creatorEmail = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;

        var request = new CreateUrlRequest(
            creatorEmail,
            longUrl
        );

        var shortUrl = await _mediator.Send(request);

        return Created("/GetById", new
        {
            shortUrl
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] UrlQueryObject queryObject
    )
    {   
        var creatorEmail = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;

        var request = new GetAllUrlsRequest(
            creatorEmail,
            queryObject
        );

        var urls = await _mediator.Send(request);

        return Ok(urls);
    }

    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetByPublicId(
        [FromRoute] string publicId
    )
    {   
        var creatorEmail = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;

        var request = new GetByPublicIdRequest(
            creatorEmail,
            publicId
        );

        var url = await _mediator.Send(request);

        return Ok(new
        {
            url
        });
    }
}

