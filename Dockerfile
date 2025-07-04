# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Install Git (required for LibGit2Sharp)
RUN apt-get update && apt-get install -y git && rm -rf /var/lib/apt/lists/*

# Use the official .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["GitVisionMCP.csproj", "."]
RUN dotnet restore "GitVisionMCP.csproj"

# Copy the source code
COPY . .

# Build the application
RUN dotnet build "GitVisionMCP.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "GitVisionMCP.csproj" -c Release -o /app/publish

# Final stage - runtime image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Copy health check script
COPY docker-healthcheck.sh /usr/local/bin/healthcheck
RUN chmod +x /usr/local/bin/healthcheck

# Create a non-root user for security
RUN useradd -m -u 1000 mcpuser && chown -R mcpuser:mcpuser /app
USER mcpuser

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=

# Add health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD /usr/local/bin/healthcheck

# Expose any ports if needed (though this is a console app)
# EXPOSE 8080

# Set the entrypoint
ENTRYPOINT ["dotnet", "GitVisionMCP.dll"]
