# Quick Setup Guide for selfDocumentMCP

A comprehensive Model Context Protocol (MCP) Server with advanced git analysis and remote branch support.

## Step 1: Build the Project

```powershell
cd "c:\Users\U00001\source\repos\MCP\selfDocumentMCP"
dotnet build --configuration Release
```

## Step 2: Test MCP Communication

```powershell
# Set environment and test initialize
$env:DOTNET_ENVIRONMENT="Production"
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0.0"}}}' | dotnet run --no-build --verbosity quiet
```

**Expected output** (clean JSON only):

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

## Step 3: Configure VS Code MCP

Update your VS Code MCP configuration file (`.vscode/mcp.json` or similar):

```json
{
  "servers": {
    "selfDocumentMCP": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:\\Users\\U00001\\source\\repos\\MCP\\selfDocumentMCP\\selfDocumentMCP.csproj",
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

## Step 4: Restart VS Code

After updating the MCP configuration, restart VS Code completely for the changes to take effect.

## Step 5: Test with Copilot

Try asking Copilot about the enhanced capabilities:

### Basic Documentation

- "Generate documentation from git logs"
- "Show me the available MCP tools"
- "Create git documentation and save it to docs/changelog.md"

### Remote Branch Features (🆕 New)

- "Compare my feature branch with origin/main"
- "List all remote branches in this repository"
- "Fetch from origin and compare branches"
- "Show me recent commits with detailed information"

### Advanced Analysis

- "Compare two commits and show me what files changed"
- "Get comprehensive diff information between commits"
- "Analyze differences between release and main branches"

## Available Tools (13 Total)

The MCP server provides comprehensive git analysis tools:

### Core Documentation (2)

- `generate_git_documentation` - Generate docs from git logs
- `generate_git_documentation_to_file` - Save docs to file

### Branch Operations (6)

- `compare_branches_documentation` - Compare local branches
- `compare_branches_with_remote` - 🆕 Compare with remote support
- `get_local_branches` - 🆕 List local branches
- `get_remote_branches` - 🆕 List remote branches
- `get_all_branches` - 🆕 List all branches
- `fetch_from_remote` - 🆕 Fetch from remote

### Commit Analysis (5)

- `compare_commits_documentation` - Compare commits
- `get_recent_commits` - 🆕 Get recent commits
- `get_changed_files_between_commits` - 🆕 List changed files
- `get_detailed_diff_between_commits` - 🆕 Detailed diffs
- `get_commit_diff_info` - 🆕 Comprehensive diff stats

## Verification Commands

### Test tools list:

```powershell
$env:DOTNET_ENVIRONMENT="Production"
echo '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}' | dotnet run --no-build --verbosity quiet
```

### Test git documentation generation:

```powershell
$env:DOTNET_ENVIRONMENT="Production"
echo '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"generate_git_documentation","arguments":{"maxCommits":5}}}' | dotnet run --no-build --verbosity quiet
```

## Common Issues

❌ **If you see build messages**: Use `--no-build` and build first
❌ **If you see log messages**: Use `DOTNET_ENVIRONMENT=Production`
❌ **If VS Code can't connect**: Check the project path in the configuration
❌ **If git operations fail**: Ensure you're in a git repository

✅ **Success indicators**:

- Clean JSON-only output from test commands
- No "Failed to parse message" warnings in VS Code
- Copilot can list and use the MCP tools
