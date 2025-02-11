using Marten.Schema;

namespace EventServer.Aggregates.Partners.Commands;

public interface IPartnerCommand
{
}

[Serializable]
public record CreatePartnerCommand(String FirstName, String LastName, String EmailAddress) : IPartnerCommand;

[Serializable]
public record PartnerLoggedInCommand([property: Identity]  string EmailAddress,DateTime LoginTime) : IPartnerCommand;

[Serializable]
public record PartnerLoggedOutCommand(string EmailAddress, DateTime LogoutTime) : IPartnerCommand;

[Serializable]
public record GetPartnerCommand(string EmailAddress) : IPartnerCommand;

[Serializable]
public record AddPartnerSkillCommand(string EmailAddress,string Skill) : IPartnerCommand;

[Serializable]
public record SetPartnerDetalsCommand(string EmailAddress, string FirstName, string LastName) : IPartnerCommand;

[Serializable]
public record AddVideoConferenceToPartnerCommand(string EmailAddress,Guid? ConferenceId) : IPartnerCommand;
