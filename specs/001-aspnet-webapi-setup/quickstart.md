# Quickstart: ASP.NET Core Web API Development

**For**: Markwell.Core Web API  
**Updated**: 2026-04-16  
**Audience**: Developers setting up the project locally

---

## Prerequisites

### System Requirements

- **Operating System**: Windows 10+, macOS 12+, or Linux (Ubuntu 20.04+)
- **.NET SDK**: Version 9.0 or later (LTS release)
  - Check: `dotnet --version`
  - Install: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
- **Git**: Version 2.30 or later (for repository access)
  - Check: `git --version`
  - Install: https://git-scm.com/downloads
- **IDE/Editor** (optional, but recommended):
  - Visual Studio 2022 (Community or higher)
  - Visual Studio Code + C# Dev Kit extension
  - JetBrains Rider

### Verify SDK Installation

```bash
dotnet --version
# Expected output: 9.0.x or higher

dotnet --info
# Shows installed runtimes and SDKs
```

---

## Clone the Repository

```bash
git clone https://github.com/your-org/Markwell.Core.git
cd Markwell.Core
```

---

## Build the Project

### Build (Debug)

```bash
dotnet build
```

**Expected output**: `Build succeeded.` with zero warnings

### Build (Release)

```bash
dotnet build --configuration Release
```

---

## Run the API

### Start the Development Server

```bash
dotnet run
```

**Expected output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

The API is now running at `http://localhost:5000`

### Run with Hot Reload (Watch Mode)

```bash
dotnet watch
```

Changes to `.cs` files automatically trigger rebuild and restart.

---

## Verify the API is Running

### Health Check Endpoint

Test the foundation endpoint:

```bash
curl http://localhost:5000/health
```

**Expected response** (200 OK):
```json
{
  "status": "healthy",
  "timestamp": "2026-04-16T14:30:00Z",
  "version": "1.0.0"
}
```

### Using PowerShell (Windows)

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/health" -Method Get | ConvertTo-Json
```

---

## Manual API Testing via Scalar

### Interactive API Documentation

Scalar provides a modern, interactive UI for testing endpoints without external tools.

1. **Start the API** (if not already running):
   ```bash
   dotnet run
   ```

2. **Open Scalar UI** in your browser:
   ```
   http://localhost:5000/scalar/v1
   ```

3. **Explore Endpoints**:
   - Click "Foundation" section
   - Select "Health Check" (`GET /health`)
   - Click "Try it out"
   - Click "Send" to execute

4. **View Response**:
   - Status: 200 OK
   - Body: JSON health status
   - Headers: Content-Type, etc.

### Benefits of Scalar Over Postman
- **Zero setup**: No external tool installation
- **Always in sync**: Automatically reflects API changes
- **Built-in docs**: OpenAPI schema embedded
- **Lightweight**: Pure browser-based UI

---

## Project Structure

```
Markwell.Core/
├── .git/                        # Git repository
├── .specify/                    # Spec Kit templates and scripts
├── specs/                       # Feature specifications
│   └── 001-aspnet-webapi-setup/
│       ├── spec.md              # Feature spec
│       ├── plan.md              # Implementation plan
│       ├── research.md          # Design decisions
│       ├── data-model.md        # Data models
│       └── contracts/           # API contracts
├── Markwell.Core.csproj         # Project file
├── Program.cs                   # Application entry point
├── appsettings.json             # Default configuration
├── appsettings.Development.json # Dev overrides
└── Properties/
    └── launchSettings.json      # Launch profiles (IIS, Kestrel)
```

---

## Configuration

### appsettings.json (Default)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json (Development Overrides)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning"
    }
  }
}
```

**Note**: When running with `dotnet run`, the Development settings are automatically applied.

---

## Common Tasks

### Stop the API

Press `Ctrl+C` in the terminal where the API is running.

### Clean Build Artifacts

```bash
dotnet clean
```

Removes `bin/` and `obj/` directories. Useful after major version upgrades or dependency changes.

### Run Tests

```bash
dotnet test
```

Executes all unit and integration tests in the solution.

### View Help

```bash
dotnet --help
dotnet run --help
```

---

## Troubleshooting

### Issue: "The term 'dotnet' is not recognized"

**Solution**: .NET SDK is not installed or not in system PATH.
- Verify: `dotnet --version`
- Install: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
- Restart terminal/IDE after installation

### Issue: Port 5000 Already in Use

**Solution**: Another service is using port 5000.

Option 1 — Use a different port:
```bash
dotnet run --urls "http://localhost:5001"
```

Option 2 — Find and stop the conflicting process:
```bash
# Windows (PowerShell)
Get-Process | Where-Object { $_.Handles -gt 0 } | Where-Object { $_.Port -eq 5000 }
# (Note: Finding by port requires netstat)

netstat -ano | findstr :5000
taskkill /PID <PID> /F

# macOS/Linux
lsof -i :5000
kill -9 <PID>
```

### Issue: Tests Fail to Run

**Solution**: Ensure test projects are properly configured.
```bash
dotnet test --verbosity detailed
```

For more details, check test output and `.csproj` file test configuration.

### Issue: Hot Reload Not Working

**Solution**: Hot reload requires certain file changes.
```bash
# Stop the watch, clean, and restart
Ctrl+C
dotnet clean
dotnet watch
```

---

## Next Steps

### For Developers Adding Features

1. **Read the Constitution**: [.specify/memory/constitution.md](../.specify/memory/constitution.md)
   - Naming conventions, layered architecture, testing discipline

2. **Explore the Feature Spec**: [specs/001-aspnet-webapi-setup/spec.md](specs/001-aspnet-webapi-setup/spec.md)
   - User stories, acceptance criteria, requirements

3. **Review the Data Model**: [specs/001-aspnet-webapi-setup/data-model.md](specs/001-aspnet-webapi-setup/data-model.md)
   - Entities and validation rules

4. **Check the API Contracts**: [specs/001-aspnet-webapi-setup/contracts/](specs/001-aspnet-webapi-setup/contracts/)
   - Health Check endpoint and error response formats

5. **Create a New Feature**:
   - Start from an issue: "Create a GitHub issue describing the feature"
   - Follow the spec-kit workflow: spec → plan → tasks → implement
   - Use feature branch: `<sequential-number>-<description>`
   - Submit as PR targeting `master` (never push directly)

### Architecture Principles

- **Layered**: Controller → Service → Broker (one-way dependency flow)
- **Named**: PascalCase classes; Services singular; Controllers plural
- **Tested**: Test-first discipline; Arrange/Act/Assert structure
- **Committed**: Every change via reviewed PR; full traceability

---

## Resources

- **ASP.NET Core Docs**: https://learn.microsoft.com/en-us/aspnet/core/
- **C# 13 Features**: https://learn.microsoft.com/en-us/dotnet/csharp/what-s-new/csharp-13
- **Scalar Docs**: https://github.com/scalar/scalar
- **Project Constitution**: [.specify/memory/constitution.md](../.specify/memory/constitution.md)

---

## Support

For questions or issues:
1. Check this quickstart
2. Review the project constitution and specs
3. Consult ASP.NET Core documentation
4. Open an issue on GitHub

---

**Last Updated**: 2026-04-16  
**Maintained By**: Development Team
