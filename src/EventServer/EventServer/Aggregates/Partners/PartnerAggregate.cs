using common.Commands;
using common.PartnerConnect;
using common.Queries;
using FluentValidation;
using org.fortium.fx.Aggregates;
using org.fortium.fx.common;
using Orleankka;
using Orleankka.Meta;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using Serilog;
using UI.Aggregates.Partners.Commands;
using UI.Aggregates.Partners.Events;

namespace UI.Aggregates.Partners;

[MayInterleave(nameof(Interleave))]
public class PartnerAggregate : EventSourcedActor, IPartnerAggregate
{
    private bool active;
    private string EmailAddress { get; set; } = "";
    private string PhoneNumber { get; set; } = "";
    private string FirstName { get; set; } = "";
    private string LastName { get; set; } = "";
    private List<string> skills { get; } = new();
    private List<Guid?> videoConferences { get; } = new();
    private List<Event> events { get; set; } = new();

    public static bool Interleave(IInvokable req)
    {
        return req.Message() is GetPartnerDetails;
    }

    private async void On(PartnerCreatedEvent e)
    {
        var api = new PartnerConnectApi();
        var user = await api.GetUser(e.EmailAddress, CancellationToken.None);

        active = true;
        EmailAddress = e.EmailAddress;
        FirstName = e.FirstName;
        LastName = e.LastName;
        PhoneNumber = user?.PrimaryPhone!;

        Log.Information("Partner {$PartnerCreatedEvent} created", e);
    }

    private void On(PartnerSkillAddedEvent e)
    {
        skills.Add(e.skill);
        Log.Information("Added skill {PartnerSkillAddedEvent}", e);
    }

    private void On(VideoConferenceAddedToPartnerEvent e)
    {
        Log.Information("Video confereince added {@VideoConferenceAddedToPartnerEvent}", e);
        videoConferences.Add(e.ConferenceId);
    }

    private IEnumerable<Event> Handle(CreatePartnerCommand cmd)
    {
        var validator = new CreatePartnerCommandValidator();
        validator.ValidateAndThrow(cmd);

        if (active)
        {
            Log.Information("Partner {$CreatePartnerCommand} already exists", cmd);
        }
        else
        {
            Log.Information("Creating partner {$CreatePartnerCommand} ", cmd);
            yield return new PartnerCreatedEvent(cmd.FirstName, cmd.LastName, cmd.EmailAddress);
        }
    }

    private IEnumerable<Event> Handle(AddVideoConferenceToPartnerCommand cmd)
    {
        if (cmd.conferenceId == null)
            throw new ArgumentException("Conference Id must be set.");

        Log.Information("Add conference {@AddVideoConferenceToPartnerCommand}", cmd);
        yield return new VideoConferenceAddedToPartnerEvent(cmd.conferenceId);
    }

    private IEnumerable<Event> Handle(AddPartnerSkillCommand cmd)
    {
        if (string.IsNullOrEmpty(cmd.skill))
            throw new ArgumentException("Skill must be set.");

        Log.Information("Adding {@AddPartnerSkillCommand} to {Id}", cmd, Id);
        yield return new PartnerSkillAddedEvent(cmd.skill);
    }

    private PartnerSnapshot Handle(GetPartnerDetails cmd)
    {
        return new PartnerSnapshot(EmailAddress, FirstName, LastName, PhoneNumber, "", skills, videoConferences);
    }

    private void CheckIsActive()
    {
        if (!active)
            throw new InvalidOperationException(Id + " item is not acctive.");
    }
}