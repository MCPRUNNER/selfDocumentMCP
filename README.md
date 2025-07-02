# selfDocumentMCP

A Model Context Protocol (MCP) Server that generates documentation from git logs, designed to be used as a Copilot Agent in VS Code.

## ⚠️ Important Setup Note

To ensure clean JSON-RPC communication, the MCP server should be run with:

- Pre-built binaries (`--no-build` flag)
- Production environment (`DOTNET_ENVIRONMENT=Production`)
- Quiet verbosity (`--verbosity quiet`)

This prevents build messages and logging output from interfering with the JSON-RPC protocol.

## Features

This MCP server provides tools for:

- **generate_git_documentation**: Generate documentation from git logs for the current workspace
- **generate_git_documentation_to_file**: Generate documentation from git logs and write to a file
- **compare_branches_documentation**: Generate documentation comparing differences between two branches
- **compare_commits_documentation**: Generate documentation comparing differences between two commits

## Output Formats

The server supports multiple output formats:

- **Markdown** (default): Human-readable markdown format
- **HTML**: Rich HTML format with styling
- **Text**: Plain text format

## Installation and Setup

### Prerequisites

- .NET 9.0 SDK
- Git repository in the workspace
- VS Code with Copilot

### Building the Project

```powershell
dotnet restore; dotnet build
```

### Running the MCP Server

```powershell
dotnet run
```

## VS Code Integration

### MCP Configuration

Create or update your MCP configuration to include this server. The example `mcp.json` file shows how to configure the server:

```json
{
  "mcpServers": {
    "selfDocumentMCP": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/selfDocumentMCP.csproj"],
      "env": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Using with Copilot

Once configured, you can use the following tools through Copilot:

1. **Generate Documentation**: Creates documentation from recent git commits
2. **Save Documentation**: Generates and saves documentation to a specified file
3. **Compare Branches**: Documents differences between two git branches
4. **Compare Commits**: Documents differences between two specific commits

## Tool Parameters

### generate_git_documentation

- `maxCommits` (optional): Maximum number of commits to include (default: 50)
- `outputFormat` (optional): Output format: markdown, html, or text (default: markdown)

### generate_git_documentation_to_file

- `filePath` (required): Path where to save the documentation file
- `maxCommits` (optional): Maximum number of commits to include (default: 50)
- `outputFormat` (optional): Output format: markdown, html, or text (default: markdown)

### compare_branches_documentation

- `branch1` (required): First branch name
- `branch2` (required): Second branch name
- `filePath` (required): Path where to save the documentation file
- `outputFormat` (optional): Output format: markdown, html, or text (default: markdown)

### compare_commits_documentation

- `commit1` (required): First commit hash
- `commit2` (required): Second commit hash
- `filePath` (required): Path where to save the documentation file
- `outputFormat` (optional): Output format: markdown, html, or text (default: markdown)

## Configuration

The server uses standard .NET configuration:

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SelfDocumentMCP": "Debug"
    }
  },
  "SelfDocumentMCP": {
    "DefaultMaxCommits": 50,
    "DefaultOutputFormat": "markdown",
    "SupportedFormats": ["markdown", "html", "text"]
  }
}
```

## Architecture

The project is structured as follows:

- **Models/**: JSON-RPC and MCP data models
- **Services/**: Core business logic
  - `GitService`: Handles git operations and documentation generation
  - `McpServer`: Implements the MCP JSON-RPC 2.0 protocol
- **Program.cs**: Application entry point and dependency injection setup

## Dependencies

- **LibGit2Sharp**: For git operations
- **Microsoft.Extensions.\*\*\***: For logging, configuration, and dependency injection
- **System.Text.Json**: For JSON serialization

## Development

### Logging

The application uses structured logging with file output to avoid interfering with JSON-RPC communication:

- **Production**: Logs written to `logs/selfdocumentmcp.log` at Information level
- **Development**: Logs written to `logs/selfdocumentmcp-dev.log` at Debug level
- **Log Configuration**: Configurable via `appsettings.json` and environment-specific files
- **Log Rotation**: Daily log files with automatic cleanup (via Serilog.Extensions.Logging.File)

The logs directory is automatically created and ignored by git (.gitignore).

- Production: Information level
- Development: Debug/Trace level

### Error Handling

All tools include comprehensive error handling and return appropriate error responses when operations fail.

## Troubleshooting

### "Failed to parse message" warnings

If you see warnings like:

```
[warning] Failed to parse message: "Using launch settings from..."
[warning] Failed to parse message: "Building..."
[warning] Failed to parse message: "info: Program[0]"
```

This indicates that build messages or logging output is interfering with JSON-RPC communication. To fix:

1. **Build the project first**:

   ```powershell
   dotnet build --configuration Release
   ```

2. **Use the correct MCP configuration**:

   ```json
   {
     "servers": {
       "selfDocumentMCP": {
         "type": "stdio",
         "command": "dotnet",
         "args": [
           "run",
           "--project",
           "path/to/selfDocumentMCP.csproj",
           "--no-build",
           "--verbosity",
           "quiet"
         ],
         "env": {
           "DOTNET_ENVIRONMENT": "Production"
         }
       }
     }
   }
   ```

3. **Verify clean output** by testing manually:

   ```powershell
   $env:DOTNET_ENVIRONMENT="Production"
   echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | dotnet run --no-build --verbosity quiet
   ```

   You should see only JSON output, no log messages.

## License

This project is open source. Please refer to the license file for details.

## Contributing

Contributions are welcome! Please follow standard .NET coding practices and ensure all changes include appropriate tests and documentation.
