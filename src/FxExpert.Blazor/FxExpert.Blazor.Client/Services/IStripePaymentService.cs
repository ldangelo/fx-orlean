namespace FxExpert.Blazor.Client.Services;

public interface IStripePaymentService
{
    Task<bool> InitializeAsync(string publishableKey);
    Task<bool> CreatePaymentFormAsync(string elementId, string clientSecret);
    Task<PaymentResult> ConfirmPaymentAsync(string returnUrl);
    Task<PaymentMethodResult> CreatePaymentMethodAsync();
    Task<bool> DestroyAsync();
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Status { get; set; }
}

public class PaymentMethodResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? PaymentMethodId { get; set; }
}