"""
Security and authentication utilities
Validates JWT tokens issued by .NET API
"""
from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from jose import JWTError, jwt
from typing import Optional
from datetime import datetime
import uuid

from app.core.config import settings

security = HTTPBearer()


class TokenData:
    """Token payload data"""
    def __init__(self, user_id: str, tenant_id: str, email: str, roles: list[str]):
        self.user_id = user_id
        self.tenant_id = tenant_id
        self.email = email
        self.roles = roles


def verify_token(credentials: HTTPAuthorizationCredentials = Depends(security)) -> TokenData:
    """
    Verify JWT token from .NET API
    """
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )

    try:
        token = credentials.credentials

        # Decode JWT token
        payload = jwt.decode(
            token,
            settings.JWT_SECRET_KEY,
            algorithms=[settings.JWT_ALGORITHM],
            options={"verify_aud": False, "verify_iss": False}  # .NET uses different claims
        )

        # Extract user information from token (.NET uses long claim URIs)
        user_id: str = (
            payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier") or
            payload.get("sub") or
            payload.get("nameid")
        )
        tenant_id: str = (
            payload.get("CompanyId") or
            payload.get("tenant_id") or
            payload.get("TenantId")
        )
        email: str = (
            payload.get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") or
            payload.get("email")
        )
        roles_claim = (
            payload.get("RoleCode") or
            payload.get("role") or
            payload.get("roles") or
            []
        )

        # Handle both single role string and role array
        if isinstance(roles_claim, str):
            roles = [roles_claim]
        else:
            roles = roles_claim

        if user_id is None or tenant_id is None:
            raise credentials_exception

        # Validate expiration
        exp = payload.get("exp")
        if exp and datetime.fromtimestamp(exp) < datetime.now():
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="Token has expired"
            )

        return TokenData(
            user_id=user_id,
            tenant_id=tenant_id,
            email=email or "",
            roles=roles
        )

    except JWTError as e:
        print(f"JWT Error: {e}")
        raise credentials_exception


def require_role(required_role: str):
    """
    Dependency to require specific role
    """
    def role_checker(token_data: TokenData = Depends(verify_token)) -> TokenData:
        if required_role not in token_data.roles:
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail=f"Role '{required_role}' required"
            )
        return token_data

    return role_checker


# Current user dependency
def get_current_user(token_data: TokenData = Depends(verify_token)) -> TokenData:
    """Get current authenticated user"""
    return token_data
