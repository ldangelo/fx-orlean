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
using Wolverine.Attributes;

public record PartnerLoginResponse(string partnerId, DateTime lastLoginTime);

public class PartnerLoggedInEventHandler
{
    [WolverineHandler]
    public Task<IEvent> Handler(PartnerLoggedInEvent loggedInEvent, IEventStream<Partner> stream)
    {
        Log.Information("Applying login event to {EmailAddress}", loggedInEvent.partnerId);
        throw new Exception("error");
    }
}

public class PartnerController
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
    public async Task GetPartners(
        [FromBody] PartnerLoggedInCommand command,
        [FromServices] IDocumentSession session,
        [FromServices] IMartenOutbox outbox
    )
    {
        outbox.Enroll(session);

        var stream = await session.Events.FetchForWriting<Partner>(command.Id);

        await outbox.PublishAsync(new PartnerLoggedInEvent(command.Id, DateTime.Now));

        await session.SaveChangesAsync();
    }
}
