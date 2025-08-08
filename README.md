# FX-Orleans: AI-Powered Expert Consultation Platform

> **Status**: Active Development - Phase 1 MVP  
> **Version**: 1.0.0  
> **Last Updated**: 2025-08-08

## 🎯 Project Overview

FX-Orleans is an AI-powered expert consultation platform that connects businesses needing fractional executive leadership (CIO, CTO, CISO) with qualified Fortium partners. The platform uses intelligent matching based on problem statements and comprehensive partner expertise profiles.

### Key Features
- **AI-Powered Partner Matching**: Natural language processing to analyze client problems and match with optimal experts
- **Integrated Booking System**: Seamless calendar integration with Google Meet and payment processing
- **Authentication & Authorization**: Keycloak integration with OpenID Connect
- **Event Sourcing Architecture**: CQRS pattern with complete audit trails
- **Real-time Collaboration**: Video conferencing with session notes and client management

## 🏗️ Architecture

### Technology Stack
- **Backend**: ASP.NET Core 9.0 with Event Sourcing + CQRS
- **Database**: PostgreSQL with Marten event store
- **Frontend**: Blazor Server + WebAssembly with MudBlazor
- **Authentication**: Keycloak with OpenID Connect
- **AI Integration**: OpenAI GPT with RAG implementation
- **Payments**: Stripe integration
- **Calendar**: Google Calendar API with Google Meet

### Core Components
```
fx-orleans/
├── src/
│   ├── EventServer/           # Event sourcing backend with CQRS
│   ├── FxExpert.Blazor/       # Blazor Server host application
│   ├── FxExpert.Blazor.Client/# Blazor WebAssembly client components
│   └── common/                # Shared domain models and utilities
├── tests/
│   ├── EventServer.Tests/     # Backend unit and integration tests
│   ├── FxExpert.Blazor.Client.Tests/ # Frontend unit tests
│   └── FxExpert.E2E.Tests/    # End-to-end testing with Playwright
├── infrastructure/            # Docker and Kubernetes configurations
└── shared-types/             # Cross-project type definitions
```

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- Docker Desktop
- Node.js (for frontend tooling)
- Visual Studio Code or Visual Studio 2022

### 1. Clone and Setup
```bash
git clone <repository-url>
cd fx-orleans
```

### 2. Start Infrastructure Services
```bash
# Start PostgreSQL and Keycloak
docker-compose up -d postgres keycloak

# Wait for services to initialize (about 30 seconds)
docker-compose logs -f keycloak
```

### 3. Configure User Secrets
```bash
# Navigate to the Blazor project
cd src/FxExpert.Blazor/FxExpert.Blazor

# Set up authentication
dotnet user-secrets set "Authentication:Keycloak:Authority" "http://localhost:8080/realms/fx-orleans"
dotnet user-secrets set "Authentication:Keycloak:ClientId" "fx-orleans-client"
dotnet user-secrets set "Authentication:Keycloak:ClientSecret" "your-client-secret"

# Set up external services
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"
dotnet user-secrets set "Stripe:SecretKey" "your-stripe-secret-key"
dotnet user-secrets set "Stripe:PublishableKey" "your-stripe-publishable-key"
dotnet user-secrets set "GoogleCalendar:ServiceAccountKey" "path-to-service-account.json"
```

### 4. Build and Run
```bash
# Build the entire solution
dotnet build

# Option 1: Run with Just (recommended)
just run

# Option 2: Run manually
# Terminal 1:
dotnet watch --project src/EventServer/EventServer.csproj

# Terminal 2:
dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj
```

### 5. Access the Application
- **Main Application**: http://localhost:5000
- **EventServer API**: http://localhost:5001
- **Keycloak Admin**: http://localhost:8080 (admin/admin123)
- **PostgreSQL**: localhost:5432 (fx_orleans_user/fx_orleans_pass)

## 📖 System Documentation

### Domain Models

#### Core Aggregates
- **Partner**: Expert consultants with skills, availability, and work history
- **User**: Client users seeking consultation services  
- **VideoConference**: Meeting sessions with scheduling and notes
- **Payment**: Stripe payment processing and authorization
- **Calendar**: Google Calendar integration for availability management

#### Event Sourcing Events
```csharp
// Partner Events
PartnerCreated, PartnerProfileUpdated, PartnerSkillsUpdated
PartnerAvailabilityUpdated, PartnerStatusChanged

// User Events  
UserCreated, UserProfileUpdated, UserAddressUpdated
UserPreferencesUpdated

// Booking Events
ConsultationRequested, ConsultationScheduled, ConsultationConfirmed
PaymentAuthorized, PaymentCaptured, SessionCompleted

// Calendar Events
CalendarEventCreated, CalendarEventUpdated, AvailabilityChanged
```

## 🔐 Authentication & Authorization

### Keycloak Configuration
The application uses Keycloak for identity management with the following setup:

**Realm**: `fx-orleans`  
**Client**: `fx-orleans-client`  
**Authentication Flow**: Authorization Code with PKCE  
**Supported Providers**: Google OAuth (configured for production)

### User Roles
- **Client**: Can browse partners, book consultations, manage profile
- **Partner**: Can manage availability, conduct sessions, take notes  
- **Admin**: Can manage partners, view analytics, system administration

### Security Features
- OpenID Connect integration with automatic token refresh
- Secure cookie-based sessions with proper SameSite configuration
- HTTPS enforcement in production environments
- CSRF protection on all forms and state-changing operations

## 🤖 AI Matching System

### Natural Language Processing
The platform uses OpenAI GPT-4 to:
1. **Parse Problem Statements**: Extract key requirements, technologies, and business context
2. **Match Partners**: Score partners based on relevant experience and skills
3. **Generate Recommendations**: Provide reasoning for partner suggestions
4. **Optimize Results**: Learn from booking patterns and feedback

### RAG Implementation
```csharp
// Partner knowledge base with vector embeddings
var partnerProfiles = await GetPartnerProfilesAsync();
var problemEmbedding = await GenerateEmbeddingAsync(problemStatement);
var relevantPartners = await FindSimilarPartnersAsync(problemEmbedding);
var recommendations = await GenerateRecommendationsAsync(relevantPartners, problemStatement);
```

## 💰 Payment Integration

### Stripe Configuration
- **Payment Flow**: Authorize → Confirm → Capture
- **Amount**: $800.00 USD for 60-minute sessions
- **Revenue Share**: 80% to partners, 20% platform fee
- **Security**: PCI-compliant with secure token handling

### Payment States
1. **Payment Intent Created**: User initiates booking process
2. **Payment Authorized**: Funds reserved before meeting
3. **Payment Confirmed**: Authorization confirmed, meeting scheduled
4. **Payment Captured**: Funds captured after successful session
5. **Payment Refunded**: Refunds for cancelled or failed sessions

## 📅 Calendar Integration

### Google Calendar API
- **Service Account**: Used for calendar access and management
- **Meeting Creation**: Automatic Google Meet link generation
- **Availability Sync**: Real-time partner availability updates
- **Notifications**: Email confirmations and reminders

### Booking Flow
1. Client selects partner and preferred time slots
2. System checks partner availability via Google Calendar
3. Payment authorization before booking confirmation
4. Calendar event created with Google Meet link
5. Email confirmations sent to both parties

## 🧪 Testing

### Test Structure
```bash
# Unit Tests
dotnet test src/EventServer.Tests/
dotnet test src/FxExpert.Blazor.Client.Tests/

# Integration Tests  
dotnet test tests/FxExpert.E2E.Tests/

# Specific Test Categories
dotnet test --filter "Category=Authentication"
dotnet test --filter "Category=PaymentFlow"
dotnet test --filter "Category=PartnerMatching"
```

### E2E Testing with Playwright
- **Cross-browser Testing**: Chrome, Firefox, Safari, Edge
- **Authentication Flow Testing**: OAuth integration testing
- **Payment Flow Testing**: Stripe integration testing
- **Visual Regression Testing**: Screenshot comparisons
- **Accessibility Testing**: WCAG compliance validation

## 📊 Monitoring & Observability

### Logging
- **Structured Logging**: Serilog with JSON formatting
- **Log Levels**: Trace, Debug, Information, Warning, Error, Critical
- **Correlation IDs**: Request tracking across services
- **Performance Metrics**: Response times, error rates, resource usage

### Health Checks
```csharp
// Health check endpoints
GET /health           # Basic health status
GET /health/ready     # Readiness probe
GET /health/live      # Liveness probe
```

## 🚢 Deployment

### Local Development
```bash
# Use Docker Compose for local infrastructure
docker-compose up -d

# Run applications with hot reload
just run
```

### Production Deployment
```bash
# Build for production
dotnet publish -c Release

# Deploy to Kubernetes
kubectl apply -f infrastructure/k8s/
```

### Environment Configuration
- **Development**: Local PostgreSQL and Keycloak instances
- **Staging**: Containerized services with persistent volumes
- **Production**: Managed services with high availability and backup

## 🔧 Development Guidelines

### Code Style
- **C# Conventions**: Follow Microsoft C# coding conventions
- **Blazor Patterns**: Component-based architecture with proper state management
- **Database Patterns**: Event sourcing with read model projections
- **Error Handling**: Comprehensive error boundaries and logging

### Git Workflow
- **Feature Branches**: Create feature branches from `main`
- **Pull Requests**: Required for all changes with code review
- **Commit Messages**: Conventional commits with clear descriptions
- **Release Tags**: Semantic versioning for releases

## 📈 Roadmap

### Phase 1 - MVP (Current)
- [x] Core partner matching and booking system
- [x] Payment authorization and processing
- [x] Basic calendar integration
- [x] Authentication and user management
- [x] E2E testing infrastructure

### Phase 2 - Enhanced UX
- [ ] Advanced partner search and filtering
- [ ] Mobile-responsive design improvements
- [ ] Session history and analytics
- [ ] Rating and review system

### Phase 3 - Business Intelligence
- [ ] Partner performance analytics
- [ ] Revenue tracking and reporting
- [ ] A/B testing framework
- [ ] Customer journey optimization

## 🤝 Contributing

### Development Setup
1. Follow the Quick Start guide above
2. Read the coding standards in `.agent-os/standards/`
3. Check active specifications in `.agent-os/specs/`
4. Create feature branches with descriptive names

### Pull Request Process
1. Ensure all tests pass locally
2. Update documentation as needed
3. Add/update tests for new functionality
4. Follow the established code review process

## 📞 Support & Contact

### Development Team
- **Project Lead**: [Contact Information]
- **Technical Lead**: [Contact Information]
- **Product Owner**: [Contact Information]

### Resources
- **Documentation**: This README and `.agent-os/` specifications
- **Issue Tracking**: GitHub Issues
- **Architecture Decisions**: `.agent-os/product/decisions.md`
- **Roadmap**: `.agent-os/product/roadmap.md`

---

**Built with ❤️ by the Fortium team using .NET 9, Event Sourcing, and AI-powered matching**