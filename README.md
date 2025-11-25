# Complaint Management System

Enterprise-grade Complaint Management System with .NET 8 API Backend and Angular 18 Frontend.

## Features

- 🎯 **Complete Complaint Lifecycle Management**
- 📧 **Email Integration** (OAuth 2.0 support for Gmail & Office 365)
- ⚡ **Auto-Escalation & SLA Management**
- 📊 **Real-time Dashboard & Analytics**
- 👥 **Multi-tenant Architecture**
- 🔐 **Role-based Access Control (RBAC)**
- 📱 **Responsive Modern UI**
- 🔄 **Oryggi ERP Integration**
- 💬 **Communication Templates**
- 📈 **Priority & Status Masters**

## Technology Stack

### Backend
- .NET 8 Web API
- Entity Framework Core 8
- SQL Server
- JWT Authentication
- AutoMapper
- MediatR (CQRS Pattern)
- Serilog Logging
- MailKit for Email

### Frontend
- Angular 18
- TypeScript
- RxJS
- Tailwind CSS with Glassmorphism Theme
- Chart.js for Analytics

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (with npm)
- [SQL Server](https://www.microsoft.com/sql-server) (Express Edition or higher)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/naveenoryggi/complaint-management-system.git
cd complaint-management-system
```

### 2. Configure Backend

```bash
cd complaint-system-dotnet/src/ComplaintManagement.API

# Copy template and configure secrets
copy appsettings.template.json appsettings.json

# Edit appsettings.json with your configuration:
# - ConnectionStrings (SQL Server)
# - JwtSettings SecretKey
# - Encryption AES Key & IV
# - AzureAd credentials (for OAuth)
```

**Important Configuration Values:**

- **ConnectionStrings.DefaultConnection**: Your SQL Server connection string
- **JwtSettings.SecretKey**: Strong secret key (min 32 characters)
- **Encryption.AES.Key**: Base64-encoded 32-byte encryption key
- **Encryption.AES.IV**: Base64-encoded 16-byte initialization vector
- **AzureAd** (Optional for OAuth 2.0):
  - ClientId: Your Azure AD Application ID
  - ClientSecret: Your Azure AD Application Secret
  - TenantId: Your Azure AD Tenant ID

### 3. Setup Database

```bash
# Run migrations
dotnet ef database update

# Or just run the application (migrations run automatically)
dotnet run
```

### 4. Configure Frontend

```bash
cd ../../complaint-system-angular

# Install dependencies
npm install

# Update API endpoint in src/environments/environment.ts if needed
# Default: http://localhost:5000
```

### 5. Run the Application

**Backend** (Terminal 1):
```bash
cd complaint-system-dotnet/src/ComplaintManagement.API
dotnet run
```
API will be available at: `http://localhost:5000`

**Frontend** (Terminal 2):
```bash
cd complaint-system-angular
npm start
```
Application will open at: `http://localhost:4200`

## Default Login Credentials

```
Email: admin@complaintmanagement.com
Password: Admin@123
```

**⚠️ IMPORTANT:** Change the default password immediately after first login!

## Installation Package

For production deployment, use the pre-built installer:

1. Download `ComplaintManagementSetup-v1.0.0.exe` from [Releases](https://github.com/naveenoryggi/complaint-management-system/releases)
2. Run the installer
3. Follow the wizard:
   - Configure SQL Server connection
   - Choose to create new database or connect to existing
   - Select installation options:
     - ☑ Install API as Windows Service
     - ☑ Configure IIS Website
4. Access the application at `http://localhost`

## Project Structure

```
complaint-management-system/
├── complaint-system-dotnet/          # .NET Backend
│   ├── src/
│   │   ├── ComplaintManagement.API/           # Web API Layer
│   │   ├── ComplaintManagement.Application/   # Business Logic
│   │   ├── ComplaintManagement.Domain/        # Domain Models
│   │   ├── ComplaintManagement.Infrastructure/# Data Access
│   │   └── ComplaintManagement.Shared/        # Shared Utilities
│
├── complaint-system-angular/         # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/         # UI Components
│   │   │   ├── services/           # API Services
│   │   │   ├── models/             # TypeScript Models
│   │   │   ├── guards/             # Route Guards
│   │   │   └── interceptors/       # HTTP Interceptors
│
├── ComplaintManagementSetup.iss     # Inno Setup Installer Script
├── installer-output/                 # Built Installer Package
└── README.md
```

## API Documentation

Once the backend is running, access Swagger UI at:
- `http://localhost:5000/swagger`

## Configuration

### Email Settings

Configure email servers for:
- Auto-acknowledgment emails
- Status update notifications
- Email-to-ticket creation

Supports:
- Gmail (OAuth 2.0 or App Password)
- Office 365 (OAuth 2.0)
- Custom SMTP servers

### SLA Configuration

Set up Service Level Agreements based on:
- Priority levels
- Categories
- Custom rules

### Auto-Escalation

Configure automatic escalation rules:
- Time-based escalation
- Multi-level escalation matrix
- Notification triggers

## Development

### Backend Development

```bash
cd complaint-system-dotnet/src/ComplaintManagement.API
dotnet watch run
```

### Frontend Development

```bash
cd complaint-system-angular
ng serve --open
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName
```

## Testing

### Run Tests
```bash
# Backend tests
cd complaint-system-dotnet
dotnet test

# Frontend tests
cd complaint-system-angular
ng test
```

## Security Features

- ✅ JWT Token Authentication
- ✅ Role-based Authorization
- ✅ AES Encryption for sensitive data
- ✅ Rate Limiting (IP & Client-based)
- ✅ XSS Protection
- ✅ CSRF Protection
- ✅ SQL Injection Prevention (EF Core)
- ✅ Password Hashing (BCrypt)
- ✅ OAuth 2.0 Support

## License

Proprietary - All Rights Reserved

## Support

For support, email: naveen.chandra@oryggitech.com

## Acknowledgments

Built with ❤️ by Oryggi Technologies

---

**🤖 Generated with [Claude Code](https://claude.com/claude-code)**
