using Marten.Schema;

namespace Fortium.Types;

[Serializable]
public class Payment
{
    [Identity]
    public string? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
    public DateTime? AuthorizationDate { get; set; }
    public DateTime? CaptureDate { get; set; }
}
