# Setup script for GitVisionMCP MCP configuration (PowerShell)
# This script helps generate the correct mcp.json configuration for your environment

param(
    [switch]$Help
)

if ($Help) {
    Write-Host "GitVisionMCP MCP Configuration Setup"
    Write-Host "Usage: .\setup-mcp-config.ps1"
    Write-Host ""
    Write-Host "This script generates a .vscode/mcp.json configuration file"
    Write-Host "with the correct paths for your environment."
    exit 0
}

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Reset = "`e[0m"

function Write-Info {
    param([string]$Message)
    Write-Host "${Green}[INFO]${Reset} $Message"
}

function Write-Warning {
    param([string]$Message)
    Write-Host "${Yellow}[WARNING]${Reset} $Message"
}

# Get the current directory (project root)
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$VsCodeDir = Join-Path $ProjectRoot ".vscode"
$McpConfig = Join-Path $VsCodeDir "mcp.json"

Write-Info "GitVisionMCP MCP Configuration Setup"
Write-Info "Project root: $ProjectRoot"

# Ensure .vscode directory exists
if (-not (Test-Path $VsCodeDir)) {
    New-Item -ItemType Directory -Path $VsCodeDir -Force | Out-Null
}

# Check if Docker is available
$DockerAvailable = $false
try {
    docker --version | Out-Null
    $DockerAvailable = $true
    Write-Info "Docker is available"
}
catch {
    Write-Warning "Docker is not available - only .NET configurations will be generated"
}

# Check if .NET is available
$DotNetAvailable = $false
try {
    $DotNetVersion = dotnet --version
    $DotNetAvailable = $true
    Write-Info ".NET is available (version: $DotNetVersion)"
}
catch {
    Write-Warning ".NET is not available - only Docker configurations will be generated"
}

# Generate the mcp.json configuration
function New-McpConfig {
    param([string]$ConfigFile)
    
    $config = @"
{
    "servers": {
"@

    # Add .NET configuration if available
    if ($DotNetAvailable) {
        $projectPath = Join-Path $ProjectRoot "GitVisionMCP.csproj"
        $projectPathEscaped = $projectPath -replace '\\', '\\'
        
        $dotnetConfig = @"
        "GitVisionMCP": {
            "type": "stdio",
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "$projectPathEscaped",
                "--no-build",
                "--verbosity",
                "quiet"
            ],
            "env": {
                "DOTNET_ENVIRONMENT": "Production"
            }
        }
"@
        $config += "`n$dotnetConfig"
    }

    # Add Docker configuration if available
    if ($DockerAvailable) {
        $workspaceMount = "$ProjectRoot" -replace '\\', '/'
        $logsMount = "$ProjectRoot/logs" -replace '\\', '/'
        
        if ($DotNetAvailable) {
            $config += ","
        }
        
        $dockerConfig = @"
        "GitVisionMCP-Docker": {
            "type": "stdio",
            "command": "docker",
            "args": [
                "run",
                "--rm",
                "-i",
                "--name", "gitvisionmcp-mcp",
                "-v", "${workspaceMount}:/workspace:ro",
                "-v", "${logsMount}:/app/logs",
                "-w", "/workspace",
                "gitvisionmcp:latest"
            ],
            "env": {
                "DOTNET_ENVIRONMENT": "Production"
            }
        },
        "GitVisionMCP-Docker-Compose": {
            "type": "stdio",
            "command": "docker-compose",
            "args": [
                "run",
                "--rm",
                "--name", "gitvisionmcp-compose",
                "gitvisionmcp"
            ],
            "cwd": "$ProjectRoot",
            "env": {
                "DOTNET_ENVIRONMENT": "Production"
            }
        }
"@
        $config += "`n$dockerConfig"
    }

    $config += @"

    }
}
"@

    $config | Out-File -FilePath $ConfigFile -Encoding UTF8
}

# Generate the configuration
Write-Info "Generating MCP configuration..."
New-McpConfig -ConfigFile $McpConfig

Write-Info "MCP configuration generated at: $McpConfig"

# Show next steps
Write-Host ""
Write-Info "Next steps:"
if ($DotNetAvailable) {
    Write-Host "  1. Build the project: dotnet build"
}
if ($DockerAvailable) {
    Write-Host "  2. Build Docker image: docker build -t gitvisionmcp:latest ."
    Write-Host "     Or use: .\build-docker.ps1"
}
Write-Host "  3. Configure your MCP client to use one of the servers in $McpConfig"
Write-Host "  4. Ensure your workspace contains a valid Git repository"

Write-Host ""
Write-Info "Configuration complete!"
