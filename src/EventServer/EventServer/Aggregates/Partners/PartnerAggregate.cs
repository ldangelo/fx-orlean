using common.Commands;
using common.PartnerConnect;
using common.Queries;
using EventServer.Aggregates.Partners.Commands;
using EventServer.Aggregates.Partners.Events;
using FluentValidation;
using Marten;
using org.fortium.commands;
using org.fortium.fx.Aggregates;
using Orleankka;
using Orleankka.Meta;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using Serilog;

namespace EventServer.Aggregates.Partners;

[MayInterleave(nameof(Interleave))]
public class PartnerAggregate : EventSourcedActor, IPartnerAggregate
{
    private readonly PartnerSnapshot _partner;

    private bool active;

    public PartnerAggregate(IDocumentStore store)
        : base(store)
    {
        _partner = new PartnerSnapshot();
    }

    protected override Task SaveState()
    {
        return Task.CompletedTask;
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
        var user = await api.GetUser(e.emailAddress, CancellationToken.None);

        active = true;
        _partner.EmailAddress = e.emailAddress;
        _partner.FirstName = e.firstName;
        _partner.LastName = e.lastName;
        _partner.PrimaryPhone = user?.PrimaryPhone!;

        Log.Information("Partner {$PartnerCreatedEvent} created", e);
    }

    private void On(PartnerSkillAddedEvent e)
    {
        _partner.Skills.Add(e.skill);
        Log.Information("Added skill {PartnerSkillAddedEvent}", e);
    }

    private void On(VideoConferenceAddedToPartnerEvent e)
    {
        Log.Information("Video confereince added {@VideoConferenceAddedToPartnerEvent}", e);
        _partner.VideoConferences.Add(e.conferenceId);
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
            yield return new PartnerCreatedEvent(Id, cmd.FirstName, cmd.LastName, cmd.EmailAddress);
        }
    }

    private IEnumerable<Event> Handle(AddVideoConferenceToPartnerCommand cmd)
    {
        if (cmd.conferenceId == null)
            throw new ArgumentException("Conference Id must be set.");

        Log.Information("Add conference {@AddVideoConferenceToPartnerCommand}", cmd);
        yield return new VideoConferenceAddedToPartnerEvent(Id, cmd.conferenceId);
    }

    private IEnumerable<Event> Handle(AddPartnerSkillCommand cmd)
    {
        if (string.IsNullOrEmpty(cmd.skill))
            throw new ArgumentException("Skill must be set.");

        Log.Information("Adding {@AddPartnerSkillCommand} to {Id}", cmd, Id);
        yield return new PartnerSkillAddedEvent(Id, cmd.skill);
    }

    private IEnumerable<Event> Handle(AddPartnerWorkExperienceCommand cmd)
    {
        Log.Information("Adding {@cmd} to {@Id}", cmd, Id);

        yield return new PartnerWorkExperienceAddedEvent(Id, cmd.WorkHistory);
    }

    private PartnerSnapshot Handle(GetPartnerDetails cmd)
    {
        return _partner;
    }

    private void CheckIsActive()
    {
        if (!active)
            throw new InvalidOperationException(Id + " item is not acctive.");
    }
}