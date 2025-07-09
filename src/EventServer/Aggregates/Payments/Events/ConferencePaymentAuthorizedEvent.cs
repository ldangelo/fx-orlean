using Fortium.Types;

namespace EventServer.Aggregates.Payments.Events;

public interface IPaymentEvent {}

[Serializable]
public record ConferencePaymentAuthorizedEvent(
    Guid PaymentId,
    Guid ConferenceId,
    decimal Amount,
    string Currency,
    string UserId,
    RateInformation RateInformation
) : IPaymentEvent;
