using org.fortium.fx.common;

namespace EventServer.Aggregates.Partners.Events;

public interface IPartnerEvent;

[Serializable]
public record PartnerLoggedInEvent(string partnerId, DateTime loginTime);

[Serializable]
public record PartnerLoggedOutEvent(string partnerId, DateTime logoutTime);

[Serializable]
public record PartnerCreatedEvent(
    string partnerId,
    string firstName,
    string lastName,
    string emailAddress
) : IPartnerEvent;

[Serializable]
public record PartnerWorkExperienceAddedEvent(string partnerId, WorkHistory workHistory)
    : IPartnerEvent;

[Serializable]
public record VideoConferenceAddedToPartnerEvent(string partnerId, Guid? conferenceId)
    : IPartnerEvent;

[Serializable]
public record PartnerSkillAddedEvent(string partnerId, string skill) : IPartnerEvent;

[Serializable]
public record PartnerBioUpdatedEvent(string partnerId, string bio) : IPartnerEvent;

