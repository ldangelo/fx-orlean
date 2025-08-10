using EventServer.Services;
using Fortium.Types;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Wolverine.Http;

namespace EventServer.Controllers;

/// <summary>
/// Enhanced AI controller with improved matching capabilities and A/B testing support
/// </summary>
public class EnhancedAIRequest
{
    public string? ProblemDescription { get; set; }
    public string? Industry { get; set; }
    public string? Priority { get; set; }
    public MatchingOptions? Options { get; set; }
    public bool UseEnhancedMatching { get; set; } = true;
    public string? SessionId { get; set; } // For analytics tracking
}

public static class EnhancedAIController
{
    /// <summary>
    /// Enhanced AI partner matching endpoint with improved RAG and analytics
    /// </summary>
    [WolverinePost("/api/ai/partners/enhanced")]
    public static async Task<IResult> GetEnhancedPartnerMatches(
        [FromBody] EnhancedAIRequest request,
        [FromServices] EnhancedAIMatchingService enhancedService,
        [FromServices] ChatGPTWithRAG legacyService,
        [FromServices] Microsoft.Extensions.Logging.ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(request.ProblemDescription))
        {
            return Results.BadRequest(new { error = "ProblemDescription is required" });
        }

        var startTime = DateTime.UtcNow;
        Log.Information("Enhanced AI Controller: Processing request for problem: {problem}", 
            request.ProblemDescription?.Substring(0, Math.Min(request.ProblemDescription.Length, 100)));

        try
        {
            EnhancedMatchingResult result;
            
            if (request.UseEnhancedMatching)
            {
                // Use enhanced matching with session history and improved prompts
                result = await enhancedService.GetEnhancedPartnerMatches(
                    request.ProblemDescription, 
                    request.Options);
            }
            else
            {
                // Fallback to legacy matching
                var legacyResult = await legacyService.GetChatGPTResponse(request.ProblemDescription);
                result = ConvertLegacyToEnhanced(legacyResult, request.ProblemDescription);
            }

            // Add processing time
            result.ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Log analytics data
            await LogMatchingAnalytics(request, result);

            Log.Information("Enhanced AI Controller: Returning {count} matches in {time}ms", 
                result.Matches.Count, result.ProcessingTimeMs);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Enhanced AI Controller: Error processing request");
            return Results.Problem(
                title: "AI Matching Error",
                detail: "An error occurred while processing the partner matching request.",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Legacy endpoint compatibility - returns simple partner list
    /// </summary>
    [WolverinePost("/api/ai/partners")]
    public static async Task<IResult> GetPartnerMatches(
        [FromBody] AIRequest request,
        [FromServices] EnhancedAIMatchingService enhancedService,
        [FromServices] ChatGPTWithRAG legacyService)
    {
        if (string.IsNullOrWhiteSpace(request.ProblemDescription))
        {
            return Results.BadRequest(new { error = "ProblemDescription is required" });
        }

        Log.Information("Legacy AI Controller: Processing request");

        try
        {
            // Use enhanced service by default, but return legacy format for compatibility
            var enhancedResult = await enhancedService.GetEnhancedPartnerMatches(request.ProblemDescription);
            
            // Convert enhanced result to legacy format
            var legacyPartners = enhancedResult.Matches
                .Where(m => m.Partner != null)
                .Select(m => m.Partner!)
                .ToList();

            Log.Information("Legacy AI Controller: Returning {count} partners", legacyPartners.Count);
            return Results.Ok(legacyPartners);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Legacy AI Controller: Error processing request, falling back to legacy service");
            
            try
            {
                var fallbackResult = await legacyService.GetChatGPTResponse(request.ProblemDescription);
                return Results.Ok(fallbackResult);
            }
            catch (Exception fallbackEx)
            {
                Log.Error(fallbackEx, "Both enhanced and legacy services failed");
                return Results.Problem(
                    title: "AI Matching Error",
                    detail: "All AI matching services are currently unavailable.",
                    statusCode: 503);
            }
        }
    }

    /// <summary>
    /// Get matching analytics for performance monitoring
    /// </summary>
    [WolverineGet("/api/ai/analytics/matching")]
    public static async Task<IResult> GetMatchingAnalytics(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int limit = 100)
    {
        Log.Information("Getting matching analytics from {from} to {to}", fromDate, toDate);

        try
        {
            // TODO: Implement analytics retrieval from event store
            var analytics = new
            {
                totalRequests = 150,
                successfulMatches = 142,
                averageProcessingTime = 1250,
                conversionRate = 0.68,
                topSkillsRequested = new[] { "leadership", "aws", "architecture", "strategic thinking" },
                averageMatchScore = 0.84,
                periodStart = fromDate ?? DateTime.UtcNow.AddDays(-30),
                periodEnd = toDate ?? DateTime.UtcNow
            };

            return Results.Ok(analytics);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving matching analytics");
            return Results.Problem("Error retrieving analytics data", statusCode: 500);
        }
    }

    /// <summary>
    /// Feedback endpoint for improving matching quality
    /// </summary>
    [WolverinePost("/api/ai/feedback")]
    public static async Task<IResult> SubmitMatchingFeedback(
        [FromBody] MatchingFeedback feedback)
    {
        if (string.IsNullOrEmpty(feedback.SessionId) || string.IsNullOrEmpty(feedback.SelectedPartnerId))
        {
            return Results.BadRequest(new { error = "SessionId and SelectedPartnerId are required" });
        }

        Log.Information("Received matching feedback for session {sessionId}, selected partner {partnerId}", 
            feedback.SessionId, feedback.SelectedPartnerId);

        try
        {
            // TODO: Store feedback in event store for analytics and improvement
            // This will be used to improve matching algorithms over time
            
            return Results.Ok(new { message = "Feedback received successfully" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing matching feedback");
            return Results.Problem("Error processing feedback", statusCode: 500);
        }
    }

    #region Helper Methods

    private static async Task LogMatchingAnalytics(EnhancedAIRequest request, EnhancedMatchingResult result)
    {
        try
        {
            var analytics = new MatchingPerformanceAnalytics
            {
                MatchingSessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                RequestTimestamp = DateTime.UtcNow,
                ProblemDescription = request.ProblemDescription ?? string.Empty,
                Recommendations = result.Matches
            };

            // TODO: Store analytics in event store for future analysis
            Log.Information("Matching analytics: {analytics}", JsonConvert.SerializeObject(analytics));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error logging matching analytics");
            // Don't fail the request if analytics logging fails
        }
    }

    private static EnhancedMatchingResult ConvertLegacyToEnhanced(List<Partner> legacyPartners, string problemDescription)
    {
        var matches = legacyPartners.Select((partner, index) => new EnhancedPartnerMatch
        {
            PartnerId = partner.EmailAddress,
            FirstName = partner.FirstName,
            LastName = partner.LastName,
            Partner = partner,
            Rank = index + 1,
            Confidence = "Medium",
            MatchScore = 0.8 - (index * 0.05), // Decreasing score
            Reasoning = "Match generated using legacy AI system with basic skill matching",
            RelevantSkills = partner.Skills?.Take(3).Select(s => s.Skill).ToList() ?? new List<string>(),
            EstimatedSuccessProbability = 0.75
        }).ToList();

        return new EnhancedMatchingResult
        {
            Matches = matches,
            Metadata = new MatchingAnalysisMetadata
            {
                TotalPartnersAnalyzed = legacyPartners.Count,
                AverageMatchScore = matches.Any() ? matches.Average(m => m.MatchScore) : 0,
                ProblemComplexity = "Unknown",
                RecommendedTopChoices = Math.Min(matches.Count, 2)
            },
            ProblemDescription = problemDescription,
            GeneratedAt = DateTime.UtcNow,
            ProcessingTimeMs = 0
        };
    }

    #endregion
}

/// <summary>
/// Feedback model for improving matching quality
/// </summary>
public class MatchingFeedback
{
    public string SessionId { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string SelectedPartnerId { get; set; } = string.Empty;
    public int RecommendationRank { get; set; } // Rank of selected partner in recommendations
    public double? UserSatisfactionRating { get; set; } // 1-5 rating of recommendation quality
    public string? Comments { get; set; }
    public bool BookingCompleted { get; set; }
    public DateTime FeedbackTimestamp { get; set; } = DateTime.UtcNow;
}