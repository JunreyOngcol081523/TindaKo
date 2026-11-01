# 02-update-net9-packages: Update compatible direct packages

Apply the smallest safe stable-version changes for the direct package references that have compatible .NET 9 versions: ClosedXML, CommunityToolkit.Maui, CommunityToolkit.Mvvm, SQLitePCLRaw.lib.e_sqlite3.android, sqlite-net-pcl, and the Syncfusion packages where compatibility is confirmed. Keep `Microcharts.Maui` at 1.0.1, and keep the .NET 9 Microsoft MAUI and logging package lines on .NET 9-compatible versions rather than adopting the assessment's .NET 10 recommendations.

**Done when**: The selected package versions are recorded in the project file, restore succeeds for all configured .NET 9 target frameworks, and no package update introduces an unsupported target framework.

## Research Findings

- The project uses standard per-project `PackageReference` entries; no `Directory.Packages.props` file is active.
- Current .NET 9-compatible stable versions resolved for this project are: `ClosedXML` 0.105.1, `CommunityToolkit.Maui` 12.3.0, `CommunityToolkit.Mvvm` 8.4.2, `SQLitePCLRaw.lib.e_sqlite3.android` 2.1.14, `sqlite-net-pcl` 1.11.285, `Syncfusion.Maui.Buttons` 35.1.42, and `Syncfusion.Maui.Sliders` 35.1.42.
- `Syncfusion.Maui.Toolkit` resolves to 1.0.11, so it remains unchanged. `Microcharts.Maui` remains 1.0.1 because no supported newer version was found.
- `Microsoft.Maui.Controls` and `Microsoft.Extensions.Logging.Debug` remain at their .NET 9 versions (`9.0.90` and `9.0.7`); the assessment's 10.x recommendations are excluded by the .NET 9 constraint.
- The package references are all defined in `TindaKo/TindaKo.csproj`.
