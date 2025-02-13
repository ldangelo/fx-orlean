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
}
