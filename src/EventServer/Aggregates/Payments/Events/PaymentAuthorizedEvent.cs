namespace EventServer.Aggregates.Payments.Events;

public record PaymentAuthorizedEvent(string PaymentId, decimal Amount, string Currency);
