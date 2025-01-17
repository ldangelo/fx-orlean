using Orleankka.Meta;

namespace UI.Aggregates.Partners.Commands;

[Immutable]
[GenerateSerializer]
public record SetPartnerDetalsCommand(string emailAddress, string firstName, string lastName) : Command
{
}