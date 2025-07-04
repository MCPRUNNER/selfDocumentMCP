# GitVisionMCP Docker Build Script for PowerShell

param(
    [switch]$Dev,
    [string]$Platform = "linux/amd64",
    [string]$Tag = "",
    [switch]$Push,
    [switch]$Help
)

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Reset = "`e[0m"

function Write-Status {
    param([string]$Message)
    Write-Host "${Green}[INFO]${Reset} $Message"
}

function Write-Warning {
    param([string]$Message)
    Write-Host "${Yellow}[WARNING]${Reset} $Message"
}

function Write-Error {
    param([string]$Message)
    Write-Host "${Red}[ERROR]${Reset} $Message"
}

if ($Help) {
    Write-Host "Usage: .\build-docker.ps1 [OPTIONS]"
    Write-Host "Options:"
    Write-Host "  -Dev                Build development image"
    Write-Host "  -Platform <name>    Target platform (default: linux/amd64)"
    Write-Host "  -Tag <name>         Docker tag"
    Write-Host "  -Push               Push image to registry"
    Write-Host "  -Help               Show this help message"
    exit 0
}

# Set defaults based on build type
if ($Dev) {
    $BuildType = "development"
    if (-not $Tag) { $Tag = "gitvisionmcp:dev" }
}
else {
    $BuildType = "production"
    if (-not $Tag) { $Tag = "gitvisionmcp:latest" }
}

Write-Status "Starting Docker build for GitVisionMCP"
Write-Status "Build type: $BuildType"
Write-Status "Platform: $Platform"
Write-Status "Tag: $Tag"

# Check if Docker is available
try {
    docker --version | Out-Null
}
catch {
    Write-Error "Docker is not installed or not in PATH"
    exit 1
}

# Check if we're in the correct directory
if (-not (Test-Path "GitVisionMCP.csproj")) {
    Write-Error "GitVisionMCP.csproj not found. Are you in the project root?"
    exit 1
}

# Build the appropriate image
try {
    if ($BuildType -eq "development") {
        Write-Status "Building development image with hot reload..."
        docker build -f Dockerfile.dev -t $Tag --platform $Platform .
    }
    else {
        Write-Status "Building production image..."
        docker build -f Dockerfile -t $Tag --platform $Platform .
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Build completed successfully!"
        
        # Show image info
        docker images | Select-String "gitvisionmcp"
        
        # Push if requested
        if ($Push) {
            Write-Status "Pushing image to registry..."
            docker push $Tag
        }
        
        Write-Status "Ready to run with:"
        if ($BuildType -eq "development") {
            Write-Host "  docker-compose --profile dev up gitvisionmcp-dev"
        }
        else {
            Write-Host "  docker-compose up gitvisionmcp"
        }
        Write-Host "  OR"
        Write-Host "  docker run -d --name gitvisionmcp-container \"
        Write-Host "    -v `"`$(pwd)/workspace:/workspace:ro`" \"
        Write-Host "    -v `"`$(pwd)/logs:/app/logs`" \"
        Write-Host "    $Tag"
    }
    else {
        Write-Error "Build failed!"
        exit 1
    }
}
catch {
    Write-Error "Build failed with error: $_"
    exit 1
}
