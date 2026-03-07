"""Security utilities for JWT token validation."""
from datetime import datetime
from typing import Optional, Dict, Any
from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from jose import JWTError, jwt

from app.core.config import settings

# HTTP Bearer token scheme
security = HTTPBearer()


class TokenData:
    """Parsed JWT token data."""

    def __init__(self, payload: Dict[str, Any]):
        # Support both .NET claim names and standard JWT claims
        self.user_id: str = (
            payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            or payload.get("sub", "")
        )
        self.email: str = (
            payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
            or payload.get("email", "")
        )
        self.full_name: str = (
            payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
            or payload.get("name", "")
        )
        self.company_id: str = payload.get("CompanyId", "")
        self.tenant_id: str = (
            payload.get("TenantId")
            or payload.get("tenant_id")
            or payload.get("CompanyId", "")
        )
        self.employee_code: str = payload.get("EmployeeCode", "")
        self.permissions: list = payload.get("Permission", [])
        self.roles: list = payload.get("RoleCode", [])
        portal_user = payload.get("PortalUser", "false")
        self.is_portal_user: bool = str(portal_user).lower() == "true"

        # Standard JWT claims
        self.exp: Optional[int] = payload.get("exp")
        self.iat: Optional[int] = payload.get("iat")
        self.iss: Optional[str] = payload.get("iss")
        self.aud: Optional[str] = payload.get("aud")


def decode_jwt_token(token: str) -> TokenData:
    """
    Decode and validate JWT token issued by .NET API.

    Args:
        token: JWT token string

    Returns:
        TokenData: Parsed token data

    Raises:
        HTTPException: If token is invalid or expired
    """
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )

    try:
        # Decode JWT token
        payload = jwt.decode(
            token,
            settings.jwt_secret_key,
            algorithms=[settings.jwt_algorithm],
            issuer=settings.jwt_issuer,
            audience=settings.jwt_audience,
        )

        # Check expiration
        exp = payload.get("exp")
        if exp is None:
            raise credentials_exception

        if datetime.utcnow().timestamp() > exp:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Token has expired",
                headers={"WWW-Authenticate": "Bearer"},
            )

        # Parse token data
        return TokenData(payload)

    except JWTError as e:
        raise credentials_exception from e


async def get_current_user(
    credentials: HTTPAuthorizationCredentials = Depends(security)
) -> TokenData:
    """
    FastAPI dependency to get current authenticated user from JWT token.

    Usage:
        @app.get("/protected")
        async def protected_route(current_user: TokenData = Depends(get_current_user)):
            # Access current_user.user_id, current_user.tenant_id, etc.
    """
    token = credentials.credentials
    return decode_jwt_token(token)


def require_permission(permission: str):
    """
    Dependency factory to require specific permission.

    Usage:
        @app.post("/admin-only")
        async def admin_route(
            current_user: TokenData = Depends(require_permission("ManageUsers"))
        ):
            # Only users with "ManageUsers" permission can access
    """
    async def permission_checker(
        current_user: TokenData = Depends(get_current_user)
    ) -> TokenData:
        # System Admin and Admin bypass all permission checks
        if "SYSTEM_ADMIN" in current_user.roles or "ADMIN" in current_user.roles:
            return current_user

        if permission not in current_user.permissions:
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail=f"Permission denied. Required permission: {permission}",
            )

        return current_user

    return permission_checker
