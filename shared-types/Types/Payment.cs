namespace Fortium.Types;

public class Payment
{
    public string PaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
    public DateTime? AuthorizationDate { get; set; }
    public DateTime? CaptureDate { get; set; }
}
