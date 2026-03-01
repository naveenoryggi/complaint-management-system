# Tender Automation Platform - Quick Start Guide

Get your MVP up and running in minutes!

## Prerequisites

- Python 3.12+
- PostgreSQL 16 (from your existing Complaint System)
- Node.js 20+ (for Angular frontend)
- Docker & Docker Compose (optional but recommended)

## Option 1: Docker Compose (Recommended)

### 1. Configure Environment

```bash
cd tender-automation/backend/python-api
cp .env.example .env
```

Edit `.env` with your configuration:

```env
# JWT Secret - MUST match your .NET API
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production-min-32-chars

# Claude API Key (get from https://console.anthropic.com/)
ANTHROPIC_API_KEY=sk-ant-your-api-key-here
```

### 2. Start Services

```bash
cd ../..  # Back to tender-automation/
docker-compose up -d
```

### 3. Run Database Migrations

```bash
docker exec -it tender-api alembic upgrade head
```

### 4. Verify

Open your browser:
- **API**: http://localhost:8000
- **Swagger Docs**: http://localhost:8000/api/v1/docs
- **Health Check**: http://localhost:8000/health

## Option 2: Local Development (Without Docker)

### 1. Set Up Python Backend

```bash
cd tender-automation/backend/python-api

# Create virtual environment
python -m venv venv

# Activate (Windows)
venv\Scripts\activate
# Activate (Mac/Linux)
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt

# Configure environment
cp .env.example .env
# Edit .env with your settings

# Run migrations
alembic upgrade head

# Start server
uvicorn app.main:app --reload --port 8000
```

### 2. Verify Backend

```bash
# Health check
curl http://localhost:8000/health

# View API docs
open http://localhost:8000/api/v1/docs
```

## Test the Document API

### 1. Get JWT Token from .NET API

First, login to get a token from your existing .NET API:

```bash
# Login (replace with your credentials)
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "your-password"
  }'
```

Copy the `accessToken` from the response.

### 2. Upload a Document

```bash
# Upload a test document
curl -X POST http://localhost:8000/api/v1/documents/upload \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -F "file=@/path/to/test.pdf" \
  -F "name=Test Certificate" \
  -F "document_type=certificate" \
  -F "tags=test,demo"
```

### 3. List Documents

```bash
curl -X GET "http://localhost:8000/api/v1/documents?page=1&page_size=20" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### 4. Search Documents

```bash
curl -X GET "http://localhost:8000/api/v1/documents/search?query=certificate&page=1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## API Endpoints Available Now

### Documents

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/documents/upload` | Upload a new document |
| GET | `/api/v1/documents` | List all documents (paginated) |
| GET | `/api/v1/documents/search` | Search documents |
| GET | `/api/v1/documents/{id}` | Get document by ID |
| PUT | `/api/v1/documents/{id}` | Update document metadata |
| DELETE | `/api/v1/documents/{id}` | Delete document |

## What's Working Now (Week 1 Complete!)

✅ **Backend Infrastructure**
- FastAPI application with async SQLAlchemy
- JWT authentication (validates .NET tokens)
- Multi-tenant isolation
- Database migrations (Alembic)
- File storage service

✅ **Document Management**
- Upload documents (PDF, DOCX, images, Excel)
- CRUD operations
- Search and filtering
- Tag management
- Tenant isolation enforced

✅ **Developer Tools**
- Swagger/OpenAPI documentation
- Docker Compose setup
- Development scripts
- Basic unit tests

## Next Steps (Week 2)

🔜 **AI Document Generation** (Coming this week!)
- Claude API integration
- Prompt templates
- Generate technical proposals
- Generate declarations
- DOCX output with formatting

## Troubleshooting

### "Could not validate credentials"
- Ensure JWT_SECRET_KEY matches your .NET API exactly
- Check token hasn't expired (15 min default)
- Verify Issuer and Audience settings

### "Connection refused" to database
```bash
# Check PostgreSQL is running
docker ps | grep postgres

# Or start it manually
docker-compose up postgres -d
```

### "Module not found" errors
```bash
# Reinstall dependencies
pip install -r requirements.txt
```

### File upload fails
```bash
# Check upload directory exists and has write permissions
mkdir -p uploads
chmod 777 uploads  # On Linux/Mac
```

## Environment Variables Reference

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `DATABASE_URL` | Yes | - | PostgreSQL connection string |
| `JWT_SECRET_KEY` | Yes | - | Must match .NET API |
| `ANTHROPIC_API_KEY` | Yes* | - | Required for AI features (Week 2) |
| `REDIS_URL` | No | `redis://localhost:6379/1` | Redis for caching |
| `UPLOAD_DIR` | No | `./uploads` | File storage directory |
| `MAX_FILE_SIZE` | No | `104857600` | Max upload size (100MB) |
| `DEBUG` | No | `false` | Enable debug mode |

## API Documentation

- **Swagger UI**: http://localhost:8000/api/v1/docs (Interactive API testing)
- **ReDoc**: http://localhost:8000/api/v1/redoc (Clean API documentation)
- **OpenAPI JSON**: http://localhost:8000/api/v1/openapi.json

## Development Commands

```bash
# Run tests
pytest

# Format code
black app/

# Lint code
ruff check app/

# Create new migration
alembic revision --autogenerate -m "description"

# View migration history
alembic history

# Rollback migration
alembic downgrade -1
```

## Support

- **Backend README**: `backend/python-api/README.md`
- **Implementation Plan**: `.claude/plans/wild-skipping-axolotl.md`
- **API Docs**: http://localhost:8000/api/v1/docs

---

🎉 **Week 1 Complete!** The document management system is fully operational. Week 2 will add AI-powered document generation with Claude API.
