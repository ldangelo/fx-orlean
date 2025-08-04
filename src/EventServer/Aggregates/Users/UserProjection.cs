using EventServer.Aggregates.Users.Events;
using Fortium.Types;
using Marten;
using Marten.Events.Aggregation;
using Serilog;

namespace EventServer.Aggregates.Users;

public class UserProjection : SingleStreamProjection<User, string>
{
    public static User Apply(UserCreatedEvent @event, User user)
    {
        Log.Information("User Projection: Applying {type} to {EmailAddress}", typeof(UserCreatedEvent),@event.EmailAddress);
        user.FirstName = @event.FirstName;
        user.LastName = @event.LastName;
        user.EmailAddress = @event.EmailAddress;
        user.CreateDate = DateTime.Now;
        user.Active = false;

        return user;
    }

    public static User Apply(VideoConferenceAddedToUserEvent @event, User user)
    {
        Log.Information("User Projection: Applying {type} to {EmailAddress}", typeof(UserCreatedEvent),@event.EmailAddress);
        user.VideoConferences.AddRange(@event.conferenceId);
        user.UpdateDate = DateTime.Now;
        user.Active = false;

        return user;
    }

    public static User Apply(UserLoggedInEvent @event, User user)
    {
        Log.Information("User Projection: Applying {type} to {EmailAddress}", typeof(UserCreatedEvent),@event.EmailAddress);
        user.Active = true;
        user.LoggedIn = true;
        user.LoginDate = @event.LoginTime;
        user.UpdateDate = DateTime.Now;

        return user;
    }

    public static User Apply(UserLoggedOutEvent @event, User user)
    {
        user.LoggedIn = false;
        user.LogoffDate = @event.LogoutTime;
        user.UpdateDate = DateTime.Now;

        return user;
    }
    
    public static User Apply(UserProfileUpdatedEvent @event, User user)
    {
        Log.Information("User Projection: Applying {type} to {EmailAddress}", typeof(UserProfileUpdatedEvent), @event.EmailAddress);
        user.FirstName = @event.FirstName;
        user.LastName = @event.LastName;
        user.PhoneNumber = @event.PhoneNumber;
        user.ProfilePictureUrl = @event.ProfilePictureUrl;
        user.UpdateDate = DateTime.Now;
        
        return user;
    }
    
    public static User Apply(UserAddressUpdatedEvent @event, User user)
    {
        Log.Information("User Projection: Applying {type} to {EmailAddress}", typeof(UserAddressUpdatedEvent), @event.EmailAddress);
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
        
        return user;
    }
    
    public static User Apply(UserPreferencesUpdatedEvent @event, User user)
    {
        Log.Information("User Projection: Applying {type} to {EmailAddress}", typeof(UserPreferencesUpdatedEvent), @event.EmailAddress);
        if (user.Preferences == null) {
            user.Preferences = new UserPreferences();
        }
        user.Preferences.ReceiveEmailNotifications = @event.ReceiveEmailNotifications;
        user.Preferences.ReceiveSmsNotifications = @event.ReceiveSmsNotifications;
        user.Preferences.PreferredLanguage = @event.PreferredLanguage;
        user.Preferences.TimeZone = @event.TimeZone;
        user.Preferences.Theme = @event.Theme;
        user.UpdateDate = DateTime.Now;
        
        return user;
    }
    
    public static User Apply(UserThemeUpdatedEvent @event, User user)
    {
        Log.Information("User Projection: Applying {type} to {EmailAddress}", typeof(UserThemeUpdatedEvent), @event.EmailAddress);
        if (user.Preferences == null) {
            user.Preferences = new UserPreferences();
        }
        user.Preferences.Theme = @event.Theme;
        user.UpdateDate = DateTime.Now;
        
        return user;
    }
}
