# NuGet Package Upgrade Plan

## Overview

**Target**: Update the TindaKo .NET 9 MAUI solution's direct NuGet packages, add a current stable .NET MAUI-compatible AdMob plugin, and apply the smallest safe vulnerability and compatibility fixes without changing target frameworks.
**Scope**: One .NET 9 MAUI project in the TindaKo solution.

## Upgrade Options

| Option | Selected | Why |
|---|---|---|
| Target framework | Keep .NET 9 | The project already targets .NET 9 for Android, iOS, Mac Catalyst, and Windows. |
| Version policy | Latest stable version compatible with .NET 9, using smallest safe changes | Preserves the user's compatibility and vulnerability-remediation requirements. |
| Microcharts.Maui reconciliation | Pin current 1.0.1 | The assessment found no supported newer version for the current target frameworks. |
| Microsoft.Maui.Controls and Microsoft.Extensions.Logging.Debug | Keep .NET 9-compatible versions | The assessment's 10.x recommendations conflict with the required .NET 9 target. |
| Package management | Per-project `PackageReference` updates | No `Directory.Packages.props` central package-management file was found. |
| Breaking-change handling | Migrate when required; validate with build | `sqlite-net-pcl` has one removed member in the API diff; exact call sites will be confirmed during validation. |

## Tasks

### 01-add-maui-admob: Add a compatible MAUI AdMob plugin

Add the current stable AdMob package selected by compatibility analysis to `TindaKo/TindaKo.csproj`. The package must target .NET MAUI and support the existing .NET 9 MAUI project; Xamarin.Forms-specific packages are excluded. Keep the change limited to package installation unless build validation requires the corresponding MAUI registration or initialization.

**Done when**: A stable AdMob package compatible with the project's .NET 9 target frameworks restores successfully, is referenced by the project, and does not introduce Xamarin.Forms dependencies or target-framework incompatibilities.

---

### 02-update-net9-packages: Update compatible direct packages

Apply the smallest safe stable-version changes for the direct package references that have compatible .NET 9 versions: ClosedXML, CommunityToolkit.Maui, CommunityToolkit.Mvvm, SQLitePCLRaw.lib.e_sqlite3.android, sqlite-net-pcl, and the Syncfusion packages where compatibility is confirmed. Keep `Microcharts.Maui` at 1.0.1, and keep the .NET 9 Microsoft MAUI and logging package lines on .NET 9-compatible versions rather than adopting the assessment's .NET 10 recommendations.

**Done when**: The selected package versions are recorded in the project file, restore succeeds for all configured .NET 9 target frameworks, and no package update introduces an unsupported target framework.

---

### 03-fix-package-api-compatibility: Resolve package API changes

Review `apidiff/sqlite-net-pcl.apidiff.md` and the affected source usage after package updates. Migrate any use of the removed member to the supported API, or retain the existing package version only if no safe migration exists and the package can be pinned without compromising the requested vulnerability fixes.

**Done when**: All package API changes surfaced by the assessment or build are resolved, with no new compilation errors or warnings caused by the package updates.

---

### 04-validate-net9-workspace: Validate restore, build, and tests

Restore and build the TindaKo solution/project across the available .NET 9 MAUI targets, treating warnings as errors for the upgrade validation. Run available tests and record any platform or workload limitations that prevent a target from being built in the current environment.

**Done when**: Restore and applicable builds complete successfully, available tests pass, and compatibility or environment limitations are documented in the task progress artifacts.
