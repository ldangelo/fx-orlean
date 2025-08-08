# API Documentation

> **Last Updated**: 2025-08-08  
> **Version**: 1.0.0

## Overview

The FX-Orleans EventServer provides a comprehensive REST API built with ASP.NET Core and Wolverine HTTP, following CQRS and Event Sourcing patterns. The API supports user management, partner operations, AI-powered matching, payment processing, and calendar integration.

## Base URLs

- **Development**: `http://localhost:5001`
- **Production**: `https://api.fx-orleans.com`

## Authentication

All API endpoints except AI matching require authentication via Bearer tokens:

```http
Authorization: Bearer {jwt_token}
```

### Authentication Flow
1. Authenticate via Keycloak OpenID Connect
2. Receive JWT token with user claims
3. Include token in subsequent API requests
4. Token auto-refreshes before expiration

## Error Handling

### Standard Error Response Format
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "field": ["Validation error message"]
  },
  "traceId": "0HMV9C49H1J05:00000001"
}
```

### HTTP Status Codes
- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource state conflict
- `422 Unprocessable Entity` - Validation failed
- `500 Internal Server Error` - Server error

## User Management API

### Create User
Creates a new user in the system.

```http
POST /users
Content-Type: application/json
```

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "emailAddress": "john.doe@example.com"
}
```

**Response:**
```http
HTTP/1.1 201 Created
Location: /users/john.doe@example.com
```

### Get User
Retrieves user information by email address.

```http
GET /users/{emailAddress}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "emailAddress": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1-555-123-4567",
  "profilePictureUrl": "https://example.com/profile.jpg",
  "addresses": [
    {
      "street1": "123 Main St",
      "city": "Austin",
      "state": "TX",
      "zipCode": "78701",
      "country": "US"
    }
  ],
  "preferences": {
    "receiveEmailNotifications": true,
    "receiveSmsNotifications": false,
    "preferredLanguage": "en",
    "timeZone": "America/Chicago",
    "theme": "Light"
  },
  "createDate": "2025-01-15T10:30:00Z",
  "updateDate": "2025-01-20T14:22:00Z"
}
```

### Update User Profile
Updates user profile information.

```http
POST /users/profile/{emailAddress}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "emailAddress": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1-555-123-4567",
  "profilePictureUrl": "https://example.com/profile.jpg"
}
```

### Update User Address
Updates user address information.

```http
POST /users/address/{emailAddress}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "emailAddress": "john.doe@example.com",
  "street1": "123 Main St",
  "street2": "Apt 4B",
  "city": "Austin",
  "state": "TX",
  "zipCode": "78701",
  "country": "US"
}
```

### Update User Preferences
Updates user notification and UI preferences.

```http
POST /users/preferences/{emailAddress}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "emailAddress": "john.doe@example.com",
  "receiveEmailNotifications": true,
  "receiveSmsNotifications": false,
  "preferredLanguage": "en",
  "timeZone": "America/Chicago",
  "theme": "Light"
}
```

### User Theme Management

#### Get User Theme
```http
GET /users/theme/{emailAddress}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "theme": "Dark"
}
```

#### Update User Theme
```http
POST /users/theme/{emailAddress}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "emailAddress": "john.doe@example.com",
  "theme": "Dark"
}
```

## Partner Management API

### Create Partner
Creates a new partner consultant.

```http
POST /partners
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "emailAddress": "jane.smith@fortiumpartners.com"
}
```

**Response:**
```http
HTTP/1.1 201 Created
Location: /partners/jane.smith@fortiumpartners.com
```

### Get Partner
Retrieves partner information by email address.

```http
GET /partners/{emailAddress}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "emailAddress": "jane.smith@fortiumpartners.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "bio": "Fractional CTO with 15+ years of experience in SaaS and fintech.",
  "photoUrl": "https://example.com/jane-smith.jpg",
  "primaryPhone": "+1-555-987-6543",
  "skills": [
    {
      "skillName": "leadership",
      "yearsOfExperience": 15,
      "level": "Expert"
    },
    {
      "skillName": "aws",
      "yearsOfExperience": 10,
      "level": "Expert"
    }
  ],
  "workHistories": [
    {
      "startDate": "2015-01-01",
      "endDate": null,
      "company": "Fortium Partners",
      "title": "Fractional CTO",
      "description": "Providing technical leadership for growing SaaS companies."
    }
  ],
  "availabilityNext30Days": 12,
  "active": true,
  "loggedIn": true,
  "createDate": "2025-01-01T09:00:00Z",
  "lastLogin": "2025-01-20T08:30:00Z"
}
```

### Update Partner Bio
Updates partner biography.

```http
POST /partners/bio/{partnerId}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "emailAddress": "jane.smith@fortiumpartners.com",
  "bio": "Fractional CTO specializing in digital transformation and cloud architecture."
}
```

### Update Partner Skills
Adds or updates partner skills.

```http
POST /partners/skills/{partnerId}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "emailAddress": "jane.smith@fortiumpartners.com",
  "skills": [
    {
      "skillName": "kubernetes",
      "yearsOfExperience": 5,
      "level": "Advanced"
    },
    {
      "skillName": "microservices",
      "yearsOfExperience": 8,
      "level": "Expert"
    }
  ]
}
```

### Partner Authentication

#### Partner Login
```http
POST /partners/loggedin/{partnerId}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "emailAddress": "jane.smith@fortiumpartners.com",
  "loginTime": "2025-01-20T08:30:00Z"
}
```

#### Partner Logout
```http
POST /partners/loggedout/{partnerId}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "emailAddress": "jane.smith@fortiumpartners.com",
  "logoutTime": "2025-01-20T17:30:00Z"
}
```

## AI Matching API

### Get Partner Recommendations
Uses AI to match client problems with suitable partners.

```http
POST /api/ai/partners
Content-Type: application/json
```

**Request Body:**
```json
{
  "problemDescription": "We need help migrating our legacy .NET application to a modern cloud-native architecture on AWS. Our team lacks experience with containers and microservices."
}
```

**Response:**
```json
[
  {
    "partnerId": "jane.smith@fortiumpartners.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "rank": 1,
    "matchScore": 0.95,
    "reason": "Expert in AWS architecture and .NET modernization with 15 years of leadership experience. Specializes in cloud-native transformations and microservices architecture.",
    "skills": [
      {
        "skillName": "aws",
        "yearsOfExperience": 10,
        "level": "Expert"
      },
      {
        "skillName": "dotnet",
        "yearsOfExperience": 15,
        "level": "Expert"
      },
      {
        "skillName": "microservices",
        "yearsOfExperience": 8,
        "level": "Expert"
      }
    ],
    "workHistories": [
      {
        "company": "TechCorp",
        "title": "VP Engineering",
        "description": "Led migration of monolithic .NET applications to AWS-based microservices architecture."
      }
    ],
    "availabilityNext30Days": 12
  }
]
```

**Response Headers:**
```http
Content-Type: application/json
Cache-Control: no-cache
```

### AI Matching Features:
- **Natural Language Processing**: Analyzes problem descriptions using OpenAI GPT-4
- **Skill Matching**: Matches required skills with partner expertise
- **Experience Scoring**: Considers years of experience and project relevance
- **Availability Filtering**: Only returns partners with availability
- **Ranking Algorithm**: Provides relevance scores and reasoning

## Payment Processing API

### Create Payment Intent
Creates a Stripe PaymentIntent for session authorization.

```http
POST /payments/create-intent
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "amount": 800.00,
  "currency": "usd"
}
```

**Response:**
```json
{
  "paymentIntentId": "pi_1234567890abcdef",
  "clientSecret": "pi_1234567890abcdef_secret_xyz123"
}
```

### Authorize Conference Payment
Authorizes payment for a consultation session.

```http
POST /payments/authorize
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "paymentId": "pay_1234567890",
  "conferenceId": "conf_abcdef123456",
  "amount": 800.00,
  "currency": "USD",
  "userId": "user@example.com",
  "rateInformation": {
    "hourlyRate": 800.00,
    "duration": 60
  }
}
```

### Capture Payment
Captures an authorized payment after session completion.

```http
POST /payments/capture/{payment_id}
Authorization: Bearer {token}
```

**Response:**
```http
HTTP/1.1 204 No Content
```

### Get Payment Status
Retrieves payment information and status.

```http
GET /payments/{paymentId}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "paymentId": "pay_1234567890",
  "conferenceId": "conf_abcdef123456",
  "amount": 800.00,
  "currency": "USD",
  "status": "Authorized",
  "userId": "user@example.com",
  "authorizedAt": "2025-01-20T10:30:00Z",
  "rateInformation": {
    "hourlyRate": 800.00,
    "duration": 60
  }
}
```

### Payment Configuration

#### Get Stripe Publishable Key
```http
GET /api/payment/config/publishable-key
```

**Response:**
```json
{
  "publishableKey": "pk_test_1234567890abcdef"
}
```

#### Get Payment Configuration Status
```http
GET /api/payment/config/status
```

**Response:**
```json
{
  "publishableKeyConfigured": true,
  "secretKeyConfigured": true,
  "publishableKeyPrefix": "pk_test_1234",
  "secretKeyPrefix": "sk_test_5678",
  "isTestMode": true
}
```

## Calendar Integration API

### Get Calendar Events
Retrieves calendar events for availability checking.

```http
GET /api/calendar/events/{calendarId}
Authorization: Bearer {token}
```

**Query Parameters:**
- `timeMin`: ISO 8601 datetime (optional)
- `timeMax`: ISO 8601 datetime (optional)
- `maxResults`: Integer (default: 40)

**Response:**
```json
{
  "items": [
    {
      "id": "event123456789",
      "summary": "Client Consultation - TechCorp",
      "start": {
        "dateTime": "2025-01-25T14:00:00-05:00"
      },
      "end": {
        "dateTime": "2025-01-25T15:00:00-05:00"
      },
      "hangoutLink": "https://meet.google.com/abc-defg-hij",
      "attendees": [
        {
          "email": "client@techcorp.com",
          "responseStatus": "accepted"
        },
        {
          "email": "partner@fortium.com",
          "responseStatus": "accepted"
        }
      ]
    }
  ]
}
```

### Create Calendar Event
Creates a new calendar event with Google Meet integration.

```http
POST /api/calendar/events/{calendarId}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "summary": "Technical Consultation - Cloud Migration",
  "description": "Discussion about migrating legacy .NET application to AWS cloud-native architecture.",
  "start": {
    "dateTime": "2025-01-25T14:00:00-05:00",
    "timeZone": "America/New_York"
  },
  "end": {
    "dateTime": "2025-01-25T15:00:00-05:00",
    "timeZone": "America/New_York"
  },
  "attendees": [
    {
      "email": "client@techcorp.com",
      "displayName": "John Client"
    },
    {
      "email": "partner@fortium.com",
      "displayName": "Jane Partner"
    }
  ],
  "conferenceData": {
    "createRequest": {
      "requestId": "unique-request-id-12345"
    }
  }
}
```

**Response:**
```json
{
  "id": "event123456789",
  "summary": "Technical Consultation - Cloud Migration",
  "start": {
    "dateTime": "2025-01-25T14:00:00-05:00"
  },
  "end": {
    "dateTime": "2025-01-25T15:00:00-05:00"
  },
  "hangoutLink": "https://meet.google.com/xyz-abcd-efg",
  "htmlLink": "https://calendar.google.com/calendar/event?eid=abc123",
  "attendees": [
    {
      "email": "client@techcorp.com",
      "responseStatus": "needsAction"
    }
  ],
  "conferenceData": {
    "conferenceId": "xyz-abcd-efg",
    "conferenceSolution": {
      "name": "Google Meet"
    },
    "entryPoints": [
      {
        "entryPointType": "video",
        "uri": "https://meet.google.com/xyz-abcd-efg"
      }
    ]
  }
}
```

## Video Conference API

### Create Video Conference
Creates a new video conference session.

```http
POST /api/videoconference
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "clientEmail": "client@example.com",
  "partnerEmail": "partner@fortiumpartners.com",
  "scheduledDateTime": "2025-01-25T14:00:00Z",
  "duration": 60,
  "problemDescription": "Need help with cloud migration strategy"
}
```

### Get Video Conference
Retrieves video conference details.

```http
GET /api/videoconference/{conferenceId}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": "conf_123456789",
  "clientEmail": "client@example.com",
  "partnerEmail": "partner@fortiumpartners.com",
  "scheduledDateTime": "2025-01-25T14:00:00Z",
  "duration": 60,
  "status": "Scheduled",
  "googleMeetLink": "https://meet.google.com/abc-defg-hij",
  "calendarEventId": "event123456789",
  "problemDescription": "Need help with cloud migration strategy",
  "notes": [],
  "createdAt": "2025-01-20T10:30:00Z"
}
```

## Data Models

### User Model
```typescript
interface User {
  emailAddress: string;          // Primary key
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  profilePictureUrl?: string;
  
  addresses: Address[];
  preferences: UserPreferences;
  
  createDate: string;           // ISO 8601
  updateDate?: string;          // ISO 8601
}

interface Address {
  street1: string;
  street2?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

interface UserPreferences {
  receiveEmailNotifications: boolean;
  receiveSmsNotifications: boolean;
  preferredLanguage: string;    // ISO 639-1 code
  timeZone: string;             // IANA timezone
  theme: 'Light' | 'Dark';
}
```

### Partner Model
```typescript
interface Partner {
  emailAddress: string;         // Primary key
  firstName: string;
  lastName: string;
  bio?: string;
  photoUrl?: string;
  primaryPhone?: string;
  
  skills: PartnerSkill[];
  workHistories: WorkHistory[];
  
  availabilityNext30Days: number;
  active: boolean;
  loggedIn: boolean;
  
  createDate: string;           // ISO 8601
  updateDate?: string;          // ISO 8601
  lastLogin?: string;           // ISO 8601
  lastLogout?: string;          // ISO 8601
}

interface PartnerSkill {
  skillName: string;
  yearsOfExperience: number;
  level: 'Novice' | 'Intermediate' | 'Advanced' | 'Expert';
}

interface WorkHistory {
  startDate: string;            // ISO 8601 date
  endDate?: string;             // ISO 8601 date, null for current
  company: string;
  title: string;
  description: string;
}
```

### Payment Model
```typescript
interface Payment {
  paymentId: string;            // Primary key
  conferenceId: string;
  amount: number;               // Decimal value
  currency: string;             // ISO 4217 currency code
  status: PaymentStatus;
  userId: string;
  
  authorizedAt?: string;        // ISO 8601
  capturedAt?: string;          // ISO 8601
  
  rateInformation: RateInfo;
}

type PaymentStatus = 
  | 'Created'
  | 'Authorized' 
  | 'Captured'
  | 'Refunded'
  | 'Failed'
  | 'Cancelled';

interface RateInfo {
  hourlyRate: number;
  duration: number;             // Minutes
}
```

## Rate Limits

### Standard Rate Limits
- **AI Matching**: 10 requests per minute per user
- **Payment Operations**: 5 requests per minute per user
- **User/Partner CRUD**: 60 requests per minute per user
- **Calendar Operations**: 20 requests per minute per user

### Rate Limit Headers
```http
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 59
X-RateLimit-Reset: 1642694400
Retry-After: 3600
```

## Webhooks

### Stripe Webhook Events
Handle Stripe webhook events for payment processing:

```http
POST /api/webhooks/stripe
Content-Type: application/json
Stripe-Signature: t=1492774577,v1=5257a...
```

**Supported Events:**
- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `payment_method.attached`
- `invoice.payment_succeeded`
- `charge.dispute.created`

### Google Calendar Webhook Events
Handle Google Calendar webhook events for availability updates:

```http
POST /api/webhooks/calendar
Content-Type: application/json
X-Goog-Channel-ID: channel-id-123
```

## SDK Examples

### JavaScript/TypeScript
```typescript
import { FxOrleansClient } from '@fx-orleans/client';

const client = new FxOrleansClient({
  baseUrl: 'https://api.fx-orleans.com',
  bearerToken: 'your-jwt-token'
});

// Get AI partner recommendations
const partners = await client.ai.getPartnerRecommendations({
  problemDescription: 'Need help with microservices architecture'
});

// Create payment intent
const paymentIntent = await client.payments.createIntent({
  amount: 800.00,
  currency: 'usd'
});

// Update user profile
await client.users.updateProfile('user@example.com', {
  firstName: 'John',
  lastName: 'Doe',
  phoneNumber: '+1-555-123-4567'
});
```

### C# (.NET)
```csharp
using FxOrleans.Client;

var client = new FxOrleansClient(new FxOrleansClientOptions
{
    BaseUrl = "https://api.fx-orleans.com",
    BearerToken = "your-jwt-token"
});

// Get AI partner recommendations
var partners = await client.AI.GetPartnerRecommendationsAsync(new AIRequest
{
    ProblemDescription = "Need help with microservices architecture"
});

// Create payment intent
var paymentIntent = await client.Payments.CreateIntentAsync(new CreatePaymentIntentRequest
{
    Amount = 800.00m,
    Currency = "usd"
});
```

## OpenAPI Specification

The complete OpenAPI 3.0 specification is available at:
- **Development**: `http://localhost:5001/swagger.json`
- **Production**: `https://api.fx-orleans.com/swagger.json`

Interactive API documentation is available via Swagger UI:
- **Development**: `http://localhost:5001/swagger`
- **Production**: `https://api.fx-orleans.com/swagger`

---

This comprehensive API documentation provides all the information needed to integrate with the FX-Orleans platform, enabling developers to build applications that leverage AI-powered partner matching, secure payment processing, and seamless calendar integration.