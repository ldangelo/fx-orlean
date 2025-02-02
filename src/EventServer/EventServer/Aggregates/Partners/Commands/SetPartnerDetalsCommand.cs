using Orleankka.Meta;

namespace EventServer.Aggregates.Partners.Commands;

[Immutable]
[GenerateSerializer]
public record SetPartnerDetalsCommand(string emailAddress, string firstName, string lastName)
    : Command { }

