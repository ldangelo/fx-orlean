# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-14-automatic-role-assignment/spec.md

> Created: 2025-08-14
> Version: 1.0.0

## Technical Requirements

- Email domain parsing during authentication callback processing
- Role assignment logic integrated with existing User aggregate and UserHandler
- Keycloak claims integration to access user email during OpenID Connect flow
- User creation command enhancement to include automatic role determination
- Authentication success handling modification to apply role assignment
- Role-based access control validation for partner vs client features

## Approach Options

**Option A:** Authentication Middleware Approach
- Pros: Centralized logic, works across all authentication flows, easy to test
- Cons: Requires middleware setup, potential performance impact on every request

**Option B:** User Creation Command Enhancement (Selected)
- Pros: Leverages existing event sourcing architecture, fits with domain model, clear audit trail
- Cons: Only applies during user creation, requires user command modification

**Option C:** Keycloak Mapper Configuration
- Pros: External to application, handled by identity provider, no code changes
- Cons: Couples business logic to Keycloak config, harder to modify, less control

**Rationale:** Option B fits best with the existing CQRS/Event Sourcing architecture, provides clear audit trails through events, and keeps business logic within the domain model where it can be easily tested and modified.

## Implementation Details

### Authentication Flow Integration

1. **Email Extraction:** Extract user email from Keycloak claims during authentication callback
2. **Domain Detection:** Parse email domain and apply business rules for role assignment
3. **User Command Enhancement:** Modify CreateUserCommand to include role parameter
4. **Event Sourcing:** Emit UserCreatedEvent with assigned role for audit trail
5. **Authorization:** Ensure role-based access control works with assigned roles

### Business Logic

```csharp
public static class RoleAssignmentService
{
    private const string FORTIUM_DOMAIN = "@fortiumpartners.com";
    
    public static string DetermineUserRole(string email)
    {
        if (string.IsNullOrEmpty(email))
            return "CLIENT"; // Default fallback
            
        return email.EndsWith(FORTIUM_DOMAIN, StringComparison.OrdinalIgnoreCase) 
            ? "PARTNER" 
            : "CLIENT";
    }
}
```

### Authentication Integration

- Modify authentication success handler to extract email from claims
- Enhance CreateUserCommand to accept role parameter  
- Update UserHandler to process role assignment during user creation
- Ensure UserCreatedEvent includes role information for projections

## External Dependencies

- **No new libraries required** - Implementation uses existing Keycloak integration and event sourcing infrastructure
- **Configuration update** - May need appsettings configuration for domain matching rules (future flexibility)