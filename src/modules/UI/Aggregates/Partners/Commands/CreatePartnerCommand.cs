using Orleankka.Meta;

namespace UI.Aggregates.Partners.Commands {

    [Serializable, GenerateSerializer]
    public class CreatePartnerCommand : Command {
        [Id(0)]
        public readonly string FirstName;

        [Id(1)]
        public readonly string LastName;

        [Id(2)]
        public readonly string EmailAddress;

        public CreatePartnerCommand(string FirstName, string LastName, string EmailAddress) {
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.EmailAddress = EmailAddress;
        }
    }
}
