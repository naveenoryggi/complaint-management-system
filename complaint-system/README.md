# Complaint Management System

Enterprise-grade HRMS Complaint Management Module with Oryggi Integration

## Project Overview

A comprehensive complaint management system that allows employees to log and track complaints regarding attendance, salary, leave, and other HRMS-related issues with flexible multi-level escalation and email alert capabilities.

## Key Features

- **Multi-Level Escalation** (2-5 configurable levels)
- **Email Alert System** (8+ alert types with customizable templates)
- **Oryggi HRMS Integration** (automatic sync, zero impact)
- **Role-Based Access Control** (organizational scopes)
- **World-Class UI/UX** (mobile-responsive, accessible)
- **Enterprise Security** (encryption, audit logs, compliance)

## Technology Stack

### Backend
- Node.js 18+ LTS
- NestJS 10.x (TypeScript framework)
- TypeScript 5.x
- PostgreSQL 15+ (Complaint System DB)
- SQL Server (Oryggi HRMS - Read Only)
- Redis 7.x (Caching + Queue)

### Frontend
- React 18+ with TypeScript
- Next.js 14+ (SSR/SSG)
- Material-UI (MUI) v5
- TanStack Query (React Query v5)
- Zustand (State management)

### Infrastructure
- Docker & Docker Compose
- PostgreSQL 15
- Redis 7
- NGINX (Reverse Proxy)

## Project Structure

```
complaint-system/
├── backend/              # NestJS backend API
│   ├── src/
│   │   ├── modules/
│   │   │   ├── complaints/
│   │   │   ├── escalation/
│   │   │   ├── email-alerts/
│   │   │   ├── roles/
│   │   │   ├── oryggi-sync/
│   │   │   └── users/
│   │   ├── common/
│   │   ├── config/
│   │   └── main.ts
│   ├── test/
│   ├── package.json
│   └── tsconfig.json
│
├── frontend/             # Next.js frontend
│   ├── src/
│   │   ├── app/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── services/
│   │   └── stores/
│   ├── public/
│   ├── package.json
│   └── next.config.js
│
├── docker/               # Docker configurations
│   ├── postgres/
│   ├── redis/
│   └── nginx/
│
├── docs/                 # Documentation
│   └── planning/         # Planning documents
│
├── docker-compose.yml    # Development environment
├── .env.example          # Environment variables template
└── README.md             # This file
```

## Prerequisites

- Node.js 18+ LTS
- Docker & Docker Compose
- SQL Server connection to Oryggi HRMS (for production)
- Git

## Quick Start

### 1. Clone and Setup

```bash
# Navigate to project directory
cd complaint-system

# Copy environment variables
cp .env.example .env

# Edit .env with your configuration
# - Database credentials
# - Oryggi SQL Server connection
# - JWT secrets
# - Email service credentials
```

### 2. Start Development Environment with Docker

```bash
# Start all services (PostgreSQL, Redis, Backend, Frontend)
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

### 3. Manual Setup (Without Docker)

#### Backend Setup

```bash
cd backend

# Install dependencies
npm install

# Run database migrations
npm run migration:run

# Start development server
npm run start:dev

# Backend will run on http://localhost:3000
```

#### Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev

# Frontend will run on http://localhost:3001
```

## Environment Variables

Create a `.env` file in the root directory:

```env
# Application
NODE_ENV=development
PORT=3000

# PostgreSQL (Complaint System Database)
DB_HOST=localhost
DB_PORT=5432
DB_USERNAME=complaint_user
DB_PASSWORD=your_password
DB_DATABASE=complaint_management

# SQL Server (Oryggi HRMS - Read Only)
ORYGGI_DB_HOST=LAPTOP-NF9BTG7Q\SQLEXPRESS
ORYGGI_DB_PORT=1433
ORYGGI_DB_USERNAME=sa
ORYGGI_DB_PASSWORD=admin@123
ORYGGI_DB_DATABASE=Oryggi

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=

# JWT Authentication
JWT_SECRET=your-super-secret-jwt-key-change-in-production
JWT_EXPIRY=15m
JWT_REFRESH_SECRET=your-refresh-token-secret
JWT_REFRESH_EXPIRY=7d

# Email Service (AWS SES / SendGrid)
EMAIL_SERVICE=ses
EMAIL_FROM=noreply@company.com
AWS_SES_REGION=us-east-1
AWS_SES_ACCESS_KEY=your_access_key
AWS_SES_SECRET_KEY=your_secret_key

# File Upload
S3_BUCKET=complaint-attachments
S3_REGION=us-east-1
MAX_FILE_SIZE=10485760  # 10MB

# Frontend URL
FRONTEND_URL=http://localhost:3001

# CORS
CORS_ORIGIN=http://localhost:3001
```

## Database Setup

### PostgreSQL Database

```bash
# Create database
createdb complaint_management

# Run migrations
cd backend
npm run migration:run

# Seed initial data (roles, categories)
npm run seed
```

### Oryggi Connection

The system connects to Oryggi HRMS in **read-only mode** to sync:
- Employee Master Data
- Organization Structure (Company, Branch, Department, Section)

No changes are made to the Oryggi database.

## Development

### Backend Development

```bash
cd backend

# Start development server with hot-reload
npm run start:dev

# Run tests
npm run test

# Run e2e tests
npm run test:e2e

# Lint code
npm run lint

# Format code
npm run format
```

### Frontend Development

```bash
cd frontend

# Start development server
npm run dev

# Build for production
npm run build

# Start production server
npm run start

# Run tests
npm run test

# Lint code
npm run lint
```

## API Documentation

Once the backend is running, API documentation is available at:

- Swagger UI: http://localhost:3000/api/docs
- OpenAPI JSON: http://localhost:3000/api/docs-json

## Testing

### Backend Tests

```bash
cd backend

# Unit tests
npm run test

# Integration tests
npm run test:integration

# E2E tests
npm run test:e2e

# Test coverage
npm run test:cov
```

### Frontend Tests

```bash
cd frontend

# Unit tests
npm run test

# E2E tests with Playwright
npm run test:e2e
```

## Deployment

See [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for deployment instructions.

### Production Build

```bash
# Build backend
cd backend
npm run build

# Build frontend
cd frontend
npm run build
```

### Docker Production

```bash
# Build and deploy with Docker
docker-compose -f docker-compose.prod.yml up -d
```

## Project Phases

### Phase 1: Foundation (Current)
- ✅ Project structure setup
- 🔄 Database schema implementation
- 🔄 Oryggi connection setup
- 🔄 Basic authentication

### Phase 2: Core Features
- Complaint creation & management
- Comment and attachment handling
- Role assignment interface
- Basic email notifications

### Phase 3: Escalation & Alerts
- Escalation matrix configuration
- SLA tracking & auto-escalation
- Email template designer
- Alert recipient configuration

### Phase 4: UI/UX
- Employee dashboard
- Manager dashboard
- HR Manager dashboard
- Admin configuration panel

### Phase 5: Integration & Testing
- Oryggi webhook integration
- Batch sync optimization
- End-to-end testing
- Performance optimization

### Phase 6: Deployment
- Production deployment
- Monitoring setup
- User training
- Go-live

## Contributing

1. Create feature branch: `git checkout -b feature/your-feature`
2. Commit changes: `git commit -am 'Add feature'`
3. Push to branch: `git push origin feature/your-feature`
4. Submit pull request

## Documentation

Complete planning and architecture documentation is available in:
- `../MASTER_PLANNING_DOCUMENT.md` - Master planning document
- `../CHUNK_03_COMPLAINT_ROLE_TABLES.md` - Database schema
- `../CHUNK_04_ESCALATION_EMAIL_TABLES.md` - Escalation system
- `../CHUNK_05_ORYGGI_INTEGRATION.md` - Integration details
- `../CHUNK_06_TECHNOLOGY_STACK.md` - Technology stack
- `../CHUNK_07_UI_UX_DESIGN.md` - UI/UX design
- `../CHUNK_08_SECURITY_DEPLOYMENT.md` - Security & deployment

## License

Proprietary - All rights reserved

## Support

For issues and questions, contact the development team.

---

**Version**: 1.0.0
**Status**: Development Phase 1
**Last Updated**: 2025-10-11
