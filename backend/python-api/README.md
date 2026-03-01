# Tender Automation Python API

FastAPI backend for AI-powered tender document generation and management.

## Features

- **Tender Management**: Create, track, and manage tenders
- **Document Library**: Upload and organize company documents
- **AI Generation**: Generate tender documents using Claude API
- **Document Assembly**: Merge and package documents

## Architecture

This Python API works alongside the existing .NET API:
- **.NET API (Port 5000)**: Authentication and user management
- **Python API (Port 8000)**: Tender automation and AI features

Both APIs share the same PostgreSQL database and authentication system.

## Prerequisites

- Python 3.12 or higher
- PostgreSQL 16
- Redis (optional, for caching)

## Installation

### 1. Install Python Dependencies

```bash
cd backend/python-api
pip install -r requirements.txt
```

### 2. Configure Environment

Copy `.env.example` to `.env` and update settings:

```bash
cp .env.example .env
```

Edit `.env` with your configuration:
- Set `DATABASE_URL` to your PostgreSQL connection string
- Set `JWT_SECRET_KEY` to match your .NET API secret
- Set `ANTHROPIC_API_KEY` for AI generation (optional for testing)

### 3. Create Database Tables

Run database migrations:

```bash
# Install Alembic if not already installed
pip install alembic

# Create migration
alembic revision --autogenerate -m "Create tender tables"

# Apply migration
alembic upgrade head
```

## Running the API

### Development Mode

```bash
cd backend/python-api
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

The API will be available at:
- API: http://localhost:8000
- Swagger Docs: http://localhost:8000/api/v1/docs
- ReDoc: http://localhost:8000/api/v1/redoc

### Production Mode

```bash
uvicorn app.main:app --host 0.0.0.0 --port 8000 --workers 4
```

## API Endpoints

### Tenders
- `POST /api/v1/tenders` - Create tender
- `GET /api/v1/tenders` - List tenders
- `GET /api/v1/tenders/{id}` - Get tender
- `PUT /api/v1/tenders/{id}` - Update tender
- `DELETE /api/v1/tenders/{id}` - Delete tender

### Documents
- `POST /api/v1/documents/upload` - Upload document
- `GET /api/v1/documents` - List documents
- `GET /api/v1/documents/{id}` - Get document
- `DELETE /api/v1/documents/{id}` - Delete document

### AI Generation
- `POST /api/v1/ai/generate` - Generate content
- `GET /api/v1/ai/templates` - List templates

### Assembly
- `POST /api/v1/assembly/merge` - Merge documents
- `POST /api/v1/assembly/export/{tender_id}` - Export tender package

## Authentication

The API validates JWT tokens issued by the .NET API. Include the token in requests:

```
Authorization: Bearer <your-jwt-token>
```

## Configuration

Key settings in `app/core/config.py`:

- `API_V1_PREFIX`: API route prefix (default: /api/v1)
- `DATABASE_URL`: PostgreSQL connection string
- `JWT_SECRET_KEY`: Must match .NET API
- `ANTHROPIC_API_KEY`: Claude API key for AI generation
- `UPLOAD_DIR`: Document upload directory

## Database Schema

### Tables Created
- `tenders`: Tender tracking
- `documents`: Document library
- `tender_documents`: Tender-document associations
- `ai_generations`: AI usage tracking

All tables include `tenant_id` for multi-tenancy isolation.

## Development

### Project Structure

```
backend/python-api/
├── app/
│   ├── api/v1/endpoints/  # API endpoints
│   ├── core/              # Config, security, database
│   ├── models/            # SQLAlchemy models
│   ├── schemas/           # Pydantic schemas
│   ├── services/          # Business logic
│   └── main.py            # FastAPI app
├── requirements.txt
└── README.md
```

### Adding New Endpoints

1. Create endpoint file in `app/api/v1/endpoints/`
2. Add router to `app/api/v1/router.py`
3. Create schemas in `app/schemas/`
4. Add models in `app/models/` if needed

## Troubleshooting

### Port Already in Use
```bash
# Windows
netstat -ano | findstr :8000
taskkill /PID <pid> /F
```

### Database Connection Issues
- Verify PostgreSQL is running
- Check DATABASE_URL in .env
- Ensure .NET API uses same database

### JWT Validation Errors
- Ensure JWT_SECRET_KEY matches .NET API
- Check token hasn't expired
- Verify token includes tenant_id claim

## License

Proprietary - Complaint Management System
