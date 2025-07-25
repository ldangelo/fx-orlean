using EventServer.Aggregates.Payments.Commands;
using EventServer.Aggregates.Payments.Events;
using Fortium.Types;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace EventServer.Controllers;

public static class PaymentController
{
    [WolverinePost("/payments/authorize")]
    public static (PaymentAuthorizedEvent, IStartStream) AuthorizePayment(
        AuthorizePaymentCommand command
    )
    {
        var paymentAuthorizedEvent = new PaymentAuthorizedEvent(
            command.PaymentMethodId,
            command.Amount,
            command.Currency
        );
        var startStream = MartenOps.StartStream<Payment>(
            command.PaymentMethodId,
            paymentAuthorizedEvent
        );

        return (paymentAuthorizedEvent, startStream);
    }

    [WolverinePost("/payments/capture/{PaymentId}")]
    [EmptyResponse]
    public static PaymentCapturedEvent CapturePayment(
        CapturePaymentCommand command,
        [Aggregate] Payment payment
    )
    {
        return new PaymentCapturedEvent(command.PaymentIntentId, DateTime.Now);
    }
}
