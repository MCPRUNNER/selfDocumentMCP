# Use the official .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Git (required for LibGit2Sharp)
RUN apt-get update && apt-get install -y git && rm -rf /var/lib/apt/lists/*



# Copy the project file and restore dependencies
COPY ["GitVisionMCP.csproj", "./"]
RUN dotnet restore "GitVisionMCP.csproj"

# Copy the source code
COPY . .

# Build the application
RUN dotnet build "GitVisionMCP.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "GitVisionMCP.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install Git (required for LibGit2Sharp)
RUN apt-get update && apt-get install -y git && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=build /app/publish .

# Copy configuration files explicitly
COPY ["appsettings.json", "./"]
COPY ["appsettings.Production.json", "./"]
COPY ["appsettings.Development.json", "./"]


# Set environment variables
ENV DOTNET_ENVIRONMENT=Production


# Set the entrypoint
ENTRYPOINT ["dotnet", "./GitVisionMCP.dll"]
