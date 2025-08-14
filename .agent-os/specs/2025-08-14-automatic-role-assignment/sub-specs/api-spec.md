# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-08-14-automatic-role-assignment/spec.md

> Created: 2025-08-14
> Version: 1.0.0

## API Changes

### User Controller Enhancements

#### POST /users/create

**Purpose:** Enhanced user creation endpoint to support automatic role assignment
**Parameters:**
- `email` (string, required): User email address
- `name` (string, required): User display name
- `keycloakId` (string, required): Keycloak user identifier
**Response:** UserCreatedEvent with assigned role
**Errors:** 
- 400 Bad Request: Invalid email format
- 409 Conflict: User already exists

**Enhanced Request Model:**
```csharp
public class CreateUserRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string KeycloakId { get; set; }
    // Role will be automatically determined from email domain
}
```

**Enhanced Response Model:**
```csharp
public class CreateUserResponse
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string AssignedRole { get; set; } // NEW: Shows automatically assigned role
    public DateTime CreatedAt { get; set; }
}
```

### Authentication Callback Enhancement

#### GET /auth/callback

**Purpose:** Enhanced authentication callback to trigger automatic role assignment
**Parameters:** Standard OpenID Connect callback parameters
**Response:** Redirect to appropriate dashboard based on assigned role
**Errors:**
- 401 Unauthorized: Invalid authentication state
- 500 Internal Server Error: Role assignment failure

**Enhanced Flow:**
1. Extract email from Keycloak claims
2. Determine role based on email domain
3. Create or update user with assigned role
4. Redirect to role-specific dashboard

### Role Validation Endpoints

#### GET /users/{userId}/role

**Purpose:** Retrieve current user role for access control validation
**Parameters:** 
- `userId` (string, required): User identifier
**Response:** User role information
**Errors:**
- 404 Not Found: User does not exist
- 403 Forbidden: Insufficient permissions

**Response Model:**
```csharp
public class UserRoleResponse
{
    public string UserId { get; set; }
    public string Role { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignmentMethod { get; set; } // "AUTOMATIC_EMAIL_DOMAIN"
}
```

## Authentication Integration

### Keycloak Claims Processing

The authentication system will extract the following claims during callback:
- `email`: User's email address for domain analysis
- `name`: User's display name
- `sub`: Keycloak subject identifier

### Role Assignment Logic

```csharp
// Integrated into authentication callback processing
var userEmail = context.Principal.FindFirst("email")?.Value;
var userRole = RoleAssignmentService.DetermineUserRole(userEmail);

var createUserCommand = new CreateUserCommand
{
    Email = userEmail,
    Name = userName,
    KeycloakId = keycloakId,
    Role = userRole // NEW: Automatically determined role
};
```

## Security Considerations

- Email domain validation to prevent spoofing
- Role assignment audit trail through event sourcing
- Proper authorization checks for role-specific endpoints
- Protection against email enumeration attacks