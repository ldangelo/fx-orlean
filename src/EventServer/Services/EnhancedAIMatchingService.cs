using EventServer.Aggregates.Partners;
using EventServer.Aggregates.VideoConference;
using Fortium.Types;
using Marten;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OpenAI.Chat;
using Serilog;

namespace EventServer.Services;

/// <summary>
/// Enhanced AI matching service with improved RAG, session history feedback, and performance optimization
/// </summary>
public class EnhancedAIMatchingService
{
    private readonly IDocumentStore _store;
    private readonly IMemoryCache _cache;
    private readonly ChatGPTWithRAG _legacyChatGPT; // Fallback to existing implementation
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);
    
    // Enhanced prompts with more sophisticated reasoning
    private const string EnhancedSystemPrompt = @"
You are an expert matching consultant for a fractional executive consulting firm. Your expertise includes:
- Deep understanding of business challenges across industries
- Extensive knowledge of executive skill requirements
- Strong pattern matching between problems and consultant expertise
- Ability to assess consultant fit based on experience and track record

Given the partner data and problem description, analyze each partner's suitability using these criteria:
1. SKILL RELEVANCE: How well do their skills align with the problem requirements?
2. EXPERIENCE DEPTH: Do they have sufficient years and level of experience?
3. INDUSTRY FIT: Does their work history match the problem domain?
4. PROBLEM COMPLEXITY: Can they handle the complexity level indicated?
5. TRACK RECORD: Based on their work history, are they likely to succeed?

Rank partners from 1 (best fit) to N (least fit). For each partner, provide:
- Clear reasoning for the ranking
- Specific skills that make them suitable
- Any concerns or limitations
- Confidence level (High/Medium/Low)

Return ONLY a valid JSON object in this exact format:
{
  ""matches"": [
    {
      ""partnerId"": ""email@domain.com"",
      ""firstName"": ""First"",
      ""lastName"": ""Last"",
      ""rank"": 1,
      ""confidence"": ""High"",
      ""matchScore"": 0.95,
      ""reasoning"": ""Detailed explanation of why this partner is the best fit"",
      ""relevantSkills"": [""skill1"", ""skill2""],
      ""potentialConcerns"": ""Any limitations or concerns (optional)"",
      ""estimatedSuccessProbability"": 0.90
    }
  ],
  ""analysisMetadata"": {
    ""totalPartnersAnalyzed"": 5,
    ""averageMatchScore"": 0.75,
    ""problemComplexity"": ""High"",
    ""recommendedTopChoices"": 3
  }
}

Do not hallucinate information. Base all assessments on provided partner data.
";

    public EnhancedAIMatchingService(
        IDocumentStore store, 
        IMemoryCache cache, 
        ChatGPTWithRAG legacyChatGPT)
    {
        _store = store;
        _cache = cache;
        _legacyChatGPT = legacyChatGPT;
    }

    /// <summary>
    /// Get enhanced AI partner matching with improved RAG and feedback integration
    /// </summary>
    public async Task<EnhancedMatchingResult> GetEnhancedPartnerMatches(string problemDescription, MatchingOptions? options = null)
    {
        try
        {
            Log.Information("Enhanced AI Matching: Starting analysis for problem: {problem}", 
                problemDescription?.Substring(0, Math.Min(problemDescription.Length, 100)));

            // Step 1: Retrieve and filter available partners
            var availablePartners = await GetAvailablePartnersWithCache();
            if (!availablePartners.Any())
            {
                Log.Warning("No available partners found, falling back to legacy implementation");
                var legacyResult = await _legacyChatGPT.GetChatGPTResponse(problemDescription);
                return ConvertLegacyResult(legacyResult, problemDescription);
            }

            // Step 2: Enrich partner data with session history and performance metrics
            var enrichedPartners = await EnrichPartnersWithSessionHistory(availablePartners);

            // Step 3: Build enhanced context for AI analysis
            var enhancedContext = await BuildEnhancedContext(enrichedPartners, problemDescription, options);

            // Step 4: Call OpenAI with enhanced prompting
            var aiResponse = await CallEnhancedOpenAIAPI(enhancedContext, problemDescription);

            // Step 5: Post-process and validate results
            var result = await PostProcessAndValidateResults(aiResponse, enrichedPartners, problemDescription);

            Log.Information("Enhanced AI Matching: Completed analysis with {count} matches", result.Matches.Count);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Enhanced AI Matching failed, falling back to legacy implementation");
            var fallbackResult = await _legacyChatGPT.GetChatGPTResponse(problemDescription);
            return ConvertLegacyResult(fallbackResult, problemDescription);
        }
    }

    /// <summary>
    /// Get available partners with intelligent caching
    /// </summary>
    private async Task<List<Partner>> GetAvailablePartnersWithCache()
    {
        return await _cache.GetOrCreateAsync("available_partners_enhanced", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiry;
            entry.Priority = CacheItemPriority.High;
            
            using var session = _store.QuerySession();
            
            var partners = await session.Query<Partner>()
                .Where(p => p.AvailabilityNext30Days > 0 && p.Active)
                .OrderByDescending(p => p.AvailabilityNext30Days)
                .ToListAsync();

            Log.Information("Retrieved {count} available partners from database", partners.Count);
            return partners.ToList();
        }) ?? new List<Partner>();
    }

    /// <summary>
    /// Enrich partner data with session history and performance metrics
    /// </summary>
    private async Task<List<EnrichedPartnerData>> EnrichPartnersWithSessionHistory(List<Partner> partners)
    {
        using var session = _store.QuerySession();
        var enrichedPartners = new List<EnrichedPartnerData>();

        foreach (var partner in partners)
        {
            // Get session history for performance metrics
            var sessionHistory = await session.Query<SessionHistoryView>()
                .Where(s => s.PartnerEmail == partner.EmailAddress)
                .ToListAsync();

            // Calculate performance metrics
            var completedSessions = sessionHistory.Where(s => s.Status == SessionStatus.Completed).ToList();
            var avgRating = completedSessions.Where(s => s.SessionRating.HasValue && s.SessionRating > 0)
                .DefaultIfEmpty().Average(s => s?.SessionRating ?? 0);

            var successRate = sessionHistory.Count > 0 
                ? (double)completedSessions.Count / sessionHistory.Count 
                : 0.5; // Neutral for new partners

            // Get recent session topics for domain expertise
            var recentTopics = completedSessions
                .Where(s => s.SessionCompletedAt.HasValue && s.SessionCompletedAt > DateTime.UtcNow.AddMonths(-6))
                .Select(s => s.ConsultationTopic)
                .Where(topic => !string.IsNullOrEmpty(topic))
                .ToList();

            enrichedPartners.Add(new EnrichedPartnerData
            {
                Partner = partner,
                AverageRating = avgRating,
                TotalSessions = sessionHistory.Count,
                CompletedSessions = completedSessions.Count,
                SuccessRate = successRate,
                RecentSessionTopics = recentTopics,
                LastSessionDate = sessionHistory.LastOrDefault()?.SessionCompletedAt
            });
        }

        return enrichedPartners;
    }

    /// <summary>
    /// Build enhanced context for AI analysis
    /// </summary>
    private async Task<string> BuildEnhancedContext(
        List<EnrichedPartnerData> enrichedPartners, 
        string problemDescription,
        MatchingOptions? options = null)
    {
        var contextBuilder = new
        {
            problemAnalysis = new
            {
                description = problemDescription,
                estimatedComplexity = EstimateProblemComplexity(problemDescription),
                suggestedSkills = ExtractImpliedSkills(problemDescription),
                urgencyIndicators = ExtractUrgencyIndicators(problemDescription)
            },
            availablePartners = enrichedPartners.Select(ep => new
            {
                basicInfo = new
                {
                    partnerId = ep.Partner.EmailAddress,
                    firstName = ep.Partner.FirstName,
                    lastName = ep.Partner.LastName,
                    availability = ep.Partner.AvailabilityNext30Days,
                    location = new { city = ep.Partner.City, state = ep.Partner.State, country = ep.Partner.Country }
                },
                professionalProfile = new
                {
                    bio = ep.Partner.Bio,
                    title = ep.Partner.Title,
                    skills = ep.Partner.Skills?.Select(s => new
                    {
                        name = s.Skill,
                        yearsExperience = s.YearsOfExperience,
                        level = s.ExperienceLevel.ToString()
                    }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
                    workHistory = ep.Partner.WorkHistories?.Select(wh => new
                    {
                        company = wh.CompanyName,
                        title = wh.Title,
                        startDate = wh.StartDate.ToString("yyyy-MM"),
                        endDate = wh.EndDate?.ToString("yyyy-MM") ?? "Present",
                        description = wh.Description
                    }).Cast<dynamic>().ToList() ?? new List<dynamic>()
                },
                performanceMetrics = new
                {
                    averageRating = Math.Round(ep.AverageRating, 2),
                    totalSessions = ep.TotalSessions,
                    completedSessions = ep.CompletedSessions,
                    successRate = Math.Round(ep.SuccessRate * 100, 1),
                    recentExpertiseAreas = ep.RecentSessionTopics?.Take(5).ToList() ?? new List<string>(),
                    lastActive = ep.LastSessionDate?.ToString("yyyy-MM-dd") ?? "No recent activity"
                }
            }).ToList(),
            matchingCriteria = options != null ? new
            {
                preferredExperienceLevel = options.MinimumExperienceLevel,
                industryPreference = options.IndustryPreference,
                urgency = options.UrgencyLevel,
                budgetRange = options.SessionBudgetRange,
                locationPreference = options.LocationPreference
            } : null
        };

        return JsonConvert.SerializeObject(contextBuilder, Formatting.Indented);
    }

    /// <summary>
    /// Call OpenAI API with enhanced prompting
    /// </summary>
    private async Task<string> CallEnhancedOpenAIAPI(string enhancedContext, string problemDescription)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        
        if (string.IsNullOrEmpty(apiKey))
        {
            Log.Warning("No OpenAI API key found, returning enhanced sample data");
            return GenerateEnhancedSampleResponse();
        }

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(EnhancedSystemPrompt),
                new UserChatMessage($@"
PARTNER DATA AND CONTEXT:
{enhancedContext}

PROBLEM TO SOLVE:
{problemDescription}

Please analyze the partners and return your matching assessment in the specified JSON format.")
            };

            var client = new ChatClient("gpt-4o", apiKey);
            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 4000,
                Temperature = 0.3f, // Lower temperature for more consistent results
                TopP = 0.9f
            };

            var response = await client.CompleteChatAsync(messages, chatOptions);
            var responseText = response.Value.Content[0].Text;

            Log.Information("Enhanced AI Response received, length: {length}", responseText?.Length ?? 0);
            return CleanJsonResponse(responseText);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Enhanced OpenAI API call failed");
            throw;
        }
    }

    /// <summary>
    /// Post-process and validate AI results
    /// </summary>
    private async Task<EnhancedMatchingResult> PostProcessAndValidateResults(
        string aiResponse, 
        List<EnrichedPartnerData> enrichedPartners,
        string problemDescription)
    {
        try
        {
            var parsedResponse = JsonConvert.DeserializeObject<EnhancedAIResponse>(aiResponse);
            
            if (parsedResponse?.Matches == null || !parsedResponse.Matches.Any())
            {
                Log.Warning("AI returned no matches, generating fallback result");
                return GenerateFallbackResult(enrichedPartners, problemDescription);
            }

            // Validate and enrich the matches
            var validatedMatches = parsedResponse.Matches
                .Where(m => !string.IsNullOrEmpty(m.PartnerId))
                .Select(match =>
                {
                    var enrichedPartner = enrichedPartners.FirstOrDefault(ep => 
                        ep.Partner.EmailAddress.Equals(match.PartnerId, StringComparison.OrdinalIgnoreCase));
                    
                    if (enrichedPartner != null)
                    {
                        // Enhance match with real partner data
                        match.Partner = enrichedPartner.Partner;
                        match.PerformanceMetrics = new PartnerPerformanceMetrics
                        {
                            AverageRating = enrichedPartner.AverageRating,
                            TotalSessions = enrichedPartner.TotalSessions,
                            SuccessRate = enrichedPartner.SuccessRate,
                            RecentExpertiseAreas = enrichedPartner.RecentSessionTopics ?? new List<string>()
                        };
                    }
                    
                    return match;
                })
                .Where(m => m.Partner != null)
                .OrderBy(m => m.Rank)
                .ToList();

            return new EnhancedMatchingResult
            {
                Matches = validatedMatches,
                Metadata = parsedResponse.AnalysisMetadata ?? new MatchingAnalysisMetadata(),
                ProblemDescription = problemDescription,
                GeneratedAt = DateTime.UtcNow,
                ProcessingTimeMs = 0 // TODO: Add timing
            };
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to parse AI response JSON: {response}", aiResponse?.Substring(0, Math.Min(aiResponse.Length, 500)));
            return GenerateFallbackResult(enrichedPartners, problemDescription);
        }
    }

    #region Helper Methods

    private string EstimateProblemComplexity(string problemDescription)
    {
        var complexityIndicators = new[]
        {
            "transformation", "modernization", "architecture", "enterprise", "scale", "strategic",
            "complex", "challenging", "critical", "urgent", "integration", "migration"
        };

        var matches = complexityIndicators.Count(indicator => 
            problemDescription.Contains(indicator, StringComparison.OrdinalIgnoreCase));

        return matches switch
        {
            >= 3 => "High",
            >= 1 => "Medium",
            _ => "Low"
        };
    }

    private List<string> ExtractImpliedSkills(string problemDescription)
    {
        var skillMappings = new Dictionary<string[], string[]>
        {
            { new[] { "cloud", "aws", "azure", "gcp" }, new[] { "cloud architecture", "devops", "infrastructure" } },
            { new[] { "digital transformation", "modernization" }, new[] { "change management", "strategic planning", "architecture" } },
            { new[] { "startup", "growth", "scale" }, new[] { "startup experience", "growth strategy", "operations" } },
            { new[] { "security", "compliance", "privacy" }, new[] { "cybersecurity", "risk management", "compliance" } },
            { new[] { "AI", "machine learning", "data" }, new[] { "artificial intelligence", "data science", "analytics" } }
        };

        var impliedSkills = new HashSet<string>();
        var lowerProblem = problemDescription.ToLowerInvariant();

        foreach (var mapping in skillMappings)
        {
            if (mapping.Key.Any(keyword => lowerProblem.Contains(keyword)))
            {
                foreach (var skill in mapping.Value)
                {
                    impliedSkills.Add(skill);
                }
            }
        }

        return impliedSkills.ToList();
    }

    private string ExtractUrgencyIndicators(string problemDescription)
    {
        var urgentWords = new[] { "urgent", "asap", "immediately", "critical", "emergency", "crisis" };
        var soonWords = new[] { "soon", "quickly", "fast", "rapid", "next week", "this month" };

        var lowerProblem = problemDescription.ToLowerInvariant();

        if (urgentWords.Any(word => lowerProblem.Contains(word)))
            return "Urgent";
        if (soonWords.Any(word => lowerProblem.Contains(word)))
            return "Soon";
        
        return "Normal";
    }

    private string CleanJsonResponse(string response)
    {
        if (string.IsNullOrEmpty(response)) return "{}";
        
        // Remove markdown code block markers
        response = response.Replace("```json", "").Replace("```", "");
        response = response.Trim();
        
        return response;
    }

    private string GenerateEnhancedSampleResponse()
    {
        return JsonConvert.SerializeObject(new EnhancedAIResponse
        {
            Matches = new List<EnhancedPartnerMatch>
            {
                new EnhancedPartnerMatch
                {
                    PartnerId = "leo.dangelo@fortiumpartners.com",
                    FirstName = "Leo",
                    LastName = "DAngelo",
                    Rank = 1,
                    Confidence = "High",
                    MatchScore = 0.95,
                    Reasoning = "Extensive leadership and technical architecture experience with 30 years in the field. Strong background in AWS, .NET, and enterprise transformations.",
                    RelevantSkills = new List<string> { "leadership", "architecture", "aws", "dotnet" },
                    EstimatedSuccessProbability = 0.90
                }
            },
            AnalysisMetadata = new MatchingAnalysisMetadata
            {
                TotalPartnersAnalyzed = 2,
                AverageMatchScore = 0.95,
                ProblemComplexity = "Medium",
                RecommendedTopChoices = 1
            }
        }, Formatting.Indented);
    }

    private EnhancedMatchingResult GenerateFallbackResult(List<EnrichedPartnerData> enrichedPartners, string problemDescription)
    {
        // Create a simple fallback based on availability and recent activity
        var fallbackMatches = enrichedPartners
            .OrderByDescending(ep => ep.SuccessRate)
            .ThenByDescending(ep => ep.Partner.AvailabilityNext30Days)
            .Take(3)
            .Select((ep, index) => new EnhancedPartnerMatch
            {
                PartnerId = ep.Partner.EmailAddress,
                FirstName = ep.Partner.FirstName,
                LastName = ep.Partner.LastName,
                Partner = ep.Partner,
                Rank = index + 1,
                Confidence = "Medium",
                MatchScore = 0.7 - (index * 0.1),
                Reasoning = $"Experienced consultant with {ep.Partner.Skills?.Count ?? 0} skills and {ep.SuccessRate:P0} success rate.",
                RelevantSkills = ep.Partner.Skills?.Take(3).Select(s => s.Skill).ToList() ?? new List<string>(),
                EstimatedSuccessProbability = ep.SuccessRate,
                PerformanceMetrics = new PartnerPerformanceMetrics
                {
                    AverageRating = ep.AverageRating,
                    TotalSessions = ep.TotalSessions,
                    SuccessRate = ep.SuccessRate,
                    RecentExpertiseAreas = ep.RecentSessionTopics ?? new List<string>()
                }
            })
            .ToList();

        return new EnhancedMatchingResult
        {
            Matches = fallbackMatches,
            Metadata = new MatchingAnalysisMetadata
            {
                TotalPartnersAnalyzed = enrichedPartners.Count,
                AverageMatchScore = fallbackMatches.Any() ? fallbackMatches.Average(m => m.MatchScore) : 0,
                ProblemComplexity = EstimateProblemComplexity(problemDescription),
                RecommendedTopChoices = Math.Min(fallbackMatches.Count, 2)
            },
            ProblemDescription = problemDescription,
            GeneratedAt = DateTime.UtcNow,
            ProcessingTimeMs = 0
        };
    }

    private EnhancedMatchingResult ConvertLegacyResult(List<Partner> legacyPartners, string problemDescription)
    {
        var convertedMatches = legacyPartners.Select((partner, index) => new EnhancedPartnerMatch
        {
            PartnerId = partner.EmailAddress,
            FirstName = partner.FirstName,
            LastName = partner.LastName,
            Partner = partner,
            Rank = index + 1,
            Confidence = "Medium",
            MatchScore = 0.8 - (index * 0.1),
            Reasoning = "Match generated by legacy AI system",
            RelevantSkills = partner.Skills?.Take(3).Select(s => s.Skill).ToList() ?? new List<string>(),
            EstimatedSuccessProbability = 0.75
        }).ToList();

        return new EnhancedMatchingResult
        {
            Matches = convertedMatches,
            Metadata = new MatchingAnalysisMetadata
            {
                TotalPartnersAnalyzed = legacyPartners.Count,
                AverageMatchScore = convertedMatches.Any() ? convertedMatches.Average(m => m.MatchScore) : 0,
                ProblemComplexity = EstimateProblemComplexity(problemDescription),
                RecommendedTopChoices = Math.Min(convertedMatches.Count, 2)
            },
            ProblemDescription = problemDescription,
            GeneratedAt = DateTime.UtcNow,
            ProcessingTimeMs = 0
        };
    }

    #endregion
}