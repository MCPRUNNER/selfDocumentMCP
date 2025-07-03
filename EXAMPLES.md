# MCP Integration Examples

This document shows comprehensive examples of how to integrate and use the GitVisionMCP server with VS Code and Copilot, including **advanced commit search capabilities** and remote branch features.

## 🔥 Featured: Advanced Commit Search

The latest enhancement includes powerful commit search functionality that revolutionizes repository analysis:

- **Deep Search**: Search through commit messages AND file contents simultaneously
- **Comprehensive Results**: Get commit details, file locations, line numbers, and content
- **Performance Optimized**: Smart filtering and configurable search depth
- **Rich Output**: Detailed markdown reports with match summaries and statistics

### Quick Search Examples

```bash
@copilot Search all commits for "authentication" and show detailed results
@copilot Find commits mentioning "bug fix" in messages or code
@copilot Look for "TODO" comments across entire commit history
@copilot Search for "deprecated" functions with line details
```

## VS Code Configuration

### 1. Production MCP Server Configuration (Recommended)

Add this to your VS Code MCP configuration file:

```json
{
  "mcpServers": {
    "GitVisionMCP": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:\\path\\to\\GitVisionMCP\\GitVisionMCP.csproj",
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
    "GitVisionMCP": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:\\path\\to\\GitVisionMCP\\GitVisionMCP.csproj"
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

### 🔥 Commit and Code Search (Advanced)

**Basic Search Commands:**

```
@copilot Search all commits for "authentication" to find related changes
@copilot Find all commits that mention "bug fix" in messages or code
@copilot Search for "deprecated" across all commit history
@copilot Look for "TODO" comments in all commits and show me where they are
@copilot Find commits containing "API" and show detailed line matches
```

**Advanced Search Use Cases:**

```
@copilot Search last 50 commits for "HttpClient" usage with detailed line info
@copilot Find all instances of "password" in commit history for security audit
@copilot Search for "Exception" in messages and code to track error handling
@copilot Look for "database" references across development history
@copilot Find commits mentioning "performance" with full context details
```

**Targeted Search Examples:**

```
@copilot Search recent 25 commits for "refactor" to see code improvements
@copilot Find all commits with "config" changes in last 100 commits
@copilot Search for "test" additions across entire project history
@copilot Look for "security" mentions with line-by-line details
```

**Results Include:**

- Commit hash, author, and timestamp
- Exact file names containing matches
- Line numbers and full line content
- Summary statistics (commits searched, matches found)
- File-by-file breakdown with detailed context

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

#### 🔥 Advanced Commit Search Use Cases

**Bug Tracking & Debugging:**

```
@copilot Search for "NullReferenceException" across all commits to track bug fixes
@copilot Find commits mentioning "crash" or "error" in the last 50 commits
@copilot Search for "memory leak" references in commit history
@copilot Look for "timeout" issues across development timeline
```

**Security & Compliance:**

```
@copilot Search for "password" or "secret" across all commits for security audit
@copilot Find all commits mentioning "encryption" or "decrypt"
@copilot Search for "API key" references in commit messages and code
@copilot Look for "vulnerability" mentions across project history
```

**Feature Development Analysis:**

```
@copilot Search for "user authentication" to trace feature development
@copilot Find all commits related to "payment processing" functionality
@copilot Search for "database migration" references with detailed context
@copilot Look for "performance optimization" across development history
```

**Code Quality & Maintenance:**

```
@copilot Search for "TODO" comments across all commits to find pending work
@copilot Find "deprecated" function usage across commit history
@copilot Search for "refactor" mentions to track code improvements
@copilot Look for "test" additions to understand testing evolution
```

**Integration & Dependencies:**

```
@copilot Search for "third-party" or "library" references in commits
@copilot Find all commits mentioning specific package names like "React" or "jQuery"
@copilot Search for "configuration" changes across project timeline
@copilot Look for "environment" or "deployment" related commits
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

#### 🔥 Search Commits for String (Advanced Search)

**Basic Search Example:**

```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "method": "tools/call",
  "params": {
    "name": "search_commits_for_string",
    "arguments": {
      "searchString": "authentication",
      "maxCommits": 50
    }
  }
}
```

**Advanced Search Examples:**

```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "method": "tools/call",
  "params": {
    "name": "search_commits_for_string",
    "arguments": {
      "searchString": "bug fix",
      "maxCommits": 100
    }
  }
}
```

**Security Audit Search:**

```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "method": "tools/call",
  "params": {
    "name": "search_commits_for_string",
    "arguments": {
      "searchString": "password",
      "maxCommits": 200
    }
  }
}
```

**Expected Response Format:**

- Search summary (commits searched, matches found, total line matches)
- Per-commit details (hash, author, timestamp, message)
- File-by-file breakdown (file names, match counts)
- Line-level details (line numbers, full content, exact matches)

#### Get Changed Files Between Commits

```json
{
  "jsonrpc": "2.0",
  "id": 9,
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
  "id": 10,
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
      "name": "GitVisionMCP",
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
        Write-host "✗ FAIL - Expected pattern not found" -ForegroundColor Red
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
.\test-mcp.ps1 -ProjectPath "path\to\GitVisionMCP.csproj"
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

7. **🔍 Search Tool Issues**

   - **Error**: "Search timeout" or "Too many results"
   - **Solutions**:
     - Reduce `maxCommits` parameter (try 25-50 for large repos)
     - Use more specific search terms to narrow results
     - Avoid very common words like "the", "and", "if"
   - **Performance Tips**:
     - Start with recent commits: `maxCommits: 25`
     - Use specific technical terms for better targeting
     - Large repositories may take 10-30 seconds for comprehensive searches

8. **Search Result Interpretation**
   - **Issue**: "Too many or too few results"
   - **Solutions**:
     - Search is case-insensitive by default
     - Searches both commit messages AND file contents
     - Try variations of search terms (e.g., "auth", "authentication", "login")
     - Use quotation marks in natural language: `"user authentication"`

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

Check logs in `logs/gitvisionmcp-dev.log` for detailed error information.

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

## 🚀 Search Optimization Guide

### Best Practices for Commit Search

**1. Search Term Selection:**

- Use specific technical terms: `"HttpClient"`, `"database"`, `"authentication"`
- Avoid very common words: `"the"`, `"and"`, `"for"`, `"in"`
- Try variations: `"auth"`, `"login"`, `"signin"`, `"authentication"`
- Use compound terms: `"user authentication"`, `"bug fix"`, `"performance optimization"`

**2. Performance Optimization:**

```bash
# Fast interactive search (immediate feedback)
@copilot Search last 25 commits for "API" changes

# Comprehensive analysis (thorough investigation)
@copilot Search all commits for "security" with maxCommits 150

# Targeted search (specific timeframe)
@copilot Search recent 50 commits for "database migration"
```

**3. Result Interpretation:**

- **High match count**: Consider more specific search terms
- **Low match count**: Try broader terms or synonyms
- **No results**: Verify spelling, try related terms
- **Performance issues**: Reduce maxCommits or use more specific terms

**4. Advanced Search Strategies:**

```bash
# Multi-phase search approach
@copilot Search for "login" in last 30 commits  # Quick overview
@copilot Search for "authentication" in last 100 commits  # Detailed analysis

# Combine with other tools for context
@copilot Search for "refactor" then show file changes between found commits
@copilot Search for "bug" then get recent commits for timeline context
```

### Search Use Case Templates

**Bug Investigation:**

```
1. @copilot Search for "[bug description]" in recent commits
2. @copilot Get detailed diff between commits found in search
3. @copilot List files changed between bug introduction and fix
```

**Feature Development Tracking:**

```
1. @copilot Search for "[feature name]" across all commits
2. @copilot Compare first and last commits mentioning feature
3. @copilot Generate documentation covering feature development
```

**Security Audit Workflow:**

```
1. @copilot Search for "password" or "secret" across all commits
2. @copilot Search for "authentication" and "authorization"
3. @copilot Search for "encryption" and security-related terms
4. @copilot Generate comprehensive security audit report
```
