# Repository Extraction Guide

This guide provides step-by-step instructions for extracting each service into separate Git repositories with independent builds, while managing the high interdependencies between services.

## **⚠️ IMPORTANT: Service Interdependencies**

The Stegasi Property Management System has **high interdependencies** between services:

### **Dependency Map:**
```
AuthenticationService ← Used by ALL services
SharedKernel ← Used by ALL services
PropertyManagementService ← Used by Tenant, Billing
TenantManagementService ← Used by Billing
BillingService ← Uses Property, Tenant, Auth
NotificationService ← Used by ALL services
ReportingService ← Uses ALL services
ApprovalWorkflowService ← Used by Property, Tenant
```

### **Key Dependencies:**
- **AuthenticationService**: Every service needs user authentication
- **SharedKernel**: All services share models, DTOs, interfaces
- **PropertyManagementService**: Tenant and Billing services depend on property data
- **TenantManagementService**: Billing service needs tenant information
- **NotificationService**: All services send notifications
- **ReportingService**: Aggregates data from all services

## Repository Structure Overview

```
StegasiPropertyMgt.Core/
├── SharedKernel/ (NuGet package)
├── AuthenticationService/
└── ApprovalWorkflowService/

StegasiPropertyMgt.Property/
└── PropertyManagementService/

StegasiPropertyMgt.Tenant/
└── TenantManagementService/

StegasiPropertyMgt.Billing/
└── BillingService/

StegasiPropertyMgt.Support/
├── NotificationService/
└── ReportingService/

StegasiPropertyMgt.Infrastructure/
├── docker-compose.yml
├── nginx/
├── scripts/
└── k8s/
```

## **Dependency Management Strategy**

### **1. SharedKernel as NuGet Package**
- **Critical**: Must be published first
- **Versioning**: Semantic versioning (1.0.0, 1.1.0, etc.)
- **Breaking Changes**: Require coordination across all repositories

### **2. Service Communication**
- **HTTP Client Calls**: Services communicate via HTTP APIs
- **Environment Variables**: Service URLs configured via environment
- **Service Discovery**: Docker Compose handles service discovery

### **3. Database Dependencies**
- **Shared Database**: All services use the same PostgreSQL database
- **Schema Coordination**: Database migrations need coordination
- **Data Consistency**: Cross-service transactions require careful handling

## 1. StegasiPropertyMgt.Core Repository

### Create Repository Structure
```bash
mkdir StegasiPropertyMgt.Core
cd StegasiPropertyMgt.Core
git init
```

### Extract Projects
```bash
# Copy from original project
cp -r ../StegasiPropertyMgt/SharedKernel ./
cp -r ../StegasiPropertyMgt/AuthenticationService ./
cp -r ../StegasiPropertyMgt/ApprovalWorkflowService ./
```

### Update SharedKernel for NuGet Publishing
```xml
<!-- SharedKernel/SharedKernel.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>StegasiPropertyMgt.SharedKernel</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Organization</Authors>
    <Description>Shared components for Stegasi Property Management System</Description>
    <PackageTags>property-management;shared;models</PackageTags>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>
</Project>
```

### Create Solution File
```bash
dotnet new sln -n StegasiPropertyMgt.Core
dotnet sln add SharedKernel/SharedKernel.csproj
dotnet sln add AuthenticationService/AuthenticationService.csproj
dotnet sln add ApprovalWorkflowService/ApprovalWorkflowService.csproj
```

### Create Docker Compose
```yaml
# docker-compose.yml
version: '3.8'
services:
  authenticationservice:
    build:
      context: .
      dockerfile: AuthenticationService/Dockerfile
    ports:
      - "5031:5031"
    environment:
      - ASPNETCORE_URLS=http://+:5031
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6

  approvalworkflowservice:
    build:
      context: .
      dockerfile: ApprovalWorkflowService/Dockerfile
    ports:
      - "5080:5080"
    environment:
      - ASPNETCORE_URLS=http://+:5080
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6
```

## 2. StegasiPropertyMgt.Property Repository

### Create Repository Structure
```bash
mkdir StegasiPropertyMgt.Property
cd StegasiPropertyMgt.Property
git init
```

### Extract Project
```bash
cp -r ../StegasiPropertyMgt/PropertyManagementService ./
```

### Update Project References
```xml
<!-- PropertyManagementService/PropertyManagementService.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="StegasiPropertyMgt.SharedKernel" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Create Solution File
```bash
dotnet new sln -n StegasiPropertyMgt.Property
dotnet sln add PropertyManagementService/PropertyManagementService.csproj
```

### Create Docker Compose
```yaml
# docker-compose.yml
version: '3.8'
services:
  propertymanagementservice:
    build:
      context: .
      dockerfile: PropertyManagementService/Dockerfile
    ports:
      - "5067:5067"
    environment:
      - ASPNETCORE_URLS=http://+:5067
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6
```

## 3. StegasiPropertyMgt.Tenant Repository

### Create Repository Structure
```bash
mkdir StegasiPropertyMgt.Tenant
cd StegasiPropertyMgt.Tenant
git init
```

### Extract Project
```bash
cp -r ../StegasiPropertyMgt/TenantManagementService ./
```

### Update Project References
```xml
<!-- TenantManagementService/TenantManagementService.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="StegasiPropertyMgt.SharedKernel" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Update HTTP Client Configuration
```csharp
// Program.cs - Update service URLs
builder.Services.AddHttpClient<IPropertyService, PropertyHttpClientService>(client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("PROPERTY_SERVICE_URL") ?? "http://localhost:5067");
});

builder.Services.AddHttpClient<IAuthService, RemoteAuthService>(client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") ?? "http://localhost:5031");
});
```

## 4. StegasiPropertyMgt.Billing Repository

### Create Repository Structure
```bash
mkdir StegasiPropertyMgt.Billing
cd StegasiPropertyMgt.Billing
git init
```

### Extract Project
```bash
cp -r ../StegasiPropertyMgt/BillingService ./
```

### Update Project References
```xml
<!-- BillingService/BillingService.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="StegasiPropertyMgt.SharedKernel" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Update HTTP Client Configuration
```csharp
// Program.cs - Update service URLs
builder.Services.AddHttpClient<IPropertyClient, PropertyClient>(client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("PROPERTY_SERVICE_URL") ?? "http://localhost:5067");
});

builder.Services.AddHttpClient<ITenantClient, TenantClient>(client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("TENANT_SERVICE_URL") ?? "http://localhost:5061");
});

builder.Services.AddHttpClient<IAuthService, RemoteAuthService>(client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") ?? "http://localhost:5031");
});
```

## 5. StegasiPropertyMgt.Support Repository

### Create Repository Structure
```bash
mkdir StegasiPropertyMgt.Support
cd StegasiPropertyMgt.Support
git init
```

### Extract Projects
```bash
cp -r ../StegasiPropertyMgt/NotificationService ./
cp -r ../StegasiPropertyMgt/ReportingService ./
```

### Update Project References
```xml
<!-- Both NotificationService and ReportingService .csproj files -->
<ItemGroup>
  <PackageReference Include="StegasiPropertyMgt.SharedKernel" Version="1.0.0" />
</ItemGroup>
```

### Create Solution File
```bash
dotnet new sln -n StegasiPropertyMgt.Support
dotnet sln add NotificationService/NotificationService.csproj
dotnet sln add ReportingService/ReportingService.csproj
```

## 6. StegasiPropertyMgt.Infrastructure Repository

### Create Repository Structure
```bash
mkdir StegasiPropertyMgt.Infrastructure
cd StegasiPropertyMgt.Infrastructure
git init
```

### Extract Infrastructure Files
```bash
cp ../StegasiPropertyMgt/docker-compose.yml ./
cp -r ../StegasiPropertyMgt/Nginx ./
cp ../StegasiPropertyMgt/setup-*.sql ./
```

### Create Master Docker Compose
```yaml
# docker-compose.yml - Orchestrates all services
version: '3.8'
services:
  authenticationservice:
    image: stegasi/auth-service:latest
    ports:
      - "5031:5031"
    environment:
      - ASPNETCORE_URLS=http://+:5031
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6

  propertymanagementservice:
    image: stegasi/property-service:latest
    ports:
      - "5067:5067"
    environment:
      - ASPNETCORE_URLS=http://+:5067
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6

  tenantmanagementservice:
    image: stegasi/tenant-service:latest
    ports:
      - "5061:5061"
    environment:
      - ASPNETCORE_URLS=http://+:5061
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6
      - PROPERTY_SERVICE_URL=http://propertymanagementservice:5067
      - AUTH_SERVICE_URL=http://authenticationservice:5031

  billingservice:
    image: stegasi/billing-service:latest
    ports:
      - "5295:5295"
    environment:
      - ASPNETCORE_URLS=http://+:5295
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6
      - PROPERTY_SERVICE_URL=http://propertymanagementservice:5067
      - TENANT_SERVICE_URL=http://tenantmanagementservice:5061
      - AUTH_SERVICE_URL=http://authenticationservice:5031

  notificationservice:
    image: stegasi/notification-service:latest
    ports:
      - "5004:5004"
    environment:
      - ASPNETCORE_URLS=http://+:5004
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6

  reportingservice:
    image: stegasi/reporting-service:latest
    ports:
      - "5024:5024"
    environment:
      - ASPNETCORE_URLS=http://+:5024
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6

  approvalworkflowservice:
    image: stegasi/approval-service:latest
    ports:
      - "5080:5080"
    environment:
      - ASPNETCORE_URLS=http://+:5080
      - ConnectionStrings__DefaultConnection=Host=192.168.185.23;Database=pms;Username=postgres;Password=devOps5.6

  nginx:
    build:
      context: ./Nginx
      dockerfile: Dockerfile
    ports:
      - "80:80"
    depends_on:
      - authenticationservice
      - propertymanagementservice
      - tenantmanagementservice
      - billingservice
      - notificationservice
      - reportingservice
      - approvalworkflowservice
```

## **Critical Dependency Management**

### **1. SharedKernel Version Coordination**
```bash
# When SharedKernel changes, update all repositories:
# 1. Update Core repository version
# 2. Publish new NuGet package
# 3. Update all other repositories to use new version
# 4. Coordinate deployment across all repositories
```

### **2. Service URL Configuration**
```bash
# Environment variables for service communication:
AUTH_SERVICE_URL=http://authenticationservice:5031
PROPERTY_SERVICE_URL=http://propertymanagementservice:5067
TENANT_SERVICE_URL=http://tenantmanagementservice:5061
BILLING_SERVICE_URL=http://billingservice:5295
NOTIFICATION_SERVICE_URL=http://notificationservice:5004
REPORTING_SERVICE_URL=http://reportingservice:5024
APPROVAL_SERVICE_URL=http://approvalworkflowservice:5080
```

### **3. Database Schema Coordination**
```bash
# Database migrations need coordination:
# 1. Core services deploy first
# 2. Domain services deploy second
# 3. Support services deploy last
# 4. All services must be compatible with database schema
```

## Common Files for Each Repository

### .gitignore
```gitignore
# .NET
bin/
obj/
*.user
*.suo
*.cache
*.dll
*.exe
*.pdb
*.log

# Docker
.dockerignore

# IDE
.vs/
.vscode/
*.swp
*.swo

# OS
.DS_Store
Thumbs.db

# Build outputs
nupkgs/
*.nupkg
```

### README.md Template
```markdown
# [Repository Name]

Brief description of the repository and its purpose.

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Docker & Docker Compose

### Local Development
```bash
git clone [repository-url]
cd [repository-name]
dotnet build
docker-compose up -d
```

### API Documentation
- Swagger UI: http://localhost:[port]/swagger

## Dependencies
- StegasiPropertyMgt.SharedKernel (NuGet package)
- [List other service dependencies]

## Service Dependencies
This service depends on:
- [List dependent services]

This service is used by:
- [List services that depend on this one]

## Contributing
1. Create feature branch
2. Make changes
3. Add tests
4. Submit pull request
```

## Migration Steps

### Phase 1: Setup Core Repository (CRITICAL)
1. Create StegasiPropertyMgt.Core repository
2. Extract SharedKernel, AuthenticationService, ApprovalWorkflowService
3. Configure NuGet publishing for SharedKernel
4. Set up CI/CD pipeline
5. Test independent build and deployment
6. **Publish first NuGet package version**

### Phase 2: Extract Domain Repositories
1. Create Property repository
2. Create Tenant repository
3. Create Billing repository
4. Update all project references to use NuGet package
5. Configure HTTP client dependencies
6. Test each repository independently

### Phase 3: Extract Support Repositories
1. Create Support repository
2. Extract NotificationService and ReportingService
3. Update project references
4. Test independent builds

### Phase 4: Create Infrastructure Repository
1. Create Infrastructure repository
2. Extract all deployment configurations
3. Create master orchestration
4. Test full system deployment

### Phase 5: Update Development Workflow
1. Update team documentation
2. Configure cross-repository CI/CD
3. Set up development environments
4. Train teams on new workflow

## **⚠️ Critical Considerations for Interdependencies**

### **1. Deployment Order**
```
1. Core Services (Auth, SharedKernel)
2. Property Service
3. Tenant Service
4. Billing Service
5. Support Services (Notification, Reporting)
```

### **2. Breaking Changes**
- **SharedKernel changes** require coordination across ALL repositories
- **API changes** require coordination between dependent services
- **Database schema changes** require coordination across all services

### **3. Testing Strategy**
- **Unit tests** within each repository
- **Integration tests** for service communication
- **End-to-end tests** in Infrastructure repository
- **Cross-repository testing** for critical workflows

### **4. Version Management**
- **SharedKernel**: Semantic versioning with breaking change coordination
- **Services**: Independent versioning with dependency tracking
- **API versions**: Maintain backward compatibility

## Benefits of This Approach

1. **Independent Development:** Teams can work on different domains
2. **Faster Builds:** Smaller solution files, targeted builds
3. **Clear Dependencies:** Explicit NuGet package references
4. **Easier Testing:** Isolated test suites per domain
5. **Scalable Teams:** Multiple teams can own different repositories
6. **Better CI/CD:** Targeted pipelines per repository
7. **Version Control:** Independent versioning per domain
8. **Deployment Flexibility:** Services can be deployed independently

## **Migration Checklist**

- [ ] Create new Git repositories
- [ ] Extract SharedKernel as NuGet package
- [ ] Update all project references
- [ ] Configure service URL environment variables
- [ ] Set up Azure DevOps pipelines
- [ ] Update Docker configurations
- [ ] Create deployment scripts
- [ ] Update documentation
- [ ] Test independent builds
- [ ] Validate service communication
- [ ] Test full system integration
- [ ] Update team workflows
- [ ] Plan for breaking changes coordination 