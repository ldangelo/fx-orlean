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

    [WolverinePost("/partners/loggedin")]
    [ProducesResponseType(200, Type = typeof(Partner))]
    public async Task<Partner?> GetPartners(
        [FromBody] PartnerLoggedInCommand command,
        [FromServices] IDocumentSession session
    )
    {
        Log.Information("Logging partner {Id} in at {time}.", command.Id, command.LoginTime);

        var stream = await session.Events.FetchForWriting<Partner>(command.Id);

        var partner = stream.Aggregate;

        //
        // create a new partner if needed
        if (partner == null) return null;

        partner.LoggedIn = true;

        session.Events.Append(command.Id,new PartnerLoggedInEvent(command.Id, command.LoginTime));
        session.Store<Partner>(partner);


        await session.SaveChangesAsync();
        return partner;
    }

    [WolverineGet("/partners/{emailAddress}")]
    public async Task<Partner> GetPartner( string emailAddress,[FromServices] IDocumentSession session) {
        Log.Information("Getting partner {emailAddress}.", emailAddress);

        var stream = await session.Events.FetchForWriting<Partner>(emailAddress);

        return stream.Aggregate;
    }

    [WolverinePost("/partners")]
    [ProducesResponseType(200, Type = typeof(Partner))]
    public async Task<Partner> CreatePartners(
        [FromBody] CreatePartnerCommand command,
        [FromServices] IDocumentSession session
    )
    {
        Log.Information("Creating partner {Id}.", command.EmailAddress);


        var stream = await session.Events.FetchForWriting<Partner>(command.EmailAddress);

        var partner = stream.Aggregate;

        if (partner != null) {
            Log.Information($"Partner {command.EmailAddress} already exists.");
            throw new InvalidOperationException($"Partner {command.EmailAddress} already exists.");
        } else {
            partner = new Partner();
        }

        partner.FirstName = command.FirstName;
        partner.LastName = command.LastName;
        partner.EmailAddress = command.EmailAddress;

        Log.Information("Saving partner: {partner}",partner.ToString());
        session.Store<Partner>(partner);

        session.Events.Append(partner.EmailAddress,new PartnerCreatedEvent(command.Id, command.FirstName, command.LastName,command.EmailAddress));
        await session.SaveChangesAsync();
        return partner;
    }

    [WolverinePost("/partners/loggedout")]
    public async Task<Partner> LogOutPartners(
        [FromBody] PartnerLoggedOutCommand command,
        [FromServices] IDocumentSession session
    )
    {
        Log.Information("Logging partner {Id} out.", command.Id);

        var stream = await session.Events.FetchForWriting<Partner>(command.Id);

        var partner = stream.Aggregate;
        partner.LoggedIn = false;

        session.Store<Partner>(partner);
        session.Events.Append(command.Id,new PartnerLoggedOutEvent(command.Id, DateTime.Now));

        await session.SaveChangesAsync();
        return partner;
    }
}
