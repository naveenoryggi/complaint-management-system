# Tender Automation API - Python FastAPI

AI-powered backend for tender document management, generation, and assembly.

## Features

- 🔐 **JWT Authentication** - Validates tokens from .NET API
- 📄 **Document Management** - Upload, tag, search company documents
- 🤖 **AI Document Generation** - Claude API integration for tender documents
- 📦 **PDF Assembly** - Merge, add covers, apply letterheads
- 🎯 **Tender Management** - Track tenders, deadlines, and requirements

## Tech Stack

- **Framework**: FastAPI 0.115.0
- **Database**: PostgreSQL (shared with .NET API)
- **ORM**: SQLAlchemy (async)
- **Migrations**: Alembic
- **AI**: Anthropic Claude API
- **Cache**: Redis
- **Document Processing**: python-docx, PyPDF2, reportlab

## Quick Start

### 1. Environment Setup

Copy the example environment file:

```bash
cp .env.example .env
```

Edit `.env` with your configuration:

```env
# Database (use existing PostgreSQL)
DATABASE_URL=postgresql+asyncpg://complaint_user:complaint_pass_dev@localhost:5432/complaint_management

# JWT (MUST match .NET API settings)
JWT_SECRET_KEY=your-super-secret-jwt-key-change-in-production-min-32-chars

# Claude API
ANTHROPIC_API_KEY=your-anthropic-api-key-here
```

### 2. Install Dependencies

```bash
# Create virtual environment
python -m venv venv

# Activate virtual environment
# Windows:
venv\Scripts\activate
# Mac/Linux:
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt
```

### 3. Run Database Migrations

```bash
# Run migrations to create tables
alembic upgrade head

# To create a new migration:
# alembic revision --autogenerate -m "description"
```

### 4. Start the Server

```bash
# Development mode (with auto-reload)
uvicorn app.main:app --reload --port 8000

# Or use the main.py directly:
python -m app.main
```

The API will be available at:
- **API**: http://localhost:8000
- **Swagger Docs**: http://localhost:8000/api/v1/docs
- **ReDoc**: http://localhost:8000/api/v1/redoc

## Docker Setup

### Using Docker Compose (Recommended)

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f tender-api

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

## Project Structure

```
app/
├── main.py              # FastAPI application entry point
├── api/
│   └── v1/
│       └── endpoints/   # API endpoints
│           ├── documents.py
│           ├── tenders.py
│           ├── ai.py
│           └── assembly.py
├── core/
│   ├── config.py        # Configuration and settings
│   ├── db.py            # Database connection
│   └── security.py      # JWT validation
├── models/              # SQLAlchemy models
│   ├── tender.py
│   ├── document.py
│   └── ai_generation.py
├── schemas/             # Pydantic schemas (request/response)
├── services/            # Business logic
│   ├── document_service.py
│   ├── ai_service.py
│   └── assembly_service.py
└── prompts/             # AI prompt templates

alembic/
└── versions/            # Database migrations
```

## API Endpoints

### Health Check
- `GET /` - Root endpoint
- `GET /health` - Health check

### Documents (Coming Soon)
- `POST /api/v1/documents/upload` - Upload document
- `GET /api/v1/documents` - List documents
- `GET /api/v1/documents/{id}` - Get document details
- `PUT /api/v1/documents/{id}` - Update document
- `DELETE /api/v1/documents/{id}` - Delete document
- `GET /api/v1/documents/search` - Search documents

### Tenders (Coming Soon)
- `POST /api/v1/tenders` - Create tender
- `GET /api/v1/tenders` - List tenders
- `GET /api/v1/tenders/{id}` - Get tender details
- `PUT /api/v1/tenders/{id}` - Update tender
- `DELETE /api/v1/tenders/{id}` - Delete tender

### AI Generation (Coming Soon)
- `POST /api/v1/ai/generate` - Generate document with AI
- `GET /api/v1/ai/templates` - List prompt templates
- `GET /api/v1/ai/history` - Generation history

### Assembly (Coming Soon)
- `POST /api/v1/assembly/merge` - Merge PDFs
- `POST /api/v1/assembly/export/{tender_id}` - Export tender package

## Database Migrations

### Create a new migration
```bash
alembic revision --autogenerate -m "Add new field to tender"
```

### Apply migrations
```bash
alembic upgrade head
```

### Rollback migration
```bash
alembic downgrade -1
```

### View migration history
```bash
alembic history
```

## Testing

```bash
# Run all tests
pytest

# Run with coverage
pytest --cov=app tests/

# Run specific test file
pytest tests/test_documents.py
```

## Development

### Code Formatting
```bash
# Format code
black app/

# Lint code
ruff check app/
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `DATABASE_URL` | PostgreSQL connection string | Required |
| `JWT_SECRET_KEY` | JWT secret (must match .NET) | Required |
| `ANTHROPIC_API_KEY` | Claude API key | Required |
| `REDIS_URL` | Redis connection string | `redis://localhost:6379/1` |
| `UPLOAD_DIR` | File upload directory | `./uploads` |
| `MAX_FILE_SIZE` | Max upload size in bytes | `104857600` (100MB) |
| `ENVIRONMENT` | Environment (development/production) | `development` |
| `DEBUG` | Enable debug mode | `false` |

## Integration with .NET API

This Python API validates JWT tokens issued by the existing .NET authentication API. Ensure:

1. **JWT Secret** matches exactly between .NET and Python
2. **Issuer** and **Audience** match
3. Both APIs connect to the **same PostgreSQL database**

The .NET API handles:
- User authentication (`POST /api/auth/login`)
- Token refresh (`POST /api/auth/refresh`)
- User management

The Python API handles:
- Document management
- AI generation
- Tender management
- Document assembly

## Troubleshooting

### Cannot connect to database
- Check PostgreSQL is running: `docker ps`
- Verify DATABASE_URL in `.env`
- Test connection: `psql -U complaint_user -d complaint_management`

### JWT token validation fails
- Verify JWT_SECRET_KEY matches .NET API
- Check token expiration
- Ensure Issuer and Audience match

### Claude API errors
- Verify ANTHROPIC_API_KEY is set correctly
- Check API quota/limits
- Review Claude API documentation

## License

Proprietary - All rights reserved
