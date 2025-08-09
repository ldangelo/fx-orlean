using Fortium.Types;

namespace FxExpert.Blazor.Client.Models;

public class PaymentAuthorizationResult
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Status { get; set; }
    public string? Error { get; set; }
}

public enum AvailabilityTimeframe
{
    ThisWeek,
    NextWeek,
    ThisMonth
}

public class PartnerFilterCriteria
{
    public List<string>? Cities { get; set; }
    public List<string>? States { get; set; }
    public List<string>? Regions { get; set; }
    public AvailabilityTimeframe? Availability { get; set; }
    public List<string>? RequiredSkills { get; set; }
    public ExperienceLevel? MinExperienceLevel { get; set; }
    public int? MinYearsExperience { get; set; }
}

public record StateInfo(string Name, string Code);