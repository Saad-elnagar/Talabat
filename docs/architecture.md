# Talabat architecture and workflows

## Repository hierarchy

```text
Talabat/
├── Backend/
│   ├── Talabat.Api/             HTTP API, authentication, controllers, static images
│   ├── Talabat.Application/     Use cases, DTOs, application services, mapping
│   ├── Talabat.Domain/          Entities, repository contracts, specifications
│   ├── Talabat.Infrastructure/  SQL Server, Redis, Stripe, Identity, repositories
│   └── Talabat.sln
├── Frontend/
│   └── Talabat.Web/             Angular storefront and Nginx proxy
├── compose.yaml                 Docker Compose for API and storefront
├── docs/
└── README.md
```

## Backend dependency hierarchy

```mermaid
flowchart LR
  Web["Angular storefront"] -->|HTTP /api| API["Talabat.Api"]
  API --> APP["Talabat.Application"]
  API --> INFRA["Talabat.Infrastructure"]
  INFRA --> APP
  APP --> DOMAIN["Talabat.Domain"]
  INFRA --> DOMAIN
  INFRA --> SQL["SQL Server"]
  INFRA --> REDIS["Redis"]
  INFRA --> STRIPE["Stripe"]
```

The Domain defines business entities and contracts. Application contains use cases and orchestration. Infrastructure implements persistence, caching, identity, and payment integrations. The API composes the layers and exposes HTTP endpoints.

## Current Angular file hierarchy

The current Angular storefront uses one standalone root component. The labels below describe template sections and UI states inside that component; they are not separate Angular components yet.

```mermaid
flowchart TD
  MAIN["src/main.ts<br/>bootstrapApplication"] --> CONFIG["src/app/app.config.ts<br/>providers and HTTP setup"]
  MAIN --> ROOT["src/app/app.ts<br/>standalone App component"]
  ROOT --> TEMPLATE["src/app/app.html<br/>root view template"]
  TEMPLATE --> SHOP["Shop view<br/>hero, filters, catalog"]
  TEMPLATE --> ACCOUNT["Orders view"]
  TEMPLATE --> OVERLAYS["Auth, basket, checkout, product dialogs"]
  ROOT --> SERVICE["src/app/store-api.service.ts<br/>HTTP API client"]
  ROOT --> MODELS["src/app/models.ts<br/>API and UI types"]
  CONFIG --> INTERCEPTOR["src/app/auth.interceptor.ts<br/>JWT header"]
  ROOT --> ROUTES["src/app/app.routes.ts<br/>route table (currently empty)"]
  ROOT --> STYLES["src/styles.css and app.css"]
```

`app.html` currently owns the shop, orders, and modal templates; `app.ts` owns their state and interactions. The routes file is not yet used for separate pages.

## Store and checkout workflow

```mermaid
sequenceDiagram
  actor Customer
  participant Angular as Angular App
  participant Client as StoreApiService
  participant API as Talabat API
  participant Cache as Redis
  participant DB as SQL Server
  participant Stripe

  Customer->>Angular: Open storefront
  Angular->>Client: Load products, brands, types, basket
  Client->>API: GET /api/Product, lookup and basket endpoints
  API->>Cache: Read/write cached catalog data
  API->>DB: Query catalog when needed
  API-->>Angular: Catalog data
  Customer->>Angular: Search/filter, add product
  Angular->>Client: Save basket
  Client->>API: POST /api/Basket
  API->>Cache: Persist customer basket
  Customer->>Angular: Sign in and checkout
  Angular->>Client: Save delivery choice, create payment intent
  Client->>API: POST /api/PaymentIntent/{basketId}
  API->>Cache: Load basket and delivery selection
  API->>Stripe: Create/update payment intent
  Stripe-->>Angular: Payment client secret
  Customer->>Stripe: Confirm payment securely
  Angular->>Client: Create order after payment succeeds
  Client->>API: POST /api/Order
  API->>DB: Save order and items
  API->>Cache: Delete completed basket
```

## Redis cache in VS Code

Install the recommended **Redis for VS Code** extension (`redis.redis-for-vscode`). Create a connection to the Redis server configured in `ConnectionStrings:Redis` (the local default is `localhost:6379`). The extension browses Redis keys and data types directly; use the app's cache diagnostics endpoint to inspect the keys the API reports. Keep connection passwords out of workspace settings and committed files.

The recommended **Markdown Mermaid** extension (`bierner.markdown-mermaid`) renders the diagrams in VS Code Markdown preview. Open this file and run **Markdown: Open Preview**.
