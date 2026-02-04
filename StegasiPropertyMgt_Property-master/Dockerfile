# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet.config first
COPY ["NuGet.config", "./"]

# Copy project files and restore dependencies
COPY ["TenantManagementService/TenantManagementService.csproj", "TenantManagementService/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]
RUN dotnet restore "TenantManagementService/TenantManagementService.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TenantManagementService"
RUN dotnet build "TenantManagementService.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "TenantManagementService.csproj" -c Release -o /app/publish

# Stage 3: Final Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TenantManagementService.dll"]