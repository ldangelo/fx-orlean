namespace Fortium.Types;

public enum EmailType
{
    Home,
    Work,
    Other,
}

public record Email
{
    public string? Address { get; }
    public EmailType? Type { get; }

    public Email(string address)
    {
        Address = address;
    }

    public Email(string address, EmailType type)
    {
        Address = address;
        Type = type;
    }

    public static implicit operator Email(string v)
    {
        return new Email(v, EmailType.Home);
    }
}
