using Orleankka.Meta;

namespace EventServer.Aggregates.Partners.Commands;

[Serializable]
[GenerateSerializer]
public record AddVideoConferenceToPartnerCommand(Guid? conferenceId) : Command;

