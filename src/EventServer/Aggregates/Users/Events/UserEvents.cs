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

[Serializable]
public record UserProfileUpdatedEvent(
    string EmailAddress, 
    string? FirstName, 
    string? LastName, 
    string? PhoneNumber, 
    string? ProfilePictureUrl): IUserEvent;

[Serializable]
public record UserAddressUpdatedEvent(
    string EmailAddress,
    string? Street1,
    string? Street2,
    string? City,
    string? State,
    string? ZipCode,
    string? Country): IUserEvent;

[Serializable]
public record UserPreferencesUpdatedEvent(
    string EmailAddress,
    bool ReceiveEmailNotifications,
    bool ReceiveSmsNotifications,
    string? PreferredLanguage,
    string? TimeZone,
    string? Theme): IUserEvent;
