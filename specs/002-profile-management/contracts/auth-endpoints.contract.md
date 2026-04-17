# API Contract: Authentication Endpoints

**Date**: April 16, 2026  
**Feature**: Profile Management & Role-Based Access Control  
**Endpoints**: `POST /auth/register`, `POST /auth/login`, `POST /auth/confirm-email`

## Endpoint: Register New User

**HTTP Method & Path**: `POST /auth/register`

**Purpose**: Create a new user account with email and password

**Request Body** (JSON):
```json
{
  "email": "string (required, email format, max 256 chars, unique)",
  "password": "string (required, min 8 chars, must include upper, lower, digit, special char)",
  "fullName": "string (required, 1-255 chars, no leading/trailing whitespace)",
  "userName": "string (optional, alphanumeric + underscore/hyphen, 3-50 chars, unique)"
}
```

**Response** (201 Created):
```json
{
  "id": "string (GUID converted to string)",
  "email": "string",
  "userName": "string",
  "fullName": "string",
  "emailConfirmed": false,
  "isActive": true,
  "createdAt": "2026-04-16T14:30:00Z",
  "roles": ["string array of role names, empty until confirmed"]
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 400 | `INVALID_EMAIL` | Email format is invalid | Email doesn't match RFC 5322 |
| 400 | `PASSWORD_WEAK` | Password must contain uppercase, lowercase, digit, special char | Password validation fails |
| 409 | `EMAIL_EXISTS` | Email is already registered | Email is not unique |
| 409 | `USERNAME_EXISTS` | Username is already taken | Username not unique (if provided) |
| 422 | `VALIDATION_ERROR` | Full name is required and cannot be empty | FullName is empty/null |

**Notes**:
- User cannot authenticate until email is confirmed via verification link
- If userName not provided, defaults to email address
- Password is immediately hashed (plaintext never stored)
- User created with EmailConfirmed=false and IsActive=true

---

## Endpoint: User Login

**HTTP Method & Path**: `POST /auth/login`

**Purpose**: Authenticate user with email and password; return authentication token

**Request Body** (JSON):
```json
{
  "email": "string (required, email format)",
  "password": "string (required)"
}
```

**Response** (200 OK):
```json
{
  "id": "string (GUID converted to string)",
  "email": "string",
  "userName": "string",
  "fullName": "string",
  "roles": ["string array of role names, e.g., ['Admin', 'Teacher']"],
  "token": "string (JWT token or session ID)",
  "expiresAt": "2026-04-16T22:30:00Z"
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 400 | `INVALID_CREDENTIALS` | Email or password is incorrect | User not found or password doesn't match |
| 401 | `EMAIL_NOT_CONFIRMED` | Please verify your email before signing in | EmailConfirmed is false |
| 403 | `ACCOUNT_DISABLED` | Your account has been deactivated | IsActive is false |
| 422 | `VALIDATION_ERROR` | Email and password are required | Email or password is empty/null |

**Token Format**:
- JWT Bearer token with claims: userId, email, roles, exp (expiration)
- Token valid for 1 hour (3600 seconds)
- Used in subsequent requests: `Authorization: Bearer <token>`

**Notes**:
- Email is case-insensitive for lookup (normalized before query)
- Password checked against bcrypt hash
- All roles are included in token claims for authorization checks

---

## Endpoint: Confirm Email

**HTTP Method & Path**: `POST /auth/confirm-email`

**Purpose**: Verify user's email address via confirmation token sent to email

**Request Body** (JSON):
```json
{
  "userId": "string (GUID as string, required)",
  "token": "string (confirmation token from email link, required, URL-decoded)"
}
```

**Response** (200 OK):
```json
{
  "message": "Email confirmed successfully",
  "emailConfirmed": true
}
```

**Error Responses**:

| Status | Code | Message | When |
|--------|------|---------|------|
| 400 | `INVALID_TOKEN` | Confirmation token is invalid or expired | Token is malformed or >24 hours old |
| 404 | `USER_NOT_FOUND` | User not found | userId doesn't exist |
| 422 | `ALREADY_CONFIRMED` | Email is already confirmed | EmailConfirmed already true |

**Token Details**:
- Generated at registration via `UserManager.GenerateEmailConfirmationTokenAsync(user)`
- Sent to user's email in link: `https://app.markwell.com/auth/confirm?userId={userId}&token={encoded-token}`
- Single-use; cannot be reused
- Valid for 24 hours

**Notes**:
- After confirmation, user can authenticate via `/auth/login`
- If token expired, user must request new confirmation email

---

## Security Considerations

1. **Password Storage**: All passwords hashed using PBKDF2-SHA256 (bcrypt equivalent) via IdentityUser framework
2. **Token Security**: JWT tokens include exp claim; validated on every request
3. **SQL Injection**: Parameterized queries via EF Core; no string concatenation
4. **Rate Limiting**: [Endpoint-specific rate limiting to be defined in implementation tasks]
5. **HTTPS Only**: All endpoints require HTTPS in production (enforced by middleware)
6. **CORS**: [CORS policy to be defined based on frontend origin]

---

## Related Contracts

- User Profile Management: `user-profile.contract.md`
- Role Management: `role-management.contract.md`
