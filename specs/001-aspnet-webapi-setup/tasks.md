# Tasks: ASP.NET Core Web API Project Setup

**Input**: Design documents from `/specs/001-aspnet-webapi-setup/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅, contracts/health-check.contract.md ✅

**Tests**: Manual verification via build, health endpoint, and Scalar UI (foundation layer requires no unit tests)

**Organization**: Tasks organized by user story (P1 and P2) enabling independent implementation and verification

---

## Format Reference

Each task uses the format: `- [ ] [TaskID] [P?] [Story] Description with file path`

- **[ID]**: Sequential task ID (T001, T002, T003...)
- **[P]**: Optional parallelization marker (can run simultaneously with same phase)
- **[Story]**: User story label (US1, US2, etc.)
- **File paths**: Exact locations for all deliverables

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Create and configure the ASP.NET Core Web API project structure

**Prerequisite Check**: .NET 9 SDK installed; Git feature branch `001-aspnet-webapi-setup` exists

- [ ] T001 Create ASP.NET Core Web API project: `dotnet new webapi -n Markwell.Core -o .` at repository root
- [ ] T002 [P] Remove template boilerplate files: Delete `Controllers/WeatherForecastController.cs`
- [ ] T003 [P] Remove template boilerplate files: Delete `Models/WeatherForecast.cs` (if exists)
- [ ] T004 [P] Remove Swagger/Swashbuckle NuGet packages: Run `dotnet remove package Swashbuckle.AspNetCore`
- [ ] T005 Add Scalar.AspNetCore NuGet package: Run `dotnet add package Scalar.AspNetCore`

---

## Phase 2: Foundational (Core Infrastructure)

**Purpose**: Setup minimal, clean API entry point and health check endpoint

**⚠️ CRITICAL**: Must complete before user story work begins

- [ ] T006 Clean Program.cs of template comments: Edit `Program.cs` to remove all auto-generated remarks and template comments
- [ ] T007 Implement minimal Program.cs: Configure builder, app creation, and health check route (reference: data-model.md#health-check-response)
- [ ] T008 Add health check endpoint: Implement `GET /health` minimal API route returning JSON with status, timestamp, version
- [ ] T009 Register Scalar UI: Add `app.MapScalarApiReference();` to Program.cs before `app.Run();`
- [ ] T010 Verify Program.cs structure: Confirm final Program.cs is ~15–20 lines with zero template comments

**Checkpoint**: Project ready for verification

---

## Phase 3: User Story 1 - Minimal Clean Web API Project (Priority: P1) 🎯 MVP

**Goal**: Deliver a ready-to-run ASP.NET Core Web API with clean Program.cs, no boilerplate endpoints, and interactive testing via Scalar

**Independent Test Criteria**:
- ✅ `dotnet build` succeeds with zero errors and zero warnings
- ✅ `dotnet run` starts API in <100ms, listens on `http://localhost:5000`
- ✅ `GET /health` responds with 200 OK and valid JSON: `{"status":"healthy","timestamp":"...","version":"1.0.0"}`
- ✅ `Program.cs` contains zero template comments or placeholder remarks
- ✅ No boilerplate controllers or sample endpoints (WeatherForecast, etc.) registered
- ✅ Scalar UI accessible at `http://localhost:5000/scalar/v1` with health endpoint visible

### Verification Tasks for User Story 1

- [ ] T011 [US1] Build verification: Run `dotnet build` and confirm zero errors/warnings
- [ ] T012 [US1] Startup verification: Run `dotnet run` and verify API starts in <100ms on `http://localhost:5000`
- [ ] T013 [US1] Health endpoint test: Execute `curl http://localhost:5000/health` and verify 200 OK with expected JSON payload
- [ ] T014 [US1] Scalar UI test: Open browser to `http://localhost:5000/scalar/v1` and verify UI loads with health endpoint listed
- [ ] T015 [US1] Code inspection: Open `Program.cs` and verify zero template comments; verify no boilerplate endpoint registrations
- [ ] T016 [US1] Git status: Verify uncommitted changes exist (`git status` shows modifications)

### Implementation Summary for User Story 1

*Completed in Phase 1 and Phase 2 (T001–T010). User Story 1 verification (T011–T016) confirms all acceptance criteria are met.*

**Deliverables**:
- `Markwell.Core.csproj` — Project file with .NET 9, no Swashbuckle, Scalar.AspNetCore added
- `Program.cs` — Clean entry point (~15–20 lines, zero comments, health endpoint only)
- `appsettings.json` — Default ASP.NET Core configuration (unchanged)
- `appsettings.Development.json` — Development overrides (unchanged)
- `Properties/launchSettings.json` — IIS/Kestrel launch profiles (auto-generated, unchanged)

**Acceptance Criteria Mapping**:
- FR-001 ✅ Project targets .NET 9 LTS (latest stable)
- FR-002 ✅ Project compiles and runs without errors (T011, T012)
- FR-003 ✅ Program.cs contains zero template comments (T006, T015)
- FR-004 ✅ No boilerplate endpoints (T002–T003, T015)
- SC-001 ✅ Builds and starts <30 seconds (T011, T012)
- SC-002 ✅ Zero boilerplate endpoints or comments (T015)

---

## Phase 4: User Story 2 - Reviewed and Merged via Pull Request (Priority: P2)

**Goal**: Create GitHub issue and pull request; ensure all changes reach `master` via reviewed PR (never direct push)

**Independent Test Criteria**:
- ✅ GitHub issue #N exists describing Web API setup task
- ✅ Pull request from `001-aspnet-webapi-setup` targeting `master` created and linked to issue
- ✅ PR description includes summary of changes and decision reasoning (per constitution)
- ✅ PR awaits review; not yet merged
- ✅ No changes on `master` branch outside of PR workflow

### Implementation Tasks for User Story 2

- [ ] T017 [US2] Create GitHub issue: Open GitHub issue #N titled "Setup: Minimal ASP.NET Core Web API Project" with description covering requirements and acceptance criteria
- [ ] T018 [US2] Stage changes: Run `git add Markwell.Core.csproj Program.cs appsettings.json appsettings.Development.json Properties/launchSettings.json`
- [ ] T019 [US2] Commit to feature branch: Run `git commit -m "feat: setup minimal ASP.NET Core Web API project with Scalar"` on branch `001-aspnet-webapi-setup`
- [ ] T020 [US2] Create pull request: Push branch and create PR from `001-aspnet-webapi-setup` targeting `master` with title, description linking issue, and AI reasoning trail
- [ ] T021 [US2] Verification: Confirm PR exists on GitHub; issue is linked; PR is not merged yet
- [ ] T022 [US2] Code review: Reviewer checks PR diff against spec requirements and constitution compliance
- [ ] T023 [US2] Merge to master: Reviewer approves and merges PR via GitHub UI

**Deliverables**:
- GitHub issue #N — Tracks setup work with acceptance criteria
- GitHub PR from `001-aspnet-webapi-setup` → `master` — Code review vehicle with full change summary
- Feature branch commit history — Clean, atomic commit with meaningful message

**Acceptance Criteria Mapping**:
- FR-005 ✅ GitHub issue created (T017)
- FR-006 ✅ All changes via PR, never direct push (T018–T021)
- FR-007 ✅ PR references issue (T020)
- SC-003 ✅ 100% of changes via reviewed PR (T021–T023)
- SC-004 ✅ Issue and PR exist before merge (T017–T022)

---

## Dependency Graph & Execution Order
