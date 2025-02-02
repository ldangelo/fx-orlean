using FluentValidation;
using Orleankka.Meta;

namespace EventServer.Aggregates.Users.Commands;

[Serializable]
[GenerateSerializer]
public class AddVideoConferenceToUserCommand : Command
{
    public AddVideoConferenceToUserCommand(Guid conferenceId)
    {
        ConferenceId = conferenceId;
    }

    [Id(0)]
    public Guid? ConferenceId { get; }
}

public class AddVideoConferenceToUserCommandValidator
    : AbstractValidator<AddVideoConferenceToUserCommand>
{
    public AddVideoConferenceToUserCommandValidator()
    {
        RuleFor(command => command.ConferenceId)
            .NotNull()
            .NotNull()
            .WithMessage("Conference Id is required.");
    }
}
