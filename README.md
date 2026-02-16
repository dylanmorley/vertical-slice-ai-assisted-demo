# Vertical Slice Architecture Demo

A reference implementation demonstrating **Vertical Slice Architecture** optimized for **AI-assisted engineering** with .NET 10, React 19, and Aspire 13.1 orchestration.


## 🎯 Purpose

This solution serves as an example of how to structure applications using Vertical Slice Architecture in a way that maximizes the effectiveness of AI-powered development tools like GitHub Copilot.

### Why Vertical Slice Architecture + AI?

**Vertical Slice Architecture** organizes code by feature rather than technical layer. Each feature (slice) contains everything it needs - endpoints, queries, commands, models - in a single, self-contained folder. This approach is **ideal for AI-assisted development** because:

- **Context Locality**: All related code lives together, making it easier for AI to understand feature scope
- **Pattern Recognition**: Consistent structure across features helps AI suggest accurate implementations
- **Reduced Coupling**: Independent slices minimize unintended side effects when AI generates code
- **Discoverability**: Clear folder structure enables AI to quickly locate relevant code examples
- **Incremental Growth**: New features can be added without modifying existing slices

## 🏗️ Architecture Overview

### Backend (.NET 10 Web API)
```
EndpointHandling/Endpoints/
├── Audits/
│   ├── GettingAudits/
│   │   ├── Endpoint.cs           # Minimal API endpoint definition
│   │   ├── Query.cs              # Query with validation
│   │   └── QueryHandler.cs       # Query execution logic
│   └── Data/
│       ├── Queries/              # Shared queries
│       └── Commands/             # Shared commands
```

**Key Patterns:**
- **Minimal APIs**: Clean, focused endpoint definitions
- **CQRS**: Separate queries and commands for clarity
- **Primary Constructors**: C# 12 syntax for concise models
- **Telemetry**: OpenTelemetry integration for observability
- **Vertical Slices**: Each feature in its own folder with everything it needs

### Frontend (React 19 + TypeScript + Vite)
```
src/
├── views/
│   ├── dashboard/Dashboard.tsx   # Home view
│   └── administration/
│       └── AuditAdmin.tsx        # Audit log view
├── api/                          # Orval-generated TypeScript client
│   ├── client.ts
│   ├── useVerticalSliceClient.ts # React hook with auto-auth
│   └── generated/                # Auto-generated from OpenAPI
├── components/                   # Reusable UI components
└── contexts/                     # Auth & API contexts
```

**Key Features:**
- **Type-Safe API Client**: Auto-generated from OpenAPI specification
- **Automatic Authentication**: Auth0 integration with token management
- **Simplified Structure**: Minimal views for reference purposes

### Orchestration (.NET Aspire 13.1)
Single command to start both frontend and backend:
```bash
dotnet run --project src/VerticalSlice.AppHost
```

Aspire provides:
- Unified dashboard for monitoring
- Service discovery and configuration
- Telemetry aggregation (OpenTelemetry)
- Environment management

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- Visual Studio 2022 / VS Code / Rider

### Quick Start
```bash
# Clone and navigate
cd src/VerticalSlice.AppHost

# Run everything (API + UI + Aspire Dashboard)
dotnet run

# Access:
# - Aspire Dashboard: http://localhost:15000 (check console output)
# - API: http://localhost:5000/swagger
# - UI: http://localhost:5173
```

### Development Workflow

**Backend Only:**
```bash
cd src/VerticalSlice.Web.Api
dotnet watch run
```

**Frontend Only:**
```bash
cd src/clientapp
npm install
npm start
```

**Run Tests:**
```bash
dotnet test
```

## 📁 Project Structure

```
src/
├── VerticalSlice.AppHost/        # Aspire orchestration
├── VerticalSlice.Web.Api/        # Main Web API
│   ├── EndpointHandling/
│   │   └── Endpoints/            # Vertical slices by feature
│   ├── Model/                    # Domain models
│   ├── Data/                     # DbContext and seeding
│   ├── Telemetry/                # OpenTelemetry configuration
│   └── openapi.json              # Auto-generated OpenAPI spec
├── VerticalSlice.Web.Api.Tests/  # Unit & integration tests (NUnit)
├── VerticalSlice.E2E.Tests/      # End-to-end tests (Playwright)
└── clientapp/                    # React 19 + TypeScript + Vite
    └── src/
        ├── api/                  # Orval-generated API client
        ├── views/                # Page components
        └── components/           # Reusable components
```

## 🎨 AI-Assisted Development Features

### 1. **Consistent Patterns**
Every vertical slice follows the same structure:
- `Endpoint.cs` - Route definition
- `Query.cs` or `Command.cs` - Request/response contracts
- `QueryHandler.cs` or `CommandHandler.cs` - Business logic

This consistency helps AI tools suggest accurate code based on existing examples.

### 2. **Self-Documenting Code**
- Minimal comments (let code explain itself)
- Clear naming conventions
- Type-safe contracts
- OpenAPI documentation auto-generated from code

### 3. **Copilot Instructions**
The `.github/copilot-instructions.md` file provides context to GitHub Copilot about:
- Architecture decisions and patterns
- Code organization principles
- Technology stack details
- Common workflows

### 4. **Type-Safe API Client**
Orval generates a fully-typed TypeScript client from the OpenAPI spec:
```typescript
import { useVerticalSliceClient } from '@/api'

function MyComponent() {
  const apiClient = useVerticalSliceClient() // Auto-authenticated!
  
  const response = await apiClient.audit.getRecords({
    page: 1,
    pageSize: 20
  })
  // response is fully typed!
}
```

## 🛠️ Technology Stack

### Backend
- **.NET 10** - Latest .NET with Native AOT support
- **Minimal APIs** - Lightweight endpoint definitions
- **Entity Framework Core 10** - SQLite for demo purposes
- **OpenTelemetry** - Distributed tracing and metrics
- **NUnit** - Unit and integration testing

### Frontend
- **React 19** - Latest React with compiler
- **TypeScript** - Type safety throughout
- **Vite** - Fast build tooling
- **CoreUI** - Professional UI components
- **Orval** - OpenAPI TypeScript code generation
- **Auth0** - Authentication provider

### DevOps
- **.NET Aspire 13.1** - Orchestration and observability
- **GitHub Copilot** - AI pair programming
- **Playwright** - End-to-end testing

## 📝 Adding a New Feature (Vertical Slice)

Once you've pulled the repo, here's a prompt to get you going. Use your tooling of choice, let's create an entirely new API and some screens in a single shot!

```plaintext
We’re generating new screens to model geography data in the system  - we need a new screen to display a list of geographies, and one that allows us to perform adds and updates.

1.	Build new endpoints that returns a paginated list of geography data, as well as 'get single geography by id', and 'add or update a geography'
2.	Add an Geography entity to the Entity Framework context and seed data following existing patterns. This should have name, short code, geo codes, and other useful data about geography informatioon
3.	Implement new CQRS query, command and handlers to handle paginated data for the list view as well as getting single items or updating items
4.	Create a new response object for the API Follow existing OpenAPI patterns.
5.	Build the project to generate the OpenAPI spec and client.
6.	Create the new React views using the Orval-generated methods.
7.	Apply existing CoreUI styling patterns to create appealing screens for listing and editing geographies
8.	Add navigation and breadcrumb plumbing to access the screens
9.	Add Web.Api tests following existing patterns to verify the API returns expected data
```

The AI and automated generation should wire everything up - you'll have a fairly large number of files created after running the prompt - but all hyper localised on 'listing and maintaining geography data'. You can quickly see how this pattern will allow you to add more and more functionality, while keeping quality high through testing the overall API architecture with automated data-seeding baked in.


### Backend
```bash
# 1. Create feature folder
mkdir -p src/VerticalSlice.Web.Api/EndpointHandling/Endpoints/MyFeature/GettingData

# 2. Add Endpoint.cs
# 3. Add Query.cs and QueryHandler.cs
# 4. Register in Configuration.cs

# 5. Build to regenerate OpenAPI spec
dotnet build
```

### Frontend
```bash
# 1. Regenerate API client from updated OpenAPI spec
cd src/clientapp
npm run generate-client

# 2. Create view component
# 3. Add route to routes.tsx
# 4. Add navigation item to _nav.tsx
```

The vertical slice pattern ensures new features are added **without modifying existing code**, reducing merge conflicts and making it easier for AI to suggest complete implementations.

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/VerticalSlice.Web.Api.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run E2E tests (requires Playwright)
dotnet test src/VerticalSlice.E2E.Tests
```

## 📊 Observability

Built-in OpenTelemetry instrumentation provides:
- **Distributed Tracing**: Track requests across services
- **Metrics**: Performance counters and custom metrics
- **Logs**: Structured logging with correlation

View in Aspire Dashboard or export to your preferred backend (Jaeger, Zipkin, Application Insights, etc.)

## 🔐 Configuration

### Backend (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "VerticalSliceDatabase": "Data Source=verticalslice.db"
  },
  "Auth0": {
    "Domain": "your-domain.auth0.com",
    "Audience": "https://vertical-slice.com/"
  }
}
```

## Using docker

Run this in the repo root to build the application

```bash
docker build -t vertical-slice .
```

Then, you can run that with

```bash
docker run -p 8080:8080 vertical-slice
```

That runs the dotnet application, serving the react app and API on the same port, with the api routing via /api/ and all other paths routing to the client react router

## 🤝 Contributing

This is a reference implementation. Feel free to:
- Clone and adapt for your own projects
- Submit issues with suggestions
- Share your own AI-assisted development patterns

## 📚 Learn More

- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [GitHub Copilot Best Practices](https://github.blog/developer-skills/github/how-to-use-github-copilot-for-explaining-code/)
- [Minimal APIs in .NET](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

## 📄 License

MIT License - feel free to use this as a template for your own projects.

---

**Built with ❤️ to demonstrate how Vertical Slice Architecture and AI-assisted development work better together.**
