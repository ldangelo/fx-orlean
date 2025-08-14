# Stripe Payment Setup Guide

This guide will help you set up Stripe payment integration for the FX-Orleans platform.

## Prerequisites

1. A Stripe account - Sign up at [https://stripe.com](https://stripe.com)
2. Access to your Stripe Dashboard

## Setup Steps

### 1. Get Your Stripe API Keys

1. Log into your [Stripe Dashboard](https://dashboard.stripe.com)
2. Go to **Developers** > **API keys**
3. Copy your **Publishable key** (starts with `pk_test_` for test mode)
4. Copy your **Secret key** (starts with `sk_test_` for test mode)

### 2. Configure Environment Variables

Create a `.env` file in the project root (copy from `.env.example`) and set:

```bash
# Test mode keys (for development)
STRIPE_PUBLISHABLE_KEY=pk_test_your_actual_key_here
STRIPE_SECRET_KEY=sk_test_your_actual_key_here

# Optional: Webhook secret (for webhook verification)
STRIPE_WEBHOOK_SECRET=whsec_your_webhook_secret_here
```

### 3. Verify Configuration

You can check if your Stripe keys are configured correctly by visiting:
- EventServer: `http://localhost:8080/api/payment/config/status`

This endpoint will show:
- Whether keys are configured
- If you're in test mode
- Key prefixes (for verification)

## Testing the Payment Flow

### Test Credit Card Numbers

Stripe provides test card numbers for development:

- **Successful payment**: `4242424242424242`
- **Requires authentication**: `4000002500003155`
- **Payment declined**: `4000000000000002`

Use any future expiry date (e.g., `12/25`) and any 3-digit CVC.

### Testing Process

1. Navigate to a partner profile
2. Click "Schedule a Consultation"  
3. Fill in date, time, and meeting topic
4. Click "Proceed to Payment"
5. Enter test card details
6. Click "Authorize Payment"

## Common Issues

### "Failed to create payment form" Error

This error typically occurs when:

1. **Stripe keys not configured**: Check environment variables
2. **EventServer not running**: Ensure `http://localhost:8080` is accessible
3. **Browser console errors**: Check for JavaScript errors
4. **DOM timing issues**: The payment form element may not be ready

**Solutions**:
- Verify environment variables are set correctly
- Restart both EventServer and Blazor applications
- Check browser console for detailed error messages
- Clear browser cache and try again

### Environment Variable Not Loading

If environment variables aren't being read:

1. Restart your development environment
2. Check the `.env` file is in the project root
3. Ensure no quotes around values in `.env` file
4. For IDEs, restart the IDE to reload environment

### Stripe Webhook Setup (Optional)

For production deployments, set up webhooks:

1. In Stripe Dashboard, go to **Developers** > **Webhooks**
2. Add endpoint: `https://yourdomain.com/api/stripe/webhook`
3. Select events: `payment_intent.succeeded`, `payment_intent.payment_failed`
4. Copy the webhook signing secret to `STRIPE_WEBHOOK_SECRET`

## Production Deployment

For production:

1. Switch to live mode in Stripe Dashboard
2. Get live API keys (starts with `pk_live_` and `sk_live_`)
3. Update environment variables with live keys
4. Set up webhook endpoints
5. Test thoroughly with small amounts

## Support

If you encounter issues:

1. Check the browser console for JavaScript errors
2. Check EventServer logs for backend errors
3. Verify all environment variables are set
4. Test with Stripe's provided test cards

## Security Notes

- Never commit actual API keys to version control
- Use test keys for development
- Rotate keys periodically in production
- Monitor Stripe Dashboard for unusual activity