using FluentValidation;

namespace EventServer.Aggregates.VideoConference.Commands;

public interface IVideoConferenceCommand
{
}

[Serializable]
public record CreateVideoConferenceCommand(
    Guid ConferenceId,
    DateTime StartTime,
    DateTime EndTime,
    string UserId,
    string PartnerId
) : IVideoConferenceCommand
{
}

public class CreateVideoConferenceCommandValidator : AbstractValidator<CreateVideoConferenceCommand>
{
    public CreateVideoConferenceCommandValidator()
    {
        RuleFor(command => command.ConferenceId)
            .NotEqual(Guid.Empty)
            .WithMessage("ConferenceId is required");
        RuleFor(command => command.StartTime)
            .NotEqual(default(DateTime))
            .WithMessage("StartTime is required")
            .LessThan(command => command.EndTime)
            .WithMessage("StartTime must be before EndTime");
        RuleFor(command => command.EndTime)
            .NotEqual(default(DateTime))
            .WithMessage("EndTime is required")
            .GreaterThan(command => command.StartTime)
            .WithMessage("EndTime must be after StartTime");
        RuleFor(command => command.UserId)
            .NotEqual(default(string))
            .WithMessage("UserId is required");
        RuleFor(command => command.PartnerId)
            .NotEqual(default(string))
            .WithMessage("PartnerId is required");
    }
}
