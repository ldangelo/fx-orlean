# Email Testing Setup with MailHog

This guide will help you set up and test email functionality in the FX-Orleans platform using MailHog, a test email server.

## What is MailHog?

MailHog is a lightweight email testing tool that:
- **Captures emails** sent by your application without sending them to real recipients
- **Provides a web interface** to view all captured emails at `http://localhost:8025`
- **Requires no authentication** - perfect for development testing
- **Supports SMTP** on port 1025 by default

## Quick Start

### 1. Start MailHog and Infrastructure

```bash
# Start all Docker services including MailHog
docker-compose up -d

# Verify MailHog is running
docker ps | grep mailhog
```

### 2. Access MailHog Web Interface

Open your browser and go to:
**http://localhost:8025**

You should see the MailHog web interface with an empty inbox.

### 3. Test Email Functionality

1. **Start the EventServer:**
   ```bash
   dotnet watch --project src/EventServer/EventServer.csproj
   ```

2. **Start the Blazor app:**
   ```bash
   dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj --launch-profile https
   ```

3. **Book a consultation** through the normal booking flow:
   - Navigate to a partner profile
   - Click "Schedule a Consultation"
   - Fill in the booking form
   - Complete the payment authorization
   - Submit the booking

4. **Check MailHog** at http://localhost:8025 to see the captured emails

## Configuration Details

### MailHog Docker Service

The MailHog service is already configured in `docker-compose.yml`:

```yaml
mailhog:
  container_name: mailhog
  image: mailhog/mailhog:latest
  platform: ${DOCKER_PLATFORM}
  ports:
    - "1025:1025"  # SMTP Server
    - "8025:8025"  # Web UI
  networks:
    - data-network
```

### Application Configuration

The email configuration is set in `src/EventServer/appsettings.Development.json`:

```json
{
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "1025",
    "Username": "test@fx-orleans.com",
    "Password": "testpassword",
    "FromAddress": "noreply@fx-orleans.com",
    "FromName": "FX-Orleans Platform"
  }
}
```

### EmailService Enhancements

The `EmailService` has been updated to:
- **Detect MailHog** automatically when using localhost:1025
- **Disable authentication and SSL** for test servers
- **Maintain compatibility** with production email providers

## Email Types You Can Test

### 1. Booking Confirmation Emails

**Client Email:**
- Subject: "Consultation Confirmed: [Topic]"
- Contains session details, Google Meet link, preparation tips
- Sent when a booking is successfully created

**Partner Email:**
- Subject: "New Consultation Booked: [Topic]"
- Contains client details, problem description, session info
- Sent to notify partners of new bookings

### 2. Meeting Reminder Emails

**24-Hour Reminder:**
- Sent 24 hours before the session
- Contains session details and join link

**1-Hour Reminder:**
- Sent 1 hour before the session
- Urgent styling with session details

## Testing Scenarios

### Basic Email Test

1. Complete a booking through the UI
2. Check MailHog for 2 emails (client + partner)
3. Verify email content and formatting
4. Test email links (Google Meet, Calendar)

### Email Template Testing

1. Book consultations with different topics and descriptions
2. Check how dynamic content appears in emails
3. Test with various partner names and client details
4. Verify HTML rendering in MailHog viewer

### Error Handling Testing

1. **Stop MailHog** temporarily: `docker stop mailhog`
2. Try to complete a booking
3. Check EventServer logs for appropriate error handling
4. **Restart MailHog**: `docker start mailhog`
5. Verify the application recovers gracefully

## MailHog Web Interface Features

### Email List View
- Shows all captured emails in chronological order
- Displays sender, recipient, subject, and timestamp
- Search and filter capabilities

### Email Detail View
- **Raw Email**: View complete email source
- **Plain Text**: Text-only version of the email
- **HTML**: Rendered HTML version (if applicable)
- **MIME**: MIME structure and attachments

### Email Actions
- **Download**: Save email as .eml file
- **Delete**: Remove individual emails
- **Clear All**: Empty the entire inbox

## Production vs Development

### Development (Current Setup)
- Uses MailHog on localhost:1025
- No authentication required
- All emails captured locally
- Web interface at http://localhost:8025

### Production Configuration
- Will use real SMTP provider (Gmail, SendGrid, etc.)
- Requires authentication credentials
- Enables SSL/TLS encryption
- Sends real emails to recipients

## Troubleshooting

### MailHog Not Receiving Emails

1. **Check Docker container:**
   ```bash
   docker logs mailhog
   ```

2. **Verify SMTP settings** in appsettings.Development.json

3. **Check EventServer logs** for email sending errors

4. **Test SMTP connection:**
   ```bash
   telnet localhost 1025
   ```

### Application Not Sending Emails

1. **Check EventServer logs** for EmailService initialization
2. **Verify configuration** is loaded correctly
3. **Test with simple email** through API endpoint
4. **Check network connectivity** to MailHog container

### MailHog Web Interface Issues

1. **Verify port 8025** is not blocked
2. **Check browser** allows localhost connections
3. **Clear browser cache** if interface seems stale
4. **Restart MailHog** container if needed

## Advanced Testing

### API Testing

You can send test emails directly via the EventServer API:

```bash
# Test booking confirmation email
curl -X POST http://localhost:8080/api/test/email \
  -H "Content-Type: application/json" \
  -d '{
    "clientEmail": "test@example.com",
    "partnerEmail": "partner@example.com",
    "partnerName": "Test Partner",
    "consultationTopic": "Test Consultation",
    "sessionStartTime": "2025-01-01T10:00:00Z"
  }'
```

### Load Testing

1. Create multiple bookings quickly
2. Monitor MailHog performance
3. Check email delivery timing
4. Verify no emails are lost

## Security Notes

- MailHog is **for development only** - never use in production
- Captured emails contain **test data only**
- No real credentials or sensitive information
- MailHog web interface is **not password protected**

## Next Steps

Once email testing is working properly:

1. **Configure production SMTP** provider (Gmail, SendGrid, etc.)
2. **Set up environment variables** for production credentials
3. **Test production deployment** with real email addresses
4. **Set up monitoring** for email delivery in production
5. **Configure email templates** for different environments

## Support

If you encounter issues:

1. Check EventServer logs for email-related errors
2. Verify MailHog container is running and accessible
3. Test SMTP connectivity manually
4. Review configuration files for typos or missing values