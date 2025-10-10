# .NET GraphQL Service Archetype

![Latest Release](https://img.shields.io/github/v/release/p6m-archetypes/dotnet-graphql-service-basic.archetype?style=flat-square&label=Latest%20Release&color=blue)

Production-ready archetype for generating modular .NET GraphQL services with Entity Framework Core, flexible persistence options, and modern observability.

## 🎯 What This Generates

This archetype creates a complete, production-ready GraphQL service with:

- **🏗️ Modular Architecture**: Namespace-organized, service-oriented design with separate API, Core, and Persistence layers
- **⚡ Modern .NET Stack**: .NET 8+ with Entity Framework Core
- **🔌 GraphQL API**: Type-safe GraphQL APIs with Hot Chocolate framework for queries, mutations, and subscriptions
- **💾 Flexible Persistence**: Choose from PostgreSQL, MySQL, MSSQL, or no database
- **🐳 Container-Ready**: Docker and Kubernetes deployment manifests
- **📊 Built-in Monitoring**: Health checks and metrics endpoints
- **🧪 Comprehensive Testing**: Unit and integration tests with Testcontainers
- **🔧 Local Development**: Tilt integration for Kubernetes development

## 📦 Generated Project Structure

```
my-shopping-cart-service/
├── src/
│   ├── ShoppingCart.Api/          # GraphQL schema and type definitions
│   ├── ShoppingCart.Core/         # Business logic and domain models
│   ├── ShoppingCart.Persistence/  # Entity Framework data layer
│   └── ShoppingCart.Server/       # GraphQL server implementation
├── tests/
│   ├── ShoppingCart.UnitTests/
│   └── ShoppingCart.IntegrationTests/
├── k8s/                            # Kubernetes manifests
├── Dockerfile
└── docker-compose.yml
```

## 🚀 Quick Start

### Prerequisites

- [Archetect](https://archetect.github.io/) CLI tool
- .NET 8 SDK or later
- Docker Desktop (for containerized development and testing)

### Generate a New Service

```bash
# Using SSH
archetect render git@github.com:p6m-archetypes/dotnet-graphql-service-basic.archetype.git

# Using HTTPS
archetect render https://github.com/p6m-archetypes/dotnet-graphql-service-basic.archetype.git

# Example prompt answers:
# project: Shopping Cart
# suffix: Service
# group-prefix: com.example
# team-name: Platform
# persistence: PostgreSQL
# service-port: 5030
```

### Development Workflow

```bash
cd shopping-cart-service

# 1. Restore dependencies
dotnet restore

# 2. Run tests
dotnet test

# 3. Start the service
dotnet run --project src/ShoppingCart.Server

# 4. Access endpoints
# - GraphQL Playground: http://localhost:5030/graphql
# - GraphQL API: http://localhost:5030/graphql
# - Health Check: http://localhost:5031/health
# - Metrics: http://localhost:5031/metrics
```

## 📋 Configuration Prompts

When rendering the archetype, you'll be prompted for the following values:

| Property | Description | Example | Required |
|----------|-------------|---------|----------|
| `project` | Service domain name used for namespaces and entities | Shopping Cart | Yes |
| `suffix` | Appended to project name for package naming | Service | Yes |
| `group-prefix` | Namespace prefix (reverse domain notation) | com.example | Yes |
| `team-name` | Owning team identifier for artifacts and documentation | Platform | Yes |
| `persistence` | Database type for data persistence | PostgreSQL | Yes |
| `service-port` | Port for GraphQL HTTP traffic | 5030 | Yes |

**Derived Properties:**
- `management-port`: Automatically set to `service-port + 1` for health/metrics endpoints
- `database-port`: Set to 5432 for PostgreSQL-based services
- `debug-port`: Set to `service-port + 9` for debugging

For complete property relationships, see [archetype.yaml](./archetype.yaml).

## ✨ Key Features

### 🏛️ Architecture & Design

- **Modular Structure**: Clean separation of API, Core, Persistence, and Server concerns
- **Domain-Driven Design**: Entity-centric business logic organization
- **Dependency Injection**: Built-in .NET DI container configuration
- **Clean Architecture**: Dependencies flow toward domain core

### 🔧 Technology Stack

- **.NET 8+**: Latest LTS framework with performance improvements
- **Hot Chocolate**: Modern, high-performance GraphQL server for .NET
- **Entity Framework Core**: Modern ORM with migration support and async operations
- **Testcontainers**: Containerized integration testing with real databases
- **Tilt**: Local Kubernetes development workflow with hot reload

### 📊 Observability & Monitoring

- **Health Checks**: Liveness and readiness endpoints for Kubernetes probes
- **Metrics**: Prometheus-compatible metrics endpoint
- **Structured Logging**: Configurable log levels and structured output
- **GraphQL Metrics**: Query performance tracking and monitoring

### 🧪 Testing & Quality

- **Unit Tests**: xUnit test projects for business logic validation
- **Integration Tests**: Full service testing with Testcontainers and real databases
- **GraphQL Testing**: Query and mutation testing with in-memory server
- **Test Coverage**: Configured coverage reporting

### 🚢 DevOps & Deployment

- **Docker**: Multi-stage Dockerfile for optimized production images
- **Kubernetes**: Complete deployment manifests with ConfigMaps and Secrets
- **Tilt**: Hot-reload development in local Kubernetes clusters
- **Artifactory**: Docker image publication configuration included

## 🎯 Use Cases

This archetype is ideal for:

1. **API Gateways**: Building flexible APIs with client-driven query capabilities
2. **Data Aggregation Services**: Services that combine data from multiple sources
3. **Customer-Facing APIs**: Public APIs requiring flexible querying without versioning concerns
4. **Backend-for-Frontend (BFF)**: Tailored APIs for specific frontend applications

## 📚 What's Inside

### Core Components

#### GraphQL Schema & Types
Hot Chocolate-based GraphQL schema with queries, mutations, and type definitions. Includes automatic schema generation from C# classes and resolver configuration.

#### Entity Framework Persistence
Database access layer with migrations, connection pooling, and async operations. Supports PostgreSQL, MySQL, MSSQL, or no persistence for stateless services.

#### Health & Metrics
Built-in health check endpoints for Kubernetes liveness/readiness probes and Prometheus metrics for monitoring query performance and resource usage.

### Development Tools

- **Tilt Configuration**: Auto-reload development in Kubernetes with live updates
- **Docker Compose**: Local development stack with database services
- **GraphQL Playground**: Interactive API exploration and testing

### Configuration Management

- **appsettings.json**: Environment-specific configuration files
- **Environment Variables**: 12-factor app configuration support
- **CLI Arguments**: Runtime configuration overrides
- **Connection Strings**: Secure database connection management

## 🔧 GraphQL-Specific Features

### Schema & Type System

- **Schema-First or Code-First**: Support for both GraphQL development approaches
- **Type Safety**: Strong typing with automatic C# to GraphQL type mapping
- **Custom Scalars**: Support for custom scalar types (DateTime, Decimal, etc.)
- **Documentation**: Automatic API documentation from code comments

### Advanced Capabilities

- **Queries & Mutations**: Full CRUD operations over GraphQL
- **Filtering & Sorting**: Built-in support for filtering and sorting collections
- **Pagination**: Relay-style cursor pagination and offset pagination
- **DataLoader**: Automatic batching and caching to prevent N+1 queries
- **Subscriptions**: Real-time updates with GraphQL subscriptions (WebSocket support)
- **Error Handling**: Structured error responses with custom error codes

### Performance

- **Query Complexity Analysis**: Prevent expensive queries from overloading the service
- **Depth Limiting**: Configurable query depth limits
- **Persisted Queries**: Support for persisted/stored queries for security and performance
- **Response Caching**: Configurable caching strategies

## 📋 Validation & Quality Assurance

Generated services are production-ready and include:

- ✅ Successful .NET build and compilation
- ✅ All unit and integration tests pass
- ✅ Docker image builds successfully
- ✅ Service starts and responds to health checks
- ✅ GraphQL schema is valid and queryable
- ✅ Database migrations execute successfully

Validate your generated service:

```bash
dotnet build
dotnet test
docker build -t my-service .
```

## 🔗 Related Archetypes

- **[.NET gRPC Service](../dotnet-grpc-service-basic.archetype)** - For high-performance RPC communication
- **[.NET REST Service](../dotnet-rest-service-basic.archetype)** - For traditional REST APIs
- **[Python GraphQL Service](../python-graphql-service-uv-basic.archetype)** - Python alternative with FastAPI and Strawberry
- **[Java GraphQL Gateway](../java-spring-boot-graphql-domain-gateway.archetype)** - Java-based GraphQL federation gateway

## 🤝 Contributing

This archetype is actively maintained. For issues or enhancements:

1. Check existing issues in the repository
2. Create detailed bug reports or feature requests
3. Follow the contribution guidelines
4. Test changes thoroughly with all persistence options

## 📄 License

This archetype is released under the MIT License. Generated services inherit this license but can be changed as needed for your organization.

---

**Ready to build production-grade GraphQL services with .NET?** Generate your first service and start building in minutes! 🚀
