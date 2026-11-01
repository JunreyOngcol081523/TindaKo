# 01-add-maui-admob: Add a compatible MAUI AdMob plugin

Add the current stable AdMob package selected by compatibility analysis to `TindaKo/TindaKo.csproj`. The package must target .NET MAUI and support the existing .NET 9 MAUI project; Xamarin.Forms-specific packages are excluded. Keep the change limited to package installation unless build validation requires the corresponding MAUI registration or initialization.

**Done when**: A stable AdMob package compatible with the project's .NET 9 target frameworks restores successfully, is referenced by the project, and does not introduce Xamarin.Forms dependencies or target-framework incompatibilities.

## Compatibility Review

- Candidate: `Plugin.MauiMTAdmob` version `2.4.0`, current stable version returned for the project.
- The package publishes asset groups for `net9.0`, `net9.0-android35.0`, `net9.0-ios18.0`, `net9.0-maccatalyst18.0`, and `net9.0-windows10.0.19041`.
- Its .NET 9 dependency groups require `Microsoft.Maui.Controls >= 9.0.10`; Android and iOS groups add Google Ads platform bindings and do not depend on Xamarin.Forms.
- The existing project targets `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, and `net9.0-windows10.0.19041`, so the package's .NET 9 asset groups align with the project. The Android/iOS platform minimums should be confirmed by restore/build because the package groups specify Android API 35 and iOS 18.
- No `PackageReference` or source-code change has been made during this review.

## Implementation Research

- `get_project_dependencies` confirms the project uses standard per-project `PackageReference` entries; no central package-management file is active.
- The package reference belongs in `TindaKo/TindaKo.csproj` with `Version="2.4.0"`.
- After adding the reference, restore/build validation must confirm that the package's Android API 35 and iOS 18 asset groups resolve with the existing .NET 9 MAUI targets.
