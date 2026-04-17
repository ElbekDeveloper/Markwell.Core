# Feature Specification: Profile Management & Role-Based Access Control

**Feature Branch**: `002-profile-management`  
**Created**: April 16, 2026  
**Status**: Draft  
**Input**: "Build a central profile management feature using EF Core Identity with ASP.NET Core default auth endpoints, custom roles (Admin, Manager, Teacher, Student), and comprehensive test coverage (acceptance tests via .http files + unit tests)"

## User Scenarios & Testing

### User Story 1 - System Administrator Creates New User with Role Assignment (Priority: P1)

A system administrator needs to onboard new users into the Markwell platform with specific role assignments. The administrator must be able to create user profiles with predetermined roles without requiring immediate password setup by the user.

**Why this priority**: Role assignment and user creation are the foundational capabilities that enable all other user management and authorization features. Without this, the system cannot distinguish between different user types (Admin, Manager, Teacher, Student).

**Independent Test**: Can be fully tested by creating a user with a specific role and verifying the role is persisted and returned in subsequent profile queries.

**Acceptance Scenarios**:

1. **Given** admin user is authenticated, **When** admin creates a new user with role "Teacher", **Then** user record is created with Teacher role and can be retrieved with assigned role
2. **Given** admin user is authenticated, **When** admin attempts to create duplicate user, **Then** system rejects creation and returns conflict error
3. **Given** admin user is authenticated, **When** admin creates user, **Then** user cannot login until password is set
4. **Given** non-admin user, **When** attempting to create a new user, **Then** system rejects request with authorization error

---

### User Story 2 - User Registers and Sets Up Profile (Priority: P1)

New users need to self-register or activate accounts after receiving an invitation, setting their own password and completing their profile information before accessing the system.

**Why this priority**: User registration is critical for onboarding and system accessibility. Both self-registration flows and invitation-based flows are common in educational platforms, supporting different user types (students may self-register, staff may be invited).

**Independent Test**: Can be fully tested by completing registration flow and verifying user can authenticate with new credentials.

**Acceptance Scenarios**:

1. **Given** registration endpoint is available, **When** new user submits valid email and password, **Then** user account is created and user can authenticate
2. **Given** user is invited, **When** user clicks activation link, **Then** user is prompted to set password and complete profile
3. **Given** user attempts registration with duplicate email, **When** submission is made, **Then** system rejects and indicates email already exists
4. **Given** user attempts registration with weak password, **When** submission is made, **Then** system rejects with password strength requirements

---

### User Story 3 - User Updates Their Profile Information (Priority: P2)

Authenticated users need to update their profile details (name, email, contact information) and manage their account settings after initial registration.

**Why this priority**: User profile management is important for maintaining accurate user information, but comes after initial registration and authentication. This enables self-service account maintenance.

**Independent Test**: Can be fully tested by updating a user's profile fields and verifying changes are persisted and reflected in subsequent profile retrievals.

**Acceptance Scenarios**:

1. **Given** authenticated user, **When** user updates their name, **Then** change is persisted and visible in profile
2. **Given** authenticated user, **When** user attempts to change email to one already in use, **Then** system rejects change with conflict error
3. **Given** authenticated user, **When** user updates profile, **Then** only authenticated user's own profile can be updated (no access to other profiles)
4. **Given** authenticated user, **When** user changes email, **Then** verification flow is triggered before email change is confirmed

---

### User Story 4 - User Changes Password and Manages Account Security (Priority: P2)

Users need to change their password and access security-related settings to maintain account security.

**Why this priority**: Password management is a critical security feature but secondary to initial authentication setup. Supports password rotation and compromised account recovery.

**Independent Test**: Can be fully tested by changing password and verifying old credentials no longer authenticate while new credentials work.

**Acceptance Scenarios**:

1. **Given** authenticated user, **When** user provides current password and new password, **Then** password is updated and user can authenticate with new password
2. **Given** authenticated user, **When** user provides incorrect current password, **Then** system rejects change and requires correct current password
3. **Given** authenticated user, **When** user attempts password change with weak password, **Then** system rejects with password strength requirements
4. **Given** password changed, **When** user attempts authentication with old password, **Then** authentication fails

---

### User Story 5 - Manager or Admin Manages User Roles and Permissions (Priority: P3)

Manager-level or admin-level users need to modify user roles and permissions for their managed scope without recreating user accounts.

**Why this priority**: Role modification capability is important for ongoing user management and team organization, but less critical than initial account creation and user self-management.

**Independent Test**: Can be fully tested by changing a user's role and verifying the new role is reflected in subsequent profile queries and authorization checks.

**Acceptance Scenarios**:

1. **Given** manager user with authority, **When** manager updates another user's role from "Student" to "Teacher", **Then** role is updated and user has corresponding authorization
2. **Given** user without authorization, **When** attempting to modify user roles, **Then** system rejects with authorization error
3. **Given** multiple roles are assigned to a user, **When** user authenticates, **Then** all roles are included in user context
4. **Given** user role is changed, **When** user makes subsequent requests, **Then** new authorization rules apply

---

### User Story 6 - List and Search Users by Role or Criteria (Priority: P3)

Administrators and managers need to list, search, and filter users by role, status, or other criteria for user management and reporting purposes.

**Why this priority**: User discovery and filtering is useful for administrative operations, but not required for core user management functionality.

**Independent Test**: Can be fully tested by creating multiple users with different roles and verifying search/filter operations return correct subsets.

**Acceptance Scenarios**:

1. **Given** admin user, **When** admin searches for all users with role "Teacher", **Then** only Teacher-role users are returned
2. **Given** manager user with scope, **When** manager lists users, **Then** only users within manager's scope are returned
3. **Given** user list endpoint, **When** non-admin user attempts list, **Then** system rejects with authorization error
4. **Given** multiple users exist, **When** admin searches by criteria, **Then** pagination is supported for large result sets

---

### Edge Cases

- What happens when user attempts to authenticate with an unconfirmed email or account that's been deactivated?
- How does the system handle role assignment conflicts or removal of last admin role?
- What happens if email verification token expires during password reset or registration?
- How are existing user profiles migrated if roles are added/modified after initial deployment?
- How does the system handle concurrent password change requests or simultaneous role modifications?

## Requirements

### Functional Requirements

- **FR-001**: System MUST support user account creation with email and password via `POST /register` endpoint (standard REST convention)
- **FR-002**: System MUST validate email addresses during registration and verify ownership before confirming accounts
- **FR-003**: System MUST support four predefined roles: Admin, Manager, Teacher, Student
- **FR-004**: System MUST allow Admin users to assign roles to users at account creation time
- **FR-005**: System MUST allow authenticated users to view their own profile (name, email, roles)
- **FR-006**: System MUST allow authenticated users to update their profile information (name, contact details)
- **FR-007**: System MUST allow authenticated users to change their password with current password verification
- **FR-008**: System MUST support role-based access control (RBAC) - only authorized users can perform role-management actions
- **FR-009**: System MUST persist user profiles and role assignments in a central repository accessible across all Markwell services
- **FR-010**: System MUST implement password strength validation (minimum length, complexity requirements)
- **FR-011**: System MUST prevent SQL injection and unauthorized role escalation attacks
- **FR-012**: System MUST return appropriate HTTP status codes (200, 201, 400, 401, 403, 404, 409) for all endpoints
- **FR-016**: System MUST support user authentication via `POST /login` endpoint with email and password credentials
- **FR-013**: System MUST support user deactivation/soft deletion (account remains in system but cannot authenticate)
- **FR-014**: System MUST track user creation, modification, and last login timestamps
- **FR-015**: System MUST provide search and list endpoints for admin/manager users to discover other users with filtering by role

### Key Entities

- **User**: Represents an individual in the Markwell system
  - Attributes: Id (unique identifier), Email, FullName, CreatedAt, UpdatedAt, LastLoginAt, IsActive, PasswordHash
  - Relationships: Has one or more Roles

- **Role**: Represents authorization level within the system
  - Predefined values: Admin, Manager, Teacher, Student
  - Attributes: Id, Name, CreatedAt

- **UserRole**: Junction entity linking Users to Roles (many-to-many relationship)
  - Represents assignment of role to user
  - Attributes: UserId, RoleId, AssignedAt

## Success Criteria

- **SC-001**: System successfully authenticates users with valid email/password combinations within 500ms
- **SC-002**: User profile updates persist within 1 second and are visible in subsequent queries
- **SC-003**: Role-based authorization decisions are made within 100ms
- **SC-004**: System supports at least 1,000 concurrent authenticated users
- **SC-005**: Password changes are effective immediately (old passwords no longer authenticate)
- **SC-006**: Role modifications propagate to all system authorization checks within 2 seconds
- **SC-007**: Email verification tokens are single-use and expire after 24 hours
- **SC-008**: Password strength validation is enforced consistently across all password creation/modification flows
- **SC-009**: Administrators can create new users and assign roles in fewer than 10 seconds (full flow)
- **SC-010**: New users can complete registration and authentication within 2 minutes

## Assumptions

- Password hashing will use industry-standard bcrypt or equivalent (not plaintext or weak hashing)
- Default ASP.NET Core Identity configuration provides OpenAPI/Swagger discovery (to be clarified)
- Role assignment follows principle of least privilege (users get minimum necessary role)
- Email is treated as unique identifier for user accounts
- System will not support role inheritance (roles are flat, not hierarchical)
- All timestamps use UTC (system-wide convention)
- Single sign-on (SSO) integration is out of scope for this feature; focus is local authentication only
- Multi-factor authentication (MFA) is out of scope for initial release

## Testing Strategy

- **Acceptance Tests**: `.http` files containing real HTTP request/response examples for each user story
  - Example endpoints: `POST /auth/register`, `GET /profile`, `PUT /profile`, `POST /auth/change-password`, `POST /users` (admin), etc.
  - Each acceptance test demonstrates happy path and key error scenarios
  
- **Unit Tests**: Test individual business logic functions
  - Password validation functions
  - Role authorization checks
  - User entity operations (create, update, retrieval)
  - Email format validation

- **Integration Tests**: Verify EF Core Identity integration
  - Database persistence and retrieval
  - User authentication via ASP.NET Core Identity
  - Role assignment and query

## Out of Scope

- Social media authentication (OAuth2, OIDC)
- Multi-factor authentication (MFA)
- LDAP or Active Directory integration
- Role hierarchy or nested roles
- User groups or team management
- Account lockout policies (beyond authentication framework defaults)
- Password history or expiration policies
- User impersonation or elevated privileges
