#!/bin/bash

# Advanced Partner Filter E2E Test Runner
# This script runs the comprehensive E2E tests for the Advanced Partner Search feature

echo "🚀 Starting Advanced Partner Filter E2E Tests"
echo "================================================"

# Set test environment variables
export PLAYWRIGHT_BROWSERS_PATH="${HOME}/.cache/ms-playwright"
export ASPNETCORE_ENVIRONMENT="Development"

# Ensure browsers are installed
echo "📦 Checking Playwright browser installation..."
pwsh bin/Debug/net9.0/playwright.ps1 install

# Create screenshot directories
mkdir -p screenshots/advanced-filtering

# Run the advanced filtering tests
echo "🧪 Running Advanced Partner Filtering E2E Tests..."

dotnet test \
    --filter "Category=AdvancedFiltering" \
    --logger "console;verbosity=detailed" \
    --logger "trx;LogFileName=AdvancedFilteringTests.trx" \
    --results-directory TestResults \
    --collect:"XPlat Code Coverage" \
    -- RunConfiguration.TestSessionTimeout=600000

echo ""
echo "📊 Test Results Summary"
echo "======================="

if [ $? -eq 0 ]; then
    echo "✅ All Advanced Partner Filtering tests passed!"
else
    echo "❌ Some tests failed. Check the detailed output above."
fi

echo ""
echo "📁 Test artifacts:"
echo "   • Screenshots: screenshots/advanced-filtering/"
echo "   • Test results: TestResults/"
echo "   • Coverage reports: TestResults/"

echo ""
echo "🏃‍♂️ Running specific test categories:"
echo ""

# Run P0 tests (critical functionality)
echo "🔴 P0 Tests (Critical Functionality):"
dotnet test \
    --filter "Category=AdvancedFiltering&Category=P0" \
    --logger "console;verbosity=normal" \
    --no-build

echo ""

# Run Mobile tests
echo "📱 Mobile Tests:"
dotnet test \
    --filter "Category=AdvancedFiltering&Category=Mobile" \
    --logger "console;verbosity=normal" \
    --no-build

echo ""

# Run Performance tests
echo "⚡ Performance Tests:"
dotnet test \
    --filter "Category=AdvancedFiltering&Category=Performance" \
    --logger "console;verbosity=normal" \
    --no-build

echo ""
echo "🎯 Advanced Partner Filter E2E Testing Complete!"
echo "Check screenshots/advanced-filtering/ for visual test evidence."