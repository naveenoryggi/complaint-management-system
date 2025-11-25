# Complaint Management System - .NET + Angular

**Enterprise HRMS Complaint Management with Oryggi Integration**

---

## 🏗️ Technology Stack

### Backend
- **.NET 8 Web API** - REST API endpoints
- **.NET 8 Worker Service** - Background job processing
- **Entity Framework Core 8** - ORM with multi-database support
- **SQL Server** - Primary database
- **Oryggi HRMS** - SQL Server integration (read-only)

### Frontend
- **Angular 17+** - Modern TypeScript framework
- **Angular Material** - UI component library
- **RxJS** - Reactive programming

### Infrastructure
- **Docker** - Containerization
- **SQL Server 2022** - Database
- **Redis** - Caching & message queue
- **Hangfire** - Background job scheduling

---

## 📁 Solution Structure

```
ComplaintManagementSystem/
├── src/
│   ├── ComplaintManagement.API/              # Web API project
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── ComplaintManagement.WorkerService/    # Background jobs
│   │   ├── Workers/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── ComplaintManagement.Domain/           # Domain entities
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── ValueObjects/
│   │
│   ├── ComplaintManagement.Application/      # Business logic
│   │   ├── Services/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   └── Validators/
│   │
│   ├── ComplaintManagement.Infrastructure/   # Data access & external services
│   │   ├── Data/
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── ComplaintDbContext.cs
│   │   ├── Repositories/
│   │   ├── Services/
│   │   └── OryggiIntegration/
│   │
│   └── ComplaintManagement.Shared/           # Shared utilities
│       ├── Constants/
│       ├── Extensions/
│       └── Helpers/
│
├── frontend/                                  # Angular application
│   ├── src/
│   │   ├── app/
│   │   │   ├── modules/
│   │   │   ├── services/
│   │   │   ├── models/
│   │   │   └── shared/
│   │   ├── assets/
│   │   └── environments/
│   ├── angular.json
│   └── package.json
│
├── docker/
│   ├── api.Dockerfile
│   ├── worker.Dockerfile
│   └── angular.Dockerfile
│
├── docker-compose.yml
├── ComplaintManagementSystem.sln
└── README.md
```

---

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+ (for Angular)
- SQL Server 2022 or Docker
- Angular CLI: `npm install -g @angular/cli`

### 1. Clone and Setup

```bash
cd complaint-system-dotnet

# Restore .NET packages
dotnet restore

# Install Angular dependencies
cd frontend
npm install
cd ..
```

### 2. Database Setup

```bash
# Update connection string in appsettings.json
# Run migrations
cd src/ComplaintManagement.Infrastructure
dotnet ef database update --startup-project ../ComplaintManagement.API
```

### 3. Run with Docker

```bash
# Start all services
docker-compose up -d

# API: http://localhost:5000
# Angular: http://localhost:4200
# SQL Server: localhost:1433
```

### 4. Run Manually

**Terminal 1 - API:**
```bash
cd src/ComplaintManagement.API
dotnet run
# API: https://localhost:5001
```

**Terminal 2 - Worker Service:**
```bash
cd src/ComplaintManagement.WorkerService
dotnet run
```

**Terminal 3 - Angular:**
```bash
cd frontend
ng serve
# App: http://localhost:4200
```

---

## 🗃️ Database Support

Entity Framework Core supports multiple databases:

### Configured Databases:
- ✅ **SQL Server** (Primary - Complaint Management)
- ✅ **SQL Server** (Oryggi HRMS - Read Only)

### Supported Databases (Easy to switch):
- PostgreSQL
- MySQL
- SQLite
- Oracle
- In-Memory (for testing)

### Switch Database:
```csharp
// In Program.cs or Startup.cs
services.AddDbContext<ComplaintDbContext>(options =>
{
    // SQL Server
    options.UseSqlServer(connectionString);

    // OR PostgreSQL
    // options.UseNpgsql(connectionString);

    // OR MySQL
    // options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});
```

---

## 📦 Key Features

### Backend API Features
- ✅ RESTful API with Swagger/OpenAPI
- ✅ JWT Authentication & Authorization
- ✅ Role-Based Access Control (RBAC)
- ✅ Multi-database support via EF Core
- ✅ Repository & Unit of Work pattern
- ✅ CQRS pattern with MediatR
- ✅ FluentValidation for input validation
- ✅ AutoMapper for DTO mapping
- ✅ Serilog for structured logging
- ✅ Global exception handling

### Worker Service Features
- ✅ Hangfire for background jobs
- ✅ Oryggi HRMS synchronization (scheduled)
- ✅ Email notification sending
- ✅ SLA breach monitoring
- ✅ Auto-escalation processing
- ✅ File virus scanning
- ✅ Report generation

### Frontend Features
- ✅ Angular 17+ with TypeScript
- ✅ Angular Material UI
- ✅ Lazy loading modules
- ✅ JWT token management
- ✅ HTTP interceptors
- ✅ Reactive forms with validation
- ✅ State management with NgRx (optional)
- ✅ Responsive design (mobile-first)

---

## 🔐 Security Features

- JWT-based authentication
- Role-based authorization
- Password hashing (BCrypt)
- SQL injection prevention (EF Core parameterized queries)
- XSS protection
- CORS configuration
- Rate limiting
- API versioning
- Audit logging

---

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/logout` - Logout

### Complaints
- `GET /api/complaints` - List complaints
- `GET /api/complaints/{id}` - Get complaint details
- `POST /api/complaints` - Create complaint
- `PUT /api/complaints/{id}` - Update complaint
- `DELETE /api/complaints/{id}` - Delete complaint
- `POST /api/complaints/{id}/assign` - Assign complaint
- `POST /api/complaints/{id}/escalate` - Escalate complaint
- `POST /api/complaints/{id}/resolve` - Resolve complaint

### Users
- `GET /api/users` - List users
- `GET /api/users/{id}` - Get user details
- `POST /api/users/{id}/roles` - Assign roles

### Roles
- `GET /api/roles` - List roles
- `POST /api/roles` - Create role
- `PUT /api/roles/{id}` - Update role
- `DELETE /api/roles/{id}` - Delete role

---

## 🔄 Background Jobs

### Scheduled Jobs (Worker Service):
1. **Oryggi Sync** - Every 6 hours
2. **SLA Monitor** - Every 5 minutes
3. **Email Queue Processor** - Continuous
4. **Escalation Processor** - Every 15 minutes
5. **Cleanup Old Data** - Daily at 2 AM

---

## 🧪 Testing

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Angular tests
cd frontend
ng test
```

---

## 📝 Environment Variables

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ComplaintManagement;User Id=sa;Password=YourPassword;TrustServerCertificate=true",
    "OryggiConnection": "Server=LAPTOP-NF9BTG7Q\\SQLEXPRESS;Database=Oryggi;User Id=sa;Password=admin@123;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-characters",
    "Issuer": "ComplaintManagementAPI",
    "Audience": "ComplaintManagementClient",
    "ExpiryMinutes": 60
  },
  "Oryggi": {
    "SyncEnabled": true,
    "SyncIntervalHours": 6
  }
}
```

---

## 🚢 Deployment

### Docker Deployment
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Manual Deployment
1. Publish API: `dotnet publish -c Release`
2. Publish Worker: `dotnet publish -c Release`
3. Build Angular: `ng build --configuration production`
4. Deploy to IIS/Azure/AWS

---

## 📚 Documentation

- API Documentation: `https://localhost:5001/swagger`
- Architecture: See `/docs/ARCHITECTURE.md`
- Database Schema: See `/docs/DATABASE_SCHEMA.md`

---

## 🤝 Contributing

1. Follow Clean Architecture principles
2. Write unit tests for new features
3. Follow C# coding conventions
4. Use async/await for I/O operations
5. Add XML documentation comments

---

**Version**: 1.0.0
**Status**: In Development
**Last Updated**: 2025-10-11
