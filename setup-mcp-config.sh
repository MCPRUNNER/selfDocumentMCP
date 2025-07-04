#!/bin/bash

# Setup script for GitVisionMCP MCP configuration
# This script helps generate the correct mcp.json configuration for your environment

set -e

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Get the current directory (project root)
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
VSCODE_DIR="$PROJECT_ROOT/.vscode"
MCP_CONFIG="$VSCODE_DIR/mcp.json"

print_info "GitVisionMCP MCP Configuration Setup"
print_info "Project root: $PROJECT_ROOT"

# Ensure .vscode directory exists
mkdir -p "$VSCODE_DIR"

# Detect OS
OS=""
case "$OSTYPE" in
    msys*|cygwin*|win32*)
        OS="windows"
        ;;
    darwin*)
        OS="macos"
        ;;
    linux*)
        OS="linux"
        ;;
    *)
        OS="unix"
        ;;
esac

print_info "Detected OS: $OS"

# Check if Docker is available
DOCKER_AVAILABLE=false
if command -v docker &> /dev/null; then
    DOCKER_AVAILABLE=true
    print_info "Docker is available"
else
    print_warning "Docker is not available - only .NET configurations will be generated"
fi

# Check if .NET is available
DOTNET_AVAILABLE=false
if command -v dotnet &> /dev/null; then
    DOTNET_AVAILABLE=true
    DOTNET_VERSION=$(dotnet --version)
    print_info ".NET is available (version: $DOTNET_VERSION)"
else
    print_warning ".NET is not available - only Docker configurations will be generated"
fi

# Generate the mcp.json configuration
generate_config() {
    local config_file="$1"
    
    cat > "$config_file" << EOF
{
    "servers": {
EOF

    # Add .NET configuration if available
    if [ "$DOTNET_AVAILABLE" = true ]; then
        if [ "$OS" = "windows" ]; then
            cat >> "$config_file" << EOF
        "GitVisionMCP": {
            "type": "stdio",
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "$(echo "$PROJECT_ROOT" | sed 's|/|\\|g')\\GitVisionMCP.csproj",
                "--no-build",
                "--verbosity",
                "quiet"
            ],
            "env": {
                "DOTNET_ENVIRONMENT": "Production"
            }
        },
EOF
        else
            cat >> "$config_file" << EOF
        "GitVisionMCP": {
            "type": "stdio",
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "$PROJECT_ROOT/GitVisionMCP.csproj",
                "--no-build",
                "--verbosity",
                "quiet"
            ],
            "env": {
                "DOTNET_ENVIRONMENT": "Production"
            }
        },
EOF
        fi
    fi

    # Add Docker configuration if available
    if [ "$DOCKER_AVAILABLE" = true ]; then
        cat >> "$config_file" << EOF
        "GitVisionMCP-Docker": {
            "type": "stdio",
            "command": "docker",
            "args": [
                "run",
                "--rm",
                "-i",
                "--name", "gitvisionmcp-mcp",
                "-v", "$PROJECT_ROOT:/workspace:ro",
                "-v", "$PROJECT_ROOT/logs:/app/logs",
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
            "cwd": "$PROJECT_ROOT",
            "env": {
                "DOTNET_ENVIRONMENT": "Production"
            }
        }
EOF
    fi

    # Remove trailing comma and close the configuration
    sed -i '$ s/,$//' "$config_file"
    cat >> "$config_file" << EOF

    }
}
EOF
}

# Generate the configuration
print_info "Generating MCP configuration..."
generate_config "$MCP_CONFIG"

print_info "MCP configuration generated at: $MCP_CONFIG"

# Show next steps
echo
print_info "Next steps:"
if [ "$DOTNET_AVAILABLE" = true ]; then
    echo "  1. Build the project: dotnet build"
fi
if [ "$DOCKER_AVAILABLE" = true ]; then
    echo "  2. Build Docker image: docker build -t gitvisionmcp:latest ."
    echo "     Or use: ./build-docker.sh"
fi
echo "  3. Configure your MCP client to use one of the servers in $MCP_CONFIG"
echo "  4. Ensure your workspace contains a valid Git repository"

echo
print_info "Configuration complete!"
