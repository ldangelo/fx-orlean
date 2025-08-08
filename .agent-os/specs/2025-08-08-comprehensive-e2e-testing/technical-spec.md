# Technical Specification: Comprehensive E2E Testing Suite

> **Epic**: Comprehensive End-to-End Testing Suite for FX-Orleans Platform  
> **Version**: 1.0.0  
> **Date**: 2025-08-08  
> **Status**: Technical Design Complete  

## System Architecture Overview

### Current State Analysis
The FX-Orleans platform currently has a solid foundation for E2E testing with the following established components:

```
✅ EXISTING INFRASTRUCTURE:
├── Microsoft.Playwright integration with NUnit
├── OAuth authentication handling with manual interaction
├── Basic PageObject Model architecture
├── Cross-browser testing framework (Chromium, Firefox, WebKit)
├── Screenshot capture and debugging capabilities
├── CI/CD integration foundation
└── Comprehensive authentication flow testing

🔄 GAPS TO ADDRESS:
├── Multi-persona test coverage (Partner, Admin workflows)
├── Advanced business logic validation
├── Performance testing under concurrent load
├── Error scenario handling and recovery
├── Mobile responsiveness validation
└── Business intelligence metrics accuracy testing
```

## Technical Architecture

### Enhanced PageObject Model Architecture

#### Base Architecture Pattern
```csharp
// Enhanced base page supporting multi-persona patterns
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;
    
    public BasePage(IPage page)
    {
        Page = page;
        BaseUrl = TestConfiguration.GetBaseUrl();
    }
    
    // Common navigation and utility methods
    public virtual async Task NavigateAsync(string path = "") { }
    public virtual async Task TakeScreenshotAsync(string name) { }
    public virtual async Task ValidateSessionPersistenceAsync() { }
}

// Persona-specific base classes
public abstract class ClientBasePage : BasePage
{
    public ClientBasePage(IPage page) : base(page) { }
    
    // Client-specific authentication and navigation
    public virtual async Task AuthenticateAsClientAsync() { }
    public virtual async Task ValidateClientContextAsync() { }
}

public abstract class PartnerBasePage : BasePage  
{
    public PartnerBasePage(IPage page) : base(page) { }
    
    // Partner-specific dashboard and session management
    public virtual async Task AuthenticateAsPartnerAsync() { }
    public virtual async Task NavigateToPartnerDashboardAsync() { }
}

public abstract class AdminBasePage : BasePage
{
    public AdminBasePage(IPage page) : base(page) { }
    
    // Admin-specific platform management
    public virtual async Task AuthenticateAsAdminAsync() { }
    public virtual async Task AccessAdminPanelAsync() { }
}
```

#### Client-Side PageObjects
```csharp
public class ClientHomePage : ClientBasePage
{
    // Existing functionality enhanced
    public async Task SubmitProblemDescriptionAsync(
        string description, 
        string industry = "Technology", 
        string priority = "High")
    {
        await Page.GetByLabel("Problem Description").FillAsync(description);
        await Page.GetByLabel("Industry").SelectOptionAsync(industry);
        await Page.GetByLabel("Priority").SelectOptionAsync(priority);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Find Partners" }).ClickAsync();
    }
    
    // Enhanced AI matching validation
    public async Task<List<PartnerMatch>> GetPartnerRecommendationsAsync()
    {
        await WaitForPartnerResultsAsync();
        var partnerElements = await Page.GetByTestId("partner-card").AllAsync();
        
        var partners = new List<PartnerMatch>();
        foreach (var element in partnerElements)
        {
            partners.Add(new PartnerMatch
            {
                Name = await element.GetByTestId("partner-name").InnerTextAsync(),
                Title = await element.GetByTestId("partner-title").InnerTextAsync(),
                MatchScore = await GetPartnerMatchScoreAsync(element),
                AvailableSlots = await GetPartnerAvailabilityAsync(element)
            });
        }
        return partners;
    }
    
    // Business logic validation methods
    private async Task<double> GetPartnerMatchScoreAsync(ILocator element)
    {
        var scoreText = await element.GetByTestId("match-score").InnerTextAsync();
        return double.Parse(scoreText.Replace("%", "")) / 100.0;
    }
}

public class ClientBookingPage : ClientBasePage
{
    public async Task CompleteBookingWorkflowAsync(BookingDetails details)
    {
        // Calendar integration validation
        await SelectTimeSlotAsync(details.PreferredTime);
        await FillMeetingTopicAsync(details.Topic);
        
        // Payment authorization with detailed validation
        await ProcessPaymentAuthorizationAsync(details.PaymentInfo);
        
        // Google Calendar integration validation
        await ValidateCalendarEventCreationAsync();
        await ValidateGoogleMeetLinkAsync();
    }
    
    private async Task ValidateCalendarEventCreationAsync()
    {
        // Validate calendar integration creates proper events
        var calendarConfirmation = Page.GetByTestId("calendar-confirmation");
        await calendarConfirmation.WaitForAsync(new() { Timeout = 15000 });
        
        var eventDetails = await calendarConfirmation.InnerTextAsync();
        Assert.That(eventDetails, Does.Contain("Event created successfully"));
    }
}
```

#### Partner-Side PageObjects
```csharp
public class PartnerDashboard : PartnerBasePage
{
    public async Task<PartnerMetrics> GetDashboardMetricsAsync()
    {
        await NavigateToPartnerDashboardAsync();
        
        return new PartnerMetrics
        {
            UtilizationRate = await GetUtilizationRateAsync(),
            MonthlyEarnings = await GetMonthlyEarningsAsync(),
            AverageRating = await GetAverageRatingAsync(),
            CompletedSessions = await GetCompletedSessionCountAsync(),
            UpcomingSessions = await GetUpcomingSessionsAsync()
        };
    }
    
    private async Task<double> GetUtilizationRateAsync()
    {
        var utilizationElement = Page.GetByTestId("utilization-rate");
        var utilizationText = await utilizationElement.InnerTextAsync();
        return double.Parse(utilizationText.Replace("%", ""));
    }
    
    private async Task<decimal> GetMonthlyEarningsAsync()
    {
        var earningsElement = Page.GetByTestId("monthly-earnings");
        var earningsText = await earningsElement.InnerTextAsync();
        return decimal.Parse(earningsText.Replace("$", "").Replace(",", ""));
    }
}

public class SessionManagementPage : PartnerBasePage
{
    public async Task CompleteSessionWorkflowAsync(string sessionId)
    {
        // Session preparation
        await NavigateToSessionAsync(sessionId);
        await ReviewClientDetailsAsync(sessionId);
        
        // Session execution simulation
        await StartSessionAsync(sessionId);
        await TakeSessionNotesAsync("Comprehensive technology strategy discussion...");
        
        // Session completion and payment
        await CompleteSessionAsync(sessionId);
        await ValidatePaymentCaptureAsync(sessionId);
    }
    
    public async Task<SessionDetails> GetSessionDetailsAsync(string sessionId)
    {
        await NavigateToSessionAsync(sessionId);
        
        return new SessionDetails
        {
            ClientName = await Page.GetByTestId("client-name").InnerTextAsync(),
            ScheduledTime = await GetScheduledTimeAsync(),
            Topic = await Page.GetByTestId("session-topic").InnerTextAsync(),
            PaymentStatus = await GetPaymentStatusAsync(),
            Notes = await GetExistingNotesAsync()
        };
    }
    
    private async Task ValidatePaymentCaptureAsync(string sessionId)
    {
        // Validate 80/20 revenue split accuracy
        var paymentElement = Page.GetByTestId("session-payment");
        var paymentAmount = await paymentElement.InnerTextAsync();
        
        var expectedPartnerAmount = 640.00m; // 80% of $800
        var actualAmount = decimal.Parse(paymentAmount.Replace("$", ""));
        
        Assert.That(actualAmount, Is.EqualTo(expectedPartnerAmount)
            .Within(0.01m), "Partner revenue should be exactly 80% of session fee");
    }
}
```

#### Admin-Side PageObjects
```csharp
public class AdminDashboard : AdminBasePage
{
    public async Task<PlatformMetrics> GetPlatformMetricsAsync()
    {
        await NavigateToAdminDashboardAsync();
        
        return new PlatformMetrics
        {
            TotalActiveUsers = await GetActiveUserCountAsync(),
            MonthlyRevenue = await GetMonthlyRevenueAsync(),
            PartnerUtilizationRate = await GetAveragePartnerUtilizationAsync(),
            ClientSatisfactionScore = await GetAverageClientSatisfactionAsync(),
            PlatformTransactions = await GetTransactionCountAsync()
        };
    }
    
    // Business intelligence validation methods
    private async Task<int> GetActiveUserCountAsync()
    {
        var element = Page.GetByTestId("active-users-count");
        var count = await element.InnerTextAsync();
        return int.Parse(count.Replace(",", ""));
    }
    
    private async Task<decimal> GetMonthlyRevenueAsync()  
    {
        var element = Page.GetByTestId("monthly-revenue");
        var revenue = await element.InnerTextAsync();
        return decimal.Parse(revenue.Replace("$", "").Replace(",", ""));
    }
    
    public async Task ValidateBusinessMetricsAccuracyAsync()
    {
        var metrics = await GetPlatformMetricsAsync();
        
        // Cross-validate metrics with database queries or API calls
        await ValidateMetricAccuracy("ActiveUsers", metrics.TotalActiveUsers);
        await ValidateMetricAccuracy("MonthlyRevenue", metrics.MonthlyRevenue);
        await ValidateMetricAccuracy("Satisfaction", metrics.ClientSatisfactionScore);
    }
}

public class UserManagementPage : AdminBasePage
{
    public async Task ExecutePartnerApprovalWorkflowAsync()
    {
        await NavigateToUserManagementAsync();
        
        // Get pending partner approvals
        var pendingPartners = await GetPendingPartnerApprovalsAsync();
        Assert.That(pendingPartners.Count, Is.GreaterThan(0), 
            "Should have pending partners for approval testing");
        
        var partnerToApprove = pendingPartners.First();
        
        // Review partner details
        await ReviewPartnerProfileAsync(partnerToApprove.Id);
        await ValidatePartnerSkillsAsync(partnerToApprove.Id);
        await CheckPartnerReferencesAsync(partnerToApprove.Id);
        
        // Approve partner
        await ApprovePartnerAsync(partnerToApprove.Id);
        
        // Validate approval notifications
        await ValidateApprovalEmailSentAsync(partnerToApprove.Email);
        await ValidatePartnerActivationAsync(partnerToApprove.Id);
    }
    
    private async Task<List<PendingPartner>> GetPendingPartnerApprovalsAsync()
    {
        var pendingElements = await Page.GetByTestId("pending-partner").AllAsync();
        var pendingPartners = new List<PendingPartner>();
        
        foreach (var element in pendingElements)
        {
            pendingPartners.Add(new PendingPartner
            {
                Id = await element.GetAttributeAsync("data-partner-id"),
                Name = await element.GetByTestId("partner-name").InnerTextAsync(),
                Email = await element.GetByTestId("partner-email").InnerTextAsync(),
                SubmissionDate = await element.GetByTestId("submission-date").InnerTextAsync()
            });
        }
        
        return pendingPartners;
    }
}
```

## Test Data Management

### Test Data Architecture
```csharp
public class TestDataManager
{
    private static readonly Dictionary<string, object> TestData = new();
    
    public static T GetTestData<T>(string key) where T : class
    {
        return TestData.ContainsKey(key) ? (T)TestData[key] : null;
    }
    
    public static void SetTestData<T>(string key, T data) where T : class
    {
        TestData[key] = data;
    }
    
    // Predefined test scenarios
    public static class ClientScenarios
    {
        public static ClientProfile GetTechStrategyClient() => new()
        {
            Email = "strategic.sarah@techcorp.com",
            FirstName = "Sarah",
            LastName = "Strategic",
            Company = "TechCorp Innovations",
            Industry = "Technology",
            ProblemDescription = "We need comprehensive technology strategy for cloud migration and DevOps implementation."
        };
        
        public static ClientProfile GetFinanceConsultingClient() => new()
        {
            Email = "finance.frank@fintech.com", 
            FirstName = "Frank",
            LastName = "Finance",
            Company = "FinTech Solutions",
            Industry = "Financial Services",
            ProblemDescription = "Seeking regulatory compliance guidance for new payment processing platform."
        };
    }
    
    public static class PartnerScenarios
    {
        public static PartnerProfile GetTechStrategyExpert() => new()
        {
            Email = "expert.emma@consulting.com",
            FirstName = "Emma",
            LastName = "Expert", 
            Title = "Chief Technology Officer",
            Skills = new[] { "Cloud Architecture", "DevOps", "Technology Strategy", "Team Leadership" },
            Experience = "15+ years in technology leadership roles",
            Hourly_Rate = 800.00m,
            Availability = GenerateAvailability()
        };
        
        public static PartnerProfile GetSecuritySpecialist() => new()
        {
            Email = "security.steve@cybersec.com",
            FirstName = "Steve",
            LastName = "Security",
            Title = "Chief Information Security Officer", 
            Skills = new[] { "Cybersecurity", "Compliance", "Risk Management", "Incident Response" },
            Experience = "12+ years in cybersecurity and risk management",
            Hourly_Rate = 800.00m,
            Availability = GenerateAvailability()
        };
    }
}
```

### Test Environment Configuration
```csharp
public class TestConfiguration
{
    public static string GetBaseUrl()
    {
        var environment = Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") ?? "Development";
        
        return environment switch
        {
            "Development" => "https://localhost:8501",
            "Staging" => "https://staging.fx-orleans.com",
            "Production" => "https://fx-orleans.com",
            _ => "https://localhost:8501"
        };
    }
    
    public static BrowserLaunchOptions GetBrowserOptions(string browserType)
    {
        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
        
        return new BrowserLaunchOptions
        {
            Headless = isCI || Environment.GetEnvironmentVariable("HEADLESS") == "true",
            SlowMo = isCI ? 0 : 500,
            Timeout = GetBrowserTimeout(browserType),
            Args = GetBrowserArgs(browserType, isCI)
        };
    }
    
    private static float GetBrowserTimeout(string browserType) => browserType switch
    {
        "chromium" => 90000,
        "firefox" => 120000,
        "webkit" => 120000,
        _ => 90000
    };
}
```

## Performance Testing Architecture

### Concurrent User Simulation
```csharp
[TestFixture]
[Category("Performance")]
public class ConcurrentUserTests : BaseTestSuite
{
    [Test]
    public async Task SimulateConcurrentUsers_PlatformLoad_ValidationAsync()
    {
        var concurrentTasks = new List<Task<TestResult>>();
        var startTime = DateTime.UtcNow;
        
        // Simulate 10 concurrent client journeys
        for (int i = 0; i < 10; i++)
        {
            concurrentTasks.Add(ExecuteClientJourneyAsync($"load.test.client.{i}@example.com"));
        }
        
        // Simulate 5 concurrent partner operations  
        for (int i = 0; i < 5; i++)
        {
            concurrentTasks.Add(ExecutePartnerWorkflowAsync($"load.test.partner.{i}@example.com"));
        }
        
        // Simulate 2 concurrent admin operations
        for (int i = 0; i < 2; i++)
        {
            concurrentTasks.Add(ExecuteAdminOperationsAsync($"load.test.admin.{i}@example.com"));
        }
        
        // Execute all concurrent operations
        var results = await Task.WhenAll(concurrentTasks);
        var endTime = DateTime.UtcNow;
        
        // Performance validation
        var totalDuration = endTime - startTime;
        var successfulOperations = results.Count(r => r.Success);
        var averageResponseTime = results.Average(r => r.ResponseTime.TotalMilliseconds);
        
        // Assertions
        Assert.That(totalDuration.TotalMinutes, Is.LessThan(5), 
            "Concurrent operations should complete within 5 minutes");
        Assert.That(successfulOperations, Is.EqualTo(17), 
            "All concurrent operations should succeed");
        Assert.That(averageResponseTime, Is.LessThan(3000), 
            "Average response time should be under 3 seconds");
    }
    
    private async Task<TestResult> ExecuteClientJourneyAsync(string clientEmail)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await using var browser = await CreateBrowserAsync();
            var page = await browser.NewPageAsync();
            
            var homePage = new ClientHomePage(page);
            var bookingPage = new ClientBookingPage(page);
            var confirmationPage = new ConfirmationPage(page);
            
            // Execute complete client journey
            await homePage.NavigateAsync();
            await homePage.SubmitProblemDescriptionAsync("Load test consultation request");
            await homePage.WaitForPartnerResultsAsync();
            await homePage.SelectPartnerAsync(0);
            
            await bookingPage.CompleteBookingWorkflowAsync(new BookingDetails
            {
                PreferredTime = DateTime.Now.AddDays(1).AddHours(10),
                Topic = "Load test consultation",
                PaymentInfo = TestDataManager.GetValidTestCard()
            });
            
            await confirmationPage.ValidateBookingCompletionAsync();
            
            stopwatch.Stop();
            return new TestResult { Success = true, ResponseTime = stopwatch.Elapsed };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new TestResult 
            { 
                Success = false, 
                ResponseTime = stopwatch.Elapsed,
                Error = ex.Message 
            };
        }
    }
}
```

## Error Handling and Recovery Testing

### Payment Error Scenarios
```csharp
[TestFixture]
[Category("ErrorHandling")]
public class PaymentErrorHandlingTests : BaseTestSuite
{
    [Test]
    public async Task PaymentDeclined_GracefulRecovery_UserExperience()
    {
        // Setup: Navigate to payment step
        await SetupBookingForPaymentTestingAsync();
        
        var paymentPage = new ClientBookingPage(Page);
        
        // Attempt payment with declined card
        await paymentPage.FillPaymentDetailsAsync(
            cardNumber: "4000000000000002", // Stripe test card for declined
            expiryDate: "12/34",
            cvc: "123",
            zipCode: "12345"
        );
        
        await paymentPage.SubmitPaymentAsync();
        
        // Validate error handling
        await paymentPage.WaitForPaymentErrorAsync();
        var errorMessage = await paymentPage.GetPaymentErrorMessageAsync();
        
        Assert.That(errorMessage, Does.Contain("declined").Or.Contain("failed"),
            "User should see clear error message for declined payment");
        
        // Validate booking state preservation
        var bookingDetails = await paymentPage.GetCurrentBookingDetailsAsync();
        Assert.That(bookingDetails.PartnerName, Is.Not.Null,
            "Partner selection should be preserved after payment error");
        Assert.That(bookingDetails.ScheduledTime, Is.Not.Null,
            "Scheduled time should be preserved after payment error");
        
        // Test recovery with valid card
        await paymentPage.FillPaymentDetailsAsync(
            cardNumber: "4242424242424242", // Stripe test card for success
            expiryDate: "12/34", 
            cvc: "123",
            zipCode: "12345"
        );
        
        await paymentPage.SubmitPaymentAsync();
        await paymentPage.WaitForPaymentSuccessAsync();
        
        // Validate successful recovery
        var confirmationPage = new ConfirmationPage(Page);
        await confirmationPage.AssertBookingConfirmationAsync();
    }
    
    [Test]
    public async Task ServiceUnavailable_GracefulDegradation_Testing()
    {
        // Test scenarios where external services are unavailable
        var testScenarios = new[]
        {
            "OpenAI API timeout during partner matching",
            "Stripe payment processing delay", 
            "Google Calendar API unavailable",
            "Keycloak authentication service down"
        };
        
        foreach (var scenario in testScenarios)
        {
            await ExecuteServiceUnavailabilityTest(scenario);
        }
    }
    
    private async Task ExecuteServiceUnavailabilityTest(string scenario)
    {
        // Mock service unavailability and test graceful degradation
        // Validate user experience during service outages
        // Ensure data consistency and recovery mechanisms
    }
}
```

## Mobile Responsiveness Testing

### Mobile Test Configuration  
```csharp
[TestFixture] 
[Category("Mobile")]
public class MobileResponsivenessTests : BaseTestSuite
{
    private static readonly Dictionary<string, ViewportSize> MobileViewports = new()
    {
        { "iPhone 13", new ViewportSize { Width = 390, Height = 844 } },
        { "iPhone 13 Mini", new ViewportSize { Width = 375, Height = 812 } },
        { "Samsung Galaxy S21", new ViewportSize { Width = 360, Height = 800 } },
        { "iPad Air", new ViewportSize { Width = 820, Height = 1180 } },
        { "iPad Mini", new ViewportSize { Width = 768, Height = 1024 } }
    };
    
    [Test]
    [TestCaseSource(nameof(GetMobileViewports))]
    public async Task ClientBookingWorkflow_MobileDevice_ResponsiveExperience(
        string deviceName, ViewportSize viewport)
    {
        // Set mobile viewport
        await Page.SetViewportSizeAsync(viewport.Width, viewport.Height);
        
        // Execute client journey on mobile
        var homePage = new ClientHomePage(Page);
        await homePage.NavigateAsync();
        await homePage.TakeScreenshotAsync($"mobile-{deviceName}-home");
        
        // Test touch interactions
        await ValidateTouchInteractionsAsync(homePage);
        
        // Test responsive partner selection
        await homePage.SubmitProblemDescriptionAsync("Mobile consultation request");
        await homePage.WaitForPartnerResultsAsync();
        await homePage.TakeScreenshotAsync($"mobile-{deviceName}-partners");
        
        // Validate mobile-optimized partner cards
        await ValidateMobilePartnerCardsAsync();
        
        // Test mobile booking flow
        await homePage.SelectPartnerAsync(0);
        var bookingPage = new ClientBookingPage(Page);
        await bookingPage.TakeScreenshotAsync($"mobile-{deviceName}-booking");
        
        // Validate mobile payment form
        await ValidateMobilePaymentFormAsync(bookingPage);
    }
    
    private async Task ValidateTouchInteractionsAsync(ClientHomePage homePage)
    {
        // Test touch-specific interactions
        await homePage.TestProblemDescriptionTextareaExpandsOnTouchAsync();
        await homePage.TestDropdownMenusWorkWithTouchAsync();
        await homePage.TestButtonsTapResponsivelyAsync();
    }
    
    private async Task ValidateMobilePartnerCardsAsync()
    {
        var partnerCards = await Page.GetByTestId("partner-card").AllAsync();
        
        foreach (var card in partnerCards)
        {
            // Validate card is fully visible and tappable
            var boundingBox = await card.BoundingBoxAsync();
            Assert.That(boundingBox?.Width, Is.GreaterThan(300),
                "Partner cards should be wide enough for mobile viewing");
            
            // Test card expansion on tap
            await card.ClickAsync();
            await Task.Delay(500); // Wait for expansion animation
            
            var expandedHeight = await card.BoundingBoxAsync();
            Assert.That(expandedHeight?.Height, Is.GreaterThan(boundingBox?.Height),
                "Partner cards should expand when tapped on mobile");
        }
    }
    
    public static IEnumerable<TestCaseData> GetMobileViewports()
    {
        foreach (var viewport in MobileViewports)
        {
            yield return new TestCaseData(viewport.Key, viewport.Value)
                .SetName($"Mobile_{viewport.Key.Replace(" ", "_")}");
        }
    }
}
```

## CI/CD Integration Architecture

### GitHub Actions Pipeline Configuration
```yaml
# .github/workflows/e2e-tests.yml
name: E2E Testing Suite

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  e2e-smoke-tests:
    runs-on: ubuntu-latest
    name: Smoke Tests (Quick Validation)
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
          
      - name: Install Playwright Browsers
        run: npx playwright install --with-deps
        
      - name: Start Application Services
        run: |
          docker-compose up -d postgres keycloak
          sleep 30 # Wait for services to be ready
          
      - name: Run Smoke Tests
        run: |
          cd tests/FxExpert.E2E.Tests
          dotnet test --filter "Category=Smoke" --logger "trx" --results-directory ./TestResults
        env:
          HEADLESS: true
          TEST_ENVIRONMENT: CI
          
      - name: Upload Test Results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: smoke-test-results
          path: tests/FxExpert.E2E.Tests/TestResults/
          
  e2e-full-regression:
    runs-on: ubuntu-latest
    needs: e2e-smoke-tests
    name: Full Regression Suite
    strategy:
      matrix:
        browser: [chromium, firefox, webkit]
        
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
          
      - name: Install Playwright Browsers
        run: npx playwright install --with-deps ${{ matrix.browser }}
        
      - name: Start Application Services
        run: |
          docker-compose up -d
          sleep 60 # Extended wait for full stack
          
      - name: Run Full Test Suite
        run: |
          cd tests/FxExpert.E2E.Tests
          dotnet test --filter "Category!=Manual" --logger "trx" --results-directory ./TestResults
        env:
          BROWSER: ${{ matrix.browser }}
          HEADLESS: true
          TEST_ENVIRONMENT: CI
          
      - name: Upload Test Results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: full-test-results-${{ matrix.browser }}
          path: tests/FxExpert.E2E.Tests/TestResults/
          
  e2e-performance-validation:
    runs-on: ubuntu-latest
    needs: e2e-smoke-tests
    name: Performance Validation
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
          
      - name: Install Playwright Browsers
        run: npx playwright install --with-deps chromium
        
      - name: Start Application Services
        run: |
          docker-compose up -d
          sleep 60
          
      - name: Run Performance Tests
        run: |
          cd tests/FxExpert.E2E.Tests  
          dotnet test --filter "Category=Performance" --logger "trx" --results-directory ./TestResults
        env:
          HEADLESS: true
          TEST_ENVIRONMENT: CI
          
      - name: Upload Performance Results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: performance-test-results
          path: tests/FxExpert.E2E.Tests/TestResults/
```

### Test Result Reporting
```csharp
public class TestResultReporter
{
    public static async Task GenerateHtmlReportAsync(List<TestResult> results)
    {
        var report = new StringBuilder();
        report.AppendLine("<!DOCTYPE html>");
        report.AppendLine("<html><head><title>E2E Test Results</title></head><body>");
        
        // Executive Summary
        var totalTests = results.Count;
        var passedTests = results.Count(r => r.Success);
        var failedTests = totalTests - passedTests;
        var passRate = (double)passedTests / totalTests * 100;
        
        report.AppendLine($"<h1>E2E Test Results Summary</h1>");
        report.AppendLine($"<p>Total Tests: {totalTests}</p>");
        report.AppendLine($"<p>Passed: {passedTests}</p>");
        report.AppendLine($"<p>Failed: {failedTests}</p>");
        report.AppendLine($"<p>Pass Rate: {passRate:F1}%</p>");
        
        // Detailed Results by Category
        var groupedResults = results.GroupBy(r => r.Category);
        
        foreach (var group in groupedResults)
        {
            report.AppendLine($"<h2>{group.Key} Tests</h2>");
            report.AppendLine("<table border='1'>");
            report.AppendLine("<tr><th>Test</th><th>Result</th><th>Duration</th><th>Details</th></tr>");
            
            foreach (var result in group)
            {
                var status = result.Success ? "PASS" : "FAIL";
                var statusColor = result.Success ? "green" : "red";
                
                report.AppendLine($"<tr>");
                report.AppendLine($"<td>{result.TestName}</td>");
                report.AppendLine($"<td style='color:{statusColor}'>{status}</td>");
                report.AppendLine($"<td>{result.Duration.TotalSeconds:F1}s</td>");
                report.AppendLine($"<td>{result.Details ?? "N/A"}</td>");
                report.AppendLine($"</tr>");
            }
            
            report.AppendLine("</table>");
        }
        
        report.AppendLine("</body></html>");
        
        await File.WriteAllTextAsync("TestResults/test-report.html", report.ToString());
    }
}
```

## Monitoring and Observability

### Test Execution Monitoring
```csharp
public class TestExecutionMonitor
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    
    public static async Task<TestMetrics> MonitorTestExecutionAsync(
        Func<Task> testExecution)
    {
        var stopwatch = Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(false);
        
        var metrics = new TestMetrics
        {
            StartTime = DateTime.UtcNow
        };
        
        try
        {
            await testExecution();
            metrics.Success = true;
        }
        catch (Exception ex)
        {
            metrics.Success = false;
            metrics.ErrorMessage = ex.Message;
            Logger.Error(ex, "Test execution failed");
        }
        finally
        {
            stopwatch.Stop();
            metrics.Duration = stopwatch.Elapsed;
            metrics.MemoryUsage = GC.GetTotalMemory(false) - memoryBefore;
            metrics.EndTime = DateTime.UtcNow;
            
            Logger.Info($"Test completed in {metrics.Duration.TotalSeconds:F1}s, " +
                       $"Memory usage: {metrics.MemoryUsage / 1024 / 1024:F1}MB");
        }
        
        return metrics;
    }
}
```

## Security Testing Integration

### Authentication Security Validation
```csharp
[TestFixture]
[Category("Security")]
public class SecurityValidationTests : BaseTestSuite
{
    [Test]
    public async Task AuthenticationSecurity_SessionManagement_Validation()
    {
        var homePage = new ClientHomePage(Page);
        await homePage.NavigateAsync();
        
        // Test session timeout
        await ValidateSessionTimeoutAsync();
        
        // Test authentication token security
        await ValidateTokenSecurityAsync();
        
        // Test cross-site scripting protection
        await ValidateXSSProtectionAsync();
        
        // Test CSRF protection
        await ValidateCSRFProtectionAsync();
    }
    
    private async Task ValidateSessionTimeoutAsync()
    {
        // Validate session expires after inactivity
        // Test automatic logout functionality
        // Verify session data is properly cleared
    }
    
    private async Task ValidateTokenSecurityAsync()
    {
        // Validate JWT tokens are properly secured
        // Test token refresh mechanisms
        // Verify token expiration handling
    }
}
```

This technical specification provides the comprehensive foundation needed to implement the E2E testing suite. The architecture supports multi-persona testing, performance validation, error handling, mobile responsiveness, and integration with modern CI/CD pipelines while maintaining the existing OAuth authentication patterns and browser compatibility requirements.