namespace EventServer.Aggregates.Payments.Commands;

public record AuthorizePaymentCommand(
    decimal Amount,
    string Currency,
    string PaymentMethodId,
    DateTime authorizeDate
);

public record CapturePaymentCommand(string PaymentIntentId, DateTime captureDate);
