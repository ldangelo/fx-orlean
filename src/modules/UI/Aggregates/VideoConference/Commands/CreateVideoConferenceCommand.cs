using FluentValidation;
using Orleankka.Meta;

namespace UI.Aggregates.VideoConference.Commands;

[Serializable]
[GenerateSerializer]
public record CreateVideoConferenceCommand(
    Guid conferenceId,
    DateTime startTime,
    DateTime endTime,
    string userId,
    string partnerId
) : Command { }

public class CreateVideoConferenceCommandValidator : AbstractValidator<CreateVideoConferenceCommand>
{
    public CreateVideoConferenceCommandValidator()
    {
        RuleFor(command => command.conferenceId)
            .NotEqual(Guid.Empty)
            .WithMessage("ConferenceId is required");
        RuleFor(command => command.startTime)
            .NotEqual(default(DateTime))
            .WithMessage("StartTime is required")
            .LessThan(command => command.endTime)
            .WithMessage("StartTime must be before EndTime")
            .GreaterThanOrEqualTo(DateTime.Now)
            .WithMessage("StartTime must be after the current date");
        RuleFor(command => command.endTime)
            .NotEqual(default(DateTime))
            .WithMessage("EndTime is required")
            .GreaterThan(command => command.startTime)
            .WithMessage("EndTime must be after StartTime");
        RuleFor(command => command.userId)
            .NotEqual(default(string))
            .WithMessage("UserId is required");
        RuleFor(command => command.partnerId)
            .NotEqual(default(string))
            .WithMessage("PartnerId is required");
    }
}
