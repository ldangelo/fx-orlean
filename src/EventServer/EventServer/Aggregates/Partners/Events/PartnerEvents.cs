using org.fortium.fx.common;
using Orleankka.Meta;

namespace EventServer.Aggregates.Partners.Events;

public interface IPartnerEvent : Event;

[Serializable]
[GenerateSerializer]
public record PartnerLoggedInEvent(string partnerId, DateTime loginTime);

[Serializable]
[GenerateSerializer]
public record PartnerLoggedOutEvent(string partnerId, DateTime loginTime);

[Serializable]
[GenerateSerializer]
public record PartnerCreatedEvent(
    string partnerId,
    string firstName,
    string lastName,
    string emailAddress
) : IPartnerEvent;

[Serializable]
[GenerateSerializer]
public record PartnerWorkExperienceAddedEvent(string partnerId, WorkHistory workHistory)
    : IPartnerEvent;

[Serializable]
[GenerateSerializer]
public record VideoConferenceAddedToPartnerEvent(string partnerId, Guid? conferenceId)
    : IPartnerEvent;

[Serializable]
[GenerateSerializer]
public record PartnerSkillAddedEvent(string partnerId, string skill) : IPartnerEvent;

[Serializable]
[GenerateSerializer]
public record PartnerBioUpdatedEvent(string partnerId, string bio) : IPartnerEvent;

