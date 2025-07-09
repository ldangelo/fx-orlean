using EventServer.Aggregates.Payments.Commands;
using EventServer.Aggregates.Payments.Events;
using Fortium.Types;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace EventServer.Controllers;

public static class PaymentController
{
    [WolverinePost("/payments/authorize")]
    public static (Payment, IStartStream) AuthorizePayment(
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

        var payment = new Payment
        {
            PaymentId = command.PaymentMethodId,
            Amount = command.Amount,
            Currency = command.Currency,
            Status = "Authorized"
        };

        return (payment, startStream);
    }

    [WolverinePost("/payments/capture/{payment_id}")]  // Using snake_case for consistency with payment ID in tests
    [EmptyResponse]
    public static PaymentCapturedEvent CapturePayment(
        [FromRoute] string payment_id,
        [Aggregate] Payment payment
    )
    {
        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment not found: {payment_id}");
        }

        return new PaymentCapturedEvent(payment_id, DateTime.UtcNow);
    }
}
