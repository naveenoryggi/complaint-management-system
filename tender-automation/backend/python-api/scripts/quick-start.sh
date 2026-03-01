#!/bin/bash

# Quick Start Script for Tender Automation API
# This script sets up and runs the development environment

echo "🚀 Tender Automation API - Quick Start"
echo "======================================"

# Check if .env exists
if [ ! -f .env ]; then
    echo "⚠️  .env file not found. Creating from .env.example..."
    cp .env.example .env
    echo "✅ Created .env file. Please edit it with your configuration."
    echo ""
    echo "Required settings:"
    echo "  - JWT_SECRET_KEY (must match .NET API)"
    echo "  - ANTHROPIC_API_KEY (get from anthropic.com)"
    echo ""
    read -p "Press Enter after updating .env..."
fi

# Check if virtual environment exists
if [ ! -d "venv" ]; then
    echo "📦 Creating virtual environment..."
    python -m venv venv
fi

# Activate virtual environment
echo "🔄 Activating virtual environment..."
source venv/bin/activate

# Install dependencies
echo "📚 Installing dependencies..."
pip install -r requirements.txt

# Run database migrations
echo "🗄️  Running database migrations..."
alembic upgrade head

# Start the server
echo "🚀 Starting FastAPI server..."
echo ""
echo "API will be available at:"
echo "  - http://localhost:8000"
echo "  - Swagger Docs: http://localhost:8000/api/v1/docs"
echo ""
uvicorn app.main:app --reload --port 8000
