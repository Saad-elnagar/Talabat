# Talabat Angular storefront

This Angular 21 application is the frontend for the Talabat API. The backend is in `../../Backend`; architecture and workflow diagrams are in `../../docs/architecture.md`.

## Run locally

From this directory:

```bash
npm ci
npm start
```

The development server runs at `http://localhost:4200` and proxies `/api` and `/Images` to the API at `http://127.0.0.1:5080`. Start the API from the repository root:

```bash
dotnet run --project Backend/Talabat.Api/Talabat.Api.csproj --urls http://127.0.0.1:5080
```

## Run with Docker Compose

From the repository root:

```bash
docker compose up --build -d
```

Open `http://127.0.0.1:4200`. Stop the app containers with:

```bash
docker compose down
```

The Compose file reads the API's existing .NET User Secrets file read-only. SQL Server and Redis must already be running and reachable using the configured connection strings.

## VS Code

Install recommended workspace extensions to get Angular template support, Mermaid previews, and a Redis browser. Connect the Redis extension to the same Redis instance configured for the API.
