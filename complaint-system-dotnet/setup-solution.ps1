# PowerShell script to create .NET solution structure
# Run this with: powershell -ExecutionPolicy Bypass -File setup-solution.ps1

Write-Host "🚀 Creating .NET 8 Solution Structure..." -ForegroundColor Green
Write-Host ""

# Create solution
Write-Host "📦 Creating solution..." -ForegroundColor Cyan
dotnet new sln -n ComplaintManagementSystem

# Create src directory
New-Item -ItemType Directory -Force -Path "src" | Out-Null

# 1. Create Domain Layer (Class Library)
Write-Host "📦 Creating Domain project..." -ForegroundColor Cyan
dotnet new classlib -n ComplaintManagement.Domain -o src/ComplaintManagement.Domain -f net8.0
dotnet sln add src/ComplaintManagement.Domain/ComplaintManagement.Domain.csproj

# 2. Create Application Layer (Class Library)
Write-Host "📦 Creating Application project..." -ForegroundColor Cyan
dotnet new classlib -n ComplaintManagement.Application -o src/ComplaintManagement.Application -f net8.0
dotnet sln add src/ComplaintManagement.Application/ComplaintManagement.Application.csproj
dotnet add src/ComplaintManagement.Application reference src/ComplaintManagement.Domain

# 3. Create Infrastructure Layer (Class Library)
Write-Host "📦 Creating Infrastructure project..." -ForegroundColor Cyan
dotnet new classlib -n ComplaintManagement.Infrastructure -o src/ComplaintManagement.Infrastructure -f net8.0
dotnet sln add src/ComplaintManagement.Infrastructure/ComplaintManagement.Infrastructure.csproj
dotnet add src/ComplaintManagement.Infrastructure reference src/ComplaintManagement.Domain
dotnet add src/ComplaintManagement.Infrastructure reference src/ComplaintManagement.Application

# 4. Create Shared Layer (Class Library)
Write-Host "📦 Creating Shared project..." -ForegroundColor Cyan
dotnet new classlib -n ComplaintManagement.Shared -o src/ComplaintManagement.Shared -f net8.0
dotnet sln add src/ComplaintManagement.Shared/ComplaintManagement.Shared.csproj

# 5. Create Web API
Write-Host "📦 Creating Web API project..." -ForegroundColor Cyan
dotnet new webapi -n ComplaintManagement.API -o src/ComplaintManagement.API -f net8.0
dotnet sln add src/ComplaintManagement.API/ComplaintManagement.API.csproj
dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Application
dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Infrastructure
dotnet add src/ComplaintManagement.API reference src/ComplaintManagement.Shared

# 6. Create Worker Service
Write-Host "📦 Creating Worker Service project..." -ForegroundColor Cyan
dotnet new worker -n ComplaintManagement.WorkerService -o src/ComplaintManagement.WorkerService -f net8.0
dotnet sln add src/ComplaintManagement.WorkerService/ComplaintManagement.WorkerService.csproj
dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Application
dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Infrastructure
dotnet add src/ComplaintManagement.WorkerService reference src/ComplaintManagement.Shared

# Install NuGet packages
Write-Host "📦 Installing NuGet packages..." -ForegroundColor Cyan

# Domain packages
Write-Host "  - Domain packages..." -ForegroundColor Yellow

# Application packages
Write-Host "  - Application packages..." -ForegroundColor Yellow
dotnet add src/ComplaintManagement.Application package AutoMapper
dotnet add src/ComplaintManagement.Application package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add src/ComplaintManagement.Application package FluentValidation
dotnet add src/ComplaintManagement.Application package FluentValidation.DependencyInjectionExtensions
dotnet add src/ComplaintManagement.Application package MediatR

# Infrastructure packages
Write-Host "  - Infrastructure packages..." -ForegroundColor Yellow
dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add src/ComplaintManagement.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/ComplaintManagement.Infrastructure package Dapper
dotnet add src/ComplaintManagement.Infrastructure package StackExchange.Redis
dotnet add src/ComplaintManagement.Infrastructure package Hangfire.AspNetCore
dotnet add src/ComplaintManagement.Infrastructure package Hangfire.SqlServer

# API packages
Write-Host "  - API packages..." -ForegroundColor Yellow
dotnet add src/ComplaintManagement.API package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/ComplaintManagement.API package Microsoft.AspNetCore.Mvc.Versioning
dotnet add src/ComplaintManagement.API package Swashbuckle.AspNetCore
dotnet add src/ComplaintManagement.API package Serilog.AspNetCore
dotnet add src/ComplaintManagement.API package Serilog.Sinks.Console
dotnet add src/ComplaintManagement.API package Serilog.Sinks.File
dotnet add src/ComplaintManagement.API package BCrypt.Net-Next

# Worker Service packages
Write-Host "  - Worker Service packages..." -ForegroundColor Yellow
dotnet add src/ComplaintManagement.WorkerService package Hangfire.AspNetCore
dotnet add src/ComplaintManagement.WorkerService package Serilog.Extensions.Hosting

Write-Host ""
Write-Host "✅ .NET Solution created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Solution structure:" -ForegroundColor Cyan
Write-Host "  - ComplaintManagement.Domain (Entities)"
Write-Host "  - ComplaintManagement.Application (Business Logic)"
Write-Host "  - ComplaintManagement.Infrastructure (Data Access)"
Write-Host "  - ComplaintManagement.Shared (Utilities)"
Write-Host "  - ComplaintManagement.API (REST API)"
Write-Host "  - ComplaintManagement.WorkerService (Background Jobs)"
Write-Host ""
Write-Host "🎯 Next steps:" -ForegroundColor Yellow
Write-Host "  1. Create entity classes in Domain project"
Write-Host "  2. Create DbContext in Infrastructure project"
Write-Host "  3. Run: dotnet ef migrations add InitialCreate"
Write-Host "  4. Run: dotnet ef database update"
Write-Host ""
