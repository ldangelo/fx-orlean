namespace EventServer.Aggregates.Payments.Commands;

public record AuthorizePaymentCommand(decimal Amount, string Currency, string PaymentMethodId);

public record CapturePaymentCommand(string PaymentIntentId);
