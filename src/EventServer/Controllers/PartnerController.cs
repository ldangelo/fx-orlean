using EventServer.Aggregates.Partners;
using EventServer.Aggregates.Partners.Commands;
using EventServer.Aggregates.Partners.Events;
using Marten;
using Marten.Events;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace EventServer.Controllers;

public record PartnerLoginResponse(Partner partner);

public class PartnerAggregateHandler
{
    public static void Handle(PartnerLoggedInCommand command,IEventStream<Partner> stream)
    {
        Log.Information("PartnerLoggedInEventHandler: Applying login event to {EmailAddress}", command.Id);
        var partner = stream.Aggregate;

        if(partner.IsLoggedIn()) {
            Log.Debug($"Partner {command.Id} is already logged in.");
            throw new InvalidOperationException($"Partner {command.Id} is already logged in.");
        } else {
            stream.AppendOne(new PartnerLoggedInEvent(command.Id, command.LoginTime));
        }
    }

    public static PartnerLoggedOutEvent Handler(PartnerLoggedOutCommand command, IDocumentSession session, IMessageBus bus)
    {
        Log.Information("PartnerLoggedInEventHandler: Applying login event to {EmailAddress}", command.Id);
        return new PartnerLoggedOutEvent(command.Id, command.LogoutTime);
    }

}

public class PartnerController: ControllerBase
{

    [WolverineGet("/partners/{emailAddress}")]
    public Partner GetPartner( [Document("emailAddress")] Partner partner) {
        Log.Information("Getting partner {emailAddress}.", partner.EmailAddress);


        return partner;
    }



    [WolverinePost("/partners")]
    public static (CreationResponse, IStartStream) CreatePartners(
        CreatePartnerCommand command,
        IDocumentSession session
    )
    {
        Log.Information("Creating partner {Id}.", command.EmailAddress);


        var startStream = MartenOps.StartStream<Partner>(command.EmailAddress,new PartnerCreatedEvent(command.Id, command.FirstName, command.LastName,command.EmailAddress));

        return (
            new CreationResponse(Url: $"/partners/{command.EmailAddress}"),
            startStream
        );
    }

    [WolverinePost("/partners/loggedin/{partnerId}"), EmptyResponse]
    public static PartnerLoggedInEvent GetPartners(
        [FromBody] PartnerLoggedInCommand command,
        [Aggregate] Partner partner
    )
    {
        Log.Information("Logging partner {Id} in at {time}.", command.Id, command.LoginTime);

        return new PartnerLoggedInEvent(command.Id, command.LoginTime);
    }

    [WolverinePost("/partners/loggedout/{partnerId}"), EmptyResponse]
    public static PartnerLoggedOutEvent LogOutPartners(
        [FromBody] PartnerLoggedOutCommand command,
        [Aggregate] Partner partner
    )
    {
        Log.Information("Logging partner {Id} out.", command.Id);

        return new PartnerLoggedOutEvent(command.Id, command.LogoutTime);
    }
}
