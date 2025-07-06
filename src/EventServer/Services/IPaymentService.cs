namespace EventServer.Services;

public interface IPaymentService
{
    Task<string> AuthorizePaymentAsync(decimal amount, string currency, string paymentMethodId);
    Task CapturePaymentAsync(string paymentIntentId);
}
