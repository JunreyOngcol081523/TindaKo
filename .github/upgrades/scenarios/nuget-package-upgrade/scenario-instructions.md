# NuGet Package Upgrade

## Strategy
Assess all direct NuGet packages in the TindaKo .NET 9 MAUI solution, add an appropriate current stable .NET MAUI AdMob plugin, and use the smallest safe version changes for vulnerabilities and compatibility.

## Preferences
- **Flow Mode**: Automatic
- **Scope**: TindaKo solution
- **Target Framework**: Keep all projects targeting .NET 9
- **AdMob Package Policy**: Select a current stable package compatible with .NET MAUI and .NET 9; do not use Xamarin.Forms-specific packages
- **Other Packages**: Assess all existing direct package references
- **Version Policy**: Latest stable versions supported by the project, with smallest safe changes for vulnerabilities
- **Full Breaking-Change Scan**: Disabled for initial assessment
- **Commit Strategy**: After Each Task

## Decisions
- Keep all target frameworks on .NET 9; do not adopt the assessment's .NET 10 Microsoft package recommendations.
- Keep Microcharts.Maui at 1.0.1 because no supported newer version was found for the current target frameworks.
- Use per-project PackageReference updates because no Directory.Packages.props file exists.
- Migrate sqlite-net-pcl API usage if the removed member is used; otherwise retain the smallest safe compatible version.
- Add only a current stable AdMob package that is compatible with .NET MAUI and .NET 9; exclude Xamarin.Forms-specific packages.

## Source Control
- **Source Branch**: master
- **Working Branch**: nuget-package-upgrade
- **Pending Changes**: Unknown; Git is unavailable in the terminal, and no changes will be discarded
- **Branch Sync**: Disabled
