namespace Fortium.Types;

/// <summary>
/// Availability timeframes for partner filtering
/// </summary>
public enum AvailabilityTimeframe
{
    ThisWeek,
    NextWeek,
    ThisMonth
}

/// <summary>
/// Partner specialization areas for advanced filtering
/// </summary>
public enum PartnerSpecialization
{
    DigitalTransformation,
    CloudMigration,
    Cybersecurity,
    DataStrategy,
    AIAndMachineLearning,
    ITGovernance,
    EnterpriseArchitecture,
    DevOpsAndAutomation,
    ProductManagement,
    TechnicalDueDiligence,
    VendorManagement,
    RiskManagement,
    ComplianceAndRegulation,
    CrisisManagement,
    TurnaroundAndRestructuring,
    MergerAndAcquisition,
    ScalingOperations,
    TeamBuilding,
    StrategicPlanning,
    BudgetManagement
}

/// <summary>
/// Filter criteria for partner search
/// </summary>
public class PartnerFilterCriteria
{
    public List<string>? Cities { get; set; }
    public List<string>? States { get; set; }
    public List<string>? Regions { get; set; }
    public AvailabilityTimeframe? Availability { get; set; }
    public List<string>? RequiredSkills { get; set; }
    public ExperienceLevel? MinExperienceLevel { get; set; }
    public int? MinYearsExperience { get; set; }
    
    // Advanced search filters
    public List<string>? Industries { get; set; }
    public List<string>? Technologies { get; set; }
    public List<string>? Certifications { get; set; }
    public decimal? MinRate { get; set; }
    public decimal? MaxRate { get; set; }
    public bool? RemoteWork { get; set; }
    public bool? OnSiteWork { get; set; }
    public bool? TravelWillingness { get; set; }
    public List<string>? Languages { get; set; }
    public int? MinProjectSize { get; set; } // In team members or budget
    public int? MaxProjectSize { get; set; }
    public bool? HasSecurityClearance { get; set; }
    public bool? ExecutiveExperience { get; set; } // C-level or VP experience
    public bool? StartupExperience { get; set; }
    public bool? EnterpriseExperience { get; set; }
    public bool? ConsultingExperience { get; set; }
    public List<PartnerSpecialization>? Specializations { get; set; }
}

/// <summary>
/// State information for US states
/// </summary>
public record StateInfo(string Name, string Code);

/// <summary>
/// Result of creating a consultation booking
/// </summary>
public class ConsultationBookingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GoogleCalendarEventId { get; set; }
    public string? GoogleMeetLink { get; set; }
    public string? CalendarEventLink { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string PartnerEmail { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}

/// <summary>
/// Request model for consultation booking
/// </summary>
public class ConsultationBookingRequest
{
    public string PartnerEmail { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
}

/// <summary>
/// Response model for consultation booking results
/// </summary>
public class ConsultationBookingResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GoogleCalendarEventId { get; set; }
    public string? GoogleMeetLink { get; set; }
    public string? CalendarEventLink { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string PartnerEmail { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}

/// <summary>
/// Represents a time period for calendar availability calculations
/// </summary>
public class TimePeriod
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    
    /// <summary>
    /// Checks if this time period overlaps with another time period
    /// </summary>
    public bool OverlapsWith(TimePeriod other)
    {
        return Start < other.End && End > other.Start;
    }
    
    /// <summary>
    /// Gets the duration of this time period
    /// </summary>
    public TimeSpan Duration => End - Start;
}