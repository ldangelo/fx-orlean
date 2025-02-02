
namespace EventServer.Aggregates.Users.Events {
    using Orleankka.Meta;

    public interface IUserEvent: Event;

    [Serializable, GenerateSerializer]
    public record UserCreatedEvent(string FirstName, string LastName, string EmailAddress): IUserEvent;

    [Serializable, GenerateSerializer]
    public record UserDetailsSetEvent(string emailAddress, string firstName, string lastName): IUserEvent;

    [Serializable, GenerateSerializer]
    public record VideoConferenceAddedToUserEvent(Guid? conferenceId): IUserEvent;

}
