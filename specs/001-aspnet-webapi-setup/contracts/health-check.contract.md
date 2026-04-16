# API Contract: Health Check Endpoint

**Version**: 1.0.0  
**Format**: OpenAPI 3.0 (Scalar-compatible)  
**Status**: Foundation layer  
**Updated**: 2026-04-16

---

## Overview

The Health Check endpoint provides a liveness probe for orchestration systems, load balancers, and monitoring tools. It verifies the API is running and accessible without requiring authentication or complex dependencies.

---

## Endpoint Specification

### GET /health

**Purpose**: Retrieve the current health status of the API.

**Method**: `GET`  
**Path**: `/health`  
**Authentication**: None required  
**Rate Limiting**: None (foundation layer)

---

## Request

### Headers (Optional)

| Header | Value | Required | Notes |
|--------|-------|----------|-------|
| `Accept` | `application/json` | No | Assume JSON if omitted |
| `User-Agent` | Any string | No | For logging/analytics |

### Query Parameters

None.

### Request Body

None.

### Example cURL Request

```bash
curl -X GET "http://localhost:5000/health" \
  -H "Accept: application/json"
```

---

## Response

### Success Response (200 OK)

**Content-Type**: `application/json`

**Body**:
```json
{
  "status": "healthy",
  "timestamp": "2026-04-16T14:30:00Z",
  "version": "1.0.0"
}
```

**Fields**:
- `status` (string): One of `"healthy"`, `"degraded"`, `"unhealthy"`. Typically `"healthy"` for foundation layer.
- `timestamp` (string): ISO 8601 date-time (UTC) when the response was generated.
- `version` (string): API version in semantic versioning format (X.Y.Z).

### HTTP Status Codes

| Code | Condition | Description |
|------|-----------|-------------|
| 200 | API is running | Healthy status |
| 503 | API is shutting down | Service Unavailable (optional; advanced scenarios) |

---

## OpenAPI Definition

```yaml
openapi: 3.0.0
info:
  title: Markwell.Core API
  version: 1.0.0
  description: ASP.NET Core Web API Foundation

paths:
  /health:
    get:
      summary: Health Check
      operationId: getHealth
      tags:
        - Foundation
      responses:
        '200':
          description: API is healthy
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/HealthCheckResponse'

components:
  schemas:
    HealthCheckResponse:
      type: object
      required:
        - status
        - timestamp
        - version
      properties:
        status:
          type: string
          enum:
            - healthy
            - degraded
            - unhealthy
          description: Current health status
        timestamp:
          type: string
          format: date-time
          description: ISO 8601 timestamp (UTC)
        version:
          type: string
          pattern: '^\d+\.\d+\.\d+$'
          description: API version (semantic versioning)
```

---

## Implementation Notes

### C# Minimal API Example

```csharp
app.MapGet("/health", () =>
    new { status = "healthy", timestamp = DateTimeOffset.UtcNow, version = "1.0.0" }
)
.WithName("GetHealth")
.WithOpenApi()
.Produces(StatusCodes.Status200OK);
```

### Integration with Scalar

When Scalar is configured, this endpoint is automatically documented and testable:

```csharp
app.MapScalarApiReference();
```

**Access**: Navigate to `http://localhost:5000/scalar/v1` to test interactively.

---

## Testing Guide (Manual via Scalar)

1. **Start the API**: `dotnet run`
2. **Open Scalar UI**: `http://localhost:5000/scalar/v1`
3. **Navigate to**: Health Check endpoint (Foundation section)
4. **Click**: "Try it out"
5. **Verify**:
   - Status code: 200 OK
   - Response body contains `status: "healthy"`
   - `timestamp` is recent
   - `version` is `1.0.0`

---

## Contract Compliance Checklist

- ✅ No authentication required (foundation layer)
- ✅ Single operation (GET)
- ✅ Deterministic response (no side effects)
- ✅ JSON serialization
- ✅ ISO 8601 timestamps
- ✅ OpenAPI 3.0 compatible
- ✅ Scalar-compatible endpoint
- ✅ No boilerplate; minimal endpoint (1–3 lines of code)

---

## Future Enhancements (Out of Scope)

- Database connectivity checks
- Dependency health (external APIs, caches)
- Graceful degradation (`"degraded"` status)
- Metrics/observability integration

These will be added as domain-specific features require external dependencies.
