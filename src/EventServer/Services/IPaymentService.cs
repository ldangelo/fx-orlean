namespace EventServer.Services;

public interface IPaymentService
{
    Task<string> AuthorizePaymentAsync(decimal amount, string currency, string paymentMethodId);
    Task CapturePaymentAsync(string paymentIntentId);
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency);
    Task<string> GetPaymentIntentClientSecretAsync(string paymentIntentId);
}
