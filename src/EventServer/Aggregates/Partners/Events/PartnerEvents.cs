using Fortium.Types;

namespace EventServer.Aggregates.Partners.Events;

public interface IPartnerEvent;

[Serializable]
public record PartnerLoggedInEvent(string EmailAddress,DateTime LoginTime);

[Serializable]
public record PartnerLoggedOutEvent(string EmailAddress,DateTime LogoutTime);

[Serializable]
public record PartnerCreatedEvent(
    string FirstName,
    string LastName,
    string EmailAddress
) : IPartnerEvent;

[Serializable]
public record PartnerWorkExperienceAddedEvent(string EmailAddress,WorkHistory WorkHistory)
    : IPartnerEvent;

[Serializable]
public record VideoConferenceAddedToPartnerEvent(string EmailAddress,Guid? ConferenceId)
    : IPartnerEvent;

[Serializable]
public record PartnerSkillAddedEvent(string EmailAddress,string Skill) : IPartnerEvent;

[Serializable]
public record PartnerBioUpdatedEvent(string EmailAddress,string Bio) : IPartnerEvent;
