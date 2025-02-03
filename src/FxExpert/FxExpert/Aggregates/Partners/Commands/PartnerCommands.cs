namespace EventServer.Aggregates.Partners.Commands;

public interface IPartnerCommand
{
}

[Serializable]
public record GetPartnerCommand(string Id) : IPartnerCommand;

[Serializable]
public record AddPartnerSkillCommand(string skill) : IPartnerCommand;

[Serializable]
public record SetPartnerDetalsCommand(string emailAddress, string firstName, string lastName) : IPartnerCommand;

[Serializable]
public record AddVideoConferenceToPartnerCommand(Guid? conferenceId) : IPartnerCommand;