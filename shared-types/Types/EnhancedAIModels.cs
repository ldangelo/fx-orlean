using Fortium.Types;
using Newtonsoft.Json;

namespace Fortium.Types;

/// <summary>
/// Enhanced partner matching result with AI analysis and performance metrics
/// </summary>
public class EnhancedMatchingResult
{
    public List<EnhancedPartnerMatch> Matches { get; set; } = new();
    public MatchingAnalysisMetadata Metadata { get; set; } = new();
    public string ProblemDescription { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public long ProcessingTimeMs { get; set; }
}

/// <summary>
/// Enhanced partner match with detailed AI reasoning and performance metrics
/// </summary>
public class EnhancedPartnerMatch
{
    [JsonProperty("partnerId")]
    public string PartnerId { get; set; } = string.Empty;
    
    [JsonProperty("firstName")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonProperty("lastName")]
    public string LastName { get; set; } = string.Empty;
    
    [JsonProperty("rank")]
    public int Rank { get; set; }
    
    [JsonProperty("confidence")]
    public string Confidence { get; set; } = string.Empty; // High, Medium, Low
    
    [JsonProperty("matchScore")]
    public double MatchScore { get; set; } // 0.0 to 1.0
    
    [JsonProperty("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
    
    [JsonProperty("relevantSkills")]
    public List<string> RelevantSkills { get; set; } = new();
    
    [JsonProperty("potentialConcerns")]
    public string? PotentialConcerns { get; set; }
    
    [JsonProperty("estimatedSuccessProbability")]
    public double EstimatedSuccessProbability { get; set; } // 0.0 to 1.0
    
    // Enhanced data (not from AI, populated by service)
    public Partner? Partner { get; set; }
    public PartnerPerformanceMetrics? PerformanceMetrics { get; set; }
    public DateTime? LastSessionDate { get; set; }
}

/// <summary>
/// Partner performance metrics derived from session history
/// </summary>
public class PartnerPerformanceMetrics
{
    public double AverageRating { get; set; }
    public int TotalSessions { get; set; }
    public double SuccessRate { get; set; } // Completion rate
    public List<string> RecentExpertiseAreas { get; set; } = new();
    public DateTime? LastActive { get; set; }
    public int ConsecutiveSuccessfulSessions { get; set; }
    public List<string> ClientIndustries { get; set; } = new(); // Industries worked with
}

/// <summary>
/// Metadata about the AI matching analysis
/// </summary>
public class MatchingAnalysisMetadata
{
    [JsonProperty("totalPartnersAnalyzed")]
    public int TotalPartnersAnalyzed { get; set; }
    
    [JsonProperty("averageMatchScore")]
    public double AverageMatchScore { get; set; }
    
    [JsonProperty("problemComplexity")]
    public string ProblemComplexity { get; set; } = string.Empty; // Low, Medium, High
    
    [JsonProperty("recommendedTopChoices")]
    public int RecommendedTopChoices { get; set; }
    
    public List<string> KeySkillsIdentified { get; set; } = new();
    public string UrgencyLevel { get; set; } = string.Empty;
    public List<string> IndustryIndicators { get; set; } = new();
}

/// <summary>
/// AI response structure for enhanced matching
/// </summary>
public class EnhancedAIResponse
{
    [JsonProperty("matches")]
    public List<EnhancedPartnerMatch> Matches { get; set; } = new();
    
    [JsonProperty("analysisMetadata")]
    public MatchingAnalysisMetadata AnalysisMetadata { get; set; } = new();
}

/// <summary>
/// Options for customizing matching behavior
/// </summary>
public class MatchingOptions
{
    public string? IndustryPreference { get; set; }
    public ExperienceLevel? MinimumExperienceLevel { get; set; }
    public string? LocationPreference { get; set; }
    public string? UrgencyLevel { get; set; } // Urgent, Soon, Normal
    public string? SessionBudgetRange { get; set; }
    public int MaxResults { get; set; } = 5;
    public bool IncludeNewPartners { get; set; } = true; // Include partners with no session history
    public double MinimumSuccessRate { get; set; } = 0.0; // Minimum success rate filter
}

/// <summary>
/// Internal enriched partner data with performance metrics
/// </summary>
public class EnrichedPartnerData
{
    public Partner Partner { get; set; } = new();
    public double AverageRating { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public double SuccessRate { get; set; }
    public List<string>? RecentSessionTopics { get; set; }
    public DateTime? LastSessionDate { get; set; }
    public List<string> ClientIndustries { get; set; } = new();
    
    // Additional analytics
    public double RevenueGenerated { get; set; }
    public int ConsecutiveSuccessfulSessions { get; set; }
    public bool IsTopPerformer => AverageRating >= 4.5 && SuccessRate >= 0.85;
    public bool IsHighlyActive => TotalSessions >= 10 && LastSessionDate >= DateTime.UtcNow.AddMonths(-3);
}

/// <summary>
/// Matching performance analytics for continuous improvement
/// </summary>
public class MatchingPerformanceAnalytics
{
    public string MatchingSessionId { get; set; } = string.Empty;
    public DateTime RequestTimestamp { get; set; }
    public string ProblemDescription { get; set; } = string.Empty;
    public List<EnhancedPartnerMatch> Recommendations { get; set; } = new();
    public string? SelectedPartnerId { get; set; }
    public bool BookingCompleted { get; set; }
    public DateTime? BookingTimestamp { get; set; }
    public double? SessionRating { get; set; }
    public bool SessionCompleted { get; set; }
    
    // Calculated metrics
    public int RecommendationRankOfSelected { get; set; }
    public double RecommendationAccuracy => SelectedPartnerId != null ? 1.0 / RecommendationRankOfSelected : 0.0;
    public bool WasTopRecommendationSelected => RecommendationRankOfSelected == 1;
}

/// <summary>
/// A/B testing framework for different matching algorithms
/// </summary>
public class MatchingExperiment
{
    public string ExperimentId { get; set; } = string.Empty;
    public string ExperimentName { get; set; } = string.Empty;
    public string AlgorithmVersion { get; set; } = string.Empty; // "legacy", "enhanced-v1", "semantic-v1"
    public double TrafficAllocation { get; set; } // Percentage of traffic (0.0 to 1.0)
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    
    // Experiment metrics
    public int TotalRequests { get; set; }
    public int TotalBookings { get; set; }
    public double ConversionRate => TotalRequests > 0 ? (double)TotalBookings / TotalRequests : 0.0;
    public double AverageMatchScore { get; set; }
    public double AverageSessionRating { get; set; }
}