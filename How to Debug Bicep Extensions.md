# How to Debug Bicep Extensions

A comprehensive guide for developers on locally debugging Bicep extensions using gRPC reflection and HTTP mode.

---

## Overview

Bicep extensions expose a gRPC-based API that allows the Bicep runtime to interact with custom resource types. During development, you can run your extension in **HTTP mode** to enable direct interaction with the gRPC endpoints using tools like [grpcurl](https://github.com/fullstorydev/grpcurl). This guide covers the available endpoints, request/response contracts, and step-by-step debugging instructions.

---

## Prerequisites

Before getting started, ensure you have the following installed:

- **.NET 9 SDK** or later
- **grpcurl** – Install from [grpcurl releases](https://github.com/fullstorydev/grpcurl/releases) or via package managers:
  - **Windows (Chocolatey):** `choco install grpcurl`
  - **macOS (Homebrew):** `brew install grpcurl`
  - **Linux:** Download binary from releases
- **Bicep CLI** version 0.37.4 or later
- Your Bicep extension project

---

## Starting Your Extension in HTTP Mode

To enable HTTP-based gRPC communication for debugging, start your extension with the `--http` flag:

```powershell
# Windows PowerShell
dotnet run --project .\MyExtension.csproj -- --http 5001
```

```bash
# macOS/Linux
dotnet run --project ./MyExtension.csproj -- --http 5001
```

### Startup Options

Your extension supports three communication modes:

| Option | Description | Example |
|--------|-------------|---------|
| `--http <port>` | TCP port (1-65535) for HTTP/2 gRPC | `--http 5001` |
| `--socket <path>` | Unix domain socket path | `--socket /tmp/bicep-ext.sock` |
| `--pipe <name>` | Named pipe for Windows IPC | `--pipe bicep-extension-pipe` |

> **Note:** Only one option can be specified at a time.

---

## Enabling gRPC Reflection

gRPC reflection is automatically enabled when your extension runs in **Development mode**. Set the `ASPNETCORE_ENVIRONMENT` environment variable before starting:

```powershell
# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\MyExtension.csproj -- --http 5001
```

```bash
# macOS/Linux
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project ./MyExtension.csproj -- --http 5001
```

### Enabling Trace Logging

For detailed request/response logging, enable tracing:

```powershell
# Windows PowerShell
$env:BICEP_TRACING_ENABLED = $true
```

```bash
# macOS/Linux
export BICEP_TRACING_ENABLED=true
```

---

## Configuring Your IDE for Debugging

### Visual Studio Configuration

1. **Create or update `Properties/launchSettings.json`** in your extension project:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "Debug HTTP Mode": {
      "commandName": "Project",
      "commandLineArgs": "--http 5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "BICEP_TRACING_ENABLED": "true"
      }
    },
    "Debug Named Pipe": {
      "commandName": "Project",
      "commandLineArgs": "--pipe bicep-ext-debug",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "BICEP_TRACING_ENABLED": "true"
      }
    },
    "Debug Unix Socket": {
      "commandName": "Project",
      "commandLineArgs": "--socket /tmp/bicep-ext-debug.sock",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "BICEP_TRACING_ENABLED": "true"
      }
    }
  }
}
```

2. **Select the debug profile**:
   - In Visual Studio, use the dropdown next to the Start button to select "Debug HTTP Mode"
   - Press **F5** to start debugging

### Visual Studio Code Configuration

1. **Create or update `.vscode/launch.json`** in your project root:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Debug Extension (HTTP)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net9.0/MyExtension.dll",
      "args": ["--http", "5001"],
      "cwd": "${workspaceFolder}",
      "console": "integratedTerminal",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "BICEP_TRACING_ENABLED": "true"
      }
    },
    {
      "name": "Debug Extension (Named Pipe)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net9.0/MyExtension.dll",
      "args": ["--pipe", "bicep-ext-debug"],
      "cwd": "${workspaceFolder}",
      "console": "integratedTerminal",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "BICEP_TRACING_ENABLED": "true"
      }
    },
    {
      "name": "Debug Extension (Unix Socket)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net9.0/MyExtension.dll",
      "args": ["--socket", "/tmp/bicep-ext-debug.sock"],
      "cwd": "${workspaceFolder}",
      "console": "integratedTerminal",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "BICEP_TRACING_ENABLED": "true"
      }
    },
    {
      "name": "Attach to Extension Process",
      "type": "coreclr",
      "request": "attach",
      "processName": "MyExtension"
    }
  ]
}
```

2. **Create or update `.vscode/tasks.json`** for the build task:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/MyExtension.csproj",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

3. **Start debugging**:
   - Open the Run and Debug panel (**Ctrl+Shift+D** / **Cmd+Shift+D**)
   - Select "Debug Extension (HTTP)" from the dropdown
   - Press **F5** to start debugging

---

## Debugging Workflow

1. **Start the debugger** (F5) with the HTTP profile selected
2. **Set breakpoints** in your handler methods
3. **Send a request** using grpcurl from a terminal:

```bash
grpcurl -plaintext -d '{
  "type": "MyResource",
  "properties": "{\"name\": \"test\", \"operation\": \"Uppercase\"}",
  "config": "{}"
}' localhost:5001 extension.BicepExtension/CreateOrUpdate
```

1. **Step through the code** using:
   - **F10** - Step Over
   - **F11** - Step Into
   - **Shift+F11** - Step Out
   - **F5** - Continue

2. **Inspect variables** in the debugger:
   - `request.Type` - The resource type being requested
   - `request.Properties` - The JSON properties string
   - `request.Config` - Extension configuration
   - `context.CancellationToken` - For timeout handling

---

## Using grpcurl for Debugging

Once your extension is running in HTTP mode with reflection enabled, you can interact with it using grpcurl.

### Discover Available Services

```bash
grpcurl -plaintext localhost:5001 list
```

**Expected output:**

```
extension.BicepExtension
grpc.reflection.v1alpha.ServerReflection
```

### Describe the BicepExtension Service

```bash
grpcurl -plaintext localhost:5001 describe extension.BicepExtension
```

**Expected output:**

```protobuf
extension.BicepExtension is a service:
service BicepExtension {
  rpc CreateOrUpdate ( .extension.ResourceSpecification ) returns ( .extension.LocalExtensibilityOperationResponse );
  rpc Delete ( .extension.ResourceReference ) returns ( .extension.LocalExtensibilityOperationResponse );
  rpc Get ( .extension.ResourceReference ) returns ( .extension.LocalExtensibilityOperationResponse );
  rpc GetTypeFiles ( .extension.Empty ) returns ( .extension.TypeFilesResponse );
  rpc Ping ( .extension.Empty ) returns ( .extension.Empty );
  rpc Preview ( .extension.ResourceSpecification ) returns ( .extension.LocalExtensibilityOperationResponse );
}
```

---

## gRPC Service Contract

The Bicep extension service is defined by the following protobuf contract:

```protobuf
syntax = "proto3";

option csharp_namespace = "Bicep.Local.Rpc";

package extension;

service BicepExtension {
  rpc CreateOrUpdate (ResourceSpecification) returns (LocalExtensibilityOperationResponse);
  rpc Preview (ResourceSpecification) returns (LocalExtensibilityOperationResponse);
  rpc Get (ResourceReference) returns (LocalExtensibilityOperationResponse);
  rpc Delete (ResourceReference) returns (LocalExtensibilityOperationResponse);
  rpc GetTypeFiles(Empty) returns (TypeFilesResponse);
  rpc Ping(Empty) returns (Empty);
}
```

### Message Types

#### ResourceSpecification

Used for `CreateOrUpdate` and `Preview` operations:

```protobuf
message ResourceSpecification {
  optional string config = 1;      // Extension configuration (JSON)
  string type = 2;                 // Resource type name
  optional string apiVersion = 3;  // API version (optional)
  string properties = 4;           // Resource properties (JSON)
}
```

#### ResourceReference

Used for `Get` and `Delete` operations:

```protobuf
message ResourceReference {
  string identifiers = 1;          // Resource identifiers (JSON)
  optional string config = 2;      // Extension configuration (JSON)
  string type = 3;                 // Resource type name
  optional string apiVersion = 4;  // API version (optional)
}
```

#### LocalExtensibilityOperationResponse

Returned by all resource operations:

```protobuf
message LocalExtensibilityOperationResponse {
  optional Resource resource = 1;  // Successful resource data
  optional ErrorData errorData = 2; // Error information (if failed)
}

message Resource {
  string type = 1;
  optional string apiVersion = 2;
  string identifiers = 3;          // Resource identifiers (JSON)
  string properties = 4;           // Resource properties (JSON)
  optional string status = 5;
}

message ErrorData {
  Error error = 1;
}

message Error {
  string code = 1;
  optional string target = 2;
  string message = 3;
  repeated ErrorDetail details = 4;
  optional string innerError = 5;
}

message ErrorDetail {
  string code = 1;
  optional string target = 2;
  string message = 3;
}
```

#### TypeFilesResponse

Returned by `GetTypeFiles`:

```protobuf
message TypeFilesResponse {
  string indexFile = 1;            // Type index content
  map<string, string> typeFiles = 2; // Type definition files
}
```

---

## Endpoint Reference & Examples

### 1. Ping

Health check endpoint to verify the extension is running.

```bash
grpcurl -plaintext localhost:5001 extension.BicepExtension/Ping
```

**Expected output:**

```json
{}
```

---

### 2. GetTypeFiles

Retrieves type definitions exposed by the extension.

```bash
grpcurl -plaintext localhost:5001 extension.BicepExtension/GetTypeFiles
```

**Expected output:**

```json
{}
```

---

### 3. CreateOrUpdate

Creates or updates a resource.

```bash
grpcurl -plaintext -d '{
  "type": "echo",
  "properties": "{\"payload\": \"Hello, World!\"}",
  "config": "{}"
}' localhost:5001 extension.BicepExtension/CreateOrUpdate
```

**Expected output:**

```json
{
  "resource": {
    "type": "echo",
    "identifiers": "{}",
    "properties": "{\"payload\":\"Hello, World!\"}"
  }
}
```

#### Example with API Version

```bash
grpcurl -plaintext -d '{
  "type": "MyResource",
  "apiVersion": "2024-01-01",
  "properties": "{\"name\": \"test-resource\", \"operation\": \"Uppercase\"}",
  "config": "{}"
}' localhost:5001 extension.BicepExtension/CreateOrUpdate
```

---

### 4. Preview

Previews what a resource deployment would look like without actually deploying.

```bash
grpcurl -plaintext -d '{
  "type": "MyResource",
  "properties": "{\"name\": \"preview-test\", \"operation\": \"Reverse\"}",
  "config": "{}"
}' localhost:5001 extension.BicepExtension/Preview
```

**Expected output:**

```json
{
  "resource": {
    "type": "MyResource",
    "identifiers": "{\"name\":\"preview-test\"}",
    "properties": "{\"name\":\"preview-test\",\"operation\":\"Reverse\"}"
  }
}
```

---

### 5. Get

Retrieves an existing resource by its identifiers.

```bash
grpcurl -plaintext -d '{
  "type": "MyResource",
  "identifiers": "{\"name\": \"my-resource-name\"}",
  "config": "{}"
}' localhost:5001 extension.BicepExtension/Get
```

**Expected response (if implemented):**

```json
{
  "resource": {
    "type": "MyResource",
    "identifiers": "{\"name\":\"my-resource-name\"}",
    "properties": "{...}"
  }
}
```

**Error response (if not implemented):**

```json
{
  "errorData": {
    "error": {
      "code": "NotImplemented",
      "message": "Operation 'MyResourceHandler.Get' has not been implemented."
    }
  }
}
```

---

### 6. Delete

Deletes a resource by its identifiers.

```bash
grpcurl -plaintext -d '{
  "type": "MyResource",
  "identifiers": "{\"name\": \"resource-to-delete\"}",
  "config": "{}"
}' localhost:5001 extension.BicepExtension/Delete
```

---

## Common Debugging Scenarios

### Scenario 1: Handler Not Registered

If you invoke an endpoint for a resource type that doesn't have a registered handler:

```bash
grpcurl -plaintext -d '{
  "type": "UnknownType",
  "properties": "{}",
  "config": "{}"
}' localhost:5001 extension.BicepExtension/CreateOrUpdate
```

**Response:**

```json
{
  "errorData": {
    "error": {
      "code": "HandlerNotRegistered",
      "message": "No handler registered for type 'UnknownType'."
    }
  }
}
```

### Scenario 2: Describe Message Types

To understand the structure of request/response messages:

```bash
# Describe ResourceSpecification
grpcurl -plaintext localhost:5001 describe extension.ResourceSpecification

# Describe LocalExtensibilityOperationResponse
grpcurl -plaintext localhost:5001 describe extension.LocalExtensibilityOperationResponse
```

### Scenario 3: JSON Property Formatting

When sending JSON strings within grpcurl, ensure proper escaping:

```bash
# Properties and identifiers are JSON strings, so inner quotes must be escaped
grpcurl -plaintext -d '{
  "type": "MyResource",
  "properties": "{\"name\": \"test\", \"nested\": {\"key\": \"value\"}}",
  "config": "{}"
}' localhost:5001 extension.BicepExtension/CreateOrUpdate
```

---

## Quick Reference Card

| Action | Command |
|--------|---------|
| List services | `grpcurl -plaintext localhost:5001 list` |
| Describe service | `grpcurl -plaintext localhost:5001 describe extension.BicepExtension` |
| Ping | `grpcurl -plaintext localhost:5001 extension.BicepExtension/Ping` |
| Get types | `grpcurl -plaintext localhost:5001 extension.BicepExtension/GetTypeFiles` |
| CreateOrUpdate | `grpcurl -plaintext -d '{"type":"...", "properties":"...", "config":"{}"}' localhost:5001 extension.BicepExtension/CreateOrUpdate` |
| Preview | `grpcurl -plaintext -d '{"type":"...", "properties":"...", "config":"{}"}' localhost:5001 extension.BicepExtension/Preview` |
| Get | `grpcurl -plaintext -d '{"type":"...", "identifiers":"...", "config":"{}"}' localhost:5001 extension.BicepExtension/Get` |
| Delete | `grpcurl -plaintext -d '{"type":"...", "identifiers":"...", "config":"{}"}' localhost:5001 extension.BicepExtension/Delete` |

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Failed to dial target host" | Ensure extension is running and port is correct |
| "reflection: rpc error: code = Unimplemented" | Set `ASPNETCORE_ENVIRONMENT=Development` |
| "No handler registered for type" | Verify resource type name matches your handler registration |
| Malformed JSON in properties | Ensure proper escaping of quotes in JSON strings |
| Connection refused | Check if another process is using the port |
| Breakpoints not hitting | Ensure you're running in Debug configuration, not Release |
| launchSettings.json not recognized | Ensure file is in `Properties/` folder for Visual Studio |

---

## Additional Resources

- [Bicep Local Deploy Documentation](https://github.com/Azure/bicep/blob/main/docs/experimental/local-deploy.md)
- [Creating a Local Extension with .NET](https://github.com/Azure/bicep/blob/main/docs/experimental/local-deploy-dotnet-quickstart.md)
- [gRPC Contract Definition](https://github.com/Azure/bicep/blob/main/src/Bicep.Local.Rpc/extension.proto)
- [grpcurl Documentation](https://github.com/fullstorydev/grpcurl#readme)

---

*For questions or issues, please file an issue on the [Bicep GitHub repository](https://github.com/Azure/bicep/issues).*