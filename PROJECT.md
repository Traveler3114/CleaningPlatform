# Project context

## Stack
- **Language**: C# 12+ (backend API), HTML/CSS/JavaScript (frontend)
- **Framework**: ASP.NET Core 10.0 Web API
- **Database**: Microsoft SQL Server via Entity Framework Core 10.0
- **Auth**: JWT Bearer + policy-based authorization
- **Password hashing**: BCrypt.Net-Next
- **Email**: SendGrid
- **Excel export**: ClosedXML

## Project structure

```text
CleaningPlatformAPI/
├── Program.cs
├── Authorization/
├── Common/
├── Contracts/
├── Controllers/
├── Data/
├── Entities/
├── Enums/
├── Extensions/
├── Managers/
├── Mapping/
├── Services/
└── wwwroot/
```

## Conventions

- Use file-scoped namespaces
- Use `CancellationToken ct = default` as last async parameter
- Managers contain business logic
- Controllers remain thin
- Use `OperationResult<T>`
- Avoid hard deletes for operational records
