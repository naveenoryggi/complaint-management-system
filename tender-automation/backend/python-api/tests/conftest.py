"""Pytest configuration and fixtures."""
import pytest
import asyncio
from typing import Generator


@pytest.fixture(scope="session")
def event_loop() -> Generator:
    """Create an instance of the default event loop for the test session."""
    loop = asyncio.get_event_loop_policy().new_event_loop()
    yield loop
    loop.close()


# Add more fixtures as needed:
# - test_db: Database session fixture
# - test_client: FastAPI TestClient fixture
# - mock_user: Mock authenticated user fixture
