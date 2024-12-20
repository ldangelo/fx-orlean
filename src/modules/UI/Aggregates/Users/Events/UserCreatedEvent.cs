using Orleankka.Meta;

namespace UI.Aggregates.Users.Events;

public class UserCreatedEvent : Event
{
    public UserCreatedEvent(string firstName, string lastName, string emailAddress)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string EmailAddress { get; }
}