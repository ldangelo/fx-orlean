namespace Fortium.Types;

public enum PhoneType
{
    Home,
    Work,
    Other,
}

[Serializable]
public class Phone
{
    public string PhoneNumber { get; }
    public PhoneType Type { get; }

    public Phone(string number)
    {
        PhoneNumber = number;
        Type = PhoneType.Home;
    }

    public Phone(string number, PhoneType type)
    {
        this.PhoneNumber = number;
        this.Type = type;
    }

    public Phone(Phone p)
    {
        this.PhoneNumber = p.PhoneNumber;
        this.Type = p.Type;
    }

    public string toString()
    {
        return PhoneNumber;
    }

    public void ShouldNotBeEmpty()
    {
        if (PhoneNumber == null || PhoneNumber == "")
            throw new Exception("Can not be Empty");
    }

    public static implicit operator Phone(string v)
    {
        return new Phone(v, PhoneType.Home);
    }
}
