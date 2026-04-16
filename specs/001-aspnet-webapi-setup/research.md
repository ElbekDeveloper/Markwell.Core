# Research: ASP.NET Core Web API Foundation

**Completed**: 2026-04-16  
**Scope**: Foundation-layer Web API setup (zero boilerplate, clean Program.cs, GitHub workflow)

## Key Findings

### 1. ASP.NET Core 9 LTS (Latest Stable)

**Decision**: Target .NET 9 LTS (released November 2024; LTS until November 2027)

**Rationale**:
- Stable, production-ready; supported by Microsoft for extended period
- Includes C# 13 with modern language features (records, pattern matching, source generators)
- Minimal native dependencies; lightweight for containerization

**Alternatives Considered**:
- .NET 8: Equally stable but one generation older
- .NET Core 10 (preview): Not yet LTS; unstable for production foundation

---

### 2. Minimal Program.cs Pattern

**Decision**: Use top-level statements without auto-generated comments.

**Rationale**:
- ASP.NET Core 6+ supports minimal hosting model; eliminates boilerplate Startup.cs pattern
- Spec requirement: zero template comments in Program.cs
- Supports test-first, clean-code principles

**Template to Avoid**:
```csharp
// Auto-generated template comments (REJECTED)
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();
```

**Clean Pattern** (from spec FR-003):
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.Run();
```

---

### 3. Health Check Endpoint Strategy

**Decision**: Implement minimal `/health` endpoint for orchestration/monitoring.

**Rationale**:
- Not a boilerplate sample endpoint (spec FR-004 allows infrastructure endpoints)
- Standard in microservices: Kubernetes probes, load balancers, monitoring tools
- Can be minimal API (one-liner) or controller-based; isolated from domain logic

**Alternatives**:
- No health endpoint: Fails requirement (needs verification that API is running)
- Complex health checks: Premature (no dependencies to monitor yet)

---

### 4. Scalar API Documentation Integration

**Decision**: Use Scalar over Swagger UI for manual API testing.

**Rationale**:
- User preference (input: "use scalar for manual api testing")
- Lightweight, zero-config; built on OpenAPI standards
- Modern, clean UI; supports interactive testing without external tools
- Integrates via NuGet: `Scalar.AspNetCore`

**Setup**:
```csharp
app.MapScalarApiReference();
```

**Access**: `http://localhost:5000/scalar/v1` (configurable)

---

### 5. Repository Workflow & GitHub Integration

**Decision**: Feature branch + PR + Issue workflow (per constitution).

**Flow**:
1. Create GitHub issue (intent + scope)
2. Create feature branch: `001-aspnet-webapi-setup`
3. Push changes to branch
4. Create PR targeting `master`; reference issue in description
5. PR review required before merge (no direct commits to `master`)

**Rationale**:
- Constitution § Git Commit & PR Discipline mandates this flow
- Ensures traceability: issue (why) → PR (review) → commit (code)
- Aligns with enterprise governance; full audit trail

---

### 6. Testing Framework Choice

**Decision**: xUnit + Fluent Assertions

**Rationale**:
- xUnit: Most popular in .NET ecosystem; clean, expression-based
- Fluent Assertions: Readable test assertions; superior error messages vs. MSTest
- Both integrate seamlessly with ASP.NET Core test host

**Foundation Tests** (Phase 2 tasks):
- API starts without errors
- `/health` endpoint responds with 200 OK
- Response schema matches contract
- Zero boilerplate endpoints registered

---

### 7. Project Layout (No Domain Layers Yet)

**Decision**: Single project; defer Broker/Service/Controller separation to feature-specific tasks.

**Rationale**:
- Foundation layer: no business logic to layer
- Layering enforced in spec: constitution § Layered Architecture
- Will be added organically as domain features (User, Entity, etc.) are introduced

**Structure** (Phase 2 creation):
```
Markwell.Core/
├── Markwell.Core.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Properties/
    └── launchSettings.json
```

---

## Implementation Constraints

| Constraint | Why | Impact |
|-----------|-----|--------|
| **Zero template comments** | Spec FR-003; clean code principle | Must manually remove from `dotnet new webapi` template |
| **No sample endpoints** | Spec FR-004 | Remove WeatherForecast controller + Swagger/Swashbuckle boilerplate |
| **Minimal startup** | Spec FR-002; constitution clarity | Program.cs ≤ 20 lines |
| **No extra dependencies** | Constraint: "no external dependencies beyond ASP.NET Core framework" | Except Scalar.AspNetCore (approved: API testing UI) |
| **GitHub issue + PR required** | Constitution § Git Discipline | Cannot push directly to `master` |

---

## Next Steps

Phase 1 design artifacts ready:
- ✅ `data-model.md` — Health check schema
- ✅ `contracts/health-check.contract.md` — OpenAPI endpoint spec
- ✅ `quickstart.md` — Developer setup guide

**Proceed to**: Phase 2 task generation via `/speckit-tasks`
