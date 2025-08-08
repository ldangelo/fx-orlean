# FX-Orleans E2E Testing Guide

> **Status**: ✅ Production Ready with Google OAuth Integration  
> **Last Updated**: 2025-08-08  
> **Browsers Supported**: Chromium, Firefox, WebKit  

## Overview

The FX-Orleans E2E test suite provides comprehensive end-to-end testing with integrated Google OAuth authentication. Tests cover critical user journeys including booking workflows, payment authorization, and AI partner matching across multiple browsers.

## Quick Start

### Prerequisites

1. **.NET 9.0 SDK** installed
2. **Node.js** (for Playwright browser installation)
3. **FX-Orleans application** running on `https://localhost:8501`
4. **Google account** for OAuth authentication during tests

### Run All Tests

```bash
# Navigate to E2E test directory
cd tests/FxExpert.E2E.Tests

# Install Playwright browsers (first time only)
npx playwright install

# Run OAuth tests with visible browser (RECOMMENDED for OAuth)
dotnet test

# Run ONLY visible browser tests (best for OAuth manual authentication)
dotnet test --filter "Category=VisibleBrowser"

# Run traditional PageTest-based tests (headless by default)
dotnet test --filter "Category!=VisibleBrowser"

# Run in headless mode (for CI/CD)
PLAYWRIGHT_HEADLESS=true dotnet test --filter "Category!=VisibleBrowser"
```

### Run Specific Test Categories

```bash
# Visible browser tests (RECOMMENDED for OAuth authentication)
dotnet test --filter "Category=VisibleBrowser"

# Authentication tests only (both visible and headless)
dotnet test --filter "Category=Authentication"

# Cross-browser tests only  
dotnet test --filter "Category=Cross-Browser"

# P0 critical user journey tests (with visible browser support)
dotnet test --filter "Category=P0"

# Error handling tests (with visible browser support)
dotnet test --filter "Category=Error-Handling"

# Run specific visible browser test suites
dotnet test --filter "TestName~WithVisibleBrowser"
```

## OAuth Authentication Flow

### How OAuth Integration Works

1. **Test starts** → Navigates to FX-Orleans application
2. **Keycloak redirect detected** → Application redirects to authentication
3. **Manual login required** → Test pauses and waits for user authentication
4. **User completes OAuth** → Login with Google account in browser
5. **Authentication validated** → Test detects completion and continues
6. **Test proceeds** → Continues with booking/payment/matching workflows

### OAuth Test Behavior

**⚠️ Manual Interaction Required**: OAuth tests require you to manually log in with your Google account when the browser opens.

**🔍 Browser Visibility**: Tests run in **headed mode** by default so you can see the browser and complete OAuth authentication.

**Expected Flow**:
```
🔄 Test starts
📱 Browser window opens (visible) to FX-Orleans application  
🔐 Keycloak login page appears
👤 MANUAL STEP: Click "Login with Google" and complete authentication
✅ Test automatically detects authentication completion
🚀 Test continues with business logic
```

### Troubleshooting OAuth Browser Issues

If the browser is not appearing for OAuth authentication:

**✅ SOLUTION: Use Visible Browser Tests** (Recommended):
```bash
# Run visible browser tests (solves browser visibility issues)
dotnet test --filter "Category=VisibleBrowser"

# Run specific visible browser OAuth test
dotnet test --filter "AuthenticationPageTestsWithVisibleBrowser"

# Run P0 tests with visible browsers
dotnet test --filter "UserJourneyTestsWithVisibleBrowser"
```

**Alternative approaches** (if visible browser tests don't work):
```bash
# Ensure headed mode is enabled for PageTest-based tests
PLAYWRIGHT_HEADLESS=false dotnet test --filter "Category!=VisibleBrowser"

# Run a specific OAuth test to verify browser appears
dotnet test --filter "AuthenticationPage_ShouldExtendBasePage"

# Check if browsers are properly installed
npx playwright install
```

**🔧 Technical Background**: 
- Playwright NUnit's `PageTest` base class runs in headless mode by default and may not respect configuration files
- The new "VisibleBrowser" test category uses manual browser management to ensure browser visibility
- This approach bypasses PageTest limitations and guarantees visible browsers for OAuth authentication

## Test Categories

### 🔐 Authentication Tests

Test OAuth flow functionality and configuration management.

```bash
# Core OAuth functionality
dotnet test --filter "TestName~AuthenticationPage"

# Configuration management  
dotnet test --filter "TestName~AuthenticationConfiguration"

# Error handling
dotnet test --filter "TestName~HandleGoogleOAuth"
```

**Key Tests**:
- `AuthenticationPage_ShouldExtendBasePage` - Page object structure
- `HandleGoogleOAuthAsync_WhenTimeoutOccurs_ShouldReturnFalse` - Timeout handling
- `LoadAuthenticationConfig_WithValidConfiguration_ShouldReturnConfiguration` - Config loading

### 🌐 Cross-Browser Tests

Validate OAuth authentication works across different browser engines.

```bash
# All cross-browser tests
dotnet test --filter "Category=Cross-Browser"

# Specific browser tests
dotnet test --filter "TestName~Chromium"
dotnet test --filter "TestName~Firefox"  
dotnet test --filter "TestName~WebKit"

# Browser configuration validation
dotnet test --filter "ValidateBrowserConfigurations"
```

**Browser Performance Profiles**:
- **Chromium**: 90s timeout, 95% reliability, 3 concurrent instances
- **Firefox**: 120s timeout, 80% reliability, 1 concurrent instance
- **WebKit**: 120s timeout, 80% reliability, 1 concurrent instance

### 🎯 P0 Critical Tests

Core business workflow tests with OAuth integration.

```bash
# All P0 tests
dotnet test --filter "Category=P0"

# Individual workflows
dotnet test --filter "CompleteBookingWorkflow_NewUser_ShouldSucceed"
dotnet test --filter "PaymentAuthorization_WithValidCard_ShouldSucceed"
dotnet test --filter "AIPartnerMatching_WithTechProblem_ShouldReturnRelevantExperts"
```

**P0 Test Structure**:
```
Step 0: 🔐 OAuth Authentication (Manual)
Step 1: 🏠 Navigate to Home Page  
Step 2: 📝 Fill Problem Statement
Step 3: 🤖 AI Partner Matching
Step 4: 👥 Select Partner
Step 5: 📅 Book Consultation
Step 6: 💳 Payment Authorization
Step 7: ✅ Confirmation
```

### ⚠️ Error Handling Tests

Test OAuth error scenarios and recovery mechanisms.

```bash
# Error handling tests
dotnet test --filter "Category=Error-Handling"

# Timeout scenarios
dotnet test --filter "TestName~Timeout"

# Network error scenarios
dotnet test --filter "TestName~NetworkError"
```

## Configuration

### Environment Configuration

The tests support multiple environments with different configurations:

```bash
# Development (default)
dotnet test

# CI Environment  
dotnet test -e ASPNETCORE_ENVIRONMENT=CI

# Local testing with custom settings
dotnet test -e ASPNETCORE_ENVIRONMENT=Local
```

### User Secrets Configuration

For secure credential management during development:

```bash
# Initialize user secrets (first time only)
dotnet user-secrets init

# Set authentication mode (manual recommended)
dotnet user-secrets set "Authentication:Mode" "Manual"

# Set custom timeout if needed
dotnet user-secrets set "Authentication:Timeout" "60000"
```

### Environment Variables

Alternative to User Secrets for CI/CD:

```bash
export AUTHENTICATION_MODE="Manual"
export AUTHENTICATION_TIMEOUT="60000" 
export ASPNETCORE_ENVIRONMENT="Development"
```

## Browser Management

### Installing Browsers

```bash
# Install all Playwright browsers
npx playwright install

# Install specific browsers only
npx playwright install chromium firefox webkit
```

### Browser-Specific Testing

```bash
# Test in specific browser only
dotnet test --filter "TestName~Chromium"

# Run individual browser authentication test
dotnet test --filter "RunIndividualBrowserAuthenticationTest(\"Chromium\")"
```

### Headless vs Headed Mode

Tests run in **headless mode** by default for CI/CD. For debugging with OAuth authentication:

**Debugging Configuration** (edit test files):
```csharp
var launchOptions = new BrowserTypeLaunchOptions
{
    Headless = false,  // Show browser window
    SlowMo = 1000     // Slow down actions for debugging
};
```

## Debugging and Troubleshooting

### Common Issues

#### ❌ "Connection Refused" Errors
**Problem**: `net::ERR_CONNECTION_REFUSED at https://localhost:8501/`  
**Solution**: Start the FX-Orleans application server:

```bash
# From project root
dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj
```

#### ❌ OAuth Timeout Errors
**Problem**: Test times out during OAuth authentication  
**Solutions**:
- Complete Google login faster (60-120 second timeout)
- Check browser isn't blocking popups
- Verify Google account credentials are working

#### ❌ Browser Not Found Errors
**Problem**: `Browser executable not found`  
**Solution**: Install Playwright browsers:
```bash
npx playwright install
```

### Debug Screenshots

Tests automatically capture screenshots during OAuth flows:

```
📁 tests/FxExpert.E2E.Tests/screenshots/
├── oauth-timeout-20250808-143022.png
├── cross-browser-chromium-complete-20250808-143045.png  
└── auth-completion-error-20250808-143102.png
```

### Verbose Logging

For detailed test execution logs:

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Test Reports

Generate comprehensive test reports:

```bash
# HTML report
dotnet test --logger html --results-directory ./TestResults

# JUnit XML for CI/CD
dotnet test --logger "junit;LogFilePath=./TestResults/results.xml"
```

## CI/CD Integration

### Azure DevOps Pipeline

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Install Playwright Browsers'
  inputs:
    command: 'custom'
    custom: 'exec'
    arguments: 'npx playwright install'

- task: DotNetCoreCLI@2
  displayName: 'Run E2E Tests'
  inputs:
    command: 'test'
    projects: 'tests/FxExpert.E2E.Tests/FxExpert.E2E.Tests.csproj'
    arguments: '--logger "trx" --results-directory $(Agent.TempDirectory)'
```

### GitHub Actions

```yaml
- name: Install Playwright Browsers
  run: npx playwright install

- name: Run E2E Tests  
  run: dotnet test tests/FxExpert.E2E.Tests/FxExpert.E2E.Tests.csproj --logger "github"
```

### Docker Testing

```dockerfile
FROM mcr.microsoft.com/playwright/dotnet:v1.40.0-focal

WORKDIR /app
COPY . .

RUN dotnet restore tests/FxExpert.E2E.Tests/FxExpert.E2E.Tests.csproj
RUN dotnet build tests/FxExpert.E2E.Tests/FxExpert.E2E.Tests.csproj

# Note: OAuth tests require manual interaction, use configuration tests in Docker
CMD ["dotnet", "test", "tests/FxExpert.E2E.Tests/FxExpert.E2E.Tests.csproj", "--filter", "Category!=Authentication"]
```

## Performance Optimization

### Parallel Test Execution

```bash
# Run tests in parallel (careful with OAuth flows)
dotnet test --parallel

# Limit parallelism for OAuth tests
dotnet test -m:1
```

### Browser Resource Management

The test suite automatically manages browser resources:

- **Chromium**: Up to 3 concurrent instances
- **Firefox**: 1 instance (stability)
- **WebKit**: 1 instance (stability)

### Test Categories for CI

For CI/CD pipelines, separate tests by requirement:

```bash
# Infrastructure tests (no server needed)
dotnet test --filter "Category=Configuration|Category=Cross-Browser&TestName~Validate"

# Full integration tests (requires server + manual OAuth)
dotnet test --filter "Category=P0|Category=Authentication"
```

## Development Workflow

### Adding New Tests

1. **Follow Page Object Model pattern**:
```csharp
public class NewFeaturePage : BasePage
{
    public NewFeaturePage(IPage page) : base(page) { }
    
    public async Task NavigateToNewFeatureAsync()
    {
        await NavigateAsync("/new-feature");
    }
}
```

2. **Include OAuth authentication for user journey tests**:
```csharp
[Test]
public async Task NewFeatureWorkflow_WithAuthentication_ShouldSucceed()
{
    // Step 0: OAuth Authentication
    var authResult = await _authPage.HandleGoogleOAuthAsync();
    authResult.Should().BeTrue("OAuth authentication should succeed");
    
    // Continue with feature testing...
}
```

3. **Add appropriate test categories**:
```csharp
[Test]
[Category("P0")]
[Category("NewFeature")]
public async Task NewFeature_Test() { }
```

### Test Organization

```
📁 tests/FxExpert.E2E.Tests/
├── 📁 Configuration/           # Authentication config management
├── 📁 PageObjectModels/        # Page objects with OAuth support
├── 📁 Services/               # Cross-browser and error handling services  
├── 📁 Tests/                  # Test implementations
│   ├── AuthenticationPageTests.cs                      # OAuth unit tests (headless)
│   ├── AuthenticationPageTestsWithVisibleBrowser.cs    # OAuth unit tests (visible)
│   ├── AuthenticationErrorHandlingTests.cs             # Error scenarios (headless)
│   ├── AuthenticationErrorHandlingTestsWithVisibleBrowser.cs  # Error scenarios (visible)
│   ├── CrossBrowserAuthenticationTests.cs              # Multi-browser tests (headless)
│   ├── CrossBrowserAuthenticationTestsWithVisibleBrowser.cs   # Multi-browser tests (visible)
│   ├── CrossBrowserTestRunner.cs                       # Browser orchestration
│   ├── UserJourneyTests.cs                            # P0 integration tests (headless)
│   └── UserJourneyTestsWithVisibleBrowser.cs          # P0 integration tests (visible)
└── 📁 screenshots/           # Debug screenshots
```

**Test Variants**:
- **Regular tests**: Use Playwright NUnit `PageTest` base class (may run headless)
- **VisibleBrowser tests**: Use manual browser management (guaranteed visible browser windows)
- **Recommendation**: Use VisibleBrowser variants for OAuth authentication

## Security Considerations

### ⚠️ Important Security Notes

1. **No credentials in version control** - Use User Secrets or environment variables
2. **Manual OAuth only** - Automated credentials not supported for security
3. **Session isolation** - Each test gets fresh browser context
4. **Screenshot privacy** - Screenshots may contain sensitive information

### Secure Configuration

```bash
# ✅ Good: User Secrets
dotnet user-secrets set "Authentication:Mode" "Manual"

# ✅ Good: Environment Variables  
export AUTHENTICATION_MODE="Manual"

# ❌ Bad: Hardcoded in appsettings.json (committed to git)
```

## Support and Troubleshooting

### Getting Help

1. **Check test output** for specific error messages
2. **Review debug screenshots** in `screenshots/` directory  
3. **Run with verbose logging** using `--logger "console;verbosity=detailed"`
4. **Validate OAuth configuration** using configuration tests

### Common Solutions

| Problem | Solution |
|---------|----------|
| Connection refused | Start FX-Orleans application server |
| OAuth timeout | Complete Google login within timeout period |
| Browser not found | Run `npx playwright install` |
| Permission denied | Check User Secrets or environment variables |
| Test hangs | Check for manual OAuth interaction required |

### Test Health Check

Quick validation that E2E infrastructure is working:

```bash
# Test basic infrastructure with visible browser (RECOMMENDED)
dotnet test --filter "AuthenticationPage_ShouldExtendBasePage_WithVisibleBrowser"

# Test browser visibility (verify browser window appears)
dotnet test --filter "Category=VisibleBrowser" --filter "TestName~ShouldExtendBasePage"

# Test basic infrastructure (no server required, headless)
dotnet test --filter "AuthenticationPage_ShouldExtendBasePage|ValidateBrowserConfigurations"

# Expected result: Tests passed with visible browser window appearing
```

---

## 📚 Additional Resources

- **OAuth Implementation Spec**: `.agent-os/specs/2025-08-07-e2e-google-oauth-auth/spec.md`
- **Validation Report**: `OAuth_Implementation_Validation_Report.md`  
- **Playwright Documentation**: https://playwright.dev/dotnet
- **Project Architecture**: `CLAUDE.md`

For questions or issues, refer to the comprehensive validation report or the OAuth implementation specification in the `.agent-os/specs/` directory.