# Multi-stage build for SharedKernel (Library Project)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["NuGet.config", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]

# Restore dependencies
RUN dotnet restore "SharedKernel/SharedKernel.csproj" \
    --verbosity normal \
    --source https://api.nuget.org/v3/index.json

# Copy source code
COPY SharedKernel/ SharedKernel/

# Build the project
WORKDIR "/src/SharedKernel"
RUN dotnet build "SharedKernel.csproj" \
    -c Release \
    -o /app/build \
    --no-restore \
    --verbosity normal

# Publish the project
FROM build AS publish
RUN dotnet publish "SharedKernel.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --verbosity normal

# This is a library project, so we don't need a runtime stage
# The published output will be copied by dependent services