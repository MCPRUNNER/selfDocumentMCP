# Docker Setup for GitVisionMCP

This directory contains Docker configurations for running GitVisionMCP in containerized environments.

## Files Overview

- `Dockerfile` - Production-ready multi-stage build
- `Dockerfile.dev` - Development environment with hot reload
- `docker-compose.yml` - Orchestration for both production and development
- `.dockerignore` - Excludes unnecessary files from Docker build context

## Quick Start

### Production Build

```bash
# Build the production image
docker build -t gitvisionmcp:latest .

# Run the container
docker run -d --name gitvisionmcp \
  -v "$(pwd)/workspace:/workspace:ro" \
  -v "$(pwd)/logs:/app/logs" \
  gitvisionmcp:latest
```

### Using Docker Compose

```bash
# Production environment
docker-compose up -d gitvisionmcp

# Development environment with hot reload
docker-compose --profile dev up -d gitvisionmcp-dev
```

## Development Environment

The development setup includes:

- Hot reload with `dotnet watch`
- Source code volume mounting
- Development-specific environment variables
- Debugging capabilities

```bash
# Start development environment
docker-compose --profile dev up gitvisionmcp-dev

# View logs
docker-compose --profile dev logs -f gitvisionmcp-dev
```

## Volume Mounts

### Required Volumes

- `/workspace` - Mount your Git repository here (read-only for production)
- `/app/logs` - Persistent log storage

### Example with specific repository

```bash
docker run -d --name gitvisionmcp \
  -v "/path/to/your/git/repo:/workspace:ro" \
  -v "$(pwd)/logs:/app/logs" \
  gitvisionmcp:latest
```

## Environment Variables

- `DOTNET_ENVIRONMENT` - Set to `Production` or `Development`
- `ASPNETCORE_URLS` - Configure URLs if needed

## Security Considerations

- The container runs as a non-root user (`mcpuser`)
- Git repositories are mounted read-only in production
- Logs are written to a dedicated volume

## Troubleshooting

### Common Issues

1. **Git operations fail**: Ensure Git is installed and the repository is properly mounted
2. **Permission errors**: Check that the mounted volumes have proper permissions
3. **LibGit2Sharp errors**: Ensure the native Git libraries are available

### Debugging

```bash
# Enter the container for debugging
docker exec -it gitvisionmcp bash

# Check logs
docker logs gitvisionmcp

# View container processes
docker exec gitvisionmcp ps aux
```

## Building for Different Platforms

```bash
# Build for multiple platforms
docker buildx build --platform linux/amd64,linux/arm64 -t gitvisionmcp:latest .

# Build for specific platform
docker build --platform linux/amd64 -t gitvisionmcp:amd64 .
```

## Integration with MCP

When using GitVisionMCP with Model Context Protocol clients, you can configure the client to use the Docker container in several ways:

### Option 1: Direct Docker Run

```json
{
  "servers": {
    "GitVisionMCP-Docker": {
      "type": "stdio",
      "command": "docker",
      "args": [
        "run",
        "--rm",
        "-i",
        "--name",
        "gitvisionmcp-mcp",
        "-v",
        "/path/to/your/workspace:/workspace:ro",
        "-v",
        "/path/to/logs:/app/logs",
        "-w",
        "/workspace",
        "gitvisionmcp:latest"
      ],
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      }
    }
  }
}
```

### Option 2: Docker Compose

```json
{
  "servers": {
    "GitVisionMCP-Compose": {
      "type": "stdio",
      "command": "docker-compose",
      "args": ["run", "--rm", "--name", "gitvisionmcp-compose", "gitvisionmcp"],
      "cwd": "/path/to/GitVisionMCP",
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      }
    }
  }
}
```

### Setup Scripts

Use the provided setup scripts to generate the correct configuration for your environment:

```bash
# Linux/Mac
./setup-mcp-config.sh

# Windows PowerShell
./setup-mcp-config.ps1
```

These scripts will:

- Detect your operating system
- Check for Docker and .NET availability
- Generate appropriate `.vscode/mcp.json` configuration
- Provide correct paths for your environment

### Configuration Examples

See `.vscode/mcp.examples.json` for comprehensive examples including:

- Windows and Unix path formats
- Development mode configurations
- Registry-based deployments
- Security considerations

## Performance Considerations

- Use `.dockerignore` to minimize build context
- Multi-stage builds keep final image size small
- Read-only repository mounts improve security
- Persistent log volumes for debugging

## Updates and Maintenance

```bash
# Rebuild after code changes
docker-compose build gitvisionmcp

# Update and restart
docker-compose down
docker-compose pull
docker-compose up -d

# Clean up old images
docker image prune -f
```
