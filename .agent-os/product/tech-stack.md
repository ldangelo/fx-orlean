# Technical Stack

> Last Updated: 2025-08-02
> Version: 1.0.0

## Backend Framework
- **Application Framework:** ASP.NET Core 9.0
- **Architecture Pattern:** Event Sourcing + CQRS with Marten/Wolverine
- **API Framework:** FastEndpoints 5.35.0 + Wolverine HTTP
- **Validation:** FluentValidation 12.0.0

## Database & Persistence
- **Database System:** PostgreSQL (latest)
- **ORM/Event Store:** Marten with Wolverine integration
- **Event Sourcing:** Marten Events with inline projections
- **Database Hosting:** Docker containerized PostgreSQL

## Frontend Framework
- **UI Framework:** Blazor Server + WebAssembly (.NET 9)
- **Component Library:** MudBlazor + MudExRichTextEditor
- **Authentication:** Microsoft.AspNetCore.Authentication.OpenIdConnect
- **Asset Management:** ASP.NET Core static files

## Authentication & Security
- **Identity Provider:** Keycloak (containerized)
- **Authentication Flow:** OpenID Connect
- **API Security:** Integrated with Wolverine HTTP

## External Integrations
- **AI/ML Service:** OpenAI GPT (2.0.0) with RAG implementation
- **Payment Processing:** Stripe.net (47.4.0)
- **Calendar Integration:** Google.Apis.Calendar.v3 (1.69.0.3746)
- **Video Conferencing:** Google Meet (via Calendar API)

## Development & Deployment
- **Container Platform:** Docker + Docker Compose
- **Configuration Management:** appsettings.json + User Secrets
- **Logging:** Serilog with structured logging
- **API Documentation:** Swagger/OpenAPI + Scalar
- **Testing Framework:** xUnit + Alba (HTTP testing) + Shouldly

## Infrastructure & Monitoring
- **Service Discovery:** Eureka (Fortium custom image)
- **Message Broker:** Wolverine in-memory (event forwarding)
- **Tracing:** OpenTelemetry + Zipkin
- **Metrics:** Prometheus + Grafana
- **Reverse Proxy:** Not specified (likely direct exposure for MVP)

## Code Repository
- **Version Control:** Git
- **Repository URL:** Local development (not specified)
- **Build Tool:** .NET CLI + Docker

## Deployment Solution
- **Local Development:** Docker Compose with process-compose.yaml
- **Container Orchestration:** Kubernetes manifests available in infrastructure/k8s/
- **Cloud Platform:** Not specified (infrastructure-agnostic)

## Development Tools
- **Build Automation:** Justfile for task management
- **Package Management:** NuGet (.NET packages)
- **Environment Configuration:** devbox.json + devbox.lock
- **IDE Integration:** User Secrets for development configuration