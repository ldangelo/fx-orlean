using EventServer.Aggregates.Users.Commands;
using EventServer.Aggregates.Users.Events;
using Fortium.Types;
using Serilog;

namespace EventServer.Aggregates.Users;

public class UserHandler {

    public static void Handle(CreateUserCommand command)
    {
        Log.Information("UserHandler: Applying {type} to {EmailAddress}", typeof(CreateUserCommand),command.EmailAddress);
    }

    public static void Handle(UserCreatedEvent @event, User user)
    {
        Log.Information("UserHandler: Applying {type} to {EmailAddress}", typeof(UserCreatedEvent),@event.EmailAddress);
        user.FirstName = @event.FirstName;
        user.LastName = @event.LastName;
        user.EmailAddress = @event.EmailAddress;
        user.CreateDate = DateTime.Now;
    }

    public static void Handle(VideoConferenceAddedToUserEvent @event, User user) {
        Log.Information("UserHandler: Applying {type} to {EmailAddress}", typeof(VideoConferenceAddedToUserEvent), @event.EmailAddress);
        user.VideoConferences.AddRange<Guid?>(@event.conferenceId);
        user.UpdateDate = DateTime.Now;
    }
    
    public static void Handle(UserProfileUpdatedEvent @event, User user) {
        Log.Information("UserHandler: Applying {type} to {EmailAddress}", typeof(UserProfileUpdatedEvent), @event.EmailAddress);
        user.FirstName = @event.FirstName;
        user.LastName = @event.LastName;
        user.PhoneNumber = @event.PhoneNumber;
        user.ProfilePictureUrl = @event.ProfilePictureUrl;
        user.UpdateDate = DateTime.Now;
    }
    
    public static void Handle(UserAddressUpdatedEvent @event, User user) {
        Log.Information("UserHandler: Applying {type} to {EmailAddress}", typeof(UserAddressUpdatedEvent), @event.EmailAddress);
        if (user.Address == null) {
            user.Address = new Address();
        }
        user.Address.Street1 = @event.Street1;
        user.Address.Street2 = @event.Street2;
        user.Address.City = @event.City;
        user.Address.State = @event.State;
        user.Address.ZipCode = @event.ZipCode;
        user.Address.Country = @event.Country;
        user.UpdateDate = DateTime.Now;
    }
    
    public static void Handle(UserPreferencesUpdatedEvent @event, User user) {
        Log.Information("UserHandler: Applying {type} to {EmailAddress}", typeof(UserPreferencesUpdatedEvent), @event.EmailAddress);
        if (user.Preferences == null) {
            user.Preferences = new UserPreferences();
        }
        user.Preferences.ReceiveEmailNotifications = @event.ReceiveEmailNotifications;
        user.Preferences.ReceiveSmsNotifications = @event.ReceiveSmsNotifications;
        user.Preferences.PreferredLanguage = @event.PreferredLanguage;
        user.Preferences.TimeZone = @event.TimeZone;
        user.Preferences.Theme = @event.Theme;
        user.UpdateDate = DateTime.Now;
    }
}
