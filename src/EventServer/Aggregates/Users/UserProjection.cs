using EventServer.Aggregates.Users.Events;
using Fortium.Types;
using Marten;
using Marten.Events.Aggregation;
using Serilog;

namespace EventServer.Aggregates.Users;

public class UserProjection : SingleStreamProjection<User>
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
}
