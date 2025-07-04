# GitVisionMCP Docker MCP Configuration Quick Reference

This file provides quick copy-paste configurations for using GitVisionMCP with Docker in MCP clients.

## Prerequisites

1. Build the Docker image:

   ```bash
   docker build -t gitvisionmcp:latest .
   ```

2. Ensure your workspace contains a Git repository

3. Create logs directory if it doesn't exist:
   ```bash
   mkdir -p logs
   ```

## Configuration Templates

### Windows Paths

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
        "C:\\path\\to\\your\\workspace:/workspace:ro",
        "-v",
        "C:\\path\\to\\logs:/app/logs",
        "gitvisionmcp:latest"
      ],
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      }
    }
  }
}
```

### Linux/Mac Paths

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
        "gitvisionmcp:latest"
      ],
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      }
    }
  }
}
```

### Docker Compose

```json
{
  "servers": {
    "GitVisionMCP-Compose": {
      "type": "stdio",
      "command": "docker-compose",
      "args": ["run", "--rm", "--name", "gitvisionmcp-compose", "gitvisionmcp"],
      "cwd": "/path/to/GitVisionMCP/project",
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      }
    }
  }
}
```

### Development Mode (Hot Reload)

```json
{
  "servers": {
    "GitVisionMCP-Dev": {
      "type": "stdio",
      "command": "docker-compose",
      "args": [
        "--profile",
        "dev",
        "run",
        "--rm",
        "--name",
        "gitvisionmcp-dev",
        "gitvisionmcp-dev"
      ],
      "cwd": "/path/to/GitVisionMCP/project",
      "env": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## Troubleshooting

### Common Issues

1. **Container exits immediately**

   - Check if the workspace directory contains a valid Git repository
   - Verify volume mounts are correct
   - Check Docker logs: `docker logs gitvisionmcp-mcp`

2. **Permission denied errors**

   - Ensure the mounted directories have correct permissions
   - Check if Docker has access to the mounted paths

3. **Image not found**
   - Build the image: `docker build -t gitvisionmcp:latest .`
   - Or pull from registry if published

### Debug Commands

```bash
# Check if image exists
docker images | grep gitvisionmcp

# Test container manually
docker run --rm -it gitvisionmcp:latest /bin/bash

# View container logs
docker logs gitvisionmcp-mcp

# Check running containers
docker ps | grep gitvisionmcp
```

## Volume Explanations

- **Workspace Mount**: `-v "/path/to/workspace:/workspace:ro"`

  - Mounts your Git repository as read-only
  - The `:ro` flag provides security by preventing modifications

- **Logs Mount**: `-v "/path/to/logs:/app/logs"`

  - Persistent storage for application logs
  - Read-write access for log output

## Security Notes

- Container runs as non-root user (mcpuser)
- Logs directory is the only writable mount
- Network access is limited to what's needed for Git operations
