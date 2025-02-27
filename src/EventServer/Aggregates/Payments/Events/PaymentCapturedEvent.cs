namespace EventServer.Aggregates.Payments.Events;

public record PaymentCapturedEvent(string PaymentIntentId);
