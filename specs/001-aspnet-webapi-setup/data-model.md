# Data Model: ASP.NET Core Web API Foundation

**Version**: 1.0.0  
**Scope**: Foundation layer; no domain entities  
**Updated**: 2026-04-16

---

## Overview

This feature establishes the baseline Web API infrastructure. No persistent business entities are introduced at this stage; focus is on application startup, health verification, and API documentation.

**Data Model Includes**:
1. Health Check Response (ephemeral; no persistence)
2. Standard Error Response (template for future domain models)

---

## 1. Health Check Response

**Purpose**: Liveness probe for orchestration, load balancers, and monitoring systems.

**Entity Name**: `HealthCheckResponse`  
**Scope**: Ephemeral; no database persistence required  
**Serialization**: JSON (ASP.NET Core default)

### Fields

| Field | Type | Required | Validation | Notes |
|-------|------|----------|-----------|-------|
| `status` | `string` | Yes | Enum: `"healthy"`, `"degraded"`, `"unhealthy"` | Operational state |
| `timestamp` | `DateTimeOffset` | Yes | ISO 8601 format | Server time (UTC) |
| `version` | `string` | Yes | Semantic versioning (e.g., `"1.0.0"`) | API version |

### JSON Schema

```json
{
  "type": "object",
  "properties": {
    "status": {
      "type": "string",
      "enum": ["healthy", "degraded", "unhealthy"]
    },
    "timestamp": {
      "type": "string",
      "format": "date-time"
    },
    "version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+\\.\\d+$"
    }
  },
  "required": ["status", "timestamp", "version"]
}
```

### Example Response (200 OK)

```json
{
  "status": "healthy",
  "timestamp": "2026-04-16T14:30:00Z",
  "version": "1.0.0"
}
```

### C# Model

```csharp
public record HealthCheckResponse(
    string Status,
    DateTimeOffset Timestamp,
    string Version
)
{
    public static HealthCheckResponse Healthy() =>
        new(Status: "healthy", Timestamp: DateTimeOffset.UtcNow, Version: "1.0.0");
}
```

---

## 2. Standard Error Response

**Purpose**: Unified error format for all API responses (future-facing template; not used in foundation setup).

**Entity Name**: `ErrorResponse`  
**Scope**: Ephemeral; applies to all HTTP error status codes (4xx, 5xx)  
**Serialization**: JSON

### Fields

| Field | Type | Required | Validation | Notes |
|-------|------|----------|-----------|-------|
| `error` | `string` | Yes | Non-empty | Human-readable error message |
| `statusCode` | `int` | Yes | HTTP status code (400, 404, 500, etc.) | Matches HTTP response code |
| `timestamp` | `DateTimeOffset` | Yes | ISO 8601 format | When error occurred |
| `traceId` | `string` | No | UUID format | For logging/debugging correlation |

### JSON Schema

```json
{
  "type": "object",
  "properties": {
    "error": {
      "type": "string",
      "minLength": 1
    },
    "statusCode": {
      "type": "integer",
      "minimum": 400,
      "maximum": 599
    },
    "timestamp": {
      "type": "string",
      "format": "date-time"
    },
    "traceId": {
      "type": "string",
      "format": "uuid"
    }
  },
  "required": ["error", "statusCode", "timestamp"]
}
```

### Example Response (400 Bad Request)

```json
{
  "error": "Invalid request parameters.",
  "statusCode": 400,
  "timestamp": "2026-04-16T14:30:00Z",
  "traceId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### C# Model

```csharp
public record ErrorResponse(
    string Error,
    int StatusCode,
    DateTimeOffset Timestamp,
    string? TraceId = null
)
{
    public static ErrorResponse BadRequest(string message, string? traceId = null) =>
        new(Error: message, StatusCode: 400, Timestamp: DateTimeOffset.UtcNow, TraceId: traceId);

    public static ErrorResponse InternalServerError(string message, string? traceId = null) =>
        new(Error: message, StatusCode: 500, Timestamp: DateTimeOffset.UtcNow, TraceId: traceId);
}
```

---

## 3. State Transitions

No stateful entities in foundation layer. Health Check response is stateless; reflects current server status only.

---

## 4. Validation Rules

### HealthCheckResponse Validation
- `status` must be one of: `healthy`, `degraded`, `unhealthy`
- `timestamp` must be valid ISO 8601 date-time (UTC)
- `version` must follow semantic versioning: `X.Y.Z` where X, Y, Z are non-negative integers

### ErrorResponse Validation
- `error` must not be empty or null
- `statusCode` must be valid HTTP status code (400–599)
- `timestamp` must be valid ISO 8601 date-time (UTC)
- `traceId` (optional) must be valid UUID if provided

---

## 5. Future Domain Models

When domain features are introduced (e.g., User, Student, Entity), they will follow:
- Constitution naming: PascalCase class names
- Validation per business logic (see spec)
- Layered structure: Models → Services → Controllers
- Testing discipline: Unit tests before implementation

**Not in scope for foundation setup.**

---

## Summary

| Model | Persistence | Scope | Purpose |
|-------|-------------|-------|---------|
| `HealthCheckResponse` | Ephemeral | Foundation | Liveness probe for orchestration |
| `ErrorResponse` | Ephemeral | Template (future) | Unified error format |

**Next Artifact**: [contracts/health-check.contract.md](contracts/health-check.contract.md)
