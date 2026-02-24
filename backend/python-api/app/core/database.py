"""
Database connection and session management
"""
from sqlalchemy import create_engine
from sqlalchemy.orm import declarative_base, sessionmaker, Session
from typing import Generator

from app.core.config import settings

# Create sync engine for SQL Server (no good async driver)
# Replace +asyncpg with nothing for sync, or +pyodbc for SQL Server
db_url = settings.DATABASE_URL.replace("+asyncpg", "")

engine = create_engine(
    db_url,
    echo=settings.DEBUG,
    pool_size=settings.DB_POOL_SIZE,
    max_overflow=settings.DB_MAX_OVERFLOW,
    pool_pre_ping=True
)

# Create session factory
SessionLocal = sessionmaker(
    bind=engine,
    expire_on_commit=False,
    autocommit=False,
    autoflush=False
)

# Base class for models
Base = declarative_base()


def get_db() -> Generator[Session, None, None]:
    """
    Dependency function to get database session
    """
    db = SessionLocal()
    try:
        yield db
        db.commit()
    except Exception:
        db.rollback()
        raise
    finally:
        db.close()
