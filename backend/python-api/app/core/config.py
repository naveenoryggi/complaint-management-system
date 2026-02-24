"""
Configuration settings for Tender Automation API
"""
from pydantic_settings import BaseSettings
from typing import Optional


class Settings(BaseSettings):
    """Application settings"""

    # API Settings
    API_V1_PREFIX: str = "/api/v1"
    PROJECT_NAME: str = "Tender Automation API"
    VERSION: str = "1.0.0"
    DEBUG: bool = False

    # CORS
    CORS_ORIGINS: list = [
        "http://localhost:4200",
        "http://localhost:5000",
        "http://127.0.0.1:4200",
        "http://127.0.0.1:5000"
    ]

    # Database - SQL Server (using pyodbc driver)
    DATABASE_URL: str = "mssql+pyodbc://sa:admin%40123@LAPTOP-NF9BTG7Q\\SQLEXPRESS/ComplaintManagementDB?driver=ODBC+Driver+17+for+SQL+Server&TrustServerCertificate=yes"
    DB_POOL_SIZE: int = 10
    DB_MAX_OVERFLOW: int = 20

    # JWT Authentication (validate tokens from .NET API)
    JWT_SECRET_KEY: str = "development-secret-key-min-32-chars-long-for-jwt-token-signing"  # Must match .NET API (Development)
    JWT_ALGORITHM: str = "HS256"
    JWT_ISSUER: str = "ComplaintManagementSystem"
    JWT_AUDIENCE: str = "ComplaintManagementAPI"

    # Anthropic Claude API
    ANTHROPIC_API_KEY: str = ""  # Set via environment variable
    ANTHROPIC_MODEL: str = "claude-sonnet-4-5-20250929"
    MAX_TOKENS_PER_REQUEST: int = 4096

    # File Storage
    UPLOAD_DIR: str = "./uploads"
    MAX_UPLOAD_SIZE: int = 50 * 1024 * 1024  # 50 MB
    ALLOWED_EXTENSIONS: set = {".pdf", ".docx", ".doc", ".txt", ".png", ".jpg", ".jpeg"}

    # Redis Cache
    REDIS_URL: str = "redis://localhost:6379/0"
    CACHE_TTL: int = 3600  # 1 hour

    # Rate Limiting
    RATE_LIMIT_PER_MINUTE: int = 60
    AI_RATE_LIMIT_PER_HOUR: int = 100

    # Logging
    LOG_LEVEL: str = "INFO"

    class Config:
        env_file = ".env"
        case_sensitive = True


settings = Settings()
