using FluentValidation;
using Fortium.Types;

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
    string PartnerId,
    RateInformation RateInformation
) : IVideoConferenceCommand
{
}

/// <summary>
/// Command to complete a booking with all integrations (payment, calendar, notifications)
/// </summary>
[Serializable]
public record CompleteBookingCommand(
    Guid BookingId,
    Guid ConferenceId,
    string ClientEmail,
    string PartnerEmail,
    DateTime StartTime,
    DateTime EndTime,
    string ConsultationTopic,
    string ClientProblemDescription,
    string PaymentIntentId,
    decimal SessionFee = 800.00m
) : IVideoConferenceCommand;

/// <summary>
/// Command to mark a session as completed by the partner
/// </summary>
[Serializable]
public record CompleteSessionCommand(
    Guid ConferenceId,
    Guid BookingId,
    string PartnerEmail,
    string SessionNotes,
    int SessionRating,
    bool CapturePayment = true
) : IVideoConferenceCommand;

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
            .NotEmpty()
            .WithMessage("UserId is required");
            
        RuleFor(command => command.PartnerId)
            .NotEmpty()
            .WithMessage("PartnerId is required");
            
        RuleFor(command => command.RateInformation)
            .NotNull()
            .WithMessage("RateInformation is required");
            
        RuleFor(command => command.RateInformation.RatePerMinute)
            .GreaterThan(0)
            .When(command => command.RateInformation != null)
            .WithMessage("RatePerMinute must be greater than 0");
            
        RuleFor(command => command.RateInformation.MinimumCharge)
            .GreaterThanOrEqualTo(0)
            .When(command => command.RateInformation != null)
            .WithMessage("MinimumCharge must be greater than or equal to 0");
            
        RuleFor(command => command.RateInformation.MinimumMinutes)
            .GreaterThan(0)
            .When(command => command.RateInformation != null)
            .WithMessage("MinimumMinutes must be greater than 0");
            
        RuleFor(command => command.RateInformation.BillingIncrementMinutes)
            .GreaterThan(0)
            .When(command => command.RateInformation != null)
            .WithMessage("BillingIncrementMinutes must be greater than 0");
            
        RuleFor(command => command.RateInformation.EffectiveDate)
            .LessThanOrEqualTo(command => command.StartTime)
            .When(command => command.RateInformation != null)
            .WithMessage("Rate must be effective before or at conference start time");
            
        RuleFor(command => command.RateInformation.ExpirationDate)
            .GreaterThanOrEqualTo(command => command.EndTime)
            .When(command => command.RateInformation?.ExpirationDate != null)
            .WithMessage("Rate must not expire before conference end time");
            
        RuleFor(command => command.RateInformation.IsActive)
            .Equal(true)
            .When(command => command.RateInformation != null)
            .WithMessage("Rate must be active");
    }
}
