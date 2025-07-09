using EventServer.Aggregates.Payments.Events;
using EventServer.Aggregates.VideoConference;
using Fortium.Types;
using Marten.Events.Aggregation;
using Serilog;

namespace EventServer.Aggregates.Payments;

public class PaymentState
{
    public required string Id {get; set; }
    public required string PaymentId { get; set; }
    public required decimal Amount { get; set; }
    public required string Currency { get; set; }
    public required string Status { get; set; }
    public DateTime? AuthorizationDate { get; set; }
    public DateTime? CaptureDate { get; set; }
    public Guid? ConferenceId { get; set; }
    public RateInformation? RateInformation { get; set; }
}

public class PaymentProjection : SingleStreamProjection<PaymentState, string>
{
    public PaymentProjection()
    {
        ProjectEvent<PaymentAuthorizedEvent>((state, @event) =>
        {
            state = state ?? new PaymentState
            {
                Id = Guid.NewGuid().ToString(),
                PaymentId = @event.PaymentId,
                Amount = @event.Amount,
                Currency = @event.Currency,
                Status = "Authorized"
            };
            
            Log.Information(
                "PaymentProjection: Applying {type} to {PaymentIntentId}",
                typeof(PaymentAuthorizedEvent),
                @event.PaymentId
            );

            state.AuthorizationDate = DateTime.Now;
            
            return state;
        });

        ProjectEvent<PaymentCapturedEvent>((state, @event) =>
        {
            if (state == null) return null;
            
            Log.Information(
                "PaymentProjection: Applying {type} to {PaymentIntentId}",
                typeof(PaymentCapturedEvent),
                @event.PaymentId
            );

            state.Status = "Captured";
            state.CaptureDate = DateTime.Now;
            
            return state;
        });

        ProjectEvent<ConferencePaymentAuthorizedEvent>((state, @event) =>
        {
            state = state ?? new PaymentState
            {
                Id = Guid.NewGuid().ToString(),
                PaymentId = @event.PaymentId.ToString(),
                Amount = @event.Amount,
                Currency = @event.Currency,
                Status = "Authorized"
            };
            
            Log.Information(
                "PaymentProjection: Applying {type} to Payment {PaymentId} for Conference {ConferenceId}",
                typeof(ConferencePaymentAuthorizedEvent),
                @event.PaymentId,
                @event.ConferenceId
            );

            state.AuthorizationDate = DateTime.Now;
            state.ConferenceId = @event.ConferenceId;
            state.RateInformation = @event.RateInformation;
            
            return state;
        });
    }
}
