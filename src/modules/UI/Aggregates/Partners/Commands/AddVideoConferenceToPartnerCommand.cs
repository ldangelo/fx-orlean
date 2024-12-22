using Orleankka.Meta;

namespace UI.Aggregates.Partners.Commands;

[Serializable]
[GenerateSerializer]
public record AddVideoConferenceToPartnerCommand(Guid? conferenceId) : Command;