using Orleankka.Meta;

namespace UI.Aggregates.Partners.Events;

[Serializable, GenerateSerializer]
public record PartnerSkillAddedEvent(string skill) : Event { }

