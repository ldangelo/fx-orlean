namespace EventServer.Aggregates.Partners.Commands;

public interface IPartnerCommand
{
}

[Serializable]
public record CreatePartnerCommand(string Id, String FirstName, String LastName, String EmailAddress) : IPartnerCommand;

[Serializable]
public record PartnerLoggedInCommand(string Id, DateTime LoginTime) : IPartnerCommand;

[Serializable]
public record PartnerLoggedOutCommand(string Id, DateTime LogoutTime) : IPartnerCommand;

[Serializable]
public record GetPartnerCommand(string Id) : IPartnerCommand;

[Serializable]
public record AddPartnerSkillCommand(string skill) : IPartnerCommand;

[Serializable]
public record SetPartnerDetalsCommand(string emailAddress, string firstName, string lastName) : IPartnerCommand;

[Serializable]
public record AddVideoConferenceToPartnerCommand(Guid? conferenceId) : IPartnerCommand;
