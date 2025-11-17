using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using NetDevPack.SimpleMediator;
using UrlShortener.Application.UseCases.Urls.Queries.GetLongUrlByPublicId;
using UrlShortener.Application.UseCases.UrlsAccessesLogs.Commands.Create;

namespace UrlShortener.Api.Controllers;

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ChannelWriter<CreateUrlAccessLogRequest> _writer;

    public RedirectController(
        IMediator mediator,
        Channel<CreateUrlAccessLogRequest> channel
    )
    {
        _mediator = mediator;
        _writer = channel.Writer;
    }

    /*

    Even though the action performs a write in the database, 
    the standard HTTP method applied by the browsers is the GET.

    */

    // [HttpGet("{publicId}")]
    // public async Task<IActionResult> RedirectToLongUrl(
    //     [FromRoute] string publicId
    // )
    // {   
    //     var result = Guid.TryParse(Request.Cookies["visitor-id"], out var visitorId);

    //     if (!result)
    //     {
    //         visitorId = Guid.NewGuid();

    //         Response.Cookies.Append("visitor-id", visitorId.ToString(), new CookieOptions
    //         {
    //             HttpOnly = true,
    //             Secure = true,
    //             SameSite = SameSiteMode.None
    //         });
    //     }

    //     var referrer = Request.Headers.Referer.ToString();

    //     var request = new CreateUrlAccessLogRequest(
    //         publicId,
    //         visitorId,
    //         referrer
    //     );

    //     var longUrl = await _mediator.Send(request);

    //     return Redirect(longUrl.ToString());
    // }

    [HttpGet("{publicId}")]
    public async Task<IActionResult> RedirectToLongUrl(
        [FromRoute] string publicId
    )
    {
        var request = new GetLongUrlByPublicIdRequest(publicId);

        var longUrl = await _mediator.Send(request);

        /*

        Handling the url access log creation in a background service
        removes the write operation time from the controller flow.

        */

        var result = Guid.TryParse(Request.Cookies["visitor-id"], out var visitorId);

        if (!result)
        {
            visitorId = Guid.NewGuid();

            Response.Cookies.Append("visitor-id", visitorId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
        }

        var referrer = Request.Headers.Referer.ToString();

        await _writer.WriteAsync(new(
            publicId,
            visitorId,
            referrer
        ));

        // ----------

        return Redirect(longUrl.ToString());
    }
}
