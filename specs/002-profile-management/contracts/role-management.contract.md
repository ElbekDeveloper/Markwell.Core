# API Contract: Role Management Endpoints

**Date**: April 16, 2026  
**Feature**: Profile Management & Role-Based Access Control  
**Endpoints**: `POST /users/{id}/roles`, `DELETE /users/{id}/roles/{roleId}`, `GET /roles`

## Endpoint: Assign Role to User

**HTTP Method & Path**: `POST /users/{id}/roles`

**Purpose**: Assign a role to a user (admin only)

**Authorization**: Required; must have Admin role

**Path Parameters**:
- `id` (string, required): User ID (GUID as string)

**Request Body** (JSON):
```json
{
  "roleName": "string (required, one of: Admin, Manager, Teacher, Student)"
}
```

**Response** (201 Created):
```json
{
  "userId": "string",
  "roleId": "string",
  "roleName": "string",
  "assignedAt": "2026-04-16T14:45:00Z",
  "assignedBy": "string (admin user ID who made assignment)"
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 400 | `INVALID_ROLE` | Role must be one of: Admin, Manager, Teacher, Student | roleName not valid |
| 401 | `UNAUTHORIZED` | Authentication required | No Bearer token |
| 403 | `FORBIDDEN` | Admin role required | Non-admin user attempts assignment |
| 404 | `USER_NOT_FOUND` | User not found | id doesn't exist |
| 409 | `ROLE_EXISTS` | User already has this role | (userId, roleName) combination already exists |

**Behavior**:
- User can have multiple roles
- Duplicate role assignment rejected (409 conflict)
- AssignedAt set to current UTC time
- AssignedBy set to authenticated admin's user ID
- Idempotent at database level: assigning same role twice fails with 409

**Authorization Rules**:
- Only Admin users can assign roles
- Cannot assign Admin role to self (prevent accidental removal of admin access)

---

## Endpoint: Remove Role from User

**HTTP Method & Path**: `DELETE /users/{id}/roles/{roleId}`

**Purpose**: Remove a role from a user (admin only)

**Authorization**: Required; must have Admin role

**Path Parameters**:
- `id` (string, required): User ID
- `roleId` (string, required): Role ID (GUID as string)

**Response** (204 No Content):
- No response body on success

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 400 | `CANNOT_REMOVE_LAST_ROLE` | User must have at least one role | User has only this role |
| 400 | `CANNOT_REMOVE_LAST_ADMIN` | Cannot remove the last Admin user from system | Last Admin role removal |
| 401 | `UNAUTHORIZED` | Authentication required | No Bearer token |
| 403 | `FORBIDDEN` | Admin role required | Non-admin user attempts removal |
| 404 | `USER_NOT_FOUND` | User not found | id doesn't exist |
| 404 | `ROLE_NOT_FOUND` | Role not found | roleId doesn't exist or user doesn't have this role |

**Behavior**:
- Cannot remove user's last role (user must always have at least one role)
- Cannot remove last Admin role from system (must have at least one Admin)
- Idempotent: removing non-existent role returns 404
- Returns 204 on success (no content)

**Authorization Rules**:
- Only Admin users can remove roles
- Cannot remove your own Admin role (prevents accidental admin lockout)

---

## Endpoint: List Available Roles

**HTTP Method & Path**: `GET /roles`

**Purpose**: List all available predefined roles in the system

**Authorization**: Optional (public endpoint, but may be restricted to authenticated users)

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "string (GUID)",
      "name": "string (Admin, Manager, Teacher, or Student)",
      "normalizedName": "string (ADMIN, MANAGER, TEACHER, STUDENT)",
      "createdAt": "2026-04-16T10:00:00Z"
    }
  ],
  "totalCount": 4
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 401 | `UNAUTHORIZED` | Authentication required | If endpoint requires auth and token missing |

**Behavior**:
- Returns all four predefined roles (Admin, Manager, Teacher, Student)
- Ordered by creation date (seed order)
- Static response; roles cannot be created or deleted dynamically
- Cached for 1 hour to reduce database queries

---

## Endpoint: Get User's Roles

**HTTP Method & Path**: `GET /users/{id}/roles`

**Purpose**: List all roles assigned to a specific user

**Authorization**: Required (Bearer token); must be the user or Admin

**Path Parameters**:
- `id` (string, required): User ID

**Response** (200 OK):
```json
{
  "userId": "string",
  "roles": [
    {
      "id": "string (GUID)",
      "name": "string",
      "normalizedName": "string",
      "assignedAt": "2026-04-16T14:00:00Z",
      "assignedBy": "string (admin user ID, nullable)"
    }
  ],
  "totalCount": 2
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 401 | `UNAUTHORIZED` | Authentication required | No Bearer token |
| 403 | `FORBIDDEN` | Can only view your own roles or be Admin | Non-admin user requests another user |
| 404 | `USER_NOT_FOUND` | User not found | id doesn't exist |

**Authorization Rules**:
- Users can only view their own roles
- Admin can view any user's roles
- Non-admin non-owner receives 403

---

## Role-Based Access Control Enforcement

**Authorization Checks** (all endpoints respect):

| Role | Can View Own Profile | Can Edit Own Profile | Can View Other Users | Can Edit Other Users | Can Assign/Remove Roles | Can View System Users |
|------|-----|-----|---|---|-----|---|
| Admin | ✅ | ✅ | ✅ All | ✅ All | ✅ | ✅ All |
| Manager | ✅ | ✅ | ✅ Scope | ✅ Scope | ❌ | ✅ Scope |
| Teacher | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Student | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

**Scope Definition** (to be configured in implementation):
- Manager "scope" typically includes users created by manager, team members, or organizational unit members
- Default: Manager can view/edit users created within same organizational context

---

## Security Considerations

1. **Role Assignment Authority**: Only Admin can assign/remove roles
2. **Atomic Operations**: Role assignments are atomic (all-or-nothing)
3. **Audit Trail**: AssignedAt and AssignedBy tracked for all role changes
4. **System Integrity**: Last Admin and user must have at least one role are enforced
5. **Token-Based Auth**: All operations require valid JWT Bearer token

---

## Related Contracts

- Authentication: `auth-endpoints.contract.md`
- User Profile: `user-profile.contract.md`
