# Copilot Instructions for HAL Repository

## Summary

HAL is a .NET 10 library implementing the Hypertext Application Language (HAL) and HAL-Forms for building discoverable REST APIs. It includes server-side ASP.NET Core integration, OData support, and client libraries for both .NET and Angular.

## Repository Layout

```text
src/HAL/HAL.slnx              # SDK-style solution (slnx format)
src/HAL/NuGet.config           # NuGet configuration
src/HAL/HAL.Common/            # Core models: Resource, Link, FormsResource, JSON converters
src/HAL/HAL.AspNetCore/        # ASP.NET Core integration (IResourceFactory, IFormFactory, ILinkFactory)
src/HAL/Hal.AspNetCore.OData/  # OData support for list/paging endpoints (NOTE: folder casing is "Hal" not "HAL")
src/HAL/HAL.Client.Net/        # .NET client library (IHalClient, IHalClientFactory)
src/HAL/HAL.Client.Angular/    # Angular client library (@wertzui/ngx-hal-client) - esproj
src/HAL/HAL.Tests/             # Test project (MSTest + NSubstitute, 130 tests)
build/                         # NuGet publish/clean scripts (not for CI)
docs/                          # Detailed docs (hal-aspnetcore.md, hal-forms-generation.md, hal-client-*.md)
```

### Project Dependency Graph

```text
HAL.Common (no project deps)
  └─ HAL.AspNetCore → HAL.Common
       └─ HAL.AspNetCore.OData → HAL.AspNetCore
  └─ HAL.Client.Net → HAL.Common
HAL.Tests → HAL.AspNetCore, HAL.Client.Net
```

## Tech Stack

- **Runtime:** .NET 10 (`net10.0` TFM), C# with nullable enabled
- **Test framework:** MSTest (v4.x) with NSubstitute for mocking
- **Angular project:** Angular 21, TypeScript 5.9, built via `ng-packagr`
- **Code style:** `EnableNETAnalyzers` and `EnforceCodeStyleInBuild` are enabled in all .csproj files. Ensure code compiles without analyzer errors.
- **Nullable:** All projects use `<Nullable>enable</Nullable>`. Always use nullable annotations.

## Build & Test Commands

All commands run from `src/HAL/` (the solution directory).

### Build (.NET)

```bash
dotnet build HAL.slnx --configuration Release
```

Build succeeds with 0 errors. There are known warnings (NU1510 for Microsoft.Extensions.Http, CS1584/CS1658 XML doc warnings in KeyValueDictionaryConverter.cs) — do not attempt to fix these unless asked.

### Test (.NET)

```bash
dotnet test HAL.slnx --configuration Release
```

All 130 tests should pass. Always build before testing, or omit `--no-build`.

### Angular Build

From `src/HAL/HAL.Client.Angular/`:

```bash
npm install
ng build
```

The Angular project builds as a library via ng-packagr. Output goes to `dist/ngx-hal-client/`.

## Key Conventions

- All library projects set `<GeneratePackageOnBuild>true</GeneratePackageOnBuild>` — NuGet packages are produced on Release builds.
- Each project manages its own `<VersionPrefix>` in its .csproj. There is no central version file.
- No `Directory.Build.props`, `Directory.Packages.props`, or `global.json` exists. Each .csproj is self-contained.
- No CI/CD workflow YAML files exist in `.github/workflows/`. There is no automated CI pipeline to replicate.

## Validation Checklist

After making changes, always:

1. `dotnet build HAL.slnx` — must produce 0 errors
2. `dotnet test HAL.slnx` — all 130+ tests must pass
3. If Angular files were changed: `cd HAL.Client.Angular && npm install && ng build`

## Commit Message Format

The commit message must start with a non-bulleted list of all changed projects that have a `<VersionPrefix>` in their .csproj, in the form `ProjectName VersionPrefix`. Do not add anything before that list. Exclude projects without a `<VersionPrefix>` (e.g. HAL.Tests). Then add a blank line before any additional description.

Example:

```text

HAL.Common 9.0.6
HAL.AspNetCore 21.0.0

Added new Link property for template support.
```

## Notes

- The OData project folder is `Hal.AspNetCore.OData` (lowercase 'al') but the assembly/package is `HAL.AspNetCore.OData`.
- Trust these instructions. Only search the repo if something here is incomplete or produces an error.
