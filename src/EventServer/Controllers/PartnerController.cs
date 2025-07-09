using EventServer.Aggregates.Partners.Commands;
using EventServer.Aggregates.Partners.Events;
using Fortium.Types;
using Marten;
using Marten.Events;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace EventServer.Controllers;

public record PartnerLoginResponse(Partner partner);

public class PartnerAggregateHandler
{
    public static void Handle(PartnerLoggedInCommand command, IEventStream<Partner> stream)
    {
        Log.Information(
            "PartnerLoggedInEventHandler: Applying login event to {EmailAddress}",
            command.EmailAddress
        );
        var partner = stream.Aggregate;

        if (partner != null && partner.IsLoggedIn())
        {
            Log.Debug($"Partner {command.EmailAddress} is already logged in.");
            throw new InvalidOperationException(
                $"Partner {command.EmailAddress} is already logged in."
            );
        }

        stream.AppendOne(new PartnerLoggedInEvent(command.EmailAddress, command.LoginTime));
    }

    public static PartnerLoggedOutEvent Handler(
        PartnerLoggedOutCommand command,
        IDocumentSession session,
        IMessageBus bus
    )
    {
        Log.Information(
            "PartnerLoggedInEventHandler: Applying login event to {EmailAddress}",
            command.EmailAddress
        );
        return new PartnerLoggedOutEvent(command.EmailAddress, command.LogoutTime);
    }
}

public static class PartnerController
{
    [WolverineGet("/partners/{emailAddress}")]
    public static Partner GetPartner([Document("emailAddress")] Partner partner)
    {
        Log.Information("Getting partner {emailAddress}.", partner.EmailAddress);

        return partner;
    }

    [WolverinePost("/partners")]
    public static (CreationResponse, IStartStream) CreatePartners(CreatePartnerCommand command)
    {
        Log.Information("Creating partner {Id}.", command.EmailAddress);

        var startStream = MartenOps.StartStream<Partner>(
            command.EmailAddress,
            new PartnerCreatedEvent(command.FirstName, command.LastName, command.EmailAddress)
        );

        return (new CreationResponse($"/partners/{command.EmailAddress}"), startStream);
    }

    [WolverinePost("/partners/loggedin/{partnerId}")]
    [EmptyResponse]
    public static PartnerLoggedInEvent GetPartners(
        [FromBody] PartnerLoggedInCommand command,
        [Aggregate("partnerId")] Partner partner
    )
    {
        Log.Information(
            "Logging partner {Id} in at {time}.",
            command.EmailAddress,
            command.LoginTime
        );

        return new PartnerLoggedInEvent(command.EmailAddress, command.LoginTime);
    }

    [WolverinePost("/partners/loggedout/{partnerId}")]
    [EmptyResponse]
    public static PartnerLoggedOutEvent LogOutPartners(
        [FromBody] PartnerLoggedOutCommand command,
        [Aggregate("partnerId")] Partner partner
    )
    {
        Log.Information("Logging partner {Id} out.", command.EmailAddress);

        return new PartnerLoggedOutEvent(command.EmailAddress, command.LogoutTime);
    }

    [WolverinePost("/partners/bio/{partnerId}")]
    [EmptyResponse]
    public static PartnerBioUpdatedEvent UpdatePartnerBio(
        [FromBody] SetPartnerBioCommand command,
        [Aggregate("partnerId")] Partner partner
    )
    {
        Log.Information("Updating partner {Id} bio {bio}", command.EmailAddress, command.Bio);

        return new PartnerBioUpdatedEvent(command.EmailAddress, command.Bio);
    }

    [WolverinePost("/partners/skills/{partnerId}")]
    [EmptyResponse]
    public static PartnerSkillAddedEvent UpdatePartnerSkills(
        [FromBody] AddPartnerSkillCommand command,
        [Aggregate("partnerId")] Partner partner
    )
    {
        Log.Information(
            "Updating partner {Id} skills {skills}",
            command.EmailAddress,
            command.Skills
        );

        return new PartnerSkillAddedEvent(command.EmailAddress, command.Skills);
    }

    [WolverinePost("/partners/primaryphone/{partnerId}")]
    [EmptyResponse]
    public static SetPartnerPrimaryPhoneEvent UpdatePartnerPrimaryPhone(
        [FromBody] SetPartnerPrimaryPhoneCommand command,
        [Aggregate("partnerId")] Partner partner
    )
    {
        Log.Information(
            "Updating partner {Id} phone {phone}",
            command.EmailAddress,
            command.PrimaryPhone
        );

        return new SetPartnerPrimaryPhoneEvent(command.EmailAddress, command.PrimaryPhone);
    }

    [WolverinePost("/partners/photourl/{partnerId}")]
    [EmptyResponse]
    public static SetPartnerPhotoUrlEvent UpdatePartnerPrimaryPhone(
        [FromBody] SetPartnerPhotoUrlCommand command,
        [Aggregate("partnerId")] Partner partner
    )
    {
        Log.Information("Updating partner {Id} url {url}", command.EmailAddress, command.PhotoUrl);

        return new SetPartnerPhotoUrlEvent(command.EmailAddress, command.PhotoUrl);
    }
}
