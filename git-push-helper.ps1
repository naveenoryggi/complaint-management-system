# Git Push Helper Script
# This script makes it easy to commit and push changes to GitHub

param(
    [string]$CommitMessage = "Update: Code changes"
)

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "   Git Push Helper" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Check git status
Write-Host "Checking git status..." -ForegroundColor Yellow
git status --short

Write-Host ""
$changes = git status --porcelain
if (!$changes) {
    Write-Host "No changes to commit." -ForegroundColor Green
    exit 0
}

# Add all changes
Write-Host "Staging all changes..." -ForegroundColor Yellow
git add -A

# Commit with message
Write-Host "Committing changes..." -ForegroundColor Yellow
$fullMessage = "$CommitMessage

🤖 Generated with Claude Code (https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"

git commit -m $fullMessage

# Push to GitHub
Write-Host ""
Write-Host "Pushing to GitHub..." -ForegroundColor Yellow
git push

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=================================" -ForegroundColor Green
    Write-Host "✓ Successfully pushed to GitHub!" -ForegroundColor Green
    Write-Host "=================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Repository: https://github.com/naveenoryggi/complaint-management-system" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "=================================" -ForegroundColor Red
    Write-Host "✗ Failed to push to GitHub" -ForegroundColor Red
    Write-Host "=================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check the error messages above." -ForegroundColor Yellow
}
