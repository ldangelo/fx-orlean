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
}
