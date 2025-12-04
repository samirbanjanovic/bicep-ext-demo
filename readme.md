# Bicep Extensions Development Guide

Build, test, and debug custom Bicep extensions using the `Bicep.Local.Extension` framework.

## What are Bicep Extensions?

Bicep extensions allow you to define custom resource types that integrate with the Bicep deployment model. Extensions expose a gRPC-based API that the Bicep runtime calls to manage your custom resources—enabling infrastructure-as-code for any system, not just Azure.

## Getting Started

### Prerequisites

| Tool | Install |
|------|---------|
| .NET 9 SDK | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Bicep CLI | v0.37.4+ ([install guide](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install)) |
| grpcurl | `choco install grpcurl` (Win) / `brew install grpcurl` (Mac) |
| grpcui | `choco install grpcui` (Win) / `brew install grpcui` (Mac) — optional |

### Quick Start

```bash
# Clone the demo project
git clone https://github.com/samirbanjanovic/bicep-ext-demo.git
cd bicep-ext-demo/src/src

# Build the extension
dotnet build

# Run in debug mode
export ASPNETCORE_ENVIRONMENT=Development
dotnet run -- --http 5001

# Test it (in another terminal)
grpcurl -plaintext localhost:5001 extension.BicepExtension/Ping
```

## Project Structure

```
bicep-ext-demo/
├── README.md              # This file
├── debugging.md           # Debugging guide
├── unittesting.md         # Unit testing guide
└── src/
    └── src/
        ├── Bicep.Extension.Demo.csproj
        ├── Program.cs
        └── Properties/
            └── launchSettings.json
```

## Documentation

### [Debugging Guide](debugging.md)

Complete guide for locally debugging Bicep extensions:

- **IDE Setup** — Visual Studio, VS Code, and CLI configurations
- **gRPC Contract** — Full protobuf service and message definitions
- **Testing Tools** — Using grpcurl (CLI) and grpcui (Web UI)
- **Endpoint Examples** — Copy-paste commands for all RPC methods
- **Troubleshooting** — Common issues and solutions

### [Unit Testing Guide](unittesting.md)

Best practices for writing testable extensions:

- **Dependency Injection** — Making handlers testable
- **Mocking Patterns** — Using Moq with strict behavior
- **Test Structure** — Arrange-Act-Assert with FluentAssertions
- **Data-Driven Tests** — Testing multiple scenarios efficiently
- **Integration Tests** — Testing services with isolated environments

## Extension Architecture

```
┌─────────────────┐     gRPC      ┌─────────────────────┐
│   Bicep CLI     │ ────────────► │  Your Extension     │
│                 │               │                     │
│  - Deploy       │               │  - Handlers         │
│  - Preview      │               │  - Services         │
│  - What-If      │               │  - Type Definitions │
└─────────────────┘               └─────────────────────┘
```

### Key Concepts

| Concept | Description |
|---------|-------------|
| **Handler** | Implements CRUD operations for a resource type |
| **ResourceSpecification** | Request for CreateOrUpdate/Preview operations |
| **ResourceReference** | Request for Get/Delete operations |
| **TypeFiles** | Bicep type definitions exposed by your extension |

## Common Commands

```bash
# Build
dotnet build

# Run with debugging enabled
dotnet run -- --http 5001

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Resources

- [Bicep Local Deploy Documentation](https://github.com/Azure/bicep/blob/main/docs/experimental/local-deploy.md)
- [Creating a Local Extension with .NET](https://github.com/Azure/bicep/blob/main/docs/experimental/local-deploy-dotnet-quickstart.md)
- [gRPC Proto Definition](https://github.com/Azure/bicep/blob/main/src/Bicep.Local.Rpc/extension.proto)
- [Microsoft Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for your changes
4. Submit a pull request

## License

MIT

---

*Questions? File an issue on the [Bicep GitHub repository](https://github.com/Azure/bicep/issues).*