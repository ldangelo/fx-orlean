using FluentValidation;

namespace EventServer.Aggregates.Users.Events;

public interface IUserEvent;

[Serializable]
public record UserCreatedEvent(string FirstName, string LastName, string EmailAddress) : IUserEvent;

[Serializable]
public record VideoConferenceAddedToUserEvent(string EmailAddress, Guid? conferenceId) : IUserEvent;

[Serializable]
public record UserLoggedInEvent(string EmailAddress, DateTime LoginTime): IUserEvent;

[Serializable]
public record UserLoggedOutEvent(string EmailAddress, DateTime LogoutTime): IUserEvent;
