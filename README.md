# 🚀 Traceon

**Traceon** aims to be a cross-platform app to track repeatable events, analyze trends, and gain insights over time.

Whether you're logging purchases, workouts, study sessions, or any custom activity, Traceon helps you understand your patterns and evolve with meaningful data.

---

## 📦 Solution Structure (may change)

```text
Traceon/
│
├── src/
│   ├── Traceon.Core/             # Domain models & interfaces
│   ├── Traceon.Infrastructure/   # Local storage & service implementations
│   ├── Traceon.Application/      # Use cases & business logic orchestration
│   └── Traceon.MAUI/             # MAUI app (MVVM UI)
│
├── tests/
│   ├── Traceon.Core.Tests/
│   ├── Traceon.Infrastructure.Tests/
│   └── Traceon.Application.Tests/
```

---

## 🔧 Tech Stack

- [.NET 9](https://dotnet.microsoft.com/)
- [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/)
- MVVM (with [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/overview))
- Clean Architecture
- SQLite / JSON (local-first)
- Authenticated ASP.NET Core Web API (planned)
- Authenticated Blazor Web frontend (planned)

---

## 🚧 Status

Traceon is in early development (MVP phase).  
We are currently working on defining the core domain models and building the first local-only version of the app.

---

## 📌 Goals

- Track any kind of action or event
- Analyze frequency, duration, cost, and efficiency
- Provide smart dashboards and visual insights
- Enable full offline/online sync across devices

---

## 📁 Getting Started

> Setup instructions coming soon.

---

## 🧠 Inspiration

This project was born from the need to quantify and analyze anything in life — routines, purchases, habits — with freedom and flexibility.

---

## 📄 License

MIT License (to be confirmed)

---

*Built with ❤️ in .NET.*
