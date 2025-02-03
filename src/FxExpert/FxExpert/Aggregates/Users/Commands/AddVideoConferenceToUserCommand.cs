using FluentValidation;

namespace EventServer.Aggregates.Users.Commands;

[Serializable]
public class AddVideoConferenceToUserCommand : IUserCommand
{
    public AddVideoConferenceToUserCommand(Guid conferenceId)
    {
        ConferenceId = conferenceId;
    }

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