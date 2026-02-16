# GitHub Copilot Instructions for Vertical Slice Demo

This repository is a reference implementation demonstrating Vertical Slice Architecture optimized for AI-assisted development using .NET 10, React 19, and Aspire orchestration. These instructions give GitHub Copilot (and other AI tools) the repository-level context and coding conventions to produce consistent, maintainable suggestions.

## Purpose

Provide concise guidance about the project's architecture, conventions, and common workflows so Copilot can generate code that matches the repository's patterns and style.

## High-level Architecture

- Backend: Minimal APIs (.NET 10) organized by vertical slices under `EndpointHandling/Endpoints/`.
- Frontend: React 19 + TypeScript + Vite with an Orval-generated TypeScript client under `src/api` or `src/services/apiClient`.
- Orchestration: Aspire hosts the API and UI during development via the AppHost project.

## Key Patterns & Conventions

- Vertical Slices: Each feature contains its `Endpoint.cs`, request/response contract (`Query.cs` / `Command.cs`), and handler (`QueryHandler.cs` / `CommandHandler.cs`). Keep feature code co-located.
- Minimal Endpoints: Endpoints should be thin—map routes and forward to handler/query/command logic. No direct DbContext logic in endpoint files.
- CQRS: Prefer separating queries and commands for clarity. If you feel the need to add a service, consider whether it should instead be a command/query + handler.
- Primary Constructors: Use C# primary constructor syntax for simple domain models where appropriate (e.g., `public class MyModel(string name)`).
- Nullable Reference Types: Project-wide nullable annotations are enabled—handle nulls explicitly.
- Keep comments minimal: Prefer clear naming and expressive code; use XML comments only for public API surfaces that require explanation.

## Frontend Conventions

- TypeScript: Strict settings are enabled; prefer typed props and return types.
- Env vars: Use `import.meta.env.VITE_*` for Vite environment variables (do not use `process.env`).
- API Client: Use the repo's hook (e.g., `useVerticalSliceClient()` or the client in `src/api`) that injects auth tokens and returns typed endpoints.
- Component structure: Keep view-level components under `src/views` and reusable UI under `src/components`.
- API client. Use the Orval-generated client for API calls to ensure consistency with backend contracts.


## Building and testing

ALWAYS build and test your changes, your work isn't complete until it compiles and passes tests. Use the provided commands in the "Common Commands" section below.

## Common Commands

Start everything (AppHost / API / UI):

```bash
dotnet run --project src/VerticalSlice.AppHost
```

Backend only:

```bash
cd src/VerticalSlice.Web.Api
dotnet watch run
```

Frontend only:

```bash
cd src/clientapp
npm install
npm start
```

Run tests:

```bash
dotnet test
```

Regenerate OpenAPI (build will produce `openapi.json`):

```bash
dotnet build
```

Regenerate TypeScript client (frontend):

```bash
cd src/clientapp
npm run generate-client
```

## Example: Minimal Endpoint (pattern)

Location: `EndpointHandling/Endpoints/FeatureName/GettingItems/Endpoint.cs`

```csharp
internal static class GetItemsEndpoint
{
    internal static IEndpointRouteBuilder UseGetItemsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet($"{ApiInfo.BasePath}/items",
                async (GetItemsHandler handler, CancellationToken ct) =>
                {
                    var query = new GetItemsQuery(); 
                    var result = await handler.Handle(query, ct);
                    return Results.Ok(result);
                })
            .Produces<ItemDto[]>()
            .WithName("GetItems")
            .WithOpenApi();

        return endpoints;
    }
}
```

Notes:
- Endpoints should only orchestrate: validate route parameters, forward to the query/command handler, and map result to an appropriate HTTP response.
- Business logic and data access belong in handler classes or domain services invoked by handlers.

## Testing

- Backend: NUnit is used for unit and integration tests. Use `WebApplicationFactory` for integration tests where needed.
- Frontend: Jest + React Testing Library (or Vitest) for component tests.

## Observability & Telemetry

- The project uses OpenTelemetry for traces and metrics. Keep instrumentation consistent with existing telemetry patterns in `Telemetry/`.

## When generating code with Copilot

- Match existing folder structure and naming conventions.
- Keep endpoints thin and delegate logic to handlers.
- Use typed DTOs and contracts found under `Contracts/` or `Model/` where applicable.
- Prefer small, focused commits and include tests for new behavior.

## Where to look for examples

- `EndpointHandling/Endpoints/` — many small feature slices that serve as canonical examples.
- `src/clientapp/src/api` or `src/clientapp/src/services/apiClient` — how the TypeScript client is used.
- `Program.cs` and `ServiceRegistration.cs` — how endpoints and services are registered.

## Contributing notes

- Follow the vertical slice pattern for new features.
- Avoid adding global services unless cross-cutting behavior cannot be expressed via handlers/commands/queries.
- Keep public API surface well-documented with XML comments when required; otherwise prefer self-documenting code.

---

Keep this file up to date as the repository evolves — small, accurate context is far more useful to Copilot than a long, outdated document.
