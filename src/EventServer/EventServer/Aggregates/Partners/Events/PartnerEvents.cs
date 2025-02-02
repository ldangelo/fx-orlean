using org.fortium.fx.common;
using Orleankka.Meta;

namespace EventServer.Aggregates.Partners.Events;

public interface IPartnerEvent : Event;

[Serializable]
[GenerateSerializer]
public record PartnerCreatedEvent(string firstName, string lastName, string emailAddress)
    : IPartnerEvent;

[Serializable]
[GenerateSerializer]
public record PartnerWorkExperienceAddedEvent(string partnerId, WorkHistory workHistory) : IPartnerEvent;

[Serializable]
[GenerateSerializer]
public record VideoConferenceAddedToPartnerEvent(Guid? conferenceId) : IPartnerEvent;

[Serializable]
[GenerateSerializer]
public record PartnerSkillAddedEvent(string skill) : IPartnerEvent;