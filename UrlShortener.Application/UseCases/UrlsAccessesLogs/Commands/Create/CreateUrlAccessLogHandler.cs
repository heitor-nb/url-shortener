using NetDevPack.SimpleMediator;
using UrlShortener.Application.Exceptions;
using UrlShortener.Application.Shared;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;

namespace UrlShortener.Application.UseCases.UrlsAccessesLogs.Commands.Create;

public class CreateUrlAccessLogHandler : IRequestHandler<CreateUrlAccessLogRequest, VoidResult>
{
    private readonly IHashids _hashids;
    private readonly IUrlRepos _urlRepos;
    private readonly IUrlAccessLogRepos _urlAccessLogRepos;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUrlAccessLogHandler(
        IHashids hashids,
        IUrlRepos urlRepos,
        IUrlAccessLogRepos urlAccessLogRepos,
        IUnitOfWork unitOfWork
    )
    {
        _hashids = hashids;
        _urlRepos = urlRepos;
        _urlAccessLogRepos = urlAccessLogRepos;
        _unitOfWork = unitOfWork;
    }

    public async Task<VoidResult> Handle(CreateUrlAccessLogRequest request, CancellationToken cancellationToken)
    {   
        var urlId = _hashids.Decode(request.UrlPublicId);

        var url = await _urlRepos.GetByIdAsync(
            urlId,
            cancellationToken,
            includeAccessLogs: true
        ) ?? throw new NotFoundException("The informed public ID is not associated with any URL.");
        
        var urlAccessLog = new UrlAccessLog(
            url,
            request.VisitorId,
            request.Referrer
        );

        url.AddAccessLog(urlAccessLog); // Important to update UniqueVisitorsCount

        await _urlAccessLogRepos.CreateAsync(urlAccessLog, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return VoidResult.Instance;
    }
}
