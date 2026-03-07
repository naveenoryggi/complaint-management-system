"""Application configuration using Pydantic Settings."""
from typing import List
from pydantic import Field, field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Application settings loaded from environment variables."""

    # Application
    app_name: str = "Tender Automation API"
    version: str = "0.1.0"
    environment: str = Field(default="development")
    debug: bool = Field(default=False)

    # Server
    host: str = Field(default="0.0.0.0")
    port: int = Field(default=8000)

    # Database
    database_url: str = Field(
        description="SQL Server connection string (mssql+aioodbc)"
    )

    # JWT Configuration (Must match .NET API settings)
    jwt_secret_key: str = Field(
        description="JWT secret key - must match .NET JWT:SecretKey"
    )
    jwt_algorithm: str = Field(default="HS256")
    jwt_issuer: str = Field(default="ComplaintManagement")
    jwt_audience: str = Field(default="ComplaintManagement")

    # Claude API (optional — can be configured via AI Provider Settings instead)
    anthropic_api_key: str = Field(
        default="",
        description="Anthropic API key for Claude (fallback if no DB provider configured)"
    )

    # Redis
    redis_url: str = Field(default="redis://localhost:6379/1")

    # File Storage
    upload_dir: str = Field(default="./uploads")
    max_file_size: int = Field(default=104857600)  # 100MB
    allowed_extensions: str = Field(
        default=".pdf,.docx,.doc,.xlsx,.xls,.jpg,.jpeg,.png,.gif"
    )

    # CORS
    cors_origins: str = Field(
        default="http://localhost:4200,http://localhost:3001"
    )

    @field_validator("cors_origins")
    @classmethod
    def parse_cors_origins(cls, v: str) -> List[str]:
        """Parse comma-separated CORS origins into list."""
        return [origin.strip() for origin in v.split(",")]

    @field_validator("allowed_extensions")
    @classmethod
    def parse_allowed_extensions(cls, v: str) -> List[str]:
        """Parse comma-separated extensions into list."""
        return [ext.strip().lower() for ext in v.split(",")]

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False
    )


# Global settings instance
settings = Settings()
