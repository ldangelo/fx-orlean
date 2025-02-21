namespace Fortium.Types;

public class DollarAmount
{
    public double Amount { get; set; }

    public DollarAmount(double amount)
    {
        Amount = amount;
    }

    public static implicit operator DollarAmount(double v)
    {
        return new DollarAmount(v);
    }
}
