# Multi-stage build for NotificationService
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["NuGet.config", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["NotificationService/NotificationService.csproj", "NotificationService/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]

# Restore dependencies
RUN dotnet restore "NotificationService/NotificationService.csproj" \
    --verbosity normal \
    --source https://api.nuget.org/v3/index.json

# Copy source code
COPY NotificationService/ NotificationService/
COPY SharedKernel/ SharedKernel/

# Build the project
WORKDIR "/src/NotificationService"
RUN dotnet build "NotificationService.csproj" \
    -c Release \
    -o /app/build \
    --no-restore \
    --verbosity normal

# Publish the project
FROM build AS publish
RUN dotnet publish "NotificationService.csproj" \
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

ENTRYPOINT ["dotnet", "NotificationService.dll"]