using Orleankka.Meta;

namespace EventServer.Aggregates.Partners.Commands;

[Immutable]
[Serializable]
[GenerateSerializer]
public record AddPartnerSkillCommand(string skill) : Command { }

