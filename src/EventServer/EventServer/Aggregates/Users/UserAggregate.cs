using EventServer.Aggregates.Users.Commands;
using EventServer.Aggregates.Users.Events;
using FluentValidation;
using Marten;
using Orleankka;
using Orleankka.Meta;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using Serilog;

namespace EventServer.Aggregates.Users;

public interface IUserAggregate : IActorGrain, IGrainWithStringKey { }

[Serializable]
[GenerateSerializer]
public class GetUserDetails : Query<UserAggregate, UserSnapshot> { }

[GenerateSerializer]
[Alias(nameof(UserAggregate))]
public class UserAggregate : EventSourcedActor, IUserAggregate
{
    public UserAggregate(IDocumentStore eventStore)
        : base(eventStore) { }

    private string FirstName { get; set; } = string.Empty;
    private string LastName { get; set; } = string.Empty;
    private string Email { get; set; } = string.Empty;
    private List<Guid?> VideoConferences { get; } = new();

    public override StreamRef<IEventEnvelope?> GetStream(string id)
    {
        var stream = System.StreamOf<IEventEnvelope?>("users", id);
        return stream;
    }

    public static bool Interleave(IInvokable req)
    {
        return req.Message() is GetUserDetails;
    }

    private void On(UserCreatedEvent e)
    {
        FirstName = e.FirstName;
        LastName = e.LastName;
        Email = e.EmailAddress;
        Log.Information("User {@UserCreatedEvent} created.", e);
    }

    private void On(VideoConferenceAddedToUserEvent e)
    {
        Log.Information(
            "video conference: {@VideoConferenceAddedToUserEvent} added to user {Id}",
            e,
            Id
        );
        VideoConferences.Add(e.conferenceId);
    }

    private IEnumerable<Event> Handle(CreateUserCommand cmd)
    {
        Log.Information("Creating user {$cmd}", cmd);
        var validator = new CreateUserCommandValdiator();

        validator.ValidateAndThrow(cmd);

        yield return new UserCreatedEvent(Id, cmd.FirstName, cmd.LastName, cmd.EmailAddress);
    }

    private IEnumerable<Event> Handle(AddVideoConferenceToUserCommand cmd)
    {
        Log.Information(
            "Adding video conference {@AddVideoConferenceToUserCommand} to user {Id}",
            cmd,
            Id
        );

        var validator = new AddVideoConferenceToUserCommandValidator();
        validator.ValidateAndThrow(cmd);

        yield return new VideoConferenceAddedToUserEvent(Id, cmd.ConferenceId);
    }

    private UserSnapshot Handle(GetUserDetails query)
    {
        return new UserSnapshot(Email, FirstName, LastName, VideoConferences);
    }

    protected override Task SaveState()
    {
        throw new NotImplementedException();
    }
}

