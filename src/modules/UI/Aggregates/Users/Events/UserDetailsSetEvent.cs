using Whaally.Domain .Abstractions;
namespace UI.Grains.Users.Commands;

public class UserDetailsSetEvent : IEvent
{
    public string emailAddress { get; set; } = "";
    public string firstName { get; set; } = "";
    public string lastName { get; set; } = "";
    public UserDetailsSetEvent(string commandEmailAddress, string commandFirstName, string commandLastName)
    {
        emailAddress = commandEmailAddress;
        firstName = commandFirstName;
        lastName = commandLastName;
    }
}

public class UserDetailsSetEventHandler : IEventHandler<UserAggregate, UserDetailsSetEvent>
{
    public UserAggregate Apply(IEventHandlerContext<UserAggregate> context, UserDetailsSetEvent @event)
    {
        context.Aggregate.FirstName = @event.firstName;
        context.Aggregate.LastName = @event.lastName;
        context.Aggregate.Email = @event.emailAddress;
        
        return context.Aggregate;
    }
}
