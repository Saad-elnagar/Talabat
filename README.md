# Talabat — E-Commerce Backend API

[![Build](https://github.com/Saad-elnagar/Talabat/actions/workflows/dotnet-build.yml/badge.svg?branch=master)](https://github.com/Saad-elnagar/Talabat/actions/workflows/dotnet-build.yml)

A multi-project e-commerce backend built with **C#, .NET 10, ASP.NET Core Web API, Entity Framework Core, SQL Server, Redis, JWT, and Stripe**.

The project explores backend application architecture, identity, product catalog queries, basket and order workflows, caching, and payment-intent integration.

## Features

- Product catalog endpoints with filtering, pagination, and specification-based queries.
- User registration and authentication using ASP.NET Core Identity and JWT.
- Customer basket operations backed by the basket repository.
- Order creation and retrieval, including order items and delivery methods.
- Stripe payment-intent integration.
- Redis-backed basket and caching infrastructure, with HybridCache configuration.
- Token-bucket rate limiting for search-related requests.
- EF Core migrations and startup seeding for sample catalog and identity data.
- OpenAPI documentation with Scalar for exploring the API.

## Tech Stack

- **Language / Runtime:** C#, .NET 10
- **API:** ASP.NET Core Web API
- **Persistence:** Entity Framework Core, SQL Server
- **Authentication:** ASP.NET Core Identity, JWT
- **Caching:** Redis, HybridCache, in-memory cache
- **Payments:** Stripe
- **API Docs:** OpenAPI, Scalar

## Architecture

```text
Talabat.Api/
 Controllers, filters, API configuration, static files
      | references
      v
Talabat.Infrastructure/ -----------------> Talabat.Application/
 EF Core, repositories, migrations          Use cases, contracts, DTOs
 Redis, Stripe, identity                         |
      | references                               | references
      +-------------------------> Talabat.Domain/
                                   Entities and contracts
```

### Project responsibilities

| Project | Responsibility |
|---|---|
| `Talabat.Domain` | Business entities, repository contracts, and query specifications |
| `Talabat.Application` | Use cases, application contracts, API-facing DTOs, and mapping |
| `Talabat.Infrastructure` | EF Core persistence, repository implementations, migrations, Redis, Stripe, and identity |
| `Talabat.Api` | HTTP endpoints, authentication configuration, filters, static files, and API documentation |

Dependencies point inward: the API composes Application and Infrastructure, Infrastructure implements the contracts, and Domain remains independent of persistence.

## Prerequisites

- .NET 10 SDK
- SQL Server
- Redis
- Stripe test credentials for payment-intent flows

## Run Locally

From the repository root:

```bash
dotnet restore Talabat.sln
dotnet user-secrets init --project Talabat.Api/Talabat.Api.csproj
```

Configure secrets locally instead of committing them to `appsettings.json`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SQL_SERVER_CONNECTION_STRING" --project Talabat.Api/Talabat.Api.csproj
dotnet user-secrets set "ConnectionStrings:Identity" "YOUR_IDENTITY_DATABASE_CONNECTION_STRING" --project Talabat.Api/Talabat.Api.csproj
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_LONG_RANDOM_JWT_SECRET" --project Talabat.Api/Talabat.Api.csproj
dotnet user-secrets set "StripeSettings:Secretkey" "YOUR_STRIPE_TEST_SECRET_KEY" --project Talabat.Api/Talabat.Api.csproj
```

Make sure Redis is reachable at the configured `ConnectionStrings:Redis` address. The repository defaults this to `localhost`.

Start the API:

```bash
dotnet run --project Talabat.Api/Talabat.Api.csproj
```

Create EF Core migrations in the Infrastructure project and use the API as the startup project:

```bash
dotnet ef migrations add <MigrationName> --project Talabat.Infrastructure/Talabat.Infrastructure.csproj --startup-project Talabat.Api/Talabat.Api.csproj --context StoreContext
dotnet ef migrations add <MigrationName> --project Talabat.Infrastructure/Talabat.Infrastructure.csproj --startup-project Talabat.Api/Talabat.Api.csproj --context AppIdentityDbContext --output-dir Identity/Migrations
```

On startup, the application attempts to apply EF Core migrations and seed sample data. The database connections must be configured and SQL Server must be available before relying on the seeded data.

In Development, OpenAPI is mapped by the application and Scalar is available for interactive API exploration. Use the URL printed by `dotnet run` as the base address.

## Configuration & Security

- Never commit real SQL Server credentials, JWT signing keys, or Stripe secret keys.
- Use .NET User Secrets for local development and environment variables or a managed secret store for deployment.
- Use Stripe test credentials during development.
- The Stripe publishable key is not a substitute for the secret key; keep the secret key private.

## What This Project Demonstrates

- Organizing a backend solution into domain, application, infrastructure, and API projects.
- Using repositories, Unit of Work, and specifications to structure data access.
- Implementing identity, token-based authentication, caching, and rate limiting.
- Connecting order workflows to payment-intent handling.
- Documenting and testing an API through OpenAPI tooling.
