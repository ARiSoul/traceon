# 🚀 Traceon

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Status: Alpha](https://img.shields.io/badge/Status-Alpha-orange)]()

**Traceon** is a web application for tracking repeatable actions, logging entries with custom fields, and analyzing trends over time.

Whether you're logging workouts, purchases, study sessions, or any custom activity — Traceon lets you define exactly what you want to track and gives you the tools to understand your patterns through dashboards and charts.

---

## ✨ Features

- **Custom Tracked Actions** — Define any action you want to monitor (e.g. "Run", "Coffee Purchase", "Study Session")
- **Flexible Field Definitions** — Create reusable field templates (Text, Integer, Decimal, Date, Boolean, Dropdown) with validation rules, units, and default values
- **Action Fields & Entries** — Attach fields to actions and log entries with per-field values and timestamps
- **Tags** — Organize tracked actions with colored tags for quick filtering
- **Dashboard & Charts** — Visualize trends with configurable metrics and aggregation (via ApexCharts)
- **OData Querying** — Filter, sort, and paginate entries using OData on the API
- **Template Packs** — Install pre-built action templates to get started quickly
- **Data Portability** — Export all your data as JSON for backup or migration
- **Authentication** — JWT-based auth with refresh tokens, email confirmation, password reset, and external login (Google, Microsoft)
- **User Preferences** — Language, theme, and display settings stored per user
- **Audit Logging** — Security-relevant actions are logged with IP and user-agent metadata
- **Structured Logging** — Serilog with console and rolling file sinks
- **Feedback** — In-app feedback form delivered via email

---

## 🔧 Tech Stack

### Backend — ASP.NET Core Web API

| Layer | Technology |
|---|---|
| Runtime | [.NET 10](https://dotnet.microsoft.com/) |
| Web Framework | ASP.NET Core Minimal APIs |
| ORM | Entity Framework Core 10 (SQL Server) |
| Identity | ASP.NET Core Identity + JWT Bearer |
| External Auth | Google, Microsoft Account |
| Validation | FluentValidation |
| Querying | Microsoft.AspNetCore.OData |
| Email | MailKit / SendGrid SMTP |
| Logging | Serilog |
| API Docs | OpenAPI + [Scalar](https://github.com/scalar/scalar) |

### Frontend — Blazor WebAssembly

| Concern | Technology |
|---|---|
| UI Framework | Blazor WebAssembly (.NET 10) |
| CSS | Bootstrap |
| Charts | [Blazor-ApexCharts](https://github.com/apexcharts/Blazor-ApexCharts) |
| Local Storage | Blazored.LocalStorage |
| Auth State | Custom `AuthenticationStateProvider` with JWT |
| Localization | Microsoft.Extensions.Localization |

### Architecture

- **Clean Architecture** — Domain → Application → Infrastructure → API
- **Shared Contracts** — `Traceon.Contracts` project shared between API and Blazor client
- **Rich Domain Model** — Entities with factory methods, encapsulated state, and `Guid v7` identifiers

---

## 📦 Solution Structure

```text
traceon/
├── src/
│   ├── Traceon.Api/                    # ASP.NET Core Web API (host, endpoints, filters)
│   │   └── src/
│   │       ├── Traceon.Api/            # Minimal API endpoints & Program.cs
│   │       ├── Traceon.Application/    # Services, validators, mapping, Result pattern
│   │       ├── Traceon.Contracts/      # Shared DTOs, request/response records, enums
│   │       ├── Traceon.Domain/         # Entities, repository interfaces
│   │       └── Traceon.Infrastructure/ # EF Core, Identity, email, audit, data export
│   │
│   ├── Traceon.Blazor/                 # Blazor WebAssembly frontend
│   │   └── Traceon.Blazor/
│   │       ├── Pages/                  # Razor pages (Auth, Dashboard, Entries, Settings…)
│   │       ├── Services/               # HTTP service clients
│   │       ├── Auth/                   # Token management & auth state
│   │       └── Layout/                 # Shared layout & navigation
│   │
│   ├── Traceon.Maui/                   # .NET MAUI app (legacy, not actively developed)
│   └── Traceon.Shared/                 # Shared resources (localization)
│
├── docs/                               # Design documents
├── .github/                            # GitHub workflows & config
└── LICENSE
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB or full instance)
- (Optional) A SendGrid API key for transactional emails

### Run the API

```bash
cd src/Traceon.Api/Traceon.Api
dotnet run --project src/Traceon.Api
```

The API starts at `https://localhost:5001` (or the configured port).  
In development, interactive API docs are available at `/scalar/v1`.

### Run the Blazor Client

```bash
cd src/Traceon.Blazor
dotnet run --project Traceon.Blazor
```

The client starts at `http://localhost:5284` by default.

### Configuration

| Setting | Location | Description |
|---|---|---|
| Connection string | `appsettings.Development.json` | `ConnectionStrings:TraceonDb` — SQL Server |
| JWT secrets | `appsettings.json` / User Secrets | `Jwt:Key`, issuer, audience, expiration |
| Email / SMTP | User Secrets | `Email:SmtpPassword` (SendGrid API key) |
| External auth | User Secrets | Google & Microsoft OAuth client secrets |
| Allowed origins | `appsettings.Development.json` | CORS origins for the Blazor client |

> **Tip:** Use `dotnet user-secrets` to store sensitive values outside of source control.

### Database Migrations

Migrations are applied automatically on startup (`ApplyMigrationsAsync`). To add new migrations:

```bash
cd src/Traceon.Api/Traceon.Api
dotnet ef migrations add <MigrationName> --project src/Traceon.Infrastructure --startup-project src/Traceon.Api
```

---

## 🚧 Status

Traceon is in **active development (alpha)**.

The core API and Blazor frontend are functional with authentication, CRUD for all entities, dashboards, and data export.

### Roadmap

- [ ] Automated tests (unit, integration, E2E)
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Containerization (Docker / Docker Compose)
- [ ] Production deployment guide
- [ ] Recurring action schedules & reminders
- [ ] Advanced analytics & insights
- [ ] PWA support for the Blazor client

---

## 🧠 Inspiration

This project was born from the need to quantify and analyze anything in life — routines, purchases, habits — with freedom and flexibility.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

*Built with ❤️ in .NET by [ARiSoul](https://github.com/ARiSoul).*
