using FluentValidation;
using Fortium.Types;

namespace EventServer.Aggregates.Payments.Commands;

public interface IPaymentCommand {}

[Serializable]
public record AuthorizeConferencePaymentCommand(
    Guid PaymentId,
    Guid ConferenceId,
    decimal Amount,
    string Currency,
    string UserId,
    RateInformation RateInformation
) : IPaymentCommand;

public class AuthorizeConferencePaymentCommandValidator : AbstractValidator<AuthorizeConferencePaymentCommand>
{
    public AuthorizeConferencePaymentCommandValidator()
    {
        RuleFor(command => command.PaymentId)
            .NotEqual(Guid.Empty)
            .WithMessage("PaymentId is required");
            
        RuleFor(command => command.ConferenceId)
            .NotEqual(Guid.Empty)
            .WithMessage("ConferenceId is required");
            
        RuleFor(command => command.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
            
        RuleFor(command => command.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Must(currency => currency.Length == 3)
            .WithMessage("Currency must be a 3-letter code (e.g., USD)");
            
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
            
        RuleFor(command => command.RateInformation)
            .NotNull()
            .WithMessage("RateInformation is required");
    }
}
