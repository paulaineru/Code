# Multi-stage build for AuthenticationService
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["NuGet.config", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["AuthenticationService/AuthenticationService.csproj", "AuthenticationService/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]

# Restore dependencies
RUN dotnet restore "AuthenticationService/AuthenticationService.csproj" \
    --verbosity normal \
    --source https://api.nuget.org/v3/index.json

# Copy source code
COPY AuthenticationService/ AuthenticationService/
COPY SharedKernel/ SharedKernel/

# Build the project
WORKDIR "/src/AuthenticationService"
RUN dotnet build "AuthenticationService.csproj" \
    -c Release \
    -o /app/build \
    --no-restore \
    --verbosity normal

# Publish the project
FROM build AS publish
RUN dotnet publish "AuthenticationService.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --verbosity normal \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "AuthenticationService.dll"]