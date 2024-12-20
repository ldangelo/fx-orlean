using Orleankka.Meta;

namespace UI.Aggregates.Partners.Commands;

[Immutable]
[Serializable]
[GenerateSerializer]
public record AddPartnerSkillCommand(string skill) : Command
{
}