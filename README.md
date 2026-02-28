# ApplicationAuth - Modern .NET 8 Identity Template

A production-ready, enterprise-grade authentication template built with **.NET 8** and **Vertical Slice Architecture**.

## 🚀 Features

- **Vertical Slice Architecture**: Clean, scalable, and easy-to-navigate project structure.
- **Minimal APIs**: High-performance, lightweight endpoint definitions.
- **Identity & Security**: Fully integrated ASP.NET Core Identity with JWT (Access & Refresh tokens).
- **Social Auth**: Native support for **Google** and **Apple** sign-in via OpenID cryptographic verification.
- **CQRS Mapping**: Powered by **MediatR** for decoupled business logic.
- **Email Verification**: Secure account activation and password recovery flows.
- **Reliability**: Integrated **Health Checks** for monitoring system vitals.
- **Protection**: Built-in **Rate Limiting** to prevent brute-force attacks.
- **Logging & Auditing**: Advanced logging via **Serilog** and EF Core auditing.

## 🛠 Tech Stack

- **Framework**: .NET 8.0
- **Database**: SQLite (EF Core)
- **Validation**: FluentValidation
- **Mapper**: Native Mapping / record types
- **API Documentation**: Swagger / OpenAPI

## 🏗 Getting Started

1. Clone the repository.
2. Configure your SMTP settings in `appsettings.json` (optional, defaults to console log).
3. Run the project:
   ```bash
   dotnet run --project ApplicationAuth/ApplicationAuth.csproj
   ```
4. Explore the API via Swagger at `http://localhost:1310/swagger`.

## 🔒 Security Note
This project uses JWT for authentication. Ensure you change the `SecretKey` in production configurations.

---
Developed as a high-quality template for agentic coding and modern web standards.
