#!/bin/bash

# Payment Integration Test Script
# This script tests the payment endpoints without needing the full UI

echo "🧪 Testing Payment Integration"
echo "==============================="

# Check if EventServer is running
echo "1. Checking if EventServer is running..."
if ! curl -s http://localhost:5032/health > /dev/null; then
    echo "❌ EventServer is not running. Please start it first:"
    echo "   dotnet watch --project src/EventServer/EventServer.csproj"
    exit 1
fi
echo "✅ EventServer is running"

# Test payment configuration endpoint
echo ""
echo "2. Testing payment configuration..."
PAYMENT_CONFIG=$(curl -s http://localhost:5032/api/payment/config/status)
echo "Payment Config Response: $PAYMENT_CONFIG"

if echo "$PAYMENT_CONFIG" | grep -q '"publishableKeyConfigured":true'; then
    echo "✅ Stripe publishable key is configured"
else
    echo "❌ Stripe publishable key is not configured"
    echo "Please set STRIPE_PUBLISHABLE_KEY in your environment"
fi

if echo "$PAYMENT_CONFIG" | grep -q '"secretKeyConfigured":true'; then
    echo "✅ Stripe secret key is configured"
else
    echo "❌ Stripe secret key is not configured"
    echo "Please set STRIPE_SECRET_KEY in your environment"
fi

# Test payment intent creation
echo ""
echo "3. Testing payment intent creation..."
PAYMENT_INTENT_RESPONSE=$(curl -s -X POST \
  http://localhost:5032/payments/create-intent \
  -H "Content-Type: application/json" \
  -d '{"amount": 800.00, "currency": "usd"}')

echo "Payment Intent Response: $PAYMENT_INTENT_RESPONSE"

if echo "$PAYMENT_INTENT_RESPONSE" | grep -q '"paymentIntentId"'; then
    echo "✅ Payment intent created successfully"
    PAYMENT_INTENT_ID=$(echo "$PAYMENT_INTENT_RESPONSE" | grep -o '"paymentIntentId":"[^"]*"' | cut -d'"' -f4)
    echo "Payment Intent ID: $PAYMENT_INTENT_ID"
else
    echo "❌ Failed to create payment intent"
fi

echo ""
echo "🎉 Payment integration test complete!"
echo ""
echo "Next steps:"
echo "1. Update .envrc with your real Stripe test keys:"
echo "   - Get keys from https://dashboard.stripe.com/test/apikeys"
echo "   - Replace pk_test_YOUR_ACTUAL_PUBLISHABLE_KEY_HERE"
echo "   - Replace sk_test_YOUR_ACTUAL_SECRET_KEY_HERE"
echo "   - Run: direnv reload"
echo ""
echo "2. Test the full payment flow in the browser:"
echo "   - Start Blazor: dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj"
echo "   - Navigate to home page and test booking flow"
echo "   - Use test card: 4242424242424242, exp: 12/34, CVC: 123"