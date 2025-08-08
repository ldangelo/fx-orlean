using Microsoft.JSInterop;

namespace FxExpert.Blazor.Services;

public class ConnectionHealthService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ConnectionHealthService> _logger;

    public ConnectionHealthService(IJSRuntime jsRuntime, ILogger<ConnectionHealthService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    [JSInvokable("CheckConnection")]
    public static Task<bool> CheckConnection()
    {
        // This method can be called from JavaScript to verify the circuit is working
        return Task.FromResult(true);
    }

    public async Task<bool> TestCircuitHealthAsync()
    {
        try
        {
            // Try to invoke a JavaScript function to test bidirectional communication
            await _jsRuntime.InvokeVoidAsync("console.log", "🔍 Circuit health check from C#");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Circuit health check failed: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    public async Task NotifyConnectionStatusAsync(string status)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("console.log", $"🔌 Connection status: {status}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify connection status: {ErrorMessage}", ex.Message);
        }
    }
}