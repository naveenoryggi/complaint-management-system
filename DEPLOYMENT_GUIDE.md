# Complaint Management System - Deployment Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Environment Configuration](#environment-configuration)
3. [Database Setup](#database-setup)
4. [Backend Deployment](#backend-deployment)
5. [Frontend Deployment](#frontend-deployment)
6. [Post-Deployment Verification](#post-deployment-verification)
7. [Rollback Procedures](#rollback-procedures)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Server Requirements

**Backend Server (IIS/Linux)**
- OS: Windows Server 2019+ or Ubuntu 20.04+
- .NET 8.0 Runtime
- SQL Server 2019+ (or Azure SQL Database)
- RAM: 4GB minimum, 8GB recommended
- Storage: 20GB minimum
- CPU: 2 cores minimum, 4 cores recommended

**Frontend Server (IIS/Nginx)**
- Static file hosting capability
- HTTPS support (SSL certificate)
- Gzip/Brotli compression support

### Software Requirements
- .NET 8.0 SDK (for build)
- Node.js 18.x or higher
- npm 9.x or higher
- SQL Server Management Studio (SSMS) or Azure Data Studio
- Git (for version control)

### Access Requirements
- Database server access (SQL authentication or Windows authentication)
- SMTP server credentials (for email notifications)
- OAuth credentials (if using OAuth email authentication)
- SSL certificate for HTTPS

---

## Environment Configuration

### Backend Configuration (`appsettings.Production.json`)

Create `appsettings.Production.json` in the API project:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ComplaintManagementDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "GENERATE_STRONG_SECRET_KEY_HERE_MINIMUM_32_CHARACTERS",
    "Issuer": "ComplaintManagementSystem",
    "Audience": "ComplaintManagementAPI",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "PasswordPolicy": {
    "MinimumLength": 8,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigit": true,
    "RequireSpecialCharacter": true,
    "MaximumFailedAttempts": 5,
    "LockoutDurationMinutes": 30,
    "PasswordExpirationDays": 90
  },
  "EmailSettings": {
    "SmtpServer": "smtp.your-domain.com",
    "SmtpPort": 587,
    "SmtpUsername": "notifications@your-domain.com",
    "SmtpPassword": "YOUR_SMTP_PASSWORD",
    "FromEmail": "noreply@your-domain.com",
    "FromName": "Complaint Management System",
    "UseSsl": true
  },
  "AutoResponse": {
    "Enabled": true,
    "SendAcknowledgmentOnWebCreation": true,
    "StatusChangeNotifications": true,
    "AssignmentNotifications": true,
    "ResolutionNotifications": true
  },
  "PasswordReset": {
    "TokenExpirationHours": 24,
    "MaxRequestsPerHour": 3,
    "ResetLinkBaseUrl": "https://your-domain.com/reset-password"
  },
  "Cors": {
    "AllowedOrigins": ["https://your-domain.com"],
    "AllowCredentials": true
  },
  "SLA": {
    "DefaultResponseTimeHours": 24,
    "DefaultResolutionTimeHours": 72,
    "EnableAutoEscalation": true,
    "EscalationCheckIntervalMinutes": 60
  }
}
```

### Frontend Configuration

Update `complaint-system-angular/src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.your-domain.com',
  apiVersion: 'v1',
  enableDebugInfo: false,
  logLevel: 'error',
  sessionTimeoutMinutes: 60,
  defaultLanguage: 'en',
  defaultTimeZone: 'UTC',
  maxFileUploadSizeMB: 10,
  allowedFileTypes: ['.pdf', '.jpg', '.jpeg', '.png', '.doc', '.docx']
};
```

---

## Database Setup

### 1. Create Database

```sql
-- Connect to your SQL Server instance
CREATE DATABASE ComplaintManagementDb;
GO

-- Create application user (recommended for production)
CREATE LOGIN ComplaintAppUser WITH PASSWORD = 'STRONG_PASSWORD_HERE';
GO

USE ComplaintManagementDb;
GO

CREATE USER ComplaintAppUser FOR LOGIN ComplaintAppUser;
GO

-- Grant necessary permissions
ALTER ROLE db_datareader ADD MEMBER ComplaintAppUser;
ALTER ROLE db_datawriter ADD MEMBER ComplaintAppUser;
ALTER ROLE db_ddladmin ADD MEMBER ComplaintAppUser; -- For migrations
GO
```

### 2. Run Database Migrations

**Option A: Using Command Line**

```bash
cd complaint-system-dotnet/src/ComplaintManagement.API

# Update connection string in appsettings.Production.json first
dotnet ef database update --environment Production
```

**Option B: Using SQL Script**

Generate migration script:

```bash
dotnet ef migrations script --output migration-script.sql --idempotent
```

Then run the script in SSMS or Azure Data Studio.

### 3. Seed Initial Data

The seeder will automatically run on first startup, creating:
- Default roles (Admin, Handler, Complainant)
- Default permissions
- System categories
- Event types
- Communication templates

---

## Backend Deployment

### Option 1: Deploy to IIS (Windows)

**1. Build the Application**

```bash
cd complaint-system-dotnet/src/ComplaintManagement.API

dotnet publish -c Release -o ./publish
```

**2. Create IIS Application Pool**

```powershell
# Open IIS Manager
# Create new Application Pool
Name: ComplaintManagementAPI
.NET CLR Version: No Managed Code
Managed Pipeline Mode: Integrated
Identity: ApplicationPoolIdentity (or custom account with DB access)
```

**3. Create IIS Website**

```powershell
# In IIS Manager:
# - Right-click Sites > Add Website
# - Site name: ComplaintManagementAPI
# - Physical path: C:\inetpub\ComplaintManagementAPI
# - Application pool: ComplaintManagementAPI
# - Binding: https on port 443
# - SSL certificate: Select your certificate
```

**4. Copy Published Files**

```powershell
# Copy all files from publish folder to IIS path
Copy-Item -Path .\publish\* -Destination C:\inetpub\ComplaintManagementAPI -Recurse -Force
```

**5. Configure Web.config**

Ensure `web.config` has correct settings:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet"
                arguments=".\ComplaintManagement.API.dll"
                stdoutLogEnabled="true"
                stdoutLogFile=".\logs\stdout"
                hostingModel="inprocess">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

### Option 2: Deploy to Linux (Ubuntu/Nginx)

**1. Install Prerequisites**

```bash
# Install .NET Runtime
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0

# Install Nginx
sudo apt-get install -y nginx
```

**2. Build and Copy Application**

```bash
# Build on development machine
dotnet publish -c Release -o ./publish

# Copy to server
scp -r ./publish user@server:/var/www/complaint-api
```

**3. Create Systemd Service**

Create `/etc/systemd/system/complaint-api.service`:

```ini
[Unit]
Description=Complaint Management API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/complaint-api
ExecStart=/usr/bin/dotnet /var/www/complaint-api/ComplaintManagement.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=complaint-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

**4. Start Service**

```bash
sudo systemctl daemon-reload
sudo systemctl enable complaint-api
sudo systemctl start complaint-api
sudo systemctl status complaint-api
```

**5. Configure Nginx**

Create `/etc/nginx/sites-available/complaint-api`:

```nginx
server {
    listen 80;
    server_name api.your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.your-domain.com;

    ssl_certificate /etc/ssl/certs/your-cert.crt;
    ssl_certificate_key /etc/ssl/private/your-key.key;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable site:

```bash
sudo ln -s /etc/nginx/sites-available/complaint-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Frontend Deployment

### Build for Production

```bash
cd complaint-system-angular

# Install dependencies
npm install

# Build for production
npm run build:prod
```

This creates optimized files in `dist/complaint-system-angular/`.

### Option 1: Deploy to IIS

**1. Create IIS Website**

```powershell
# In IIS Manager:
# - Create new Application Pool (No Managed Code)
# - Create new Website
# - Physical path: C:\inetpub\ComplaintManagementWeb
# - Binding: https on port 443
```

**2. Copy Build Files**

```powershell
Copy-Item -Path .\dist\complaint-system-angular\* -Destination C:\inetpub\ComplaintManagementWeb -Recurse -Force
```

**3. Create web.config**

Create `web.config` in the root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Angular Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff2" />
    </staticContent>
    <httpCompression>
      <dynamicTypes>
        <add mimeType="application/javascript" enabled="true" />
        <add mimeType="text/css" enabled="true" />
      </dynamicTypes>
      <staticTypes>
        <add mimeType="application/javascript" enabled="true" />
        <add mimeType="text/css" enabled="true" />
      </staticTypes>
    </httpCompression>
  </system.webServer>
</configuration>
```

### Option 2: Deploy to Nginx

**1. Copy Build Files**

```bash
sudo mkdir -p /var/www/complaint-web
sudo cp -r dist/complaint-system-angular/* /var/www/complaint-web/
sudo chown -R www-data:www-data /var/www/complaint-web
```

**2. Configure Nginx**

Create `/etc/nginx/sites-available/complaint-web`:

```nginx
server {
    listen 80;
    server_name your-domain.com www.your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com www.your-domain.com;

    ssl_certificate /etc/ssl/certs/your-cert.crt;
    ssl_certificate_key /etc/ssl/private/your-key.key;

    root /var/www/complaint-web;
    index index.html;

    # Enable gzip compression
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml;
    gzip_min_length 1000;

    # Cache static assets
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|woff|woff2)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Angular routing
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
}
```

Enable and restart:

```bash
sudo ln -s /etc/nginx/sites-available/complaint-web /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Post-Deployment Verification

### 1. Health Check Endpoints

Test backend health:

```bash
curl https://api.your-domain.com/health
# Expected: 200 OK with health status
```

### 2. Database Connectivity

```bash
curl https://api.your-domain.com/api/health/database
# Expected: 200 OK
```

### 3. Authentication Test

```bash
curl -X POST https://api.your-domain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@complaintmanagement.com","password":"Admin@123"}'
# Expected: JWT token response
```

### 4. Frontend Test

1. Navigate to `https://your-domain.com`
2. Verify login page loads
3. Login with admin credentials
4. Verify dashboard loads correctly
5. Test creating a complaint
6. Test assigning a complaint
7. Test SLA display
8. Test email notifications

### 5. Performance Test

Use browser DevTools:
- Initial load time: < 3 seconds
- Time to Interactive: < 5 seconds
- Lighthouse score: > 90

---

## Rollback Procedures

### Database Rollback

If migration causes issues:

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName --environment Production

# Or restore database from backup
sqlcmd -S SERVER -Q "RESTORE DATABASE ComplaintManagementDb FROM DISK='C:\Backups\ComplaintDb.bak' WITH REPLACE"
```

### Application Rollback

**IIS:**
```powershell
# Stop website
Stop-WebSite -Name "ComplaintManagementAPI"

# Restore previous version
Copy-Item -Path C:\Backups\ComplaintAPI\* -Destination C:\inetpub\ComplaintManagementAPI -Recurse -Force

# Start website
Start-WebSite -Name "ComplaintManagementAPI"
```

**Linux:**
```bash
# Stop service
sudo systemctl stop complaint-api

# Restore backup
sudo cp -r /var/backups/complaint-api/* /var/www/complaint-api/

# Start service
sudo systemctl start complaint-api
```

---

## Troubleshooting

### Issue: Database Connection Failure

**Symptoms:** 500 errors, "Cannot connect to database" in logs

**Solutions:**
1. Verify connection string in `appsettings.Production.json`
2. Check SQL Server is running and accessible
3. Verify firewall rules allow connection
4. Test connection using SSMS or Azure Data Studio
5. Check application pool/service account has database permissions

### Issue: CORS Errors

**Symptoms:** Frontend shows "blocked by CORS policy" errors

**Solutions:**
1. Verify `AllowedOrigins` in `appsettings.Production.json` includes frontend URL
2. Ensure HTTPS is used for both frontend and backend
3. Check CORS middleware is registered in `Program.cs`

### Issue: Authentication Failures

**Symptoms:** Users cannot login, "Invalid token" errors

**Solutions:**
1. Verify `JwtSettings.SecretKey` is at least 32 characters
2. Check system clocks are synchronized (token expiration relies on time)
3. Verify `Issuer` and `Audience` match between token generation and validation
4. Check database has user records with correct password hashes

### Issue: Email Notifications Not Sending

**Symptoms:** No emails received after complaint creation/assignment

**Solutions:**
1. Check SMTP settings in `appsettings.Production.json`
2. Verify SMTP credentials are correct
3. Test SMTP connection using telnet or third-party tool
4. Check firewall allows outbound SMTP traffic (port 587/465)
5. Review application logs for email sending errors
6. Verify email templates exist in database

### Issue: High Memory Usage

**Solutions:**
1. Check for memory leaks in logs
2. Restart application pool/service
3. Increase server memory if needed
4. Review Entity Framework query patterns (use `.AsNoTracking()` where appropriate)
5. Implement response caching for frequently accessed data

### Issue: Slow Performance

**Solutions:**
1. Enable database query logging to identify slow queries
2. Add database indexes on frequently queried columns
3. Implement caching (Redis/Memory Cache)
4. Enable response compression in IIS/Nginx
5. Use CDN for static assets
6. Optimize frontend bundle size

---

## Maintenance Tasks

### Daily
- Monitor application logs for errors
- Check email queue for failures
- Verify auto-escalation worker is running

### Weekly
- Review database size and plan for archiving if needed
- Check SLA compliance reports
- Test backup restoration procedures

### Monthly
- Apply security patches to OS and frameworks
- Review and update SSL certificates if expiring soon
- Audit user accounts and permissions
- Review and optimize slow database queries

### Quarterly
- Full security audit
- Load testing
- Review and update documentation
- Plan for capacity upgrades if needed

---

## Support Contacts

**System Administrator:** [Your IT contact]
**Database Administrator:** [Your DBA contact]
**Development Team:** [Your dev team contact]
**Security Team:** [Your security team contact]

---

**Document Version:** 1.0
**Last Updated:** November 15, 2025
**Next Review:** February 15, 2026
