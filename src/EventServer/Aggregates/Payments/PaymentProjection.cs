using EventServer.Aggregates.Payments.Events;
using EventServer.Aggregates.VideoConference;
using Fortium.Types;
using JasperFx.Core;
using JasperFx.Events;
using Marten.Events;
using Marten.Events.Aggregation;
using Serilog;

namespace EventServer.Aggregates.Payments;

public class PaymentProjection : SingleStreamProjection<Payment, string>
{
  public PaymentProjection()
  {
    ProjectEvent<PaymentAuthorizedEvent>((state, @event) =>
    {
      var newState = state ?? new Payment
      {
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

      newState.AuthorizationDate = DateTime.UtcNow;

      return newState;
    });

    ProjectEvent<PaymentCapturedEvent>((state, @event) =>
    {
      if (state == null)
      {
        Log.Warning(
                "PaymentProjection: Cannot capture payment {PaymentId} - no authorization found",
                @event.PaymentId
            );
        return new Payment
        {
          PaymentId = @event.PaymentId,
          Amount = 0,
          Currency = "USD",
          Status = "Error_NoAuthorization",
          CaptureDate = DateTime.UtcNow
        };
      }

      Log.Information(
              "PaymentProjection: Applying {type} to {PaymentIntentId}",
              typeof(PaymentCapturedEvent),
              @event.PaymentId
          );

      state.Status = "Captured";
      state.CaptureDate = DateTime.UtcNow;

      return state;
    });

    ProjectEvent<ConferencePaymentAuthorizedEvent>((state, @event) =>
    {
      var newState = state ?? new Payment();
      
      // Set properties explicitly
      newState.PaymentId = @event.PaymentId.ToString();
      newState.Amount = @event.Amount;
      newState.Currency = @event.Currency;
      newState.ConferenceId = @event.ConferenceId.ToString();
      newState.Status = "Authorized";

      Log.Information(
              "PaymentProjection: Creating state for Payment {PaymentId} with Conference {ConferenceId}",
              @event.PaymentId,
              @event.ConferenceId
          );

      newState.AuthorizationDate = DateTime.UtcNow;
      return newState;
    });

    Name = "PaymentProjection";
  }

}
