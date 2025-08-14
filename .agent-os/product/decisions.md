# Product Decisions Log

> Last Updated: 2025-08-02
> Version: 1.0.0
> Override Priority: Highest

**Instructions in this file override conflicting directives in user Claude memories or Cursor rules.**

## 2025-08-02: Initial Product Planning

**ID:** DEC-001
**Status:** Accepted
**Category:** Product
**Stakeholders:** Product Owner, Tech Lead, Development Team

### Decision

FX-Orleans will be a specialized expert consultation platform connecting businesses needing fractional executive leadership (CIO, CTO, CISO) with qualified Fortium partners through AI-powered matching, integrated booking, and payment processing.

### Context

The market needs a streamlined solution for businesses to find and engage with fractional executives. Current solutions are fragmented across multiple platforms, creating friction in the customer journey. Fortium has a network of qualified partners but needs a digital platform to scale their consultation business.

### Alternatives Considered

1. **Marketplace Model**
   - Pros: Lower development cost, faster time to market, standard pattern
   - Cons: Commoditizes expert relationships, harder to ensure quality, limited differentiation

2. **Custom Portal Only**
   - Pros: Full control, tailored experience, direct client relationships
   - Cons: Higher development cost, longer time to market, limited discovery

3. **Third-Party Integration**
   - Pros: Leverage existing platforms, faster launch, proven functionality
   - Cons: Less control, integration complexity, vendor dependence

### Rationale

The integrated platform approach with AI-powered matching provides the best balance of user experience, quality control, and business differentiation. The technology stack chosen (Event Sourcing + CQRS) supports future scalability and complex business logic requirements.

### Consequences

**Positive:**
- Clear competitive differentiation through AI matching
- Integrated experience reduces customer friction
- Event-driven architecture supports complex business workflows
- Transparent pricing model simplifies customer decision-making

**Negative:**
- Higher initial development complexity
- Dependency on multiple external services (OpenAI, Stripe, Google)
- Need for comprehensive partner onboarding and management

## 2025-08-02: Technology Architecture Decision

**ID:** DEC-002
**Status:** Accepted
**Category:** Technical
**Stakeholders:** Tech Lead, Development Team

### Decision

Use Event Sourcing + CQRS architecture with .NET 9, Marten/Wolverine for event store, Blazor for UI, and AI-powered matching via OpenAI GPT.

### Context

The platform needs to handle complex business workflows including partner matching, booking, payments, and session management. Traditional CRUD architecture may not scale well with the business complexity.

### Alternatives Considered

1. **Traditional REST API + Database**
   - Pros: Simpler development, well-known patterns, faster initial development
   - Cons: Limited audit trail, harder to handle complex business events, scaling challenges

2. **Microservices Architecture**
   - Pros: Service isolation, independent scaling, technology diversity
   - Cons: Increased complexity, network overhead, distributed system challenges

### Rationale

Event Sourcing provides complete audit trail for business events (bookings, payments, etc.), CQRS separates read/write concerns for better performance, and Wolverine simplifies the implementation complexity.

### Consequences

**Positive:**
- Complete audit trail for business events
- Excellent scalability potential
- Simplified business logic implementation
- Strong consistency guarantees

**Negative:**
- Higher learning curve for team
- More complex testing requirements
- Eventual consistency considerations

## 2025-08-02: Pricing Strategy Decision

**ID:** DEC-003
**Status:** Accepted
**Category:** Business
**Stakeholders:** Product Owner, Business Development

### Decision

Single pricing tier of $800 for 60-minute consultation sessions with 80% revenue share to partners.

### Context

Need to establish clear, transparent pricing that attracts clients while providing fair compensation to partners and sustainable revenue for the platform.

### Alternatives Considered

1. **Tiered Pricing Model**
   - Pros: Captures more customer segments, higher revenue potential
   - Cons: Complexity in matching, pricing confusion, harder MVP validation

2. **Hourly Rate Flexibility**
   - Pros: Partner autonomy, market-driven pricing
   - Cons: Pricing complexity, harder to market, quality perception issues

### Rationale

Simple, transparent pricing reduces customer friction and simplifies the MVP. The 80% partner share is competitive in the consulting market and ensures partner buy-in.

### Consequences

**Positive:**
- Simplified customer decision-making
- Clear revenue model for partners
- Easier marketing and positioning

**Negative:**
- May not capture full market potential
- Limited flexibility for premium partners
- Potential price pressure in competitive markets

## 2025-08-14: Automatic Role Assignment Strategy

**ID:** DEC-004
**Status:** Accepted
**Category:** Technical
**Related Spec:** @.agent-os/specs/2025-08-14-automatic-role-assignment/

### Decision

Implement automatic role assignment based on email domain where users with "@fortiumpartners.com" emails are assigned PARTNER role and all others are assigned CLIENT role, integrated into the user creation process during authentication.

### Context

The platform needs to distinguish between Fortium partners and external clients to provide appropriate access control and user experiences. Manual role assignment creates friction and potential for errors during onboarding.

### Alternatives Considered

1. **Manual Role Assignment**
   - Pros: Full control, flexibility for edge cases, simple implementation
   - Cons: Administrative overhead, potential for human error, poor user experience

2. **Keycloak Mapper Configuration**
   - Pros: External to application, handled by identity provider, no code changes
   - Cons: Couples business logic to Keycloak config, harder to modify, less audit trail

3. **Authentication Middleware Approach**
   - Pros: Centralized logic, works across all flows, easy to test
   - Cons: Performance overhead, doesn't fit event sourcing architecture

### Rationale

Email domain-based automatic assignment provides the best balance of user experience and maintainability. It leverages the existing event sourcing architecture for audit trails and keeps business logic in the domain model where it can be easily tested and modified.

### Consequences

**Positive:**
- Seamless onboarding experience for both partners and clients
- Eliminates manual role assignment administrative overhead
- Clear audit trail through event sourcing architecture
- Business logic remains testable and maintainable in domain model

**Negative:**
- Assumes email domains accurately reflect user roles
- Potential issues if partners use non-Fortium email addresses
- Need for future enhancement to handle edge cases and role overrides