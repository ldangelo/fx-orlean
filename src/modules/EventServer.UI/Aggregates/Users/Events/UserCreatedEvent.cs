using Orleankka.Meta;

namespace UI.Aggregates.Users.Events;

[Serializable, GenerateSerializer]
public class UserCreatedEvent : Event
{
    public UserCreatedEvent(string firstName, string lastName, string emailAddress)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }

    [Id(0)]
    public string FirstName { get; }

    [Id(1)]
    public string LastName { get; }

    [Id(2)]
    public string EmailAddress { get; }
}

