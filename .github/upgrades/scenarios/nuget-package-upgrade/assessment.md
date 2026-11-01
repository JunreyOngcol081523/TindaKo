# NuGet package upgrade assessment

_Mode: **quick assessment** — package API diffs only; no per-project source scan was run._

## Recommended versions

- **ClosedXML**: **0.105.1** (unified across 1 project(s)).
- **CommunityToolkit.Maui**: **12.3.0** (unified across 1 project(s)).
- **CommunityToolkit.Mvvm**: **8.4.2** (unified across 1 project(s)).
- **Microcharts.Maui**: ⚠ **divergent** — projects need or support different versions:
    - TindaKo.csproj: 1.0.1 → ⚠ no supported version — this project cannot be upgraded
- **Microsoft.Extensions.Logging.Debug**: **10.0.14** (unified across 1 project(s)).
- **Microsoft.Maui.Controls**: **10.0.120** (unified across 1 project(s)).
- **Plugin.MauiMTAdmob**: not referenced by any scoped project.
- **sqlite-net-pcl**: **1.11.285** (unified across 1 project(s)).
- **SQLitePCLRaw.lib.e_sqlite3**: not referenced by any scoped project.
- **SQLitePCLRaw.lib.e_sqlite3.android**: **2.1.14** (unified across 1 project(s)).
- **Syncfusion.Maui.Buttons**: **35.1.42** (unified across 1 project(s)).
- **Syncfusion.Maui.Sliders**: **35.1.42** (unified across 1 project(s)).
- **Syncfusion.Maui.Toolkit**: **1.0.11** (unified across 1 project(s)).

## Public API changes

> **Types moved (namespace changed) are not removals.** A moved type keeps its name and members;
> the fix is a `using`-directive change, not a rewrite. Do not treat a moved type as deleted.

- **ClosedXML**: no source-breaking public API changes detected — see [`apidiff/ClosedXML.apidiff.md`](apidiff/ClosedXML.apidiff.md).
- **CommunityToolkit.Maui**: no source-breaking public API changes detected — see [`apidiff/CommunityToolkit.Maui.apidiff.md`](apidiff/CommunityToolkit.Maui.apidiff.md).
- **CommunityToolkit.Mvvm**: no source-breaking public API changes detected — see [`apidiff/CommunityToolkit.Mvvm.apidiff.md`](apidiff/CommunityToolkit.Mvvm.apidiff.md).
- **Microsoft.Maui.Controls**: no source-breaking public API changes detected — see [`apidiff/Microsoft.Maui.Controls.apidiff.md`](apidiff/Microsoft.Maui.Controls.apidiff.md).
- **Microsoft.Extensions.Logging.Debug**: no source-breaking public API changes detected — see [`apidiff/Microsoft.Extensions.Logging.Debug.apidiff.md`](apidiff/Microsoft.Extensions.Logging.Debug.apidiff.md).
- **SQLitePCLRaw.lib.e_sqlite3.android**: no source-breaking public API changes detected — see [`apidiff/SQLitePCLRaw.lib.e_sqlite3.android.apidiff.md`](apidiff/SQLitePCLRaw.lib.e_sqlite3.android.apidiff.md).
- **sqlite-net-pcl**: 1 member(s) removed — see [`apidiff/sqlite-net-pcl.apidiff.md`](apidiff/sqlite-net-pcl.apidiff.md).
- **Syncfusion.Maui.Buttons**: no source-breaking public API changes detected — see [`apidiff/Syncfusion.Maui.Buttons.apidiff.md`](apidiff/Syncfusion.Maui.Buttons.apidiff.md).
- **Syncfusion.Maui.Sliders**: no source-breaking public API changes detected — see [`apidiff/Syncfusion.Maui.Sliders.apidiff.md`](apidiff/Syncfusion.Maui.Sliders.apidiff.md).

## Breaking-change findings

- Version divergence findings (Pkg.0003): 1
- Requested-version-unsupported findings (Pkg.0002): 0

- Quick mode does not scan source, so there are no per-line `PkgApi` usage findings. Review the
  per-package API diffs above and rely on build errors during execution to pinpoint affected code.
- A full code scan can locate the exact source location of every breaking-change usage across the repo.
  It is opt-in and slower — re-run the assessment with `fullScan=true` only if the user requests it.

## Next steps

1. **Resolve version divergence** — the scoped projects cannot all move to one version. Where a project shows `no supported version`, nothing published supports its target framework(s), so raising that project's framework or dropping it from the scope is the reconciliation choice; otherwise pick a common version in planning.
2. Proceed to planning to triage the API changes above and plan the code fixes.

