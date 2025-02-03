namespace FxExpert.Aggregates.Partners;

public class PartnerAggregate
{
    public string? Id { get; set; }
    public int Version { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? PrimaryPhone { get; set; }

    public bool Active { get; set; }

    private void CheckIsActive()
    {
        if (!Active)
            throw new InvalidOperationException(Id + " item is not acctive.");
    }
}