using EventServer.Aggregates.Partners.Commands;
using EventServer.Aggregates.Partners.Events;
using EventServer.Client.Services;
using FluentValidation;
using Marten;
using Wolverine.Http;

namespace EventServer.Endpoints;

public class PartnerEndpoint
{
    private readonly IPartnerService _partnerService;

    public PartnerEndpoint(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }


    [WolverineGet("/api/partnerinfo")]
    public static async Task<(PartnerInfoResponse, PartnerCreatedEvent)> Post(GetPartnerCommand command,
        IDocumentSession session)
    {
        var aggregate = await session.Events.FetchStreamAsync(command.Id);


        return (
            new PartnerInfoResponse(command.Id),
            new PartnerCreatedEvent(command.Id, "<NAME>", "<EMAIL>", "1234567890")
        );
    }
}

public class PartnerInfoRequest
{
    public PartnerInfoRequest(string commandId)
    {
        Email = commandId;
    }

    public string? Email { get; set; }
}

public class PartnerInfoRequestValidator : AbstractValidator<PartnerInfoRequest>
{
    public PartnerInfoRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
    }
}

public class PartnerInfoResponse
{
    public PartnerInfoResponse(string id)
    {
        Email = id;
    }

    public string? Email { get; set; }
}