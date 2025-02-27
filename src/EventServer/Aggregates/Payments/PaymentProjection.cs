using EventServer.Aggregates.Payments.Events;
using Serilog;
using Wolverine.Attributes;

namespace EventServer.Aggregates.Payments;

public class PaymentProjection : SingleStreamProjection<Payment>
{
    public static Payment Apply(PaymentAuthorizedEvent @event, Payment payment)
    {
        Log.Information(
            "PaymentProjection: Applying {type} to {PaymentIntentId}",
            typeof(PaymentAuthorizedEvent),
            @event.PaymentIntentId
        );

        payment.PaymentIntentId = @event.PaymentIntentId;
        payment.Amount = @event.Amount;
        payment.Currency = @event.Currency;
        payment.Status = "Authorized";
        payment.AuthorizationDate = DateTime.Now;
        return payment;
    }

    public static Payment Apply(PaymentCapturedEvent @event, Payment payment)
    {
        Log.Information(
            "PaymentProjection: Applying {type} to {PaymentIntentId}",
            typeof(PaymentCapturedEvent),
            @event.PaymentIntentId
        );

        payment.Status = "Captured";
        payment.CaptureDate = DateTime.Now;
        return payment;
    }
}
