# FX-Orleans Development Guide

> Expert consultation platform connecting businesses with fractional executives (CIO, CTO, CISO)
> Last Updated: 2025-08-17

## Project Overview

FX-Orleans is a specialized platform that helps businesses find and engage with qualified Fortium partners for fractional executive leadership through AI-powered matching, integrated booking, and payment processing.

**Current Status:** Phase 1 MVP Complete - Production-ready Kubernetes deployment with comprehensive Helm charts

## System Architecture

### Core Components

**Frontend (Blazor Server + WebAssembly)**
- **Client App:** Interactive UI components with MudBlazor
- **Server App:** Authentication and server-side rendering
- **Shared Components:** Reusable UI elements and services

**Backend (Event Sourcing + CQRS)**
- **EventServer:** ASP.NET Core 9.0 with Marten/Wolverine
- **Event Store:** PostgreSQL with Marten events
- **Aggregates:** Partners, Users, VideoConferences, Payments, Calendar

**External Integrations**
- **AI Matching:** OpenAI GPT with RAG implementation
- **Authentication:** Keycloak with OpenID Connect
- **Payments:** Stripe.net for payment processing
- **Calendar:** Google Calendar API v3
- **Video:** Google Meet integration

**Cloud Infrastructure (AWS EKS)**
- **Container Orchestration:** Kubernetes with AWS EKS
- **Service Mesh:** Helm charts for application deployment
- **Secret Management:** External Secrets Operator with AWS Secrets Manager
- **Load Balancing:** AWS Application Load Balancer (ALB)
- **Authentication:** IAM Roles for Service Accounts (IRSA)
- **Monitoring:** Prometheus + Grafana with ServiceMonitor integration

### Key Features (Implemented)

**✅ AI-Powered Partner Matching**
- Natural language processing of problem statements
- RAG-based matching against partner expertise
- Relevance scoring and ranking

**✅ Authentication & User Management**
- Keycloak integration with OpenID Connect
- Role-based access (clients, partners)
- User profile management

**✅ Partner Profiles**
- Comprehensive skill inventories
- Work history categorization
- Expertise levels and specializations

**✅ Booking System** 
- Google Calendar integration
- Real-time availability checks
- Meeting request generation with Google Meet links

**✅ Payment Processing**
- Stripe payment authorization ($800/60min sessions)
- Pre-meeting payment collection
- Revenue share tracking (80% to partners)

**✅ Session Management**
- Confirmation emails and calendar invites
- Partner dashboard for session management
- Session history tracking

**✅ Production Deployment Infrastructure**
- Comprehensive Helm charts for all services
- AWS EKS integration with IRSA authentication
- External Secrets Operator for secure configuration management
- Multi-environment support (dev, staging, production)
- Auto-scaling and high availability configuration
- Application Load Balancer with SSL/TLS termination

## Development Setup

### Prerequisites

- .NET 9.0 SDK
- Docker (for infrastructure services)
- Node.js (for frontend tooling)
- Helm 3.18+ (for Kubernetes deployments)
- kubectl (for Kubernetes cluster management)
- AWS CLI (for EKS deployments)

### Build & Run Commands

**Build Project:**
```bash
# Build entire solution
dotnet build

# Build with Justfile
just build
```

**Run Services:**
```bash
# Run EventServer only
dotnet watch --project src/EventServer/EventServer.csproj

# Run FxExpert Blazor only  
dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj --launch-profile https

# Run both services
just run

# Run infrastructure (Docker containers)
docker-compose up -d
# OR
process-compose up
```

### Testing

**Unit & Integration Tests:**
```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName=EventServer.Tests.UserTests.CreateUserTest"

# Run E2E tests
cd tests/FxExpert.E2E.Tests
dotnet test
```

**Helm Chart Tests:**
```bash
# Test all Helm charts
cd infrastructure/helm/tests
make test

# Test individual charts
make test-eventserver
make test-blazor
make test-keycloak
make test-external-secrets
```

**Test Coverage:**
- **Unit Tests:** xUnit with Alba (HTTP testing) and Shouldly (assertions)
- **E2E Tests:** Playwright with NUnit for cross-browser testing
- **Integration Tests:** Test complete booking flows and payment processes
- **Helm Chart Tests:** Terratest with Go for Kubernetes deployment validation

## Project Structure

```
fx-orleans/
├── src/
│   ├── EventServer/                    # Backend API (.NET 9)
│   │   ├── Aggregates/                # Event sourcing aggregates
│   │   ├── Controllers/               # API controllers
│   │   └── Services/                  # Business services
│   ├── FxExpert.Blazor/               # Frontend applications
│   │   ├── FxExpert.Blazor/          # Server-side Blazor
│   │   └── FxExpert.Blazor.Client/   # WebAssembly client
│   └── common/                        # Shared utilities
├── shared-types/                      # Shared data models
├── tests/
│   └── FxExpert.E2E.Tests/           # End-to-end testing
├── infrastructure/                   # Kubernetes deployment
│   └── helm/                         # Helm charts and configuration
│       ├── charts/                   # Application Helm charts
│       │   ├── eventserver/          # Backend API chart
│       │   ├── blazor-frontend/      # Frontend application chart
│       │   ├── keycloak/             # Identity management chart
│       │   └── external-secrets/     # Secret management chart
│       ├── values/                   # Environment-specific values
│       │   ├── dev-values.yaml       # Development overrides
│       │   ├── staging-values.yaml   # Staging overrides
│       │   └── prod-values.yaml      # Production overrides
│       └── tests/                    # Helm chart testing
├── docs/                             # Technical documentation
└── .agent-os/                       # Agent OS configuration
```

## Technical Standards

### Code Style Guidelines

**General Principles:**
- Use type hints everywhere for clarity
- Code should be simple, readable, and self-explanatory
- Use meaningful names that reveal intent
- Function names should describe actions performed
- Prefer exceptions over error codes for error handling
- Use immutable types whenever possible

**Framework-Specific:**
- **Controllers:** Use Wolverine attributes and return command events
- **UI:** Use Blazor with MudBlazor components
- **HTML:** Prefer MudComponents over standard HTML elements
- **Formatting:** Follow Agent OS code style standards (@~/.agent-os/standards/code-style.md)

### Architecture Patterns

**Event Sourcing + CQRS:**
- Commands represent intent to change state
- Events represent state changes that have occurred
- Projections provide read models for queries
- Marten handles event storage and projections

**Main Aggregates:**
- **Partners:** Profile management, skills, availability
- **Users:** Client profiles and preferences  
- **VideoConferences:** Session scheduling and management
- **Payments:** Stripe integration and revenue tracking
- **Calendar:** Google Calendar integration and availability

**Service Layer:**
- **AI Matching:** ChatGPT with RAG for partner recommendations
- **Calendar Service:** Google Calendar API integration
- **Payment Service:** Stripe payment processing
- **Email Service:** Automated notifications and confirmations

## Development Workflow

### Current Priorities (Phase 1 MVP)

**High Priority (Complete by Q4 2024):**
- ✅ Complete booking system with partner availability
- ✅ Payment authorization flow ($800 sessions)  
- ✅ Google Meet integration with calendar invites
- ✅ Session management for partners
- ✅ Basic note-taking and payment capture

**Next Phase (Q1 2025):**
- Advanced partner search and filtering
- Mobile-responsive design optimization
- Enhanced AI matching with feedback integration
- Partner and client dashboard improvements

### Key Development Commands

**Quick Start:**
```bash
# Start infrastructure
docker-compose up -d

# Run both services
just run

# Run tests
dotnet test
```

**Kubernetes Deployment:**
```bash
# Deploy to development environment
helm install fx-orleans-dev ./infrastructure/helm/charts/eventserver \
  -f ./infrastructure/helm/values/dev-values.yaml \
  --namespace fx-orleans-dev

# Deploy to staging environment
helm install fx-orleans-staging ./infrastructure/helm/charts/eventserver \
  -f ./infrastructure/helm/values/staging-values.yaml \
  --namespace fx-orleans-staging

# Deploy to production environment
helm install fx-orleans-prod ./infrastructure/helm/charts/eventserver \
  -f ./infrastructure/helm/values/prod-values.yaml \
  --namespace fx-orleans-prod
```

**Environment Configuration:**
- **Development:** Local with Docker containers or EKS dev cluster
- **Staging:** EKS cluster with internal load balancers
- **Production:** EKS cluster with internet-facing load balancers
- **Authentication:** Keycloak with external PostgreSQL (RDS)
- **Database:** PostgreSQL with Marten event store (RDS in production)
- **Monitoring:** OpenTelemetry + Zipkin tracing + Prometheus/Grafana

## Troubleshooting

### Common Issues

**Build Issues:**
- Ensure .NET 9 SDK is installed
- Check Docker containers are running
- Verify Keycloak realm configuration

**Authentication Issues:**
- Check Keycloak service status: `docker ps`
- Verify realm-export.json configuration
- Review OpenID Connect settings in appsettings.json

**Testing Issues:**
- Ensure test databases are clean
- Check E2E test browser configuration
- Verify Playwright dependencies

**Kubernetes/Helm Issues:**
- Verify kubectl context: `kubectl config current-context`
- Check cluster connectivity: `kubectl cluster-info`
- Validate Helm charts: `helm lint ./infrastructure/helm/charts/eventserver`
- Debug failed deployments: `kubectl describe pod <pod-name>`
- Check External Secrets: `kubectl get externalsecrets -n fx-orleans`

### Debugging

**Application Logs:**
- EventServer logs: Console output from `dotnet watch`
- Blazor logs: Browser developer tools
- Infrastructure logs: `docker-compose logs`

**Performance Monitoring:**
- OpenTelemetry traces in Zipkin (http://localhost:9411)
- Application metrics in console output
- Database query performance in Marten logs

## Agent OS Integration

### Product Documentation

- **Mission & Vision:** @.agent-os/product/mission.md
- **Technical Stack:** @.agent-os/product/tech-stack.md
- **Development Roadmap:** @.agent-os/product/roadmap.md
- **Decision History:** @.agent-os/product/decisions.md

### Development Standards

- **Code Style:** @~/.agent-os/standards/code-style.md
- **Best Practices:** @~/.agent-os/standards/best-practices.md

### Project Management

- **Active Specs:** @.agent-os/specs/
- **Spec Planning:** Use @~/.agent-os/instructions/create-spec.md
- **Task Execution:** Use @~/.agent-os/instructions/execute-tasks.md

## Contributing Guidelines

### Before Starting Work

1. **Check current priorities** in @.agent-os/product/roadmap.md
2. **Review related specifications** in @.agent-os/specs/
3. **Follow established patterns** in existing codebase
4. **Run tests** to ensure clean baseline

### Development Process

1. **Create feature branch** following Git flow
2. **Write tests first** (TDD approach)
3. **Implement functionality** following SOLID principles
4. **Ensure all tests pass** including E2E tests
5. **Update documentation** if needed

### Code Quality Standards

- **Test Coverage:** Maintain >80% unit test coverage
- **Performance:** API responses <200ms, UI interactions <100ms
- **Security:** Follow OWASP guidelines, no hardcoded secrets
- **Accessibility:** WCAG 2.1 AA compliance for UI components

## Important Notes

- **Agent OS Override:** Product-specific files in `.agent-os/product/` override global standards
- **User Instructions:** User-specific instructions override specifications in `.agent-os/specs/`
- **Pattern Consistency:** Always adhere to established patterns and conventions
- **Quality Gates:** All changes must pass automated testing pipeline
