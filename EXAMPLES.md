# MCP Integration Examples

This document shows comprehensive examples of how to integrate and use the selfDocumentMCP server with VS Code and Copilot, including the new remote branch features.

## VS Code Configuration

### 1. Production MCP Server Configuration (Recommended)

Add this to your VS Code MCP configuration file:

```json
{
  "mcpServers": {
    "selfDocumentMCP": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:\\path\\to\\selfDocumentMCP\\selfDocumentMCP.csproj",
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

### 2. Development Configuration

For development and debugging:

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

## Using with Copilot

Once configured, you can use natural language commands with Copilot:

### Core Documentation Commands

#### Generate Git Documentation

```
@copilot Generate documentation from the last 20 git commits
@copilot Create a summary of recent changes in HTML format
@copilot Show me the git history for the past 50 commits
```

#### Save Documentation to File

```
@copilot Generate git documentation and save it to docs/project-history.md
@copilot Create a change log from the last 30 commits and save to CHANGELOG.md
@copilot Export git history to docs/development-summary.html in HTML format
```

### Branch Comparison Commands

#### Local Branch Comparison

```
@copilot Compare changes between main and feature-branch and save to docs/branch-diff.md
@copilot Show differences between dev and release branches
@copilot Compare my current branch with main and save the analysis
```

#### Remote Branch Comparison (🆕 New Features)

```
@copilot Compare my feature branch with origin/main and save to analysis.md
@copilot Show differences between origin/release/v2.0 and origin/main
@copilot Compare local main with origin/main to check if we're synchronized
@copilot Fetch from origin and compare branches with remote support
```

### Git Analysis Commands

#### Recent Commits Analysis

```
@copilot Show me the last 10 commits with details
@copilot Get recent commit information for code review
@copilot List the most recent commits in this repository
```

#### File Change Analysis

```
@copilot Show me what files changed between these two commits: abc123 and def456
@copilot List all files modified between commit hashes
@copilot Get detailed diff between two specific commits
```

#### Branch Discovery

```
@copilot List all branches in this repository
@copilot Show me all remote branches
@copilot What local branches do we have?
@copilot Fetch latest changes from origin
```

### Advanced Use Cases

#### Release Planning

```
@copilot Compare release/v2.0 with main and create release notes
@copilot Analyze differences between our release branch and main
@copilot Generate documentation for the upcoming release
```

#### Code Review Preparation

```
@copilot Compare my feature/user-auth branch with origin/main for code review
@copilot Show me comprehensive diff information between two commits
@copilot Prepare documentation for pull request review
```

#### Team Synchronization

```
@copilot Fetch from origin and compare main with origin/main
@copilot Check if our local branches are up to date with remote
@copilot Compare upstream/main with our fork's main branch
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

### Core Documentation Tools

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

### Branch Comparison Tools

#### Compare Local Branches

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

#### Compare with Remote Branches (🆕 New)

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "method": "tools/call",
  "params": {
    "name": "compare_branches_with_remote",
    "arguments": {
      "branch1": "feature/user-auth",
      "branch2": "origin/main",
      "filePath": "docs/remote-comparison.md",
      "outputFormat": "markdown",
      "fetchRemote": true
    }
  }
}
```

### Git Analysis Tools

#### Get Recent Commits

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "method": "tools/call",
  "params": {
    "name": "get_recent_commits",
    "arguments": {
      "count": 15
    }
  }
}
```

#### Get Changed Files Between Commits

```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "method": "tools/call",
  "params": {
    "name": "get_changed_files_between_commits",
    "arguments": {
      "commit1": "abc123def",
      "commit2": "456ghi789"
    }
  }
}
```

#### Get Detailed Diff

```json
{
  "jsonrpc": "2.0",
  "id": 9,
  "method": "tools/call",
  "params": {
    "name": "get_detailed_diff_between_commits",
    "arguments": {
      "commit1": "abc123def",
      "commit2": "456ghi789",
      "specificFiles": ["Program.cs", "Services/GitService.cs"]
    }
  }
}
```

#### Get Comprehensive Diff Info

```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "method": "tools/call",
  "params": {
    "name": "get_commit_diff_info",
    "arguments": {
      "commit1": "abc123def",
      "commit2": "456ghi789"
    }
  }
}
```

#### Get File Line Diff

```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "method": "tools/call",
  "params": {
    "name": "get_file_line_diff_between_commits",
    "arguments": {
      "commit1": "abc123def",
      "commit2": "456ghi789",
      "filePath": "Services/GitService.cs"
    }
  }
}
```

### Branch Discovery Tools

#### Get All Branches

```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "method": "tools/call",
  "params": {
    "name": "get_all_branches",
    "arguments": {}
  }
}
```

#### Get Local Branches Only

```json
{
  "jsonrpc": "2.0",
  "id": 12,
  "method": "tools/call",
  "params": {
    "name": "get_local_branches",
    "arguments": {}
  }
}
```

#### Get Remote Branches Only

```json
{
  "jsonrpc": "2.0",
  "id": 13,
  "method": "tools/call",
  "params": {
    "name": "get_remote_branches",
    "arguments": {}
  }
}
```

#### Fetch from Remote

```json
{
  "jsonrpc": "2.0",
  "id": 14,
  "method": "tools/call",
  "params": {
    "name": "fetch_from_remote",
    "arguments": {
      "remoteName": "origin"
    }
  }
}
```

### Commit Comparison

#### Compare Commits

```json
{
  "jsonrpc": "2.0",
  "id": 15,
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

### Tools List Response (Partial)

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
      },
      {
        "name": "compare_branches_with_remote",
        "description": "Generate documentation comparing differences between two branches with remote support",
        "inputSchema": {
          "type": "object",
          "properties": {
            "branch1": {
              "type": "string",
              "description": "First branch name (can be local or remote, e.g., 'main' or 'origin/main')"
            },
            "branch2": {
              "type": "string",
              "description": "Second branch name (can be local or remote, e.g., 'feature/xyz' or 'origin/feature/xyz')"
            },
            "filePath": {
              "type": "string",
              "description": "Path where to save the documentation file"
            },
            "outputFormat": {
              "type": "string",
              "description": "Output format: markdown, html, or text (default: markdown)"
            },
            "fetchRemote": {
              "type": "boolean",
              "description": "Whether to fetch from remote before comparison (default: true)"
            }
          },
          "required": ["branch1", "branch2", "filePath"]
        }
      }
      // ... 11 more tools
    ]
  }
}
```

### Git Documentation Response

````json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "# Git Commit Documentation\n\nGenerated on: 2025-01-15 10:30:00\nTotal commits: 10\n\n## Commit: abc12345\n\n**Author:** John Doe <john@example.com>\n**Date:** 2025-01-15 09:15:00\n\n**Message:**\n```\nAdd remote branch support to MCP server\n\n- Implemented remote branch discovery\n- Added fetch from remote functionality\n- Enhanced branch comparison with remote support\n- Updated documentation and examples\n```\n\n**Changed Files:**\n- Services/GitService.cs\n- Services/McpServer.cs\n- README.md\n- EXAMPLES.md\n\n**Changes:**\n- Modified: Services/GitService.cs (+150, -25)\n- Modified: Services/McpServer.cs (+75, -10)\n- Modified: README.md (+200, -50)\n- Modified: EXAMPLES.md (+100, -20)\n\n---\n\n..."
      }
    ],
    "isError": false
  }
}
````

### Branch List Response

```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "# All Branches\n\n## Local Branches\n- main (current)\n- feature/user-auth\n- bugfix/login-error\n- dev\n\n## Remote Branches\n- origin/main\n- origin/dev\n- origin/release/v1.0\n- upstream/main\n- upstream/dev\n\n## Branch Summary\n- Total Local: 4\n- Total Remote: 5\n- Current Branch: main\n- Default Remote: origin"
      }
    ],
    "isError": false
  }
}
```

### Commit Diff Info Response

```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "# Commit Diff Analysis\n\n**From:** abc123def\n**To:** 456ghi789\n**Date Range:** 2025-01-10 to 2025-01-15\n\n## Summary\n- **Files Changed:** 8\n- **Total Insertions:** 245\n- **Total Deletions:** 67\n- **Net Change:** +178 lines\n\n## File Changes\n\n### Services/GitService.cs\n- **Status:** Modified\n- **Insertions:** 150\n- **Deletions:** 25\n- **Net:** +125\n\n### Services/McpServer.cs\n- **Status:** Modified\n- **Insertions:** 75\n- **Deletions:** 10\n- **Net:** +65\n\n### README.md\n- **Status:** Modified\n- **Insertions:** 20\n- **Deletions:** 32\n- **Net:** -12\n\n## Change Categories\n- **New Features:** 3 files\n- **Bug Fixes:** 2 files\n- **Documentation:** 3 files\n- **Tests:** 0 files"
      }
    ],
    "isError": false
  }
}
```

## PowerShell Testing Script

Create a `test-mcp.ps1` file for comprehensive testing:

```powershell
# Test MCP Server Functionality
param(
    [string]$ProjectPath = ".",
    [string]$Environment = "Production"
)

$env:DOTNET_ENVIRONMENT = $Environment

function Test-JsonRpc {
    param([string]$Message, [string]$ExpectedPattern = "")

    Write-Host "Testing: $Message" -ForegroundColor Cyan
    $response = $Message | dotnet run --project $ProjectPath --no-build --verbosity quiet

    if ($ExpectedPattern -and $response -match $ExpectedPattern) {
        Write-Host "✓ PASS" -ForegroundColor Green
    } elseif (-not $ExpectedPattern) {
        Write-Host "Response: $response" -ForegroundColor Yellow
    } else {
        Write-Host "✗ FAIL - Expected pattern not found" -ForegroundColor Red
    }
    Write-Host ""
}

# Test initialization
Test-JsonRpc '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' '"serverInfo"'

# Test tools list
Test-JsonRpc '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}' '"tools"'

# Test git documentation
Test-JsonRpc '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"generate_git_documentation","arguments":{"maxCommits":5}}}' '"content"'

# Test branch listing
Test-JsonRpc '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"get_all_branches","arguments":{}}}' '"content"'

# Test recent commits
Test-JsonRpc '{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"name":"get_recent_commits","arguments":{"count":3}}}' '"content"'

Write-Host "Testing complete!" -ForegroundColor Green
```

Run the test script:

```powershell
.\test-mcp.ps1 -ProjectPath "path\to\selfDocumentMCP.csproj"
```

## Troubleshooting

### Common Issues

1. **Git Repository Not Found**

   - **Error**: "Repository not found" or "Not a git repository"
   - **Solution**: Ensure you're running the MCP server from within a git repository
   - **Check**: `git status` should work in the current directory

2. **Branch Not Found**

   - **Error**: "Branch 'xyz' not found"
   - **Solution**: Verify branch names using the branch discovery tools
   - **Check**: Use `get_all_branches` tool to see available branches

3. **Remote Branch Access Issues**

   - **Error**: "Remote branch not accessible" or "Authentication failed"
   - **Solutions**:
     - Verify remote access: `git remote -v`
     - Check authentication (SSH keys or personal access tokens)
     - Use `fetch_from_remote` tool to update remote references

4. **Commit Hash Invalid**

   - **Error**: "Invalid commit hash" or "Commit not found"
   - **Solution**: Use `get_recent_commits` tool to find valid commit hashes
   - **Check**: `git log --oneline -10` for recent commits

5. **File Path Issues**

   - **Error**: "Access denied" or "Path not found"
   - **Solutions**:
     - Use absolute paths or ensure relative paths are correct
     - Check write permissions to the target directory
     - Create target directories if they don't exist

6. **JSON-RPC Communication Issues**
   - **Error**: "Failed to parse message" warnings
   - **Solutions**:
     - Build project first: `dotnet build --configuration Release`
     - Use production environment: `DOTNET_ENVIRONMENT=Production`
     - Add `--no-build --verbosity quiet` flags

### Performance Issues

1. **Slow Response with Large Repositories**

   - **Issue**: Timeouts or very slow responses
   - **Solutions**:
     - Use smaller `maxCommits` values (10-50 instead of 100+)
     - Specify `specificFiles` when getting detailed diffs
     - Avoid comparing very old commits

2. **Memory Usage**
   - **Issue**: High memory consumption
   - **Solutions**:
     - Limit commit count in documentation generation
     - Use text format instead of HTML for large outputs
     - Restart the MCP server periodically for long-running sessions

### Debugging

#### Enable Debug Logging

```json
{
  "env": {
    "DOTNET_ENVIRONMENT": "Development"
  }
}
```

Check logs in `logs/selfdocumentmcp-dev.log` for detailed error information.

#### Manual Testing Commands

Test basic connectivity:

```powershell
$env:DOTNET_ENVIRONMENT="Production"
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | dotnet run --no-build --verbosity quiet
```

Test tool availability:

```powershell
echo '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}' | dotnet run --no-build --verbosity quiet
```

#### VS Code Integration Testing

1. **Check MCP Configuration**: Ensure the path to the project is correct
2. **Restart VS Code**: After changing MCP configuration
3. **Check Copilot**: Verify Copilot can see the MCP tools
4. **Test Simple Commands**: Start with basic documentation generation

## Output Examples

### Markdown Format (Default)

- Clean, readable documentation with headers and code blocks
- Suitable for README files, GitHub wikis, and documentation sites
- Includes tables for structured data and proper code highlighting

### HTML Format

- Rich formatting with CSS styling and professional appearance
- Interactive elements and proper HTML structure
- Suitable for web viewing, reports, and presentations
- Includes responsive design for different screen sizes

### Text Format

- Plain text output without formatting
- Good for logs, simple documentation, and integration with other tools
- Raw information without markup or styling
- Useful for automated processing and parsing

## Best Practices

### Repository Management

1. **Keep Remotes Updated**: Regularly fetch from remote repositories
2. **Use Descriptive Branch Names**: Follow team conventions (feature/, bugfix/, hotfix/)
3. **Clean Commit Messages**: Write clear, descriptive commit messages

### Documentation Generation

1. **Reasonable Limits**: Use appropriate `maxCommits` values (10-100)
2. **Organized Output**: Save documentation to dedicated folders (docs/, analysis/)
3. **Format Selection**: Choose format based on intended use:
   - Markdown for documentation and README files
   - HTML for reports and web viewing
   - Text for logs and automated processing

### Remote Branch Usage

1. **Fetch Before Comparing**: Use `fetchRemote: true` for up-to-date comparisons
2. **Meaningful Comparisons**: Compare related branches (feature vs main, release vs main)
3. **Clear Naming**: Use consistent remote naming (origin, upstream, fork)

### Performance Optimization

1. **Incremental Analysis**: Start with small commit ranges, expand as needed
2. **Specific File Analysis**: Use file filters when analyzing large changes
3. **Batch Operations**: Group related analysis tasks together

### Integration with Workflow

1. **Pre-commit Analysis**: Review changes before committing
2. **Pull Request Preparation**: Document changes for code review
3. **Release Planning**: Compare release branches with main for release notes
4. **Team Collaboration**: Share analysis results with team members

## Advanced Examples

### Automated Release Notes

```json
{
  "name": "compare_branches_with_remote",
  "arguments": {
    "branch1": "release/v2.0",
    "branch2": "origin/main",
    "filePath": "docs/release-v2.0-notes.md",
    "outputFormat": "markdown",
    "fetchRemote": true
  }
}
```

### Code Review Documentation

```json
{
  "name": "get_commit_diff_info",
  "arguments": {
    "commit1": "feature-start-commit",
    "commit2": "feature-end-commit"
  }
}
```

### Cross-Repository Analysis

```json
{
  "name": "compare_branches_with_remote",
  "arguments": {
    "branch1": "origin/main",
    "branch2": "upstream/main",
    "filePath": "docs/upstream-sync-analysis.md"
  }
}
```

This comprehensive documentation should help you make the most of the selfDocumentMCP server's capabilities, especially the new remote branch features!
