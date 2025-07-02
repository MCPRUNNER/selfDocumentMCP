# selfDocumentMCP Test Script

# This PowerShell script demonstrates how to test the MCP server manually

Write-Host "Testing selfDocumentMCP Server" -ForegroundColor Green

# Test 1: Initialize request
$initRequest = @{
    jsonrpc = "2.0"
    id      = 1
    method  = "initialize"
    params  = @{
        protocolVersion = "2024-11-05"
        capabilities    = @{
            roots = @{
                listChanged = $true
            }
        }
        clientInfo      = @{
            name    = "test-client"
            version = "1.0.0"
        }
    }
} | ConvertTo-Json -Depth 10

Write-Host "Sending initialize request:" -ForegroundColor Yellow
Write-Host $initRequest

# Test 2: Tools list request
$toolsRequest = @{
    jsonrpc = "2.0"
    id      = 2
    method  = "tools/list"
    params  = @{}
} | ConvertTo-Json -Depth 10

Write-Host "`nSending tools/list request:" -ForegroundColor Yellow
Write-Host $toolsRequest

# Test 3: Generate documentation request
$docRequest = @{
    jsonrpc = "2.0"
    id      = 3
    method  = "tools/call"
    params  = @{
        name      = "generate_git_documentation"
        arguments = @{
            maxCommits   = 10
            outputFormat = "markdown"
        }
    }
} | ConvertTo-Json -Depth 10

Write-Host "`nSending generate documentation request:" -ForegroundColor Yellow
Write-Host $docRequest

Write-Host "`nTo test interactively, run:" -ForegroundColor Cyan
Write-Host "dotnet run" -ForegroundColor White
Write-Host "Then paste the JSON requests above (one at a time) and press Enter" -ForegroundColor White

Write-Host "`nFor VS Code integration, update your MCP configuration with:" -ForegroundColor Cyan
$mcpConfig = Get-Content "mcp.json" | ConvertFrom-Json
Write-Host ($mcpConfig | ConvertTo-Json -Depth 10) -ForegroundColor White
