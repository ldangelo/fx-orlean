# Payment Integration Testing Guide

## Prerequisites

Before testing the payment integration, ensure you have:

1. **Stripe Test Account**
   - Sign up at https://stripe.com if you don't have an account
   - Get your test API keys from the Stripe Dashboard

2. **Environment Variables**
   - Update `.envrc` with your actual Stripe test keys:
   ```bash
   export STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_ACTUAL_PUBLISHABLE_KEY_HERE
   export STRIPE_SECRET_KEY=sk_test_YOUR_ACTUAL_SECRET_KEY_HERE
   ```
   - Run `direnv reload` or `source .envrc` to load the variables

## Test Credit Cards

Stripe provides test credit card numbers that simulate different scenarios:

### Successful Payments
- **Visa**: `4242424242424242`
- **Visa (debit)**: `4000056655665556`
- **Mastercard**: `5555555555554444`
- **American Express**: `378282246310005`

### Failed Payments (for testing error handling)
- **Card declined**: `4000000000000002`
- **Insufficient funds**: `4000000000009995`
- **Expired card**: `4000000000000069`
- **Incorrect CVC**: `4000000000000127`

### Test Details for All Cards
- **Expiry**: Any future date (e.g., `12/34`)
- **CVC**: Any 3-digit number (e.g., `123`)
- **ZIP**: Any 5-digit number (e.g., `12345`)

## Testing Workflow

### 1. Start the Application
```bash
# Start EventServer
dotnet watch --project src/EventServer/EventServer.csproj

# Start Blazor UI (in another terminal)
dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj
```

### 2. Test Payment Flow
1. Navigate to the home page
2. Enter a problem description and submit
3. Select a partner from the AI recommendations
4. Click "Schedule a Consultation"
5. Fill in date, time, and meeting topic
6. Click "Proceed to Payment"
7. Enter test card details using the numbers above
8. Click "Authorize Payment"

### 3. Expected Results

#### Successful Payment
- Payment form shows "Processing Payment..." then completes
- User is redirected to confirmation page
- Stripe dashboard shows authorized payment (not captured)
- Console logs show successful payment intent creation

#### Failed Payment
- Error message displays explaining the failure
- User remains on payment form to retry
- No booking is created
- No navigation to confirmation page

## Monitoring and Debugging

### Stripe Dashboard
- View test payments: https://dashboard.stripe.com/test/payments
- Check payment intents and their status
- Review webhooks (if configured)

### Application Logs
- EventServer console shows payment-related API calls
- Browser console shows client-side payment processing
- Network tab shows API requests and responses

### Common Issues and Solutions

1. **"Payment system not configured"**
   - Verify STRIPE_PUBLISHABLE_KEY is set in environment
   - Check the /api/payment/config/publishable-key endpoint

2. **"Failed to create payment intent"**
   - Verify STRIPE_SECRET_KEY is set in EventServer environment
   - Check EventServer logs for Stripe API errors

3. **Payment form doesn't load**
   - Verify Stripe JavaScript SDK is loaded
   - Check browser console for JavaScript errors
   - Ensure stripe.js file is accessible

4. **Payment succeeds but booking fails**
   - Check conference creation API endpoint
   - Verify calendar API integration
   - Review EventServer logs for database errors

## Security Notes

- Never use real credit card numbers in test environment
- Test keys (pk_test_* and sk_test_*) only work with test cards
- Production keys will reject test card numbers
- Always use HTTPS in production environment

## Next Steps After Testing

1. Set up Stripe webhooks for payment status updates
2. Implement payment capture after successful consultation
3. Add payment refund capability for cancellations
4. Set up monitoring and alerting for payment failures