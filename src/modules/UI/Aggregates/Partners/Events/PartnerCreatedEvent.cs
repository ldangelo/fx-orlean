using Orleankka.Meta;

namespace UI.Aggregates.Partners.Events;

public class PartnerCreatedEvent : Event
{
    public string EmailAddress;
    private string FirstName;
    private string LastName;

    public PartnerCreatedEvent(string firstName, string lastName, string emailAddress)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }
}