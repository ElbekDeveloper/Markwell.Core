# API Contract: User Profile Endpoints

**Date**: April 16, 2026  
**Feature**: Profile Management & Role-Based Access Control  
**Endpoints**: `GET /users/{id}`, `PUT /users/{id}`, `POST /users/{id}/change-password`, `GET /users`

## Endpoint: Get User Profile

**HTTP Method & Path**: `GET /users/{id}`

**Purpose**: Retrieve authenticated user's own profile or admin retrieving any user's profile

**Authorization**: Required (Bearer token in header)

**Path Parameters**:
- `id` (string, required): User ID (GUID as string)

**Response** (200 OK):
```json
{
  "id": "string (GUID as string)",
  "email": "string",
  "userName": "string",
  "fullName": "string",
  "phoneNumber": "string (nullable)",
  "emailConfirmed": true,
  "phoneNumberConfirmed": false,
  "isActive": true,
  "createdAt": "2026-04-16T10:00:00Z",
  "updatedAt": "2026-04-16T14:30:00Z",
  "lastLoginAt": "2026-04-16T14:25:00Z",
  "roles": ["string array, e.g., ['Teacher', 'Manager']"]
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 401 | `UNAUTHORIZED` | Authentication token is missing or invalid | No Bearer token or token expired |
| 403 | `FORBIDDEN` | You can only view your own profile | Non-admin user requests another user's profile |
| 404 | `USER_NOT_FOUND` | User not found | id doesn't exist |

**Authorization Rules**:
- Admin: Can retrieve any user's profile
- Non-admin: Can only retrieve their own profile
- All authenticated users can retrieve their own profile

---

## Endpoint: Update User Profile

**HTTP Method & Path**: `PUT /users/{id}`

**Purpose**: Update authenticated user's profile information (name, email, phone)

**Authorization**: Required (Bearer token in header)

**Path Parameters**:
- `id` (string, required): User ID (must match authenticated user unless admin)

**Request Body** (JSON):
```json
{
  "fullName": "string (optional, 1-255 chars, no leading/trailing whitespace)",
  "email": "string (optional, email format, must be unique if changed)",
  "phoneNumber": "string (optional, E.164 format or null to remove)"
}
```

**Response** (200 OK):
```json
{
  "id": "string",
  "email": "string",
  "userName": "string",
  "fullName": "string",
  "phoneNumber": "string (nullable)",
  "emailConfirmed": "bool (true if email unchanged, false if changed)",
  "isActive": true,
  "updatedAt": "2026-04-16T14:35:00Z",
  "roles": ["string array"]
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 400 | `INVALID_EMAIL` | Email format is invalid | Email doesn't match RFC 5322 |
| 401 | `UNAUTHORIZED` | Authentication required | No Bearer token |
| 403 | `FORBIDDEN` | You can only update your own profile | Non-admin user updates another profile |
| 404 | `USER_NOT_FOUND` | User not found | id doesn't exist |
| 409 | `EMAIL_EXISTS` | Email is already in use | New email not unique |
| 422 | `VALIDATION_ERROR` | Full name must not be empty | FullName is empty string |

**Behavior**:
- Updates only non-null fields (optional = can be omitted)
- If email is changed, EmailConfirmed set to false; verification email sent
- FullName trimmed of leading/trailing whitespace
- UpdatedAt timestamp updated to current UTC time
- Idempotent: same request twice produces same result

**Authorization Rules**:
- Admin: Can update any user's profile
- Non-admin: Can only update their own profile
- Manager: Can update users within their scope (scope definition in implementation)

---

## Endpoint: Change Password

**HTTP Method & Path**: `POST /users/{id}/change-password`

**Purpose**: Allow user to change their password with current password verification

**Authorization**: Required (Bearer token in header)

**Path Parameters**:
- `id` (string, required): User ID (must match authenticated user)

**Request Body** (JSON):
```json
{
  "currentPassword": "string (required, current password for verification)",
  "newPassword": "string (required, min 8 chars, must include upper, lower, digit, special)"
}
```

**Response** (200 OK):
```json
{
  "message": "Password changed successfully",
  "updatedAt": "2026-04-16T14:40:00Z"
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 400 | `INVALID_PASSWORD` | Current password is incorrect | currentPassword doesn't match |
| 400 | `PASSWORD_WEAK` | Password must contain uppercase, lowercase, digit, special | newPassword validation fails |
| 400 | `PASSWORD_SAME` | New password must be different from current password | newPassword equals currentPassword |
| 401 | `UNAUTHORIZED` | Authentication required | No Bearer token |
| 403 | `FORBIDDEN` | Users can only change their own password | Attempting to change another user's password |
| 404 | `USER_NOT_FOUND` | User not found | id doesn't exist |

**Behavior**:
- Requires valid current password (prevents unauthorized password change if account is compromised)
- New password must pass strength validation (min 8, upper, lower, digit, special)
- New password must differ from current password
- After successful change, all existing tokens invalidated (user must login again)
- PasswordHash and SecurityStamp updated in database

---

## Endpoint: List Users (Admin/Manager)

**HTTP Method & Path**: `GET /users`

**Purpose**: List and search users (admin/manager only)

**Authorization**: Required; must have Admin or Manager role

**Query Parameters**:
```
?role=Teacher              # Filter by role name (case-insensitive)
&search=john              # Search in email or fullName (substring match)
&pageNumber=1             # Page number (1-indexed, default 1)
&pageSize=20              # Items per page (default 20, max 100)
&sortBy=createdAt         # Sort field: email, fullName, createdAt, lastLoginAt (default createdAt)
&sortOrder=desc           # Sort direction: asc or desc (default desc)
```

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "string",
      "email": "string",
      "userName": "string",
      "fullName": "string",
      "isActive": true,
      "createdAt": "2026-04-16T10:00:00Z",
      "lastLoginAt": "2026-04-16T14:25:00Z",
      "roles": ["string array"]
    }
  ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 401 | `UNAUTHORIZED` | Authentication required | No Bearer token |
| 403 | `FORBIDDEN` | Admin or Manager role required | User lacks authority |
| 400 | `INVALID_PARAMETER` | Invalid page size (1-100) | pageSize out of range |

**Filtering Behavior**:
- Admin: Can list all users
- Manager: Can list only users within their scope (scope definition in implementation)
- search: Case-insensitive partial match on email or fullName
- role: Case-insensitive exact match on role name
- Multiple filters combined with AND logic

**Pagination**:
- pageNumber: 1-indexed (first page is 1, not 0)
- pageSize: Default 20, max 100
- Returns totalCount and totalPages for client-side pagination UI

---

## Security Considerations

1. **Authentication**: All endpoints except /register and /login require valid JWT Bearer token
2. **Authorization**: Role-based access control (RBAC) enforced:
   - Admin: Full access
   - Manager: Limited scope (own users)
   - Teacher/Student: No profile modification for others
3. **Password Verification**: Change password requires current password; prevents unauthorized changes
4. **Email Verification**: Changing email triggers new verification flow
5. **Token Invalidation**: Password changes invalidate existing tokens; user must login again
6. **Rate Limiting**: [To be configured in implementation]

---

## Related Contracts

- Authentication: `auth-endpoints.contract.md`
- Role Management: `role-management.contract.md`
