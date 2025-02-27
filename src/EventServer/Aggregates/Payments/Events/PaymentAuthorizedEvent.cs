namespace EventServer.Aggregates.Payments.Events;

public record PaymentAuthorizedEvent(string PaymentIntentId, decimal Amount, string Currency);
