#!/bin/bash

echo "🧪 Testing OAuth Browser Visibility"
echo "====================================="

# Set environment variables for headed mode
export PLAYWRIGHT_HEADLESS=false
export BROWSER_LAUNCH_TIMEOUT=30000

echo "🔧 Environment configured:"
echo "  PLAYWRIGHT_HEADLESS=false"
echo "  BROWSER_LAUNCH_TIMEOUT=30000"
echo ""

echo "🚀 Starting OAuth test with visible browser..."
echo "   The browser window should appear shortly."
echo ""

# Run a specific authentication test
dotnet test --filter "AuthenticationPage_ShouldExtendBasePage" \
  --logger "console;verbosity=detailed"

echo ""
echo "✅ Test completed. Did you see the browser window?"
echo ""
echo "💡 If the browser didn't appear, try:"
echo "   1. Make sure browsers are installed: npx playwright install"
echo "   2. Check firewall settings aren't blocking browser launch"
echo "   3. Try running: PLAYWRIGHT_HEADLESS=false dotnet test"