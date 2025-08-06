namespace FxExpert.Blazor.Client.Services;

public interface IPaymentConfigurationService
{
    Task<string> GetStripePublishableKeyAsync();
}