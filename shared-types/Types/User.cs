using System.Diagnostics;
using Marten.Schema;

namespace Fortium.Types;

[Serializable]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class User
{
    public User() { }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [Identity]
    public string EmailAddress { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public Address? Address { get; set; }
    public UserPreferences? Preferences { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public DateTime? LoginDate { get; set; }
    public DateTime? LogoffDate { get; set; }
    public bool LoggedIn { get; set; }
    public bool Active { get; set; }
    public List<Guid?> VideoConferences { get; set; } = new();

    public bool IsLoggedIn()
    {
        return LoggedIn;
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }

    public override string ToString()
    {
        return $"IsActive: {Active}, EmailAddress: {EmailAddress}, FirstName: {FirstName}, LastName: {LastName}, PhoneNumber: {PhoneNumber}";
    }
}
