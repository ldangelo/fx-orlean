using Orleankka.Meta;

namespace UI.Aggregates.Users.Commands;

[GenerateSerializer(IncludePrimaryConstructorParameters = true)]
public class CreateUserCommand : Command
{
    [Id(2)] public readonly string EmailAddress;
    [Id(0)] public readonly string FirstName;
    [Id(1)] public readonly string LastName;

    public CreateUserCommand(string FirstName, string LastName, string EmailAddress)
    {
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.EmailAddress = EmailAddress;
    }

    public override String ToString()
        {
            return "FirstName: " + FirstName + " LastName: " + LastName + " Email: " + EmailAddress;
        }

}
