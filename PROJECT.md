# Project context

## Stack
- Language: C#, HTML, CSS, JavaScript
- Framework: ASP.NET Core (API + Razor Pages), .NET
- Database: Microsoft SQL Server
- Package manager: NuGet

## Dependencies
- Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
- Microsoft.EntityFrameworkCore.Tools (9.0.0)

## Conventions
- Business logic lives only in the backend API/managers.
- Frontend clients only display data and call the API (no scheduling/booking logic).
- Razor Pages admin dashboard calls managers directly inside the API project.

## Key files
- VechileCleaningAPI/Program.cs (service setup, DB config, routing)
- VechileCleaningAPI/VechileCleaningAPI.csproj (framework + NuGet dependencies)
- CustomerWebsite/index.html (customer booking UI)
- VechileCleaningAPI/Pages (Razor Pages admin dashboard)
- VechileCleaningAPI/Controllers (API endpoints)
- VechileCleaningAPI/Data (EF Core DbContext)

## Notes
- The system is a vehicle cleaning reservation platform with separate customer, admin, and worker clients.
- Backend enforces booking rules, working hours, and capacity limits.
- Customer website is a static HTML/CSS/JS frontend that talks to the API.