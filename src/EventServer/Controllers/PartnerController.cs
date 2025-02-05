using EventServer.Aggregates.Partners;
using EventServer.Aggregates.Partners.Commands;
using EventServer.Aggregates.Partners.Events;
using Marten;
using org.fortium.fx.common;
using Wolverine.Http;
using Wolverine.Marten;

namespace EventServer.Controllers;

using Marten.Events;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wolverine;
using Wolverine.Attributes;

public record PartnerLoginResponse(Partner partner);

public class PartnerLoggedInEventHandler
{
    [WolverineHandler]
    public static PartnerLoggedInEvent Handle(PartnerLoggedInCommand command, IDocumentSession session, IMessageBus bus)
    {
        Log.Information("PartnerLoggedInEventHandler: Applying login event to {EmailAddress}", command.Id);
        return new PartnerLoggedInEvent(command.Id, command.LoginTime);
    }

    [WolverineHandler]
    public static PartnerLoggedOutEvent Handler(PartnerLoggedOutCommand command, IDocumentSession session, IMessageBus bus)
    {
        Log.Information("PartnerLoggedInEventHandler: Applying login event to {EmailAddress}", command.Id);
        return new PartnerLoggedOutEvent(command.Id, command.LogoutTime);
    }

}

public class PartnerController: ControllerBase
{
    [AggregateHandler]
    public static void Handler(PartnerLoggedInCommand command, IEventStream<Partner> stream)
    {
        Log.Information("Handling login command for {Id}", command.Id);

        var partner = stream.Aggregate;

        if (partner.Active)
        {
            stream.AppendOne(new PartnerLoggedInEvent(command.Id, command.LoginTime));
        }
        else
        {
            Log.Error("Inactive artner {id} trying to login", command.Id);
            throw new InvalidOperationException("Inactive artner trying to login");
        }
    }

    [WolverinePost("/partners/loggedin")]
    [ProducesResponseType(200, Type = typeof(Partner))]
    public async Task<ActionResult> GetPartners(
        [FromBody] PartnerLoggedInCommand command,
        [FromServices] IDocumentSession session,
        [FromServices] IMessageBus outbox
    )
    {
        Log.Information("Logging partner {Id} in at {time}.", command.Id, command.LoginTime);
//        outbox.Enroll(session);

        var stream = await session.Events.FetchForWriting<Partner>(command.Id);


        var result = await outbox.InvokeAsync<Partner>(command); //new PartnerLoggedInEvent(command.Id, DateTime.Now));

        await session.SaveChangesAsync();

        return (ActionResult)Results.Created($"/partner/{result.EmailAddress}", result);
    }

    [WolverinePost("/partners/loggedout")]
    public async Task GetPartners(
        [FromBody] PartnerLoggedOutCommand command,
        [FromServices] IDocumentSession session,
        [FromServices] IMessageBus outbox
    )
    {
        Log.Information("Logging partner {Id} out.", command.Id);
 //       outbox.Enroll(session);

        var stream = await session.Events.FetchForWriting<Partner>(command.Id);

        await outbox.PublishAsync(new PartnerLoggedOutEvent(command.Id, DateTime.Now));

        await session.SaveChangesAsync();
    }
}
