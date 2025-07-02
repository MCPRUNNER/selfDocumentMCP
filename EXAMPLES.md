# MCP Integration Examples

This document shows how to integrate and use the selfDocumentMCP server with VS Code and Copilot.

## VS Code Configuration

### 1. MCP Server Configuration

Add this to your VS Code settings or MCP configuration file:

```json
{
  "mcpServers": {
    "selfDocumentMCP": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:\\path\\to\\selfDocumentMCP\\selfDocumentMCP.csproj"
      ],
      "env": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### 2. Using with Copilot

Once configured, you can ask Copilot to:

#### Generate Git Documentation

```
@copilot Generate documentation from the last 20 git commits
```

#### Save Documentation to File

```
@copilot Generate git documentation and save it to docs/changes.md
```

#### Compare Branches

```
@copilot Compare changes between main and feature-branch and save to docs/branch-diff.md
```

#### Compare Commits

```
@copilot Compare changes between commit abc123 and def456 and save to docs/commit-diff.md
```

## Manual Testing

### JSON-RPC Examples

#### Initialize the Server

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {
    "protocolVersion": "2024-11-05",
    "capabilities": {
      "roots": {
        "listChanged": true
      }
    },
    "clientInfo": {
      "name": "vscode",
      "version": "1.0.0"
    }
  }
}
```

#### List Available Tools

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/list",
  "params": {}
}
```

#### Generate Documentation

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "generate_git_documentation",
    "arguments": {
      "maxCommits": 10,
      "outputFormat": "markdown"
    }
  }
}
```

#### Save Documentation to File

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "tools/call",
  "params": {
    "name": "generate_git_documentation_to_file",
    "arguments": {
      "filePath": "docs/git-history.md",
      "maxCommits": 25,
      "outputFormat": "markdown"
    }
  }
}
```

#### Compare Branches

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "method": "tools/call",
  "params": {
    "name": "compare_branches_documentation",
    "arguments": {
      "branch1": "main",
      "branch2": "feature-branch",
      "filePath": "docs/branch-comparison.md",
      "outputFormat": "html"
    }
  }
}
```

#### Compare Commits

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "method": "tools/call",
  "params": {
    "name": "compare_commits_documentation",
    "arguments": {
      "commit1": "abc123def",
      "commit2": "456ghi789",
      "filePath": "docs/commit-comparison.txt",
      "outputFormat": "text"
    }
  }
}
```

## Expected Responses

### Initialize Response

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "protocolVersion": "2024-11-05",
    "capabilities": {
      "tools": {},
      "logging": {}
    },
    "serverInfo": {
      "name": "selfDocumentMCP",
      "version": "1.0.0"
    }
  }
}
```

### Tools List Response

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": {
    "tools": [
      {
        "name": "generate_git_documentation",
        "description": "Generate documentation from git logs for the current workspace",
        "inputSchema": {
          "type": "object",
          "properties": {
            "maxCommits": {
              "type": "integer",
              "description": "Maximum number of commits to include (default: 50)"
            },
            "outputFormat": {
              "type": "string",
              "description": "Output format: markdown, html, or text (default: markdown)"
            }
          }
        }
      }
      // ... other tools
    ]
  }
}
```

### Tool Call Response

````json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "# Git Commit Documentation\n\nGenerated on: 2025-07-02 10:30:00\nTotal commits: 10\n\n## Commit: abc12345\n\n**Author:** John Doe <john@example.com>\n**Date:** 2025-07-02 09:15:00\n\n**Message:**\n```\nAdd MCP server implementation\n\n- Implemented JSON-RPC 2.0 protocol\n- Added git documentation tools\n- Created VS Code integration\n```\n\n**Changed Files:**\n- Program.cs\n- Services/McpServer.cs\n- Models/McpModels.cs\n\n**Changes:**\n- Modified: Program.cs\n- Added: Services/McpServer.cs\n- Added: Models/McpModels.cs\n\n---\n\n..."
      }
    ],
    "isError": false
  }
}
````

## Troubleshooting

### Common Issues

1. **Git Repository Not Found**: Ensure you're running the MCP server from within a git repository
2. **Branch Not Found**: Verify branch names exist using `git branch -a`
3. **Commit Hash Invalid**: Use `git log --oneline` to find valid commit hashes
4. **File Path Issues**: Use absolute paths or ensure relative paths are correct
5. **Permissions**: Ensure the server has write permissions to the target directory

### Debugging

Enable debug logging by setting `DOTNET_ENVIRONMENT=Development` and check the console output for detailed error messages.

### Testing the Server

1. **Manual Testing**: Use PowerShell script `test-mcp.ps1`
2. **VS Code Integration**: Configure MCP and test through Copilot
3. **Direct JSON-RPC**: Send JSON requests via stdin to the running server

## Output Examples

### Markdown Format (Default)

- Clean, readable documentation
- Formatted with headers, code blocks, and lists
- Suitable for README files and documentation sites

### HTML Format

- Rich formatting with CSS styling
- Suitable for web viewing and reporting
- Professional appearance with proper HTML structure

### Text Format

- Plain text output
- Good for logs and simple documentation
- No formatting, just raw information

## Best Practices

1. **Commit Limits**: Use reasonable maxCommits values (10-100) to avoid overwhelming output
2. **File Organization**: Save documentation to dedicated docs/ folders
3. **Format Selection**: Choose format based on intended use:
   - Markdown for documentation
   - HTML for reports and web viewing
   - Text for logs and simple output
4. **Branch/Commit Selection**: Use meaningful branch names and recent commits for better documentation
