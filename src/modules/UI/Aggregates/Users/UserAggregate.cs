using org.fortium.fx.Aggregates;
using Orleankka;
using Orleankka.Meta;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using UI.Aggregates.Users.Commands;
using UI.Aggregates.Users.Events;

namespace UI.Aggregates.Users;

public interface IUserAggregate : IActorGrain, IGrainWithStringKey
{
}

[Serializable]
[GenerateSerializer]
public class GetUserDetails : Query<UserAggregate, UserSnapshot>
{
}

[GenerateSerializer]
[Alias(nameof(UserAggregate))]
[MayInterleave(nameof(Interleave))]
public class UserAggregate : EventSourcedActor, IUserAggregate
{
    private bool active;
    private string FirstName { get; set; } = string.Empty;
    private string LastName { get; set; } = string.Empty;
    private string Email { get; set; } = string.Empty;
    private List<Guid?> VideoConferences { get; } = new();

    public static bool Interleave(IInvokable req)
    {
        return req.Message() is GetUserDetails;
    }

    private void On(UserCreatedEvent e)
    {
        active = true;
        FirstName = e.FirstName;
        LastName = e.LastName;
        Email = e.EmailAddress;
    }

    private void On(VideoConferenceAddedToUserEvent e)
    {
        VideoConferences.Add(e.ConferenceId);
    }

    private IEnumerable<Event> Handle(CreateUserCommand cmd)
    {
        yield return new UserCreatedEvent(cmd.FirstName, cmd.LastName, cmd.EmailAddress);
    }

    private IEnumerable<Event> Handle(AddVideoConferenceToUserCommand cmd)
    {
        yield return new VideoConferenceAddedToUserEvent(cmd.ConferenceId);
    }

    private UserSnapshot Handle(GetUserDetails query)
    {
        return new UserSnapshot(Email, FirstName, LastName, VideoConferences);
    }
}
