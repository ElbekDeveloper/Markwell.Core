<!-- Sync Impact Report
Version change: 1.0.0 → 1.1.0
Modified principles: none (titles unchanged)
Added sections: "Git Commit & PR Discipline" paragraph under Development Workflow
Removed sections: none
Templates requiring updates:
  ✅ .specify/memory/constitution.md (this file)
  ✅ .specify/templates/plan-template.md (no changes required — git workflow not referenced)
  ✅ .specify/templates/spec-template.md (no changes required — workflow-agnostic)
  ✅ .specify/templates/tasks-template.md (no changes required — task structure unaffected)
Deferred: TODO(GOVERNANCE_AMENDMENT_HISTORY): no prior amendments yet
Reference: https://github.com/hassanhabib/CSharpCodingStandard
-->

# Markwell.Core Constitution

## Core Principles

### I. Naming Conventions

**Files** — PascalCase with `.cs` extension. Partial class files use dot-notation: `StudentService.Validations.cs`.
**Classes** — Models: no suffix (`Student`). Services: singular (`StudentService`). Brokers: singular (`StudentBroker`). Controllers: plural (`StudentsController`).
**Fields** — camelCase; no underscores; reference private fields with `this.`.
**Variables** — Full descriptive names; no abbreviations (`student`, not `s` or `stdnt`). Collections use plurals, not `studentList`. Null/default values named `noStudent = null`.
**Methods** — MUST contain a verb. Async methods MUST be postfixed with `Async`. Parameters MUST be explicit about what they represent (`studentName`, not `name` or `text`).

### II. Layered Architecture (Broker → Service → Controller)

All features MUST follow a three-layer separation:

- **Brokers** (singular) — thin shims between services and external resources (databases, APIs, clocks). No business logic. Named `*Broker`.
- **Services** (singular) — contain all business logic, validation, and orchestration. Named `*Service`.
- **Controllers** (plural) — expose REST endpoints only. Delegate all logic to services. Named `*sController`.

No layer may skip or bypass another. Dependencies flow in one direction: Controller → Service → Broker.

### III. Method Design

- One-liners MUST use fat arrows: `public List<Student> GetStudents() => ...`
- Multi-liners MUST have a blank line between logic and the final `return` statement.
- Method declarations MUST NOT exceed 120 characters; break after the parameter list if needed.
- When passing multiple parameters over 120 chars, place one parameter per line.
- Chaining MUST follow uglification: first call on the source line, each subsequent call indented one additional tab level.
- Named aliases MUST be used when passing literal values; positional literals are forbidden.

### IV. Code Clarity

- Comments MUST only be used when code cannot explain itself (invisible logic, copyright headers).
- Copyright header format MUST be: `// ---------------------------------------------------------------` block style.
- XML documentation is REQUIRED on methods that are not accessible at dev-time or perform complex functions, covering: Purpose, Inputs, Outputs, Side Effects.
- `var` MUST be used when the right-hand type is immediately clear. Explicit type MUST be used when the return type is inferred from a method call.
- Multi-line variable declarations MUST be separated by blank lines from adjacent single-line declarations.
- Class instantiations MUST honor property declaration order. Named parameters MUST be used when passing non-variable literals.

### V. Testing Discipline

- Every feature MUST be covered by unit tests before implementation is considered complete (Test-First).
- Tests MUST follow the Arrange/Act/Assert structure with a blank line between each section.
- Test method names MUST describe the scenario: `ShouldThrowValidationExceptionOnAddWhenStudentIsNull`.
- Integration tests are REQUIRED for: new broker contracts, inter-service communication, and shared schema changes.
- Mocks are permitted only at the broker boundary. Services MUST be tested against real broker contracts in integration tests.

## Development Workflow

- All PRs must be reviewed against this constitution before merge.
- CodeRabbit (assertive profile) runs on every PR and may block merges on violations.
- Spec-kit workflow (specify → plan → tasks → implement) MUST be followed for every non-trivial feature.
- Branch naming follows spec-kit sequential convention: `feature/<sequential-number>-<description>`.

### Spec Synchronization (Docs-First Discipline)

Specs are the source of truth. Any clarification, architectural correction, or bug fix discussed in chat MUST be reflected in the relevant spec document **before** implementation or code changes begin. This is non-negotiable.

Agents reading and implementing from specs MUST apply this rule:

1. **When a clarification changes a design decision** — update `spec.md` (requirements, key entities, assumptions) immediately, then proceed.
2. **When a bug or code review finding invalidates a spec section** — update the affected document(s) (`spec.md`, `plan.md`, `data-model.md`, `contracts/*.md`, `quickstart.md`) to reflect the correct design before writing any code.
3. **When implementation diverges from the spec** (e.g., merging two classes into one, changing method signatures, swapping a dependency) — the spec is wrong, not the code. Correct the spec, then align code to the corrected spec.
4. **After any PR review** — CodeRabbit or human review comments that reveal spec drift MUST be incorporated into the spec documents as part of the same feature branch before merge.

The spec documents that must be kept in sync are:
- `spec.md` — requirements, key entities, functional rules
- `plan.md` — architecture summary, project structure
- `data-model.md` — entity shapes, broker structure, DI registration, data flow
- `contracts/*.md` — interface signatures, exception contracts, usage examples
- `quickstart.md` — setup and usage code snippets
- `tasks.md` — method names and references must match implementation

### Git Commit & PR Discipline

Direct commits to `master` are STRICTLY FORBIDDEN. Every change — no matter how small — MUST follow this sequence:

1. **Open a GitHub issue** describing the intent and scope of every change before any code is written. The issue is the source of truth for what is being changed and why.
2. **Work on a feature branch** branched from `master`, named following the spec-kit sequential convention.
3. **Create a Pull Request** from the feature branch targeting `master`. The PR description MUST include a condensed summary of the AI prompts used or a distilled chat history that led to the implementation, so reviewers understand the reasoning and decisions made during development.
4. **Never push directly to `master`**. The only path to `master` is a reviewed and approved PR. Force-pushing to `master` is also forbidden.

This discipline ensures full traceability: every line of code on `master` traces back to an issue (intent), a PR (review), and an AI-assisted reasoning trail (context).

## Governance

This constitution supersedes all other coding guidance. Amendments require:
1. A proposal PR with updated version and rationale.
2. Approval from at least one other contributor.
3. Updated `Last Amended` date and version bump per semantic rules (MAJOR: breaking principle removal/redefinition; MINOR: new principle or section; PATCH: clarification or wording).

All PRs and code reviews MUST verify compliance with the principles above.

**Version**: 1.2.0 | **Ratified**: 2026-04-16 | **Last Amended**: 2026-04-22
