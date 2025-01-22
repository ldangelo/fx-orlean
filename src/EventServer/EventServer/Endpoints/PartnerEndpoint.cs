using EventServer.Client.Services;
using FastEndpoints;
using org.fortium.fx.common;

namespace EventServer.Endpoints;

public class PartnerEndpoint : EndpointWithoutRequest<PartnerSnapshot>
{
    private readonly IPartnerService _partnerService;

    public PartnerEndpoint(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    public override void Configure()
    {
        Get("/api/partner");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var partner = await _partnerService.GetPartner();
        await SendAsync(partner, cancellation: ct);
    }
}