using EventServer.Client.Services;
using FastEndpoints;
using FluentValidation;
using org.fortium.fx.common;

namespace EventServer.Endpoints;

public class PartnerEndpoint : Endpoint<PartnerInfoRequest, Partner>
{
    private readonly IPartnerService _partnerService;

    public PartnerEndpoint(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    public override void Configure()
    {
        Post("/api/partnerinfo");
        AllowAnonymous();
    }

    public override async Task HandleAsync(PartnerInfoRequest request, CancellationToken ct)
    {
        var partner = await _partnerService.GetPartner(email: request.Email!);
        if (partner == null)
            throw new Exception("Partner not found");

        await SendAsync(partner, cancellation: ct);
    }
}

public class PartnerInfoRequest
{
    public string? Email { get; set; }
}

public class PartnerInfoRequestValidator : AbstractValidator<PartnerInfoRequest>
{
    public PartnerInfoRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
    }
}

