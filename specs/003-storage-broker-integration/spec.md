# Feature Specification: Generic Storage Broker Integration

**Feature Branch**: `003-storage-broker-integration`  
**Created**: April 17, 2026  
**Status**: Draft  
**Input**: "Create an epic for database integration with generic CRUD interface (IStorageBroker), separate model-to-broker mapping, and DI-based configuration without Program.cs boilerplate"

## User Scenarios & Testing

### User Story 1 - Implement IStorageBroker Generic CRUD Interface (Priority: P1)

Development team needs a reusable, generic interface that defines standard CRUD operations (Create, Read, ReadById, Update, Delete) to be implemented by database brokers. This interface should be technology-agnostic and allow any broker implementation to provide consistent data access patterns across the application.

**Why this priority**: The generic interface is the foundation for all broker implementations. Without it, each broker would reinvent CRUD patterns, leading to inconsistency and maintenance overhead. This is a prerequisite for all subsequent broker implementations.

**Independent Test**: Can be fully tested by creating a mock implementation of IStorageBroker, verifying that all four CRUD method signatures are present and callable with appropriate generic type parameters and return types.

**Acceptance Scenarios**:

1. **Given** the IStorageBroker interface is defined, **When** a broker class attempts to implement it, **Then** the interface exposes Insert<T>(T entity), Select<T>(), SelectById<T>(string id), Update<T>(T entity), and Delete<T>(string id) methods
2. **Given** IStorageBroker is generic, **When** different entity types (User, Role, etc.) are used, **Then** the same interface works with any entity type without modification
3. **Given** a broker implements IStorageBroker, **When** the interface is used in service layer code, **Then** only the interface is referenced (dependency inversion), not the concrete implementation

---

### User Story 2 - Create StorageBroker Implementation with Configuration (Priority: P1)

Development team needs a concrete StorageBroker class that implements IStorageBroker using the ASP.NET Core Identity DbContext as the underlying persistence mechanism. The StorageBroker should receive database configuration (connection string) from dependency injection via IConfiguration, eliminating the need for Program.cs to act as a configuration orchestrator.

**Why this priority**: This is the second foundational piece. It enables actual database operations and demonstrates the configuration injection pattern. P1 because all database operations depend on this.

**Independent Test**: Can be fully tested by instantiating StorageBroker with a mock IConfiguration, verifying that connection string is read correctly and DbContext is initialized without requiring Program.cs configuration logic.

**Acceptance Scenarios**:

1. **Given** StorageBroker is instantiated with IConfiguration injected, **When** Program.cs registers it, **Then** StorageBroker reads connection string from appsettings.json automatically
2. **Given** StorageBroker receives IConfiguration, **When** different environments (Development, Production) are active, **Then** appropriate connection strings are loaded from environment-specific config files
3. **Given** StorageBroker wraps ApplicationDbContext, **When** CRUD methods are called, **Then** all operations are delegated to DbContext without additional configuration steps
4. **Given** StorageBroker is registered in DI container, **When** services request IStorageBroker, **Then** the same StorageBroker instance is provided

---

### User Story 3 - Create Model-Specific Brokers with One-to-One Mapping (Priority: P2)

Development team needs specialized broker classes for each data model (User, Role, UserRole, etc.) that inherit or wrap the generic storage broker pattern. Each model should have exactly one broker. These brokers add model-specific queries and operations while maintaining consistency with the IStorageBroker contract.

**Why this priority**: Once the base pattern is established, specialized brokers enable clean separation of concerns. Each model's data access logic is isolated in its own broker. P2 because this builds on the foundational P1 work.

**Independent Test**: Can be fully tested by creating RoleBroker for the Role model, verifying that it implements IStorageBroker and exposes model-specific methods (e.g., GetRoleByName) alongside inherited CRUD operations.

**Acceptance Scenarios**:

1. **Given** RoleBroker, UserBroker, and UserRoleBroker are created, **When** each is registered in DI, **Then** each broker is responsible for one and only one model (no broker handles multiple models)
2. **Given** a model-specific broker (e.g., RoleBroker), **When** it is used, **Then** it implements IStorageBroker and provides all four CRUD operations for Role entities
3. **Given** RoleBroker is instantiated, **When** services request it, **Then** it has access to the same StorageBroker/DbContext and can query the database directly
4. **Given** each broker is model-specific, **When** new models are added, **Then** new brokers can be created following the same pattern without modifying existing brokers

---

### User Story 4 - Dependency Injection Setup in Program.cs (Priority: P2)

Development team needs Program.cs to register IStorageBroker and all model-specific brokers with a single, clean registration pattern. Program.cs should NOT contain configuration logic—only service registration. Configuration should be handled by StorageBroker's IConfiguration injection internally.

**Why this priority**: Clean DI setup demonstrates the correct architectural pattern. P2 because it's a prerequisite for application startup but depends on P1 implementations being complete.

**Independent Test**: Can be fully tested by verifying that Program.cs contains only `services.AddScoped<IStorageBroker, StorageBroker>()` and similar one-liners for model-specific brokers, with no DbContext manual configuration or connection string setup.

**Acceptance Scenarios**:

1. **Given** Program.cs is configured, **When** IStorageBroker is injected into a service, **Then** the StorageBroker instance is already initialized with the correct database configuration
2. **Given** model-specific brokers (UserBroker, RoleBroker, etc.), **When** they are registered in Program.cs, **Then** the registration is a simple one-liner per broker with no configuration logic
3. **Given** the application starts, **When** database operations are performed, **Then** the configuration came from IConfiguration, not from Program.cs manual orchestration
4. **Given** an environment-specific configuration is needed, **When** environment is changed, **Then** only appsettings.json is updated, not Program.cs

---

### User Story 5 - Verify Predefined Roles Support (Priority: P2)

Development team needs the storage broker infrastructure to support predefined roles (Admin, Manager, Teacher, Student) defined in configuration or code. The RoleBroker should be able to query, create, and manage these roles without special application code.

**Why this priority**: Role support is required for the broader profile management feature (US2). P2 because it depends on the base broker infrastructure (P1 work).

**Independent Test**: Can be fully tested by verifying that predefined roles can be seeded into the database and retrieved via RoleBroker.SelectById<Role>("admin-role-id") or RoleBroker.Select<Role>() without errors.

**Acceptance Scenarios**:

1. **Given** predefined roles (Admin, Manager, Teacher, Student) are seeded in the database, **When** RoleBroker.Select<Role>() is called, **Then** all four roles are returned with correct names and IDs
2. **Given** RoleBroker exists, **When** a service needs to find a role by ID, **Then** it calls RoleBroker.SelectById<Role>(roleId) and receives the Role entity or null if not found
3. **Given** a new role needs to be created, **When** a service calls RoleBroker.Insert<Role>(newRole), **Then** the role is persisted to the database and can be retrieved

---

### Edge Cases

- What happens if IConfiguration does not contain a valid connection string key? (Should fail at StorageBroker initialization with clear error)
- How does StorageBroker behave if the database is unreachable at startup? (Connection validation should occur early)
- How are database transactions handled in CRUD operations? (Should follow DbContext defaults; explicit transaction management is out of scope for P1)
- Can the same StorageBroker instance be used from multiple concurrent threads? (Should be thread-safe per DbContext defaults)

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide IStorageBroker interface with generic method signatures for Insert<T>, Select<T>, SelectById<T>, Update<T>, Delete<T>
- **FR-002**: System MUST provide StorageBroker implementation that executes CRUD operations against ASP.NET Core Identity DbContext
- **FR-003**: StorageBroker MUST read database connection string from IConfiguration injected via dependency injection
- **FR-004**: StorageBroker MUST NOT require manual configuration in Program.cs beyond a single registration line
- **FR-005**: System MUST support model-specific brokers (RoleBroker, UserBroker, UserRoleBroker) with one-to-one model-to-broker mapping
- **FR-006**: Each model-specific broker MUST inherit/implement IStorageBroker contract while optionally exposing model-specific query methods
- **FR-007**: System MUST support predefined roles (Admin, Manager, Teacher, Student) queryable via RoleBroker CRUD operations
- **FR-008**: Program.cs registration MUST be limited to DI service registration only; no DbContext configuration, connection strings, or environment logic

### Key Entities

- **IStorageBroker**: Generic interface defining CRUD contract for all brokers
  - Methods: Insert<T>(T entity), Select<T>(), SelectById<T>(string id), Update<T>(T entity), Delete<T>(string id)
  - Technology-agnostic; implementation-agnostic

- **StorageBroker**: Concrete implementation of IStorageBroker
  - Wraps ApplicationDbContext (Identity DbContext)
  - Receives IConfiguration to configure connection string
  - Implements all CRUD methods delegating to DbContext

- **RoleBroker**: Model-specific broker for Role entity
  - Implements IStorageBroker for Role operations
  - Optional model-specific methods (e.g., GetRoleByName)

- **UserBroker**: Model-specific broker for User entity
  - Implements IStorageBroker for User operations

- **UserRoleBroker**: Model-specific broker for UserRole junction entity
  - Implements IStorageBroker for UserRole operations

- **Role**: Represents an authorization level (Admin, Manager, Teacher, Student)
  - Attributes: Id, Name, CreatedAt, UserRoles (navigation)

## Success Criteria

- **SC-001**: IStorageBroker interface is defined and testable with mock implementations in under 100 lines of code
- **SC-002**: StorageBroker implementation successfully reads connection string from IConfiguration without Program.cs configuration logic
- **SC-003**: Developers can register StorageBroker in Program.cs with a single line: `services.AddScoped<IStorageBroker, StorageBroker>()`
- **SC-004**: Model-specific brokers (RoleBroker, UserBroker) can be created and registered without duplicating CRUD logic
- **SC-005**: All CRUD operations complete within 100ms for typical queries (non-loaded result sets) against in-memory or local database
- **SC-006**: Predefined roles (Admin, Manager, Teacher, Student) can be seeded and queried successfully via RoleBroker
- **SC-007**: Code follows Markwell.Core Constitution naming and architecture principles (singular brokers, IStorage prefix for interfaces, etc.)
- **SC-008**: Integration tests verify that CRUD operations work end-to-end with real DbContext and configuration

## Assumptions

- IConfiguration is available in DI container at StorageBroker instantiation (standard ASP.NET Core pattern)
- ApplicationDbContext has already been registered in DI (handled separately or in foundational phase)
- Connection string is stored in appsettings.json under a standard key (e.g., "DefaultConnection")
- ASP.NET Core Identity DbContext is the exclusive persistence mechanism for this feature (no additional ORM layers)
- Entity IDs are strings (compatible with ASP.NET Core Identity default)
- CRUD operations use default EF Core behavior (no explicit transaction handling required for MVP)
- Predefined roles are seeded in database via EF Core migrations (not runtime bootstrap code)
- Model-specific brokers may wrap the base StorageBroker or inherit from it; exact pattern determined during planning
- Concurrent access to StorageBroker is handled by DbContext's built-in thread safety (no manual locking required)
- The pattern supports both development (SQLite in-memory) and production (PostgreSQL) databases via connection string switching
