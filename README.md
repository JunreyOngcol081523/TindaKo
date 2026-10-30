# TindaKo

TindaKo is a .NET MAUI mobile application designed for retail inventory and sales management. The project helps manage products, sales transactions, incoming stock, creditors, reports, and basic store settings in a single app interface.

## Project Overview

This app is built for store operations and includes modules for:
- Product management
- Sales processing
- Incoming stock records
- Inventory alerts for low-stock items
- Reports and sales summaries
- Customer/creditor tracking
- Database backup and maintenance
- Store/user settings

## Tech Stack

- .NET 9 MAUI
- C#
- SQLite
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
- Syncfusion MAUI components
- Microcharts for charts and dashboard visuals

## Features Already Finished

The following features are already in place in the current project:

- Login screen and remembered-user login flow
- Main menu dashboard with sales summary cards
- Product management page
- Sales page and transaction workflow
- Incoming stocks tracking
- Product low-stock alert monitoring
- Fast-moving product detection
- Reports page with chart-based analytics
- Creditors / unpaid due tracking
- Database backup page
- Basic user settings and product settings screens
- Store settings configuration
- Navigation between major app modules
- SQLite-based data persistence

## On-Going Work

Current development focus includes:

- Finalizing and polishing the app flow across screens
- Improving data validation and consistency in transactions and inventory updates
- Refining reports and analytics behavior
- Enhancing settings and store configuration screens
- Testing the app across main use cases and edge scenarios
- Improving UI/UX consistency across pages and dialogs

## To-Do / Planned Improvements

Planned enhancements for future development:

- Add stronger error handling and validation messages
- Improve report filtering and export capabilities
- Enhance data backup/restore reliability and user guidance
- Add more detailed product and sales analytics
- Improve accessibility and mobile responsiveness
- Add more role-based restrictions and admin management features
- Improve app performance and reduce redundant data loading
- Expand documentation and user onboarding flow
- Add automated testing for critical inventory and sales workflows

## Project Structure

- `Views/` – XAML pages and UI screens
- `ViewModel/` – ViewModels for business logic and data binding
- `Services/` – Database and app service logic
- `Models/` – Data models
- `Resources/` – Images, fonts, and app assets
- `App.xaml.cs` – App startup and navigation setup
- `MauiProgram.cs` – MAUI app registration and service setup

## How to Run

1. Open the solution in Visual Studio 2026 or later.
2. Restore NuGet packages.
3. Select a target platform (Android, iOS, Windows, etc.).
4. Build and run the project.

## Notes

This project is a working retail management application with core inventory and sales features already implemented. The app is currently moving toward completion and ongoing refinement before broader feature expansion or final deployment readiness.

## Status Summary

- Completed: Core retail management modules and app navigation
- In progress: Final polish, validation, and feature refinement
- Planned: Extra reporting, automation, testing, and system hardening
