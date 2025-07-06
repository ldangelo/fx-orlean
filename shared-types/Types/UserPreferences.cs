using System.Diagnostics;

namespace Fortium.Types;

[Serializable]
public class UserPreferences
{
    public UserPreferences() {}
    
    public bool ReceiveEmailNotifications { get; set; } = true;
    public bool ReceiveSmsNotifications { get; set; } = false;
    public string? PreferredLanguage { get; set; } = "en-US";
    public string? TimeZone { get; set; } = "UTC";
    public string? Theme { get; set; } = "Light";
}