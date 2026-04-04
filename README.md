# 🚀 Traceon

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)](https://docs.docker.com/compose/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Status: Alpha](https://img.shields.io/badge/Status-Alpha-orange)]()

**Traceon** is a web application for tracking repeatable actions, logging entries with custom fields, and analyzing trends over time.

Whether you're logging workouts, purchases, finances, study sessions, or any custom activity — Traceon lets you define exactly what you want to track and gives you the tools to understand your patterns through dashboards, charts, and analytics.

### 🌐 Try it live

> **[traceon.arisoul.org](https://traceon.arisoul.org)** — Free to use. Create an account and start tracking.

---

## ✨ Features

### Tracking & Data Entry

- **Custom Tracked Actions** — Define any action you want to monitor (e.g. "Run", "Coffee Purchase", "Budget Transaction")
- **Flexible Field Definitions** — Create reusable field templates (Text, Integer, Decimal, Date, Boolean, Dropdown) with validation rules, units, default values, and target values
- **Action Fields & Entries** — Attach fields to actions and log entries with per-field values and timestamps
- **Quick Tracking** — One-click entry logging from the home page with a streamlined modal, drag-and-drop card reordering, and instant feedback
- **Creation Wizard** — Multi-step guided flow to create new tracked actions (basic info → fields → tags → initial entries → review)
- **Tags** — Organize tracked actions with colored tags for quick filtering

### Dashboard & Analytics

- **Dashboard** — Overview with summary cards (total actions, entries, today, this week, day streak), global entry frequency chart, and per-action metric cards
- **Action Detail Analytics** — Deep-dive page per action with key metrics (entry count, fields, average gap, streak, last entry), entry frequency charts, trend lines, and field-level analytics
- **Field Analytics Rules** — Cross-field analytics with configurable measure, group-by, filter, and sign fields — supporting Sum, Avg, Min, Max, Count, and SignedSum aggregations displayed as tables, bar charts, pie charts, or stacked bar charts
- **Balance Tracking** — Real-time balance indicators on quick-track cards for finance-oriented actions (income/expense with signed sums)
- **Summary Metrics** — Per-column summaries (Sum, Avg, Min, Max, Count) on data grids with persistent preferences
- **Trend Charts** — Configurable trend visualization per field (Line, Bar, Area) with aggregation options (All Points, Average, Min, Max, Sum)
- **Period Filters** — Filter dashboard and analytics data by last N days/months/years, specific month, or custom date range

### Data Grid & Filtering

- **Reusable DataGrid Component** — Sortable columns, full-text search, server-side pagination, column summaries, persistent sort/filter/page-size state via localStorage
- **Field-Level Filters** — Filter entries by any action field (dropdown selectors, boolean toggles, text search) directly in the entries grid
- **Mobile Responsive** — Card-based layout on small screens with collapsible filters and mobile sort controls

### Finance Features

- **Transaction Tracking** — Pre-built finance templates for tracking income, expenses, and transactions
- **SignedSum Aggregation** — Running balance calculation using a sign field to distinguish income vs. expense entries
- **Category Breakdowns** — Group-by analytics to see spending/income by category, payment method, or any custom field

### Templates & Onboarding

- **Template Packs** — Pre-built packs for Health, Fitness, Habits, and Finance — each with localized action names, field definitions, and analytics rules
- **Localized Templates** — Template names, descriptions, and dropdown values are installed in the user's language (English, Spanish, Portuguese)
- **Quick Start** — Choose a template pack on first login or start from scratch

### Soft Delete & Trash

- **Soft Delete** — Actions, field definitions, and tags are soft-deleted with timestamps instead of permanently removed
- **Trash Management** — View and restore soft-deleted items from a dedicated trash page
- **Automatic Purge** — Background service permanently removes soft-deleted data after a configurable retention period (per-user or system default)

### Settings & Account

- **Theme Support** — Light, Dark, and System theme modes
- **Language Selection** — English, Spanish, and Portuguese with full UI localization
- **Account Management** — Linked external accounts (Google, Microsoft), password change, and account deletion with confirmation
- **Activity Log** — Browse security-relevant audit events with date, action, and text filters
- **Data Portability** — Export and import all your data as JSON for backup or migration

### Platform & Infrastructure

- **OData Querying** — Filter, sort, and paginate entries using OData on the API
- **Authentication** — JWT-based auth with refresh tokens, email confirmation, password reset, and external login (Google, Microsoft)
- **Audit Logging** — Security-relevant actions are logged with IP and user-agent metadata
- **Structured Logging** — Serilog with console and rolling file sinks
- **Feedback** — In-app feedback form delivered via email
- **Docker Compose** — One-command deployment with SQL Server, API, Blazor (nginx), and Caddy reverse proxy with automatic HTTPS

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
| CSS / Icons | Bootstrap + Bootstrap Icons |
| Charts | [Blazor-ApexCharts](https://github.com/apexcharts/Blazor-ApexCharts) |
| Local Storage | Blazored.LocalStorage |
| Auth State | Custom `AuthenticationStateProvider` with JWT |
| Localization | Microsoft.Extensions.Localization (EN, ES, PT) |

### Deployment

| Component | Technology |
|---|---|
| Containers | Docker + Docker Compose |
| Reverse Proxy | [Caddy](https://caddyserver.com/) (automatic HTTPS) |
| Frontend Server | nginx (static WASM files) |
| Database | SQL Server 2022 |

### Architecture

- **Clean Architecture** — Domain → Application → Infrastructure → API
- **Shared Contracts** — `Traceon.Contracts` project shared between API and Blazor client
- **Rich Domain Model** — Entities with factory methods, encapsulated state, soft delete, and `Guid v7` identifiers
- **Result Pattern** — Service methods return `Result<T>` for explicit error handling without exceptions

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
│   │       ├── Components/             # Reusable components (DataGrid, QuickTrack, EntryDataGrid…)
│   │       ├── Services/               # HTTP service clients
│   │       ├── Auth/                   # Token management & auth state
│   │       └── Layout/                 # Shared layout & navigation
│   │
│   └── Traceon.Shared/                 # Shared resources (localization)
│
├── docs/                               # Deployment & design documents
├── docker-compose.yml                  # Full-stack deployment
├── Caddyfile                           # Reverse proxy configuration
├── .env.example                        # Environment variable template
├── .github/                            # GitHub workflows & config
└── LICENSE
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB or full instance)
- (Optional) [Docker](https://docs.docker.com/get-docker/) for containerized deployment
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

### Run with Docker Compose

```bash
cp .env.example .env
# Edit .env with your secrets and domain names
docker compose up -d --build
```

This starts SQL Server, the API, Blazor (nginx), and Caddy with automatic HTTPS.

### Configuration

| Setting | Location | Description |
|---|---|---|
| Connection string | `appsettings.Development.json` | `ConnectionStrings:TraceonDb` — SQL Server |
| JWT secrets | `appsettings.json` / User Secrets | `Jwt:Key`, issuer, audience, expiration |
| Email / SMTP | User Secrets | `Email:SmtpPassword` (SendGrid API key) |
| External auth | User Secrets | Google & Microsoft OAuth client secrets |
| Allowed origins | `appsettings.Development.json` | CORS origins for the Blazor client |
| Purge settings | `appsettings.json` | `Purge:IntervalHours`, `Purge:DefaultRetentionDays` |

> **Tip:** Use `dotnet user-secrets` to store sensitive values outside of source control.

### Database Migrations

Migrations are applied automatically on startup (`ApplyMigrationsAsync`). To add new migrations:

```bash
cd src/Traceon.Api/Traceon.Api
dotnet ef migrations add <MigrationName> --project src/Traceon.Infrastructure --startup-project src/Traceon.Api
```

---

## 🐳 Deployment

A full Docker Compose setup is provided for production deployment:

| Service | Image / Build | Port |
|---|---|---|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | 1433 |
| `api` | Custom build (ASP.NET Core) | 8080 |
| `blazor` | Custom build (nginx + WASM) | 80 |
| `caddy` | `caddy:2-alpine` | 80, 443 |

See [`docs/deployment-hetzner.md`](docs/deployment-hetzner.md) for a step-by-step guide to deploying on a VPS.

---

## 🚧 Status

Traceon is in **active development (alpha)**.

The core API and Blazor frontend are fully functional with authentication, CRUD for all entities, dashboards, analytics, finance tracking, data grid with filters and summaries, soft delete with trash management, template packs, Docker deployment, and data export/import.

### Roadmap

- [ ] Automated tests (unit, integration, E2E)
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Recurring action schedules & reminders
- [ ] PWA support for the Blazor client

---

## 🧠 Inspiration

This project was born from the need to quantify and analyze anything in life — routines, purchases, habits, finances — with freedom and flexibility.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

*Built with ❤️ in .NET by [ARiSoul](https://github.com/ARiSoul) and [Claude Opus 4](https://claude.ai).*
