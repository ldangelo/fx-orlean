using Orleankka.Meta;

namespace UI.Grains.Users.Commands;

public class UserDetailsSetEvent : Event
{
    public UserDetailsSetEvent(string commandEmailAddress, string commandFirstName, string commandLastName)
    {
        emailAddress = commandEmailAddress;
        firstName = commandFirstName;
        lastName = commandLastName;
    }

    public string emailAddress { get; set; } = "";
    public string firstName { get; set; } = "";
    public string lastName { get; set; } = "";
}