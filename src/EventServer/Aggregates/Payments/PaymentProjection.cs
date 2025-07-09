using EventServer.Aggregates.Payments.Events;
using Fortium.Types;
using Marten.Events.Aggregation;
using Serilog;
using Wolverine.Attributes;

namespace EventServer.Aggregates.Payments;

public class PaymentProjection : SingleStreamProjection<Payment, string>
{
    public static Payment Apply(PaymentAuthorizedEvent @event, Payment payment)
    {
        Log.Information(
            "PaymentProjection: Applying {type} to {PaymentIntentId}",
            typeof(PaymentAuthorizedEvent),
            @event.PaymentId
        );

        payment.PaymentId = @event.PaymentId;
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
            @event.PaymentId
        );

        payment.Status = "Captured";
        payment.CaptureDate = DateTime.Now;
        return payment;
    }
}
