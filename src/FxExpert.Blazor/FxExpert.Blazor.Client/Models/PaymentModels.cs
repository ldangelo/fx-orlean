using Fortium.Types;

namespace FxExpert.Blazor.Client.Models;

public class PaymentAuthorizationResult
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Status { get; set; }
    public string? Error { get; set; }
}