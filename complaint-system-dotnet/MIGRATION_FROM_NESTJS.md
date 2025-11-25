# Migration from NestJS to .NET Stack

**Date**: 2025-10-11
**Reason**: Client requirement - API must be on .NET, Frontend on Angular, Backend on SQL

---

## 🔄 What Changed

### Previous Stack (Archived)
- ❌ Backend: NestJS (Node.js + TypeScript)
- ❌ Frontend: React + Next.js
- ❌ Database: PostgreSQL
- ❌ ORM: TypeORM

### New Stack (Current)
- ✅ Backend API: **.NET 8 Web API** (C#)
- ✅ Background Jobs: **.NET 8 Worker Service**
- ✅ Frontend: **Angular 17+** (TypeScript)
- ✅ Database: **SQL Server** (primary)
- ✅ ORM: **Entity Framework Core 8**
- ✅ Multi-database support: PostgreSQL, MySQL, Oracle, SQLite

---

## 📦 What Was Preserved

### ✅ Database Schema Design
All 13 entity designs from planning documents are preserved:
- Master Data entities (6): Tenant, Company, Branch, Department, Section, User
- Complaint entities (4): ComplaintCategory, Complaint, ComplaintComment, ComplaintAttachment
- Role entities (3): ComplaintRole, UserComplaintRole, ComplaintRolePermission

### ✅ Planning Documents
All documentation remains valid:
- ✅ MASTER_PLANNING_DOCUMENT.md
- ✅ CHUNK_03_COMPLAINT_ROLE_TABLES.md
- ✅ CHUNK_04_ESCALATION_EMAIL_TABLES.md
- ✅ CHUNK_05_ORYGGI_INTEGRATION.md
- ✅ CHUNK_06_TECHNOLOGY_STACK.md (updated)
- ✅ CHUNK_07_UI_UX_DESIGN.md
- ✅ CHUNK_08_SECURITY_DEPLOYMENT.md

### ✅ Architecture Principles
- Multi-tenant architecture
- N-level escalation (2-5 levels)
- Email alert system
- Oryggi HRMS integration
- Role-based access control
- SLA tracking

---

## 🗂️ Archived NestJS Code

All NestJS code has been preserved in: `complaint-system/` directory

**What was built** (40+ hours of work):
- ✅ 13 TypeORM entities (~1,130 lines)
- ✅ NestJS backend structure
- ✅ PostgreSQL migrations
- ✅ Docker configuration
- ✅ Environment setup
- ✅ Seed data scripts

**Status**: Archived for reference, not deleted

---

## 🆕 .NET Solution Structure

```
complaint-system-dotnet/
├── src/
│   ├── ComplaintManagement.Domain/           # Entities (same schema)
│   ├── ComplaintManagement.Application/      # Business logic
│   ├── ComplaintManagement.Infrastructure/   # EF Core + Data access
│   ├── ComplaintManagement.Shared/           # Utilities
│   ├── ComplaintManagement.API/              # Web API
│   └── ComplaintManagement.WorkerService/    # Background jobs
│
├── frontend/                                  # Angular app
├── docker/
├── docker-compose.yml
└── ComplaintManagementSystem.sln
```

---

## 🔀 Technology Mapping

### Backend Framework
| NestJS | .NET | Purpose |
|--------|------|---------|
| @nestjs/common | ASP.NET Core | Framework |
| @nestjs/typeorm | Entity Framework Core | ORM |
| TypeORM entities | EF Core entities | Database models |
| @nestjs/jwt | Microsoft.AspNetCore.Authentication.JwtBearer | JWT auth |
| @nestjs/bull | Hangfire | Background jobs |
| class-validator | FluentValidation | Input validation |
| @nestjs/swagger | Swashbuckle | API documentation |

### Frontend Framework
| Next.js/React | Angular | Purpose |
|---------------|---------|---------|
| React components | Angular components | UI components |
| Next.js routing | Angular Router | Routing |
| TanStack Query | HttpClient + RxJS | API calls |
| Zustand | NgRx (optional) | State management |
| Material-UI | Angular Material | UI library |

### Database
| Previous | Current | Notes |
|----------|---------|-------|
| PostgreSQL | SQL Server | Primary database |
| TypeORM | Entity Framework Core | Supports both + more |
| node-mssql | Built-in EF Core | SQL Server connector |

---

## ⚡ Quick Setup (New .NET Stack)

### 1. Prerequisites
```bash
# Install .NET 8 SDK
https://dotnet.microsoft.com/download/dotnet/8.0

# Install Node.js 18+ (for Angular)
https://nodejs.org/

# Install Angular CLI
npm install -g @angular/cli

# Install SQL Server (or use Docker)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Create Solution
```bash
cd complaint-system-dotnet

# Run setup script
powershell -ExecutionPolicy Bypass -File setup-solution.ps1

# This creates:
# - .NET solution with 6 projects
# - All necessary NuGet packages
# - Project references
```

### 3. Create Entities
```bash
# Entities will be created in:
# src/ComplaintManagement.Domain/Entities/

# Same schema as NestJS TypeORM entities, but in C#
```

### 4. Run Migrations
```bash
cd src/ComplaintManagement.Infrastructure

# Add migration
dotnet ef migrations add InitialCreate --startup-project ../ComplaintManagement.API

# Update database
dotnet ef database update --startup-project ../ComplaintManagement.API
```

### 5. Run API
```bash
cd src/ComplaintManagement.API
dotnet run

# API: https://localhost:5001
# Swagger: https://localhost:5001/swagger
```

### 6. Run Worker Service
```bash
cd src/ComplaintManagement.WorkerService
dotnet run
```

### 7. Create Angular Frontend
```bash
cd complaint-system-dotnet
ng new frontend --routing --style=scss
cd frontend
npm install @angular/material @angular/cdk
ng serve

# App: http://localhost:4200
```

---

## 📊 Entity Conversion Example

### NestJS TypeORM Entity:
```typescript
@Entity('users')
export class User {
  @PrimaryGeneratedColumn('uuid')
  user_id: string;

  @Column({ type: 'varchar', length: 255 })
  email: string;

  @Column({ type: 'boolean', default: true })
  is_active: boolean;

  @CreateDateColumn()
  created_at: Date;

  @ManyToOne(() => Company)
  @JoinColumn({ name: 'company_id' })
  company: Company;
}
```

### .NET EF Core Entity:
```csharp
[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    public Company Company { get; set; }
}
```

---

## 🎯 Benefits of .NET Stack

### 1. Performance
- ✅ **Faster** - .NET 8 is one of the fastest web frameworks
- ✅ **Native compilation** - ahead-of-time compilation
- ✅ **Better memory management** - garbage collection optimizations

### 2. Enterprise Support
- ✅ **Microsoft backing** - enterprise-grade support
- ✅ **SQL Server integration** - first-class support
- ✅ **Azure integration** - seamless cloud deployment

### 3. Worker Service
- ✅ **True background service** - .NET Worker Service template
- ✅ **Windows Service** - can run as Windows Service
- ✅ **Hangfire** - robust job scheduling

### 4. Multi-Database Support
- ✅ **EF Core** - supports 10+ database providers
- ✅ **Easy switching** - just change connection string + provider
- ✅ **Migrations** - database-agnostic

### 5. Tooling
- ✅ **Visual Studio** - best-in-class IDE
- ✅ **VS Code** - C# Dev Kit extension
- ✅ **Entity Framework Tools** - migration management
- ✅ **Swagger** - built-in API documentation

---

## 📚 Learning Resources

### .NET
- Official Docs: https://learn.microsoft.com/en-us/aspnet/core/
- EF Core: https://learn.microsoft.com/en-us/ef/core/
- Clean Architecture: https://github.com/jasontaylordev/CleanArchitecture

### Angular
- Official Docs: https://angular.io/docs
- Angular Material: https://material.angular.io/
- RxJS: https://rxjs.dev/

---

## 🔄 Migration Timeline

| Date | Task | Status |
|------|------|--------|
| 2025-10-11 | NestJS backend created | ✅ Complete (archived) |
| 2025-10-11 | Client requirement change | ✅ Confirmed |
| 2025-10-11 | .NET solution structure | 🔄 In Progress |
| TBD | EF Core entities | ⏳ Pending |
| TBD | DbContext setup | ⏳ Pending |
| TBD | Web API controllers | ⏳ Pending |
| TBD | Worker Service jobs | ⏳ Pending |
| TBD | Angular frontend | ⏳ Pending |

---

## ✅ Checklist

### Setup
- [ ] Install .NET 8 SDK
- [ ] Install Angular CLI
- [ ] Install SQL Server
- [ ] Run setup-solution.ps1
- [ ] Verify solution builds

### Domain Layer
- [ ] Create all 13 entities
- [ ] Create enums
- [ ] Create value objects

### Infrastructure Layer
- [ ] Create DbContext
- [ ] Configure entity mappings
- [ ] Create repositories
- [ ] Setup Oryggi integration

### API Layer
- [ ] Create controllers
- [ ] Setup JWT authentication
- [ ] Configure Swagger
- [ ] Add logging

### Worker Service
- [ ] Setup Hangfire
- [ ] Create Oryggi sync job
- [ ] Create email job
- [ ] Create escalation job

### Frontend
- [ ] Create Angular project
- [ ] Setup Angular Material
- [ ] Create services
- [ ] Create components

---

## 🤝 Support

For questions about the migration:
1. Review this document
2. Check planning documents (CHUNK_* files)
3. Refer to .NET documentation

---

**Status**: ✅ Migration in progress
**Next**: Run setup-solution.ps1 to create .NET projects
