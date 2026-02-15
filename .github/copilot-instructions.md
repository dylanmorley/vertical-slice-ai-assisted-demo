# GitHub Copilot Instructions for Node Guard

Node Guard is an **enterprise risk management and visualization system** that models organizations as interconnected networks of risk-bearing entities (Nodes) and their dependencies (Links). Built with .NET 10 Aspire 13.1 orchestration, a Web API backend using minimal APIs, and a React 19 + TypeScript frontend with CoreUI components.

## Core Domain Model

The system models **risk contagion** through graph relationships:
- **Nodes**: Risk-bearing entities (systems, assets, projects, concepts) with owners, risk scores (Impact × Likelihood), and hierarchical parent-child relationships
- **Links**: Dependencies between nodes with `propagationWeight` that determines risk contagion
- **Organizations**: Groups that own nodes and define access boundaries
- **Node Types**: Categorizations with configurable weights for risk calculations

Example: If "Payment API" (Node A) depends on "Auth Service" (Node B) with `propagationWeight: 0.75`, then 75% of B's risk score propagates to A during contagion analysis.

## Project Architecture

### Backend (.NET 10 Web API)
- **Minimal APIs** with vertical slice architecture - each feature in its own folder
- **Endpoint structure**: `EndpointHandling/Endpoints/{Domain}/{Verb+EntityName}/Endpoint.cs`
- **Registration pattern**: Extension methods like `UseNodeEndpoints()` called from `MapNodeGuardEndpoints()`
- **Service registration**: Domain-specific extension methods in `ServiceRegistration.cs` (e.g., `AddOrganizationServices()`)
- **Primary constructors**: Used throughout models (e.g., `Node(string name)`)
- **Nullable reference types**: Enabled project-wide - always handle null scenarios

### Frontend (React 19 + TypeScript)
- **Recently migrated from JS to TS** - all `.jsx` files converted to `.tsx`
- **Vite build system** - use `import.meta.env.VITE_*` for env vars (NOT `process.env`)
- **Path alias**: `@/` maps to `src/` (configured in tsconfig.json)
- **API client**: Fully-typed TypeScript client at `src/services/apiClient/` with hook-based auth

### Orchestration (.NET Aspire)
- `NodeGuard.AppHost` orchestrates both backend and frontend with a single command
- Frontend configured as npm app with reverse proxy to API
- Start development: `dotnet run --project src/NodeGuard.AppHost`

## Project Structure

```
src/
├── NodeGuard.AppHost/                    # Aspire orchestration - starts everything
├── NodeGuard.Web.Api/
│   ├── EndpointHandling/Endpoints/       # Vertical slice endpoints by domain
│   │   ├── Nodes/                        # GettingNodes/, CreatingNodes/, etc.
│   │   ├── Organizations/
│   │   └── NodeTypes/
│   ├── Model/                            # Domain models (Node.cs, RiskLinkDetail.cs, etc.)
│   ├── Contracts/                        # Objects for API requests/responses, and for async messaging
│   ├── ServiceRegistration.cs            # DI configuration
│   └── Program.cs                        # Minimal API bootstrap
├── NodeGuard.Web.Api.Tests/              # NUnit tests
│   ├── HttpTests/                        # Integration tests
│   └── ModelTests/                       # Unit tests
└── clientapp/                            # React 19 + TypeScript + Vite
    ├── src/
    │   ├── services/apiClient/           # TypeScript API client with types
    │   ├── contexts/                     # UserContext (Auth0) & API contexts
    │   ├── views/                        # Page components (nodes/, analytics/, etc.)
    │   ├── components/                   # Reusable UI components
    │   ├── routes.tsx                    # Route definitions (lazy loaded)
    │   └── _nav.tsx                      # Sidebar navigation config
    └── vite.config.ts                    # Vite configuration with proxy
```

## .NET API Development

### Code Patterns & Conventions

### Important!

Keep comments in code to a minimum. It's OK to add some XML comments for public APIs, but avoid redundant comments that just restate what the code does. Focus on clear, self-explanatory code instead.

We prefer clean code over comment-heavy code.

#### Follow the patterns

You must absolutely follow the established patterns for endpoints models. This ensures consistency across the codebase and makes it easier for other developers to understand and maintain your code.

1) Endpoints must be CLEAN. Absolutely no data context directly in enddpoints, they just define the route and call appropriate query and commands

2) Avoid Services where possible - we follow the CQRS pattern with Queries and Commands. If you find yourself writing a service, consider if it can be refactored into a query or command instead.

3) We follow the vertical slice architecture, so each feature (e.g., GettingNodes, CreatingNodes) gets its own folder under `EndpointHandling/Endpoints/`. This keeps related code together and makes it easier to find.

This allows us to maintain a clean separation of concerns and ensures that our codebase remains organized and maintainable as it grows. We can add new screens, new endpoints, new queries and commands without creating a tangled mess of code.

Generating Code

1) the API defines an OpenAPI spec, and there is a build tool in the csproj that will create it on build - always use this to publish updates to the spec after creating or modifying endpoints

2) The clinet is using orval to generate the TypeScript API client from the OpenAPI spec - this ensures that the client is always up to date with the API and that we have strong typing across the stack.

Always follow these patterns when adding new features to ensure consistency and maintainability of the codebase.


```csharp
// Minimal API endpoint structure (vertical slice)
// Location: EndpointHandling/Endpoints/Nodes/GettingNodes/Endpoint.cs
internal static class GetNodeDetailsEndpoint
{
    internal static IEndpointRouteBuilder UseGetNodesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet($"{ApiInfo.BasePath}/nodes", 
                async (NodeService nodeService, CancellationToken ct) =>
                {
                    var result = await nodeService.GetNodesAsync(ct);
                    return Results.Ok(result);
                })
            .Produces<Node[]>()
            .WithName("GetNodes")
            .WithOpenApi();
        return endpoints;
    }
}

// Domain models use primary constructors and validation
public class Node(string name)
{
    public string Name { get; set; } = name;
    
    public double Impact
    {
        get => _impact;
        set
        {
            if (value is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(value), 
                    "Impact must be between 0 and 100.");
            _impact = value;
        }
    }
    
    // Hierarchical relationships
    public int? ParentId { get; set; }
    [ForeignKey("ParentId")]
    public Node? Parent { get; set; }
    [InverseProperty("Parent")]
    public List<Node> Children { get; set; } = [];
    
    // Risk contagion
    public List<Node> GivesRiskToNodes { get; init; } = [];
    public List<Node> TakesRiskFromNodes { get; init; } = [];
}
```

### Key Development Guidelines
- **Endpoint organization**: Each endpoint gets its own folder with `Endpoint.cs` and supporting files
- **Service registration**: Add domain services via extension methods (see `AddOrganizationServices()` in existing code)
- **Testing**: NUnit for unit tests, integration tests use `TestHost`/`WebApplicationFactory`
- **API versioning**: Base path defined in `ApiInfo.BasePath` (currently `/api/v1`)
- **Error handling**: Global middleware configured in `ExceptionMiddlewareExtensions.cs`

## React Frontend Development

### TypeScript Migration Complete ✅
- All `.jsx` files converted to `.tsx` - **no JSX files remain**
- Strict TypeScript enabled in `tsconfig.json`
- Path alias: `@/` not configured - use relative imports or `src/`
- **CRITICAL**: Use `import.meta.env.VITE_*` for environment variables, NOT `process.env`

### API Client Architecture
The project has a **fully-typed TypeScript API client** with automatic authentication:

```typescript
// ALWAYS use the hook for automatic Auth0 token injection
import { useNodeGuardClient } from '../../services/apiClient';

function MyComponent() {
  const apiClient = useNodeGuardClient(); // Token automatically injected!
  const [nodes, setNodes] = useState<NodeSummary[]>([]);

  useEffect(() => {
    async function loadNodes() {
      try {
        const response = await apiClient.nodes.getAll({
          page: 1,
          pageSize: 20,
          riskLevel: 'High',
          sortBy: 'riskScore',
        });
        setNodes(response.data);
        console.log('Pagination:', response.pagination);
      } catch (error) {
        if (error instanceof ApiError) {
          if (error.isNotFound()) console.log('Not found');
          if (error.isUnauthorized()) console.log('Auth required');
        }
      }
    }
    loadNodes();
  }, [apiClient]);
}
```

**API Client Endpoints**:
- `apiClient.nodes` - Node CRUD, hierarchy, contagion analysis
- `apiClient.riskLinks` - Risk link management
- `apiClient.organizations` - Organization management
- `apiClient.nodeTypes` - Node type definitions
- `apiClient.analytics` - Global risk, trends, critical paths
- `apiClient.users` - User profile and permissions
- `apiClient.communications` - Broadcast messages

All types exported: `NodeSummary`, `NodeDetail`, `CreateNodeRequest`, `PagedResponse`, etc.

### Component Patterns
```typescript
// Lazy-loaded routes (see routes.tsx)
const NodesList = React.lazy(() => import('./views/nodes/NodesList'));

// Protected routes use Auth0 (see protectedRoute.tsx)
<ProtectedRoute>
  <NodesList />
</ProtectedRoute>

// CoreUI components are standard
import { CCard, CCardBody, CButton } from '@coreui/react';
import CIcon from '@coreui/icons-react';
import { cilPlus } from '@coreui/icons';
```

### Authentication Flow
- **UserContext** (`contexts/UserContext.tsx`) wraps Auth0 with helper methods
- `useUser()` hook provides: `isAuthenticated`, `getToken()`, `hasPermission()`, etc.
- `useNodeGuardClient()` automatically consumes UserContext for token injection
- Protected routes redirect to Auth0 login if not authenticated

### Key Files
- `routes.tsx` - Lazy-loaded route definitions
- `_nav.tsx` - Sidebar navigation structure
- `vite.config.ts` - Vite config with `/api` proxy to backend
- `src/services/apiClient/` - Complete API client with documentation

## Development Workflow

### Running the Application
```powershell
# Start both API and frontend with Aspire (RECOMMENDED)
dotnet run --project src/NodeGuard.AppHost

# This starts:
# - Web API with Swagger UI at http://localhost:5000/swagger
# - React frontend at http://localhost:3000
# - Aspire dashboard for monitoring
```

### Frontend-Only Development
```powershell
cd src/clientapp
npm install
npm start
```

### Running Tests
```powershell
# Backend tests
dotnet test src/NodeGuard.Web.Api.Tests
dotnet test src/NodeGuard.Web.Api.ScenarioTests

# Frontend tests
cd src/clientapp
npm test
```

### Environment Configuration
**Backend** (`appsettings.Development.json`):
```json
{
  "Auth0": {
    "Domain": "your-auth0-domain.auth0.com",
    "ClientId": "your-client-id",
    "Audience": "https://api.nodeguard.local"
  }
}
```

**Frontend** (`.env.local`):
```bash
VITE_API_URL=http://localhost:5000/api/v1
VITE_AUTH0_DOMAIN=your-auth0-domain.auth0.com
VITE_AUTH0_CLIENT_ID=your-client-id
VITE_AUTH0_AUDIENCE=https://api.nodeguard.local
```

**CRITICAL**: Frontend uses `import.meta.env.VITE_*`, not `process.env.*`

## Common Patterns & Examples

### Adding a New Minimal API Endpoint
```csharp
// 1. Create folder: EndpointHandling/Endpoints/Nodes/UpdatingNodes/
// 2. Create Endpoint.cs:
using static Results;

internal static class UpdatingNodeEndpoint
{
    internal static IEndpointRouteBuilder UseUpdateNodeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPut($"{ApiInfo.BasePath}/nodes/{{id}}", 
                async (int id, Node node, NodeService service, CancellationToken ct) =>
                {
                    var result = await service.UpdateNodeAsync(id, node, ct);
                    return result is not null ? Ok(result) : NotFound();
                })
            .Produces<Node>()
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateNode")
            .WithOpenApi();
        return endpoints;
    }
}

// 3. Register in Configuration.cs:
app.UseUpdateNodeEndpoint();
```

### Using the API Client in React
```tsx
import { useNodeGuardClient } from '../../services/apiClient'
import type { NodeSummary } from '../../services/apiClient/types'

function NodesList() {
  const apiClient = useNodeGuardClient() // Auto-authenticated!
  const [nodes, setNodes] = useState<NodeSummary[]>([])
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    async function loadNodes() {
      setLoading(true)
      try {
        const response = await apiClient.nodes.getAll({ page: 1, pageSize: 20 })
        setNodes(response.data)
      } catch (error) {
        console.error('Failed to load nodes:', error)
      } finally {
        setLoading(false)
      }
    }
    loadNodes()
  }, [apiClient])

  return (
    <CCard>
      <CCardHeader>Nodes</CCardHeader>
      <CCardBody>
        {loading ? 'Loading...' : nodes.map(node => <div key={node.nodeId}>{node.name}</div>)}
      </CCardBody>
    </CCard>
  )
}
```

### Creating Protected Routes
```tsx
// routes.tsx
import ProtectedRoute from './protectedRoute'

const routes = [
  {
    path: '/nodes',
    element: (
      <ProtectedRoute>
        <NodesList />
      </ProtectedRoute>
    ),
  },
]
```

## Testing Guidelines

### .NET API Testing (NUnit)
```csharp
// ModelTests/NodeTests.cs
public class NodeTests
{
    [Test]
    public void NodeHasName()
    {
        var node = new Node("Test Node");
        Assert.That(node.Name, Is.EqualTo("Test Node"));
    }
    
    [Test]
    public void ImpactMustBeBetween0And100()
    {
        var node = new Node("Test");
        Assert.Throws<ArgumentOutOfRangeException>(() => node.Impact = 101);
    }
}
```

### React Component Testing
```typescript
// Use Jest + React Testing Library (setupTests.js)
import { render, screen } from '@testing-library/react'
import { UserContext } from './contexts/UserContext'

describe('NodesList', () => {
  it('should render nodes', async () => {
    const mockUserContext = {
      getToken: async () => 'test-token',
      isAuthenticated: true,
      isLoading: false,
    }
    
    render(
      <UserContext.Provider value={mockUserContext}>
        <NodesList />
      </UserContext.Provider>
    )
    
    // Assertions...
  })
})
```

## Deployment & Infrastructure

### .NET Aspire Deployment
```csharp
// AppHost/Program.cs - Azure Container Apps configuration
var nodeGuardApi = builder.AddProject<Projects.NodeGuard_Web_Api>("nodeguardapi")
    .PublishAsAzureContainerApp((infra, app) => {
        app.Configuration.Ingress.AllowInsecure = true;
    });

builder.AddNpmApp("nodeguardui", "../clientapp")
    .WithNpmPackageInstallation()
    .WithHttpEndpoint(env: "PORT")
    .WithReverseProxy(nodeGuardApi.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .WithOtlpExporter();
```

### Environment Variables
Backend uses `appsettings.json` / `appsettings.Development.json` for configuration.
Frontend uses Vite environment variables with `VITE_` prefix (e.g., `VITE_API_URL`).

## Critical Gotchas

1. **Frontend env vars**: Must use `import.meta.env.VITE_*` NOT `process.env.*`
2. **Path alias**: `@/` is not configured in vite.config.ts - use relative imports
3. **Primary constructors**: Models use C# 12 primary constructor syntax: `public class Node(string name)`
4. **Minimal APIs**: Endpoints use extension methods pattern, not controllers
5. **Auth0 tokens**: Always use `useNodeGuardClient()` hook in React components for automatic token injection
6. **API base path**: All endpoints prefixed with `ApiInfo.BasePath` (currently `/api/v1`)
7. **Testing framework**: Backend uses NUnit (not xUnit) with `[Test]` attributes

---

When working with this codebase, always consider the full-stack nature of the application and ensure changes are coordinated between the API and frontend components.
