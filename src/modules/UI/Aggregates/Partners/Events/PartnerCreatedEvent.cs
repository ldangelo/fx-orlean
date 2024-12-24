using System.Diagnostics;
using Orleankka.Meta;

namespace UI.Aggregates.Partners.Events;

[Serializable,GenerateSerializer]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class PartnerCreatedEvent : Event
{
    public string EmailAddress;
    public string FirstName;
    public string LastName;

    public PartnerCreatedEvent(string firstName, string lastName, string emailAddress)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }

    public override String ToString() {
        return $"FirstName: {FirstName}, LastName: {LastName}, Email: {EmailAddress}";
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
