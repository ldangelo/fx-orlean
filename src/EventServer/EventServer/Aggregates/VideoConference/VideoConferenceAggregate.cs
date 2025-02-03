using EventServer.Aggregates.VideoConference.Commands;
using EventServer.Aggregates.VideoConference.Events;
using FluentValidation;
using Marten;
using Orleankka;
using Orleankka.Meta;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using Serilog;

namespace EventServer.Aggregates.VideoConference;

[Alias("UI.Aggregates.VideoConference.IVideoConferenceAggregate")]
public interface IVideoConferenceAggregate : IActorGrain, IGrainWithStringKey
{
}

[Serializable]
[GenerateSerializer]
public class GetVideoConferenceDetails
    : Query<VideoConferenceAggregate, VideoConferenceSnapshot>
{
}

[MayInterleave(nameof(Interleave))]
public class VideoConferenceAggregate : EventSourcedActor, IVideoConferenceAggregate
{
    private DateTime _conferenceEndTime;
    private Guid _conferenceId;
    private DateTime _conferenceStartTime;
    private string? _partnerId;
    private string? _userId;

    public VideoConferenceAggregate(IDocumentStore eventStore)
        : base(eventStore)
    {
    }

    public override StreamRef<IEventEnvelope?> GetStream(string id)
    {
        var stream = System.StreamOf<IEventEnvelope?>("videoconferences", id);
        return stream;
    }

    private async void On(VideoConferenceCreatedEvent e)
    {
        _conferenceId = e.ConferenceId;
        _conferenceStartTime = e.StartTime;
        _conferenceEndTime = e.EndTime;
        _userId = e.UserId;
        _partnerId = e.PartnerId;

        /*
         This does not work.  It causes an eventstream error with MartinDB (LAD)
        //
        // Tell the User too add the Conference
        var user = this.System.ActorOf<IUserAggregate>(e.UserId);
        await user.Tell(new AddVideoConferenceToUserCommand(e.ConferenceId));

        //
        // Tell the partner too add the Conference
        var partner = this.System.ActorOf<IPartnerAggregate>(e.PartnerId);
        await partner.Tell(new AddVideoConferenceToPartnerCommand(e.ConferenceId));
        */
    }

    public static bool Interleave(IInvokable req)
    {
        return req.Message() is GetVideoConferenceDetails;
    }

    private IEnumerable<Event> Handle(CreateVideoConferenceCommand command)
    {
        Log.Information("Creating VideoConference: " + command.conferenceId);
        var validator = new CreateVideoConferenceCommandValidator();
        validator.ValidateAndThrow(command);

        yield return new VideoConferenceCreatedEvent(
            command.conferenceId,
            command.startTime,
            command.endTime,
            command.userId,
            command.partnerId
        );
    }

    protected override Task SaveState()
    {
        throw new NotImplementedException();
    }
}