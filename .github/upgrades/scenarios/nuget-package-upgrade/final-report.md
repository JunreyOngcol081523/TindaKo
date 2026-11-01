# NuGet Package Upgrade — Report

**Scenario:** Update the TindaKo .NET 9 MAUI solution's direct NuGet packages, add a current stable .NET MAUI-compatible AdMob plugin, and apply minimal safe compatibility fixes.
**Outcome:** ✅ Fully completed with documented restore/workload warnings
**Projects affected:** 1
**Tasks:** 4/4 completed

---

## Summary

The TindaKo MAUI project retained all .NET 9 target frameworks while adding `Plugin.MauiMTAdmob` 2.4.0 and updating compatible direct NuGet packages. The `sqlite-net-pcl` API diff was reviewed; its only removed member is unused, so no source migration was required. Restore and the full workspace build succeeded after aligning `Microsoft.Maui.Controls` to 9.0.120.

---

## What Changed

### Packages

| Project | Package | Change | From → To |
|---------|---------|--------|-----------|
| TindaKo | Plugin.MauiMTAdmob | Added | — → 2.4.0 |
| TindaKo | ClosedXML | Updated | 0.105.0 → 0.105.1 |
| TindaKo | CommunityToolkit.Maui | Updated | 12.1.0 → 12.3.0 |
| TindaKo | CommunityToolkit.Mvvm | Updated | 8.4.0 → 8.4.2 |
| TindaKo | Microsoft.Maui.Controls | Updated for dependency compatibility | 9.0.90 → 9.0.120 |
| TindaKo | SQLitePCLRaw.lib.e_sqlite3.android | Updated | 2.1.12-pre20260709125052 → 2.1.14 |
| TindaKo | sqlite-net-pcl | Updated | 1.9.172 → 1.11.285 |
| TindaKo | Syncfusion.Maui.Buttons | Updated | 30.1.42 → 35.1.42 |
| TindaKo | Syncfusion.Maui.Sliders | Updated | 30.2.5 → 35.1.42 |

Intentionally unchanged: `Microcharts.Maui` 1.0.1 because no supported newer version was found; `Microsoft.Extensions.Logging.Debug` 9.0.7; and `Syncfusion.Maui.Toolkit` 1.0.11.

### Code Modifications

- **Project file changes** — Updated direct `PackageReference` entries in `TindaKo/TindaKo.csproj`.
- **API compatibility** — No source changes were required. `SQLite.Orm.InflateAttribute`, the only removed member identified in the `sqlite-net-pcl` API diff, is not used by the project.
- **AdMob configuration** — Package installation only; runtime ad initialization and platform-specific ad configuration were not added.

### Git Commits

No commits were recorded. Git was unavailable in the terminal during the workflow, and no changes were discarded.

---

## Task Breakdown

- **01-add-maui-admob** — Added and validated `Plugin.MauiMTAdmob` 2.4.0. [Task](tasks/01-add-maui-admob/task.md) · [Details](tasks/01-add-maui-admob/progress-details.md)
- **02-update-net9-packages** — Updated compatible direct packages while keeping .NET 9. [Task](tasks/02-update-net9-packages/task.md) · [Details](tasks/02-update-net9-packages/progress-details.md)
- **03-fix-package-api-compatibility** — Reviewed the `sqlite-net-pcl` breaking API diff; no migration was needed. [Task](tasks/03-fix-package-api-compatibility/task.md) · [Details](tasks/03-fix-package-api-compatibility/progress-details.md)
- **04-validate-net9-workspace** — Restored and built the workspace; no test projects were found. [Task](tasks/04-validate-net9-workspace/task.md) · [Details](tasks/04-validate-net9-workspace/progress-details.md)

---

## Decisions Made

- Keep all target frameworks on .NET 9.
- Exclude Xamarin.Forms-specific AdMob solutions and use the .NET MAUI-compatible `Plugin.MauiMTAdmob` package.
- Use the latest stable versions resolved as compatible with .NET 9, with minimal safe changes.
- Keep `Microcharts.Maui` at 1.0.1 because no supported newer version was found.
- Use per-project package references because no central package-management file exists.
- Keep `Microsoft.Maui.Controls` on .NET 9; update it to 9.0.120 because it is required by `CommunityToolkit.Maui` 12.3.0.

---

## Build & Test Results

| Project | Build | Tests | Warnings |
|---------|-------|-------|----------|
| TindaKo | ✅ Full workspace build succeeded | Not applicable — no test projects discovered | NU1608 transitive AndroidX dependency-range warnings; NETSDK1202 for the .NET 9 Android workload |

An initial validation attempt exposed NU1605 for the `Microsoft.Maui.Controls` direct dependency; this was resolved by updating it from 9.0.90 to 9.0.120. Restore subsequently succeeded.

---

## Known Gaps & Follow-up Items

- **Transitive AndroidX warnings** — Restore reports seven NU1608 dependency-range warnings involving AndroidX lifecycle, saved-state, fragment, and activity packages. Review the MAUI/Android workload and AdMob dependency graph if warning-free restore is required.
- **.NET 9 Android workload support** — The installed SDK reports NETSDK1202 because the .NET 9 Android workload is out of support. Framework targets were intentionally not changed.
- **AdMob runtime setup** — Add platform-specific app ID configuration, consent handling, and `MauiProgram` initialization when integrating ads into the application.
