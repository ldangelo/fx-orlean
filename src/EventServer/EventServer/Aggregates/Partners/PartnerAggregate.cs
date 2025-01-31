using common.Commands;
using common.PartnerConnect;
using common.Queries;
using FluentValidation;
using Marten;
using org.fortium.commands;
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
    private readonly IPersistentState<PartnerSnapshot> _partnerSnapshot;

    private bool active;

    public PartnerAggregate(
        [PersistentState("partners", "partners")]
        IPersistentState<PartnerSnapshot> partnerSnapshot,
        DocumentStore store) : base(store)
    {
        _partnerSnapshot = partnerSnapshot;
    }

    protected override async Task SaveState()
    {
        await _partnerSnapshot.WriteStateAsync();
    }

    public override StreamRef<IEventEnvelope?> GetStream(string id)
    {
        var stream = System.StreamOf<IEventEnvelope?>("partners", id);
        return stream;
    }

    public static bool Interleave(IInvokable req)
    {
        return req.Message() is GetPartnerDetails;
    }

    private async void On(PartnerCreatedEvent e)
    {
        var api = new PartnerConnectApi();
        var user = await api.GetUser(e.EmailAddress, CancellationToken.None);

        active = true;
        _partnerSnapshot.State.EmailAddress = e.EmailAddress;
        _partnerSnapshot.State.FirstName = e.FirstName;
        _partnerSnapshot.State.LastName = e.LastName;
        _partnerSnapshot.State.PrimaryPhone = user?.PrimaryPhone!;

        Log.Information("Partner {$PartnerCreatedEvent} created", e);
    }

    private void On(PartnerSkillAddedEvent e)
    {
        _partnerSnapshot.State.Skills.Add(e.skill);
        Log.Information("Added skill {PartnerSkillAddedEvent}", e);
    }

    private void On(VideoConferenceAddedToPartnerEvent e)
    {
        Log.Information("Video confereince added {@VideoConferenceAddedToPartnerEvent}", e);
        _partnerSnapshot.State.VideoConferences.Add(e.ConferenceId);
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

    private IEnumerable<Event> Handle(AddPartnerWorkExperienceCommand cmd)
    {
        Log.Information("Adding {@cmd} to {@Id}", cmd, Id);

        yield return new PartnerWorkExperienceAddedEvent(Id, cmd.WorkHistory);
    }

    private PartnerSnapshot Handle(GetPartnerDetails cmd)
    {
        return _partnerSnapshot.State;
    }

    private void CheckIsActive()
    {
        if (!active)
            throw new InvalidOperationException(Id + " item is not acctive.");
    }
}