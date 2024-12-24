using org.fortium.fx.Aggregates;
using Orleankka;
using Orleankka.Meta;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using Serilog;
using UI.Aggregates.Partners.Commands;
using UI.Aggregates.Partners.Events;
using UI.Aggregates.VideoConference.Events;

namespace UI.Aggregates.Partners;

public interface IPartnerAggregate : IActorGrain, IGrainWithStringKey { }

[Serializable]
[GenerateSerializer]
public class GetPartnerDetails : Query<PartnerAggregate, PartnerSnapshot> { }

[MayInterleave(nameof(Interleave))]
public class PartnerAggregate : EventSourcedActor, IPartnerAggregate
{
    private bool active;
    private string EmailAddress { get; } = "";
    private string FirstName { get; } = "";
    private string LastName { get; } = "";
    private List<string> skills { get; } = new();
    private List<Guid?> videoConferences { get; } = new();
    private List<Event> events { get; set; } = new();

    public static bool Interleave(IInvokable req)
    {
        return req.Message() is GetPartnerDetails;
    }

    private void On(PartnerCreatedEvent e)
    {
        active = true;
        Log.Information("Partner {$PartnerCreatedEvent} created" ,e);
    }

    private void On(PartnerSkillAddedEvent e)
    {
        skills.Add(e.skill);
        Log.Information("Added skill {PartnerSkillAddedEvent}",e);
    }

    private void On(VideoConferenceAddedToPartnerEvent e)
    {
        Log.Information("Video confereince added {@VideoConferenceAddedToPartnerEvent}",e );
        videoConferences.Add(e.ConferenceId);
    }

    private IEnumerable<Event> Handle(CreatePartnerCommand cmd)
    {
        // TODO: Change to fluid validator
        if (string.IsNullOrEmpty(cmd.FirstName))
            throw new ArgumentException("Partner first name is required.");
        if (string.IsNullOrEmpty(cmd.LastName))
            throw new ArgumentException("Partner last name is required.");
        if (string.IsNullOrEmpty(cmd.EmailAddress))
            throw new ArgumentException("Partner email address is required.");

        if (active) {
            Log.Information("Partner {$CreatePartnerCommand} already exists",cmd);
        } else {
            Log.Information("Creating partner {$CreatePartnerCommand} ",cmd);
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

        Log.Information("Adding {@AddPartnerSkillCommand} to {Id}",cmd,Id);
        yield return new PartnerSkillAddedEvent(cmd.skill);
    }

    private PartnerSnapshot Handle(GetPartnerDetails cmd)
    {
        return new PartnerSnapshot(EmailAddress, FirstName, LastName, skills, videoConferences);
    }

    private void CheckIsActive()
    {
        if (!active)
            throw new InvalidOperationException(Id + " item is not acctive.");
    }
}
