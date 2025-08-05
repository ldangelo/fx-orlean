using EventServer.Aggregates.Payments.Commands;
using EventServer.Aggregates.Payments.Events;
using EventServer.Aggregates.VideoConference;
using EventServer.Services;
using Fortium.Types;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;
using Serilog;

namespace EventServer.Controllers;

public static class PaymentController
{
    [WolverinePost("/payments/authorize")]
    public static async Task<(CreationResponse, IStartStream)> AuthorizePayment(
        AuthorizeConferencePaymentCommand command,
        IQuerySession session
    )
    {
        Log.Information("Authorizing payment {PaymentId} for conference {ConferenceId}", command.PaymentId, command.ConferenceId);
        
        var conferenceId = command.ConferenceId.ToString();
        var conference = await session.LoadAsync<VideoConferenceState>(conferenceId);
        
        if (conference == null)
        {
            Log.Warning("Conference {ConferenceId} not found for payment authorization", command.ConferenceId);
            throw new KeyNotFoundException($"Conference {command.ConferenceId} not found");
        }
        
        var paymentId = command.PaymentId.ToString();
        var paymentAuthorizedEvent = new ConferencePaymentAuthorizedEvent(
            command.PaymentId,
            command.ConferenceId,
            command.Amount,
            command.Currency,
            command.UserId,
            command.RateInformation
        );
        var startStream = MartenOps.StartStream<Payment>(
            paymentId,
            paymentAuthorizedEvent
        );

        return (new CreationResponse($"/payments/{paymentId}"), startStream);
    }

    [WolverinePost("/payments/capture/{payment_id}")]  // Using snake_case for consistency with payment ID in tests
    [EmptyResponse]
    public static PaymentCapturedEvent CapturePayment(
        [FromRoute] string payment_id,
        [Aggregate("payment_id")] Payment payment
    )
    {
        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment not found: {payment_id}");
        }

        return new PaymentCapturedEvent(payment_id, DateTime.UtcNow);
    }

    [WolverineGet("/payments/{paymentId}")]
    public static IResult GetPayment([Document("paymentId")] Payment payment)
    {
        if (payment == null)
        {
            Log.Warning("Payment not found for ID: {paymentId}");
            return Results.NotFound();
        }

        Log.Information("Retrieved payment: {PaymentId}, Status: {Status}, Amount: {Amount}", payment.PaymentId, payment.Status, payment.Amount);
        return Results.Ok(payment);
    }

    [WolverinePost("/payments/create-intent")]
    public static async Task<IResult> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request,
        [FromServices] IPaymentService paymentService
    )
    {
        try
        {
            Log.Information("Creating payment intent for amount {Amount}", request.Amount);
            
            // Create PaymentIntent with Stripe - this will be used for authorization
            var paymentIntentId = await paymentService.CreatePaymentIntentAsync(request.Amount, request.Currency ?? "usd");
            
            // Get client secret for frontend
            var clientSecret = await paymentService.GetPaymentIntentClientSecretAsync(paymentIntentId);
            
            return Results.Ok(new CreatePaymentIntentResponse(paymentIntentId, clientSecret));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create payment intent");
            return Results.Problem("Failed to create payment intent");
        }
    }
}

public record CreatePaymentIntentRequest(decimal Amount, string? Currency = "usd");
public record CreatePaymentIntentResponse(string PaymentIntentId, string ClientSecret);
