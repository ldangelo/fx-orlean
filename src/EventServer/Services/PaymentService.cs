using Stripe;
using Stripe.Checkout;

namespace EventServer.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentIntentService _paymentIntentService;

    public PaymentService()
    {
        StripeConfiguration.ApiKey = "your-stripe-secret-key";
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<string> AuthorizePaymentAsync(decimal amount, string currency, string paymentMethodId)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Convert to smallest currency unit
            Currency = currency,
            PaymentMethod = paymentMethodId,
            ConfirmationMethod = "manual",
            Confirm = true,
        };

        var paymentIntent = await _paymentIntentService.CreateAsync(options);
        return paymentIntent.Id;
    }

    public async Task CapturePaymentAsync(string paymentIntentId)
    {
        await _paymentIntentService.CaptureAsync(paymentIntentId);
    }
}
