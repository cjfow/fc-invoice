# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# IMPORTANT
- Always follow SOLID principles to ensure modular, maintainable code.
- Use modern C# 13 and .NET 9 features when appropriate; add a brief comment explaining any newer feature where it’s used.
- Prioritize clarity over cleverness; no obscure micro-optimizations or magic numbers.
- Write comments in plain English, as if explaining to a junior developer.
- Avoid assumptions. Ask first; if you must assume, add a short “assumption note” comment so it can be reviewed.
- Organize files into logical folders. If WPF is used, follow MVVM; otherwise use best-practice naming and grouping.
- Work in phases: ship an MVP, then iterate in controlled steps with clear goals.

# CODE STYLE
- Comments:
  - // comments above methods, complex operations, properties, and fields.
  - XML <summary> on classes, interfaces, records, model properties, and interface method declarations.
  - Comment complex sections and public APIs; avoid noisy inline comments unless truly helpful.
 - Methods: single responsibility, avoid over abstraction; prefer readability. 
 - Write inline => methods for methods with a single line, as well as for single-line if and leave a blank line after.
- Design: prefer interfaces (strategy pattern) + dependency injection for swappable components; apply Strategy when behavior must vary.
- Immutability: mark fields readonly when they don’t change after initialization.
- Error handling: use try/catch when recovery/fallback is meaningful; log clearly; otherwise fail fast.
- Place new classes in folders with appropriate names alongside other related classes. Don't over abstract.
- Write ternary statements inline: condition ? if true : if false;
- Use linq when neccesary, but avoid complex, multi step linq queries.
- use collection list definitions when possible: [];

# GIT / CI-CD
- I handle pushing and merging.
- Add CI/CD after MVP stabilization: build, test, package, release. Keep workflows simple and explicit.

# NAMING CONVENTIONS
- Maximize for readability over brevity: longer, descriptive names win (e.g., playerConnectionCount, not pcc).
- PascalCase: types, methods, properties, events.
- camelCase: locals and parameters.
- _camelCase: private fields.
- Avoid abbreviations unless industry-standard.

# SECURITY
- Never hardcode secrets; use user-secrets, environment variables, or a secrets vault.
- For encryption, document assumptions and key exchange behavior in comments.
- Add basic rate limiting / flood protection where networked I/O exists.

# LOGGING
- Development: human-readable console logs.
- Production: file or structured logs.
- Include timestamps and helpful context; do not log sensitive data.

# DEFINITION OF DONE
- Clean build (no errors/warnings that matter), tests pass for happy and error paths.
- Public surface documented (XML summaries).
- Style/naming rules followed; obvious perf foot-guns avoided for scope.
- Logging and security checks present and sane.


# PROJECT OVERLAY

## Project Goals
FCInvoice is a WPF desktop application for creating and managing billing invoices with secure encrypted storage. The application focuses on simplicity, data security, and print-ready invoice generation.

## Project Structure
- **FCInvoiceUI/**: Main WPF application
  - **Models/**: BillingInvoice, InvoiceItem, IInvoiceStorageService
  - **ViewModels/**: MainViewModel, PrintViewModel (CommunityToolkit.Mvvm)
  - **Views/**: MainView.xaml, PrintView.xaml
  - **Services/**: EncryptionService, JsonInvoiceStorageService, ComboBoxFormatService, etc.
  - **Resources/**: UI styles and encrypted data storage
- **FCInvoiceTests/**: MSTest unit tests with method-level parallelization

## Architecture Notes
- **MVVM Pattern**: Using CommunityToolkit.Mvvm with ObservableObject and RelayCommand
- **Data Security**: All invoice data encrypted with AES-256 before file storage
- **Key Management**: Encryption keys protected via Windows DPAPI (ProtectedData API)
- **Data Binding**: Two-way binding between ViewModels and XAML views
- **Service Layer**: Modular services following single responsibility (encryption, storage, UI data)

## Build Commands
```bash
# Build solution
dotnet build FCInvoice.sln

# Run tests
dotnet test FCInvoiceTests/FCInvoiceTests.csproj

# Run application
dotnet run --project FCInvoiceUI/FCInvoiceUI.csproj
```

## Security Implementation
- Invoice files stored as .enc with AES-256 encryption
- Keys stored in Resources/Data/KeyIV.dat with user-scope DPAPI protection
- EncryptionService handles all crypto operations with proper error handling
- No secrets or keys in source code

## Development Notes
- MainViewModel contains TODO about abstracting functionality to services
- Invoice numbering uses InvoiceNumberGeneratorService for sequential numbers
- ComboBox data loaded via ComboBoxFormatService for previous invoices
- Print preview handled by separate PrintView window

## Build & Deployment

### Local Development
- Build: `dotnet build`
- Test: `dotnet test`
- Publish (modern): `.\publish.ps1` (PowerShell script with flexible options)
- Publish options:
  - `.\publish.ps1 -SelfContained` - Creates standalone executable
  - `.\publish.ps1 -Runtime win-arm64` - Target ARM64 Windows
  - `.\publish.ps1 -Configuration Debug` - Debug build
- Avoid .pubxml files - use command line or CI/CD instead

### CI/CD Pipeline
- **Continuous Integration** (`.github/workflows/build.yml`):
  - Triggered on push to master/develop and pull requests
  - Multi-job pipeline: build-and-test, code-quality, build-artifacts
  - Runs tests with coverage reporting
  - Security scanning and dependency checks
  - Builds release artifacts for win-x64 and win-arm64
  - Caches NuGet packages for faster builds

- **Continuous Deployment** (`.github/workflows/cd.yml`):
  - Triggered on GitHub releases or manual dispatch
  - Validates release version format (vX.Y.Z)
  - Updates project file versions automatically
  - Creates self-contained, trimmed executables
  - Generates installation packages with README and checksums
  - Creates GitHub releases with proper release notes
  - Supports both stable and pre-release versions

### Release Process
1. Push changes to master branch (triggers CI)
2. Create release: `gh release create v1.0.0 --title "FCInvoice v1.0.0"`
3. Or manual trigger: Actions → CD Release → Run workflow
4. Artifacts automatically uploaded to GitHub releases
5. Downloads available for Windows x64 and ARM64

