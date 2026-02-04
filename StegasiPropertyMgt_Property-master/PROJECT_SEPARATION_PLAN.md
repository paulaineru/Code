# Stegasi Property Management System - Project Separation Plan

## Repository Structure

### 1. **StegasiPropertyMgt.Core** (Foundation Repository)
**Git Repository:** `https://github.com/your-org/StegasiPropertyMgt.Core`

**Projects:**
- `SharedKernel` (NuGet package)
- `AuthenticationService`
- `ApprovalWorkflowService`

**Dependencies:** None (foundational)
**Used By:** All other repositories

**Build Strategy:**
- SharedKernel published as NuGet package
- AuthenticationService and ApprovalWorkflowService as Docker images
- CI/CD pipeline builds and publishes NuGet package first, then services

---

### 2. **StegasiPropertyMgt.Property** (Property Domain)
**Git Repository:** `https://github.com/your-org/StegasiPropertyMgt.Property`

**Projects:**
- `PropertyManagementService`

**Dependencies:** 
- StegasiPropertyMgt.Core (NuGet package)
- SharedKernel (NuGet package)

**Build Strategy:**
- References Core NuGet packages
- Independent Docker build
- Can be developed and tested in isolation

---

### 3. **StegasiPropertyMgt.Tenant** (Tenant Domain)
**Git Repository:** `https://github.com/your-org/StegasiPropertyMgt.Tenant`

**Projects:**
- `TenantManagementService`

**Dependencies:**
- StegasiPropertyMgt.Core (NuGet package)
- StegasiPropertyMgt.Property (HTTP client calls)

**Build Strategy:**
- References Core NuGet packages
- HTTP client configuration for Property service
- Independent Docker build

---

### 4. **StegasiPropertyMgt.Billing** (Billing Domain)
**Git Repository:** `https://github.com/your-org/StegasiPropertyMgt.Billing`

**Projects:**
- `BillingService`

**Dependencies:**
- StegasiPropertyMgt.Core (NuGet package)
- StegasiPropertyMgt.Property (HTTP client calls)
- StegasiPropertyMgt.Tenant (HTTP client calls)

**Build Strategy:**
- References Core NuGet packages
- HTTP client configuration for Property and Tenant services
- Independent Docker build

---

### 5. **StegasiPropertyMgt.Support** (Support Services)
**Git Repository:** `https://github.com/your-org/StegasiPropertyMgt.Support`

**Projects:**
- `NotificationService`
- `ReportingService`

**Dependencies:**
- StegasiPropertyMgt.Core (NuGet package)

**Build Strategy:**
- References Core NuGet packages
- Independent Docker builds for each service
- Can be developed and deployed separately

---

### 6. **StegasiPropertyMgt.Infrastructure** (DevOps & Deployment)
**Git Repository:** `https://github.com/your-org/StegasiPropertyMgt.Infrastructure`

**Contents:**
- Docker Compose files
- Kubernetes manifests
- CI/CD pipelines
- Database scripts
- Nginx configuration
- Environment configurations

**Build Strategy:**
- Orchestrates all services
- Manages deployment configurations
- Handles environment-specific settings

---

## Git Implementation Strategy

### 1. **SharedKernel as NuGet Package**
```bash
# In StegasiPropertyMgt.Core repository
dotnet pack SharedKernel/SharedKernel.csproj -c Release -o ./nupkgs
dotnet nuget push ./nupkgs/SharedKernel.*.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY
```

### 2. **Repository Dependencies**
Each repository will have:
- `.gitignore` optimized for .NET
- `README.md` with setup instructions
- `Dockerfile` for containerization
- `docker-compose.yml` for local development
- CI/CD pipeline (GitHub Actions/Azure DevOps)

### 3. **Version Management**
- SharedKernel: Semantic versioning (1.0.0, 1.1.0, etc.)
- Services: Independent versioning
- API versioning maintained in each service

### 4. **Development Workflow**
1. Core changes → Build and publish NuGet package
2. Service changes → Update NuGet package reference
3. Independent development per repository
4. Integration testing in Infrastructure repository

## Implementation Steps

### Phase 1: Extract SharedKernel
1. Create StegasiPropertyMgt.Core repository
2. Move SharedKernel project
3. Configure NuGet publishing
4. Update all project references

### Phase 2: Extract Core Services
1. Move AuthenticationService and ApprovalWorkflowService
2. Update project references to use NuGet package
3. Configure independent builds

### Phase 3: Extract Domain Services
1. Create Property repository
2. Create Tenant repository  
3. Create Billing repository
4. Update HTTP client configurations

### Phase 4: Extract Support Services
1. Create Support repository
2. Move NotificationService and ReportingService
3. Configure independent builds

### Phase 5: Create Infrastructure Repository
1. Move all deployment configurations
2. Create orchestration scripts
3. Configure multi-repository CI/CD

## Benefits of This Approach

1. **Independent Development:** Teams can work on different domains simultaneously
2. **Faster Builds:** Smaller solution files, targeted builds
3. **Clear Dependencies:** Explicit NuGet package references
4. **Easier Testing:** Isolated test suites per domain
5. **Scalable Teams:** Multiple teams can own different repositories
6. **Better CI/CD:** Targeted pipelines per repository
7. **Version Control:** Independent versioning per domain
8. **Deployment Flexibility:** Services can be deployed independently

## Migration Checklist

- [ ] Create new Git repositories
- [ ] Extract SharedKernel as NuGet package
- [ ] Update all project references
- [ ] Configure CI/CD pipelines
- [ ] Update Docker configurations
- [ ] Create deployment scripts
- [ ] Update documentation
- [ ] Test independent builds
- [ ] Validate integration
- [ ] Update team workflows 