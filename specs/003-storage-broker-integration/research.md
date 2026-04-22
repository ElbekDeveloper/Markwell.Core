# Research: Generic Storage Broker Integration

**Feature**: Generic Storage Broker Integration (`003-storage-broker-integration`)  
**Date**: April 17, 2026  
**Purpose**: Resolve technical unknowns and verify design decisions for broker pattern implementation

## Executive Summary

All technical context is **established and verified**. No blockers identified. Proceed to Phase 1 design with confidence. This research confirms:
- ✅ Generic interface pattern viable with C# 10+ generic constraints
- ✅ IConfiguration dependency injection matches ASP.NET Core standards
- ✅ DbContext thread safety supports concurrent CRUD operations
- ✅ String entity IDs fully compatible with ASP.NET Core Identity defaults
- ✅ Constitution alignment confirmed across all proposed patterns

---

## Technical Decisions Verified

### 1. Generic Interface Pattern: IStorageBroker<T>

**Decision**: Define IStorageBroker as generic interface with Insert<T>, Select<T>, SelectById<T>, Update<T>, Delete<T> methods

**Rationale**: 
- C# 10+ fully supports generic interfaces with type constraints
- Eliminates code duplication across model-specific brokers
- Enables compile-time type safety for CRUD operations
- Matches standard repository pattern in .NET ecosystem

**Verification**:
- ✅ C# 13 (project version) supports unconstrained generics
- ✅ ASP.NET Core DI supports `IStorageBroker<T>` registration patterns
- ✅ EF Core DbSet<T> integrates seamlessly with generic CRUD methods

**Conclusion**: ✅ VIABLE - Proceed with generic interface design

---

### 2. Configuration Injection: IConfiguration in StorageBroker

**Decision**: StorageBroker constructor receives IConfiguration to read connection string; Program.cs contains zero configuration logic

**Rationale**:
- IConfiguration is built-in ASP.NET Core service (automatically registered)
- Connection string read from appsettings.json at StorageBroker instantiation
- Program.cs limited to service registration only (`services.AddScoped<...>()`)
- Supports environment-specific configs (Development/Production) via appsettings.{env}.json

**Verification**:
- ✅ ASP.NET Core host builder automatically adds IConfiguration to DI
- ✅ IConfiguration.GetConnectionString(key) is standard pattern
- ✅ Multiple appsettings.json files (dev/prod) supported by framework defaults
- ✅ No additional configuration code required in Program.cs

**Implementation Pattern**:
```csharp
// StorageBroker constructor
public StorageBroker(IConfiguration configuration, ApplicationDbContext dbContext)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    // Configure DbContext if needed
    _dbContext = dbContext;
}

// Program.cs registration
services.AddScoped<IStorageBroker, StorageBroker>();
```

**Conclusion**: ✅ VERIFIED - IConfiguration pattern is ASP.NET Core standard

---

### 3. DbContext Thread Safety for Concurrent Access

**Decision**: StorageBroker instance is scoped per request; DbContext handles concurrent CRUD via built-in locking

**Rationale**:
- ASP.NET Core Scoped services provide one instance per HTTP request
- EF Core DbContext is NOT thread-safe but Scoped lifetime ensures single-threaded usage per request
- Multiple concurrent requests each get their own DbContext instance
- DbContext change tracking handles concurrent modifications at database level

**Verification**:
- ✅ EF Core documentation: DbContext instances MUST be scoped per request (DO NOT share across requests)
- ✅ ASP.NET Core default DI lifetime `AddScoped` provides request-scoped instances
- ✅ Database-level ACID transactions handle concurrent writes
- ✅ EF Core optimistic concurrency control (RowVersion) available for race condition handling

**Conclusion**: ✅ SECURE - Scoped lifetime + DbContext default behavior is sufficient

---

### 4. String Entity IDs and ASP.NET Core Identity Compatibility

**Decision**: Entity IDs are strings (matching Identity defaults); all brokers work with string IDs

**Rationale**:
- ASP.NET Core Identity User<T> and Role<T> use generic Key type (defaults to string)
- User.Id and Role.Id are already strings in existing implementation
- String IDs support GUID-based keys naturally (easier than int/long in distributed systems)
- Brokers can use `string id` parameter uniformly across all operations

**Verification**:
- ✅ Existing entities (User, Role from US2) use string IDs
- ✅ ApplicationDbContext DbSet<User> and DbSet<Role> instantiated with string keys
- ✅ SelectById<T>(string id) parameter matches Identity framework expectations
- ✅ No migration required; aligns with established schema

**Conclusion**: ✅ ALIGNED - String ID strategy confirmed

---

## Constitution Alignment Confirmation

| Principle | Finding | Status |
|-----------|---------|--------|
| **Naming** | Singular brokers (StorageBroker, RoleBroker); interface IStorageBroker | ✅ Compliant |
| **Architecture** | Layered pattern maintained (Broker → Service → Controller) | ✅ Compliant |
| **Method Design** | Generic methods with verb-based names, <T> parameters | ✅ Compliant |
| **Code Clarity** | XML docs required for generic interface | ✅ Compliant |
| **Testing Discipline** | Mocks at broker boundary, integration tests with real DbContext | ✅ Compliant |
| **Development Workflow** | Spec-kit process, feature branch, PR discipline | ✅ Compliant |

**Result**: ✅ **NO CONSTITUTION VIOLATIONS** - All principles supported by design

---

## Risk Assessment

| Risk | Mitigation | Severity |
|---|---|---|
| Generic interface learning curve | Documentation in quickstart.md; examples for each model broker | LOW |
| IConfiguration not found at runtime | Unit tests verify configuration key exists; clear error messages | LOW |
| DbContext connection failures | Connection pooling via EF Core built-ins; early validation | LOW |
| Concurrent modification conflicts | Optimistic concurrency control available (RowVersion column) | LOW |

**Overall Risk**: ✅ LOW - Standard patterns, well-established technologies, no novel architecture

---

## Design Ready for Implementation

✅ **Phase 0 Complete**: All unknowns resolved, all decisions verified, no blockers identified

**Proceed to Phase 1** with confidence:
- ✅ Generic interface pattern finalized
- ✅ Configuration injection pattern confirmed
- ✅ Thread safety and concurrency verified
- ✅ Entity ID strategy aligned with Identity framework
- ✅ Constitution compliance validated
- ✅ Risk assessment completed

**Next Artifact**: data-model.md (Phase 1 design output)
