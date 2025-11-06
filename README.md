# GenesisCars

GenesisCars is an experimental ASP.NET Core MVC application that prototypes an operations hub for the Genesis Cars dealership. The solution explores how one unified tool could streamline daily dealership workflows by bringing inventory management, user administration, dashboards, and authentication into a single experience.

## Highlights

- **Dealership Dashboard** – summarizes user registrations and vehicle inventory so the team can monitor engagement at a glance.
- **User Management** – CRUD workflows for dealership team members, including registration, login, and secure cookie-based authentication.
- **Inventory Management** – add, edit, and retire vehicles while experimenting with simplified data entry flows.
- **Domain-Driven Design** – layered Domain, Application, Infrastructure, and Web projects to keep responsibilities clear and ready for future persistence changes.
- **In-Memory Persistence** – repositories run in memory to keep experimentation fast without external dependencies.

## Solution Layout

```
GenesisCars.sln
├── GenesisCars.Domain           // Entities, value objects, repository contracts
├── GenesisCars.Application      // DTOs, services, and application layer exceptions
├── GenesisCars.Infrastructure   // In-memory repository implementations and DI wiring
├── GenesisCars.Web              // ASP.NET Core MVC site, controllers, views, and auth
├── GenesisCars.Jobs             // Placeholder console job project for future automation
└── GenesisCars.Tests            // xUnit test suites covering core behaviors
```

## Getting Started

### Prerequisites

- [.NET SDK 9.0](https://dotnet.microsoft.com/download)
- Optional: Visual Studio 2022, Rider, or VS Code with the C# extension

### Run the application

```powershell
cd GenesisCars
dotnet run --project GenesisCars.Web
```

Browse to `https://localhost:5001` (or the port shown in the console). You can register a new account from the landing page to explore the secured areas.

### Execute the test suite

```powershell
cd GenesisCars
dotnet test
```

## Authentication Notes

- Registration and login are designed for experimentation only; use sample data when signing up.
- Accounts live in memory for the lifetime of the app. Restarting the site clears registered users and inventory.
- Cookies secure the authenticated session so that only logged-in participants can access the dashboard, users, and inventory features.

## Experimental Scope

This codebase is **not** a production system. All data entered is treated as test data and should be synthetic. The project exists to validate ideas with dealership stakeholders before any long-term investments are made.

## Contributing

The repository currently targets internal experimentation. If you have feedback or want to extend a capability, coordinate with the Genesis Cars product team so changes stay aligned with evaluation goals.
