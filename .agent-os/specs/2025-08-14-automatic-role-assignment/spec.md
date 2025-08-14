# Spec Requirements Document

> Spec: Automatic Role Assignment Based on Email Domain
> Created: 2025-08-14
> Status: Planning

## Overview

Implement automatic role assignment during user authentication where users with email addresses ending in "@fortiumpartners.com" are assigned the PARTNER role, while all other users are assigned the CLIENT role. This feature streamlines the onboarding process and ensures proper access control based on organizational affiliation.

## User Stories

### Fortium Partner Authentication

As a Fortium partner with a @fortiumpartners.com email address, I want to be automatically assigned the PARTNER role when I authenticate, so that I can immediately access partner-specific features like session management, availability settings, and earnings tracking without manual intervention.

**Detailed Workflow:**
1. Partner logs in using Keycloak with fortiumpartners.com email
2. System detects email domain during authentication callback
3. User is automatically assigned PARTNER role
4. Partner is redirected to partner dashboard with full access
5. Partner can immediately manage availability and view upcoming sessions

### Client Authentication

As a business user with any other email domain, I want to be automatically assigned the CLIENT role when I authenticate, so that I can access client features like partner search, booking consultations, and payment processing without confusion about my permissions.

**Detailed Workflow:**
1. Client logs in using Keycloak with non-Fortium email domain
2. System detects non-fortiumpartners.com email during authentication
3. User is automatically assigned CLIENT role  
4. Client is redirected to client interface with booking capabilities
5. Client can search partners, book consultations, and manage sessions

## Spec Scope

1. **Email Domain Detection** - Parse user email during authentication to identify domain
2. **Role Assignment Logic** - Implement business rules for PARTNER vs CLIENT role assignment
3. **Keycloak Integration** - Ensure role assignment works with existing OpenID Connect flow
4. **User Creation Enhancement** - Update user creation process to include automatic role assignment
5. **Authentication Callback Updates** - Modify authentication success handling to apply role logic

## Out of Scope

- Manual role override functionality (future enhancement)
- Multiple role assignments per user
- Email domain whitelist management UI
- Role change notifications or audit logs
- Integration with external HR systems for role validation

## Expected Deliverable

1. **Automatic Role Assignment** - Users are assigned correct roles based on email domain without manual intervention
2. **Seamless Authentication Flow** - Role assignment happens transparently during login process
3. **Proper Access Control** - Users immediately have access to role-appropriate features after authentication

## Spec Documentation

- Tasks: @.agent-os/specs/2025-08-14-automatic-role-assignment/tasks.md
- Technical Specification: @.agent-os/specs/2025-08-14-automatic-role-assignment/sub-specs/technical-spec.md
- API Specification: @.agent-os/specs/2025-08-14-automatic-role-assignment/sub-specs/api-spec.md
- Tests Specification: @.agent-os/specs/2025-08-14-automatic-role-assignment/sub-specs/tests.md