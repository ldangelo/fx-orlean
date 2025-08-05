using Stripe;
using Stripe.Checkout;

namespace EventServer.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentIntentService _paymentIntentService;

    public PaymentService()
    {
        // TODO: Move to configuration
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? "sk_test_...";
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

    public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Convert to smallest currency unit (cents)
            Currency = currency.ToLower(),
            CaptureMethod = "manual", // Authorize only, capture later
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };

        var paymentIntent = await _paymentIntentService.CreateAsync(options);
        return paymentIntent.Id;
    }

    public async Task<string> GetPaymentIntentClientSecretAsync(string paymentIntentId)
    {
        var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId);
        return paymentIntent.ClientSecret ?? throw new InvalidOperationException("Payment Intent client secret is null");
    }
}
