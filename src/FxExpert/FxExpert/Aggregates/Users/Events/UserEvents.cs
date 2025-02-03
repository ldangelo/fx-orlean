namespace EventServer.Aggregates.Users.Events;

public interface IUserEvent;

[Serializable]
public record UserCreatedEvent(
    string userId,
    string FirstName,
    string LastName,
    string EmailAddress
) : IUserEvent;

[Serializable]
public record UserDetailsSetEvent(
    string userId,
    string emailAddress,
    string firstName,
    string lastName
) : IUserEvent;

[Serializable]
public record VideoConferenceAddedToUserEvent(string userId, Guid? conferenceId) : IUserEvent;