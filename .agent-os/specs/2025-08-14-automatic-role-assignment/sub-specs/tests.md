# Tests Specification

This is the tests coverage details for the spec detailed in @.agent-os/specs/2025-08-14-automatic-role-assignment/spec.md

> Created: 2025-08-14
> Version: 1.0.0

## Test Coverage

### Unit Tests

**RoleAssignmentService**
- Test PARTNER role assignment for @fortiumpartners.com emails
- Test CLIENT role assignment for other email domains
- Test case-insensitive domain matching
- Test null/empty email handling with CLIENT default
- Test various email formats and edge cases
- Test email with multiple @ symbols
- Test subdomain scenarios (user@sub.fortiumpartners.com)

**CreateUserCommand Enhancement**
- Test user creation with automatic role assignment
- Test command validation with role parameter
- Test event emission includes assigned role
- Test duplicate user creation prevention

**UserHandler**
- Test UserCreatedEvent handling with role assignment
- Test user projection updates include role information
- Test event sourcing stores role assignment audit trail

### Integration Tests

**Authentication Flow**
- Test complete authentication callback with role assignment
- Test Keycloak claims extraction during login
- Test user creation during first-time authentication
- Test existing user login without role conflicts
- Test redirect behavior based on assigned role

**User Controller API**
- Test POST /users/create with automatic role assignment
- Test GET /users/{userId}/role endpoint functionality
- Test authentication callback role assignment integration
- Test error handling for invalid email formats

**Role-Based Access Control**
- Test PARTNER role access to partner-specific features
- Test CLIENT role access to client-specific features
- Test role-based authorization across application endpoints
- Test unauthorized access prevention

### Feature Tests

**End-to-End Authentication Scenarios**
- New Fortium partner first-time login → PARTNER role → partner dashboard
- New client first-time login → CLIENT role → client interface
- Existing user login maintains assigned role
- Role-appropriate feature access after authentication

**Edge Case Scenarios**
- Invalid email domain format handling
- Authentication failure recovery
- Concurrent user creation prevention
- Email case sensitivity testing

## Mocking Requirements

**Keycloak Integration**
- Mock OpenID Connect authentication flow
- Mock Keycloak claims with test email addresses
- Mock authentication callback processing
- Simulate Keycloak failures and recovery

**Email Service**
- Mock email validation and parsing
- Mock domain extraction logic
- Simulate various email format scenarios

**Event Store**
- Mock event sourcing for user creation
- Mock UserCreatedEvent emission and handling
- Simulate event store failures and recovery

## Test Data

### Test Email Addresses

**PARTNER Role Assignment:**
- john.doe@fortiumpartners.com
- admin@fortiumpartners.com
- consultant@FORTIUMPARTNERS.COM (case insensitive)

**CLIENT Role Assignment:**
- user@gmail.com
- business@company.com
- contact@startup.io
- admin@clientcompany.com

**Edge Cases:**
- user@subdomain.fortiumpartners.com (should be PARTNER)
- invalid@email (should default to CLIENT)
- null or empty email (should default to CLIENT)

### Test Scenarios

**Successful Role Assignment:**
1. Authentication with fortiumpartners.com email → PARTNER role
2. Authentication with other domain → CLIENT role
3. Role-based dashboard redirect
4. Proper access control enforcement

**Error Handling:**
1. Invalid email format during authentication
2. Keycloak claims missing email
3. User creation failure handling
4. Role assignment failure recovery