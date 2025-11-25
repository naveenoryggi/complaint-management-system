# Quick Push Guide

## 🚀 How to Push Code to GitHub

### Method 1: Helper Script (Recommended)

```powershell
# Default commit message
powershell -ExecutionPolicy Bypass -File git-push-helper.ps1

# Custom commit message
powershell -ExecutionPolicy Bypass -File git-push-helper.ps1 -CommitMessage "feat: Add new feature"
```

### Method 2: Manual Commands

```bash
git add -A
git commit -m "Your commit message"
git push
```

**No credentials needed!** They're stored locally in `.git-credentials`

## 📍 Repository

**URL**: https://github.com/naveenoryggi/complaint-management-system

**Account**: naveenoryggi (naveen.chandra@oryggitech.com)

## 📋 Commit Message Format

Use conventional commit format:

- `feat: Add new feature`
- `fix: Fix bug in component`
- `docs: Update documentation`
- `style: Format code`
- `refactor: Refactor code structure`
- `test: Add tests`
- `chore: Update dependencies`

## ✅ Quick Checklist

Before pushing:

- [ ] Code compiles without errors
- [ ] No secrets in code (check appsettings.json)
- [ ] Meaningful commit message
- [ ] Run: `git status` to see what's being committed

## 🔒 Security

- ✅ Credentials stored locally only
- ✅ `.git-credentials` is in `.gitignore`
- ✅ Personal access token (not password)
- ✅ Can be revoked anytime from GitHub

## 📚 More Info

See `.github-credentials-README.txt` for detailed information.
