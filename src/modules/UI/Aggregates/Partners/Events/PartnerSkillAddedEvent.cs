using Orleankka.Meta;

namespace UI.Aggregates.Partners.Events;

public record PartnerSkillAddedEvent(string skill) : Event
{
}