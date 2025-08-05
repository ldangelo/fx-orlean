using Microsoft.JSInterop;
using System.Text.Json;

namespace FxExpert.Blazor.Client.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IJSRuntime _jsRuntime;

    public StripePaymentService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> InitializeAsync(string publishableKey)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("stripeInterop.initialize", publishableKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing Stripe: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreatePaymentFormAsync(string elementId, string clientSecret)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("stripeInterop.createPaymentForm", elementId, clientSecret);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating payment form: {ex.Message}");
            return false;
        }
    }

    public async Task<PaymentResult> ConfirmPaymentAsync(string returnUrl)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("stripeInterop.confirmPayment", returnUrl);
            
            return new PaymentResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Error = result.TryGetProperty("error", out var errorProp) ? errorProp.GetString() : null,
                PaymentIntentId = result.TryGetProperty("paymentIntentId", out var idProp) ? idProp.GetString() : null,
                Status = result.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error confirming payment: {ex.Message}");
            return new PaymentResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<PaymentMethodResult> CreatePaymentMethodAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("stripeInterop.createPaymentMethod");
            
            return new PaymentMethodResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Error = result.TryGetProperty("error", out var errorProp) ? errorProp.GetString() : null,
                PaymentMethodId = result.TryGetProperty("paymentMethodId", out var idProp) ? idProp.GetString() : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating payment method: {ex.Message}");
            return new PaymentMethodResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<bool> DestroyAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("stripeInterop.destroy");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error destroying Stripe elements: {ex.Message}");
            return false;
        }
    }
}