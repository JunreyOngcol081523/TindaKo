# 04-validate-net9-workspace: Validate restore, build, and tests

Restore and build the TindaKo solution/project across the available .NET 9 MAUI targets, treating warnings as errors for the upgrade validation. Run available tests and record any platform or workload limitations that prevent a target from being built in the current environment.

**Done when**: Restore and applicable builds complete successfully, available tests pass, and compatibility or environment limitations are documented in the task progress artifacts.

## Validation Findings

- The first solution validation exposed NU1605 because `CommunityToolkit.Maui` 12.3.0 requires `Microsoft.Maui.Controls` 9.0.120 while the project referenced 9.0.90. The direct reference was aligned to 9.0.120, preserving .NET 9.
- Restore and full workspace build then completed successfully.
- No test projects were discovered.
- Restore reports transitive NU1608 AndroidX dependency-range warnings and NETSDK1202 for the out-of-support .NET 9 Android workload; these are documented in `progress-details.md`.
