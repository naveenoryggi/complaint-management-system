# Deployment Checklist

Use this checklist to ensure all steps are completed during deployment.

## Pre-Deployment Checklist

### Code Preparation
- [ ] All code merged to `main`/`master` branch
- [ ] All unit tests passing
- [ ] All E2E tests passing (100% pass rate verified)
- [ ] Code review completed
- [ ] Security scan completed (no critical vulnerabilities)
- [ ] Performance testing completed

### Environment Preparation
- [ ] Production database server provisioned
- [ ] Web server (IIS/Nginx) configured
- [ ] SSL certificate installed and validated
- [ ] Firewall rules configured
- [ ] SMTP server credentials obtained
- [ ] OAuth credentials configured (if using OAuth email)
- [ ] Backup storage configured

### Configuration Files
- [ ] `appsettings.Production.json` created and validated
- [ ] `environment.prod.ts` updated with production API URL
- [ ] Connection strings verified (test connectivity)
- [ ] JWT secret key generated (minimum 32 characters)
- [ ] CORS origins configured correctly
- [ ] Email settings configured and tested

---

## Database Deployment Checklist

### Pre-Migration
- [ ] **CRITICAL:** Backup current production database (if upgrading)
- [ ] Test migration script on staging environment first
- [ ] Verify database user permissions
- [ ] Check disk space on database server
- [ ] Schedule maintenance window if required

### Migration Execution
- [ ] Run migration script: `dotnet ef database update --environment Production`
- [ ] Verify all migrations applied: Check `__EFMigrationsHistory` table
- [ ] Verify seed data created (roles, permissions, categories, event types, templates)
- [ ] Run post-migration validation queries
- [ ] Document migration completion time

### Post-Migration Verification
- [ ] Check database integrity: `DBCC CHECKDB`
- [ ] Verify table row counts match expectations
- [ ] Test sample queries for performance
- [ ] Backup database after successful migration
- [ ] Update database documentation

---

## Backend Deployment Checklist

### Build and Publish
- [ ] Clean solution: `dotnet clean`
- [ ] Restore packages: `dotnet restore`
- [ ] Build in Release mode: `dotnet build -c Release`
- [ ] Run tests: `dotnet test`
- [ ] Publish application: `dotnet publish -c Release -o ./publish`
- [ ] Verify all dependencies included in publish folder

### IIS Deployment (Windows)
- [ ] Create application pool (No Managed Code, Integrated pipeline)
- [ ] Configure application pool identity
- [ ] Create IIS website with HTTPS binding
- [ ] Copy published files to IIS directory
- [ ] Configure `web.config` with correct environment variables
- [ ] Set folder permissions (IIS AppPool identity needs read access)
- [ ] Create logs directory and set write permissions
- [ ] Start application pool
- [ ] Start website

### Linux Deployment (Ubuntu/Nginx)
- [ ] Install .NET 8.0 runtime
- [ ] Copy published files to `/var/www/complaint-api`
- [ ] Set folder ownership: `chown -R www-data:www-data`
- [ ] Create systemd service file
- [ ] Reload systemd: `systemctl daemon-reload`
- [ ] Enable service: `systemctl enable complaint-api`
- [ ] Start service: `systemctl start complaint-api`
- [ ] Check service status: `systemctl status complaint-api`
- [ ] Configure Nginx reverse proxy
- [ ] Test Nginx config: `nginx -t`
- [ ] Reload Nginx: `systemctl reload nginx`

### Backend Verification
- [ ] Test health endpoint: `curl https://api.your-domain.com/health`
- [ ] Test database health: `curl https://api.your-domain.com/api/health/database`
- [ ] Test login endpoint with admin credentials
- [ ] Check application logs for startup errors
- [ ] Verify background services started (auto-escalation, email polling, OAuth refresh)
- [ ] Test API endpoints using Postman/curl
- [ ] Monitor memory usage for first 30 minutes

---

## Frontend Deployment Checklist

### Build
- [ ] Install dependencies: `npm install`
- [ ] Update `environment.prod.ts` with production API URL
- [ ] Build for production: `npm run build:prod`
- [ ] Verify build completed without errors
- [ ] Check bundle sizes (warn if > 5MB total)
- [ ] Test build locally: `npm run serve:prod`

### IIS Deployment (Windows)
- [ ] Create application pool (No Managed Code)
- [ ] Create IIS website with HTTPS binding
- [ ] Copy dist files to IIS directory: `C:\inetpub\ComplaintManagementWeb`
- [ ] Create `web.config` with URL rewrite rules
- [ ] Configure MIME types for fonts (.woff, .woff2)
- [ ] Enable static content compression
- [ ] Set cache headers for static assets
- [ ] Start website

### Linux Deployment (Ubuntu/Nginx)
- [ ] Create web directory: `/var/www/complaint-web`
- [ ] Copy dist files to web directory
- [ ] Set folder ownership: `chown -R www-data:www-data`
- [ ] Configure Nginx virtual host
- [ ] Enable gzip compression
- [ ] Configure cache headers for static assets
- [ ] Set security headers (X-Frame-Options, X-XSS-Protection, etc.)
- [ ] Test Nginx config: `nginx -t`
- [ ] Reload Nginx: `systemctl reload nginx`

### Frontend Verification
- [ ] Navigate to `https://your-domain.com`
- [ ] Verify login page loads without console errors
- [ ] Test login functionality
- [ ] Verify dashboard loads correctly
- [ ] Test responsive design (mobile, tablet, desktop)
- [ ] Check browser console for errors (should be 0)
- [ ] Test navigation between pages
- [ ] Verify images and fonts load correctly
- [ ] Run Lighthouse audit (target: > 90 score)

---

## Security Verification Checklist

### SSL/TLS
- [ ] SSL certificate installed correctly
- [ ] HTTPS redirect working (HTTP → HTTPS)
- [ ] Certificate validity checked (not expired)
- [ ] Certificate chain complete
- [ ] TLS 1.2+ only (TLS 1.0/1.1 disabled)
- [ ] Test with SSL Labs (https://www.ssllabs.com/ssltest/)

### Application Security
- [ ] CORS configured correctly (only allow production domains)
- [ ] Authentication working (JWT tokens generated correctly)
- [ ] Authorization working (role-based access control)
- [ ] Password policy enforced
- [ ] SQL injection prevention verified (parameterized queries)
- [ ] XSS prevention verified (input sanitization)
- [ ] CSRF protection enabled
- [ ] Sensitive data encrypted in database
- [ ] API rate limiting configured
- [ ] Security headers configured (X-Frame-Options, CSP, etc.)

### Data Protection
- [ ] Database connection encrypted
- [ ] Passwords hashed (not stored plain text)
- [ ] JWT secret key strong and unique
- [ ] SMTP credentials encrypted
- [ ] OAuth tokens encrypted at rest
- [ ] File upload validation working
- [ ] Personal data anonymization working (if applicable)

---

## Functional Testing Checklist

### Authentication & Authorization
- [ ] Admin can login successfully
- [ ] Handler can login successfully
- [ ] Complainant can login successfully
- [ ] Invalid credentials rejected
- [ ] Account lockout after failed attempts
- [ ] Password reset flow works
- [ ] Role-based access control working (Admin sees all features, Complainant limited)

### Core Functionality
- [ ] Create new complaint (web form)
- [ ] Create complaint via email (if email ticketing enabled)
- [ ] View complaint list (filtered by role)
- [ ] View complaint detail
- [ ] Edit complaint (handlers only)
- [ ] Assign complaint to handler
- [ ] Change complaint status
- [ ] Add comments to complaint
- [ ] Upload attachments
- [ ] Download attachments
- [ ] Escalate complaint
- [ ] Close complaint
- [ ] Reopen complaint

### SLA Management
- [ ] SLA deadlines calculated correctly
- [ ] SLA progress bars display correctly
- [ ] SLA breach warnings shown
- [ ] Auto-escalation triggers on SLA breach
- [ ] SLA reports accurate

### Email System
- [ ] Email notifications send correctly
- [ ] Auto-acknowledgment emails send on complaint creation
- [ ] Assignment notification emails send
- [ ] Status change notification emails send
- [ ] Email threading works (replies grouped correctly)
- [ ] Email attachments saved correctly
- [ ] OAuth token refresh works (if using OAuth)

### Dashboard & Reports
- [ ] Dashboard statistics accurate
- [ ] Charts render correctly
- [ ] Role-based dashboard filtering works
- [ ] Export functionality works
- [ ] Date range filters work
- [ ] Search functionality works

### User Management
- [ ] Create new user
- [ ] Edit user details
- [ ] Deactivate user
- [ ] Assign roles to user
- [ ] Password change works
- [ ] User profile update works

---

## Performance Testing Checklist

### Frontend Performance
- [ ] Initial page load < 3 seconds
- [ ] Time to Interactive < 5 seconds
- [ ] Bundle sizes optimized (< 5MB total)
- [ ] Images optimized and lazy-loaded
- [ ] Fonts loaded efficiently
- [ ] No memory leaks detected
- [ ] Smooth scrolling and animations

### Backend Performance
- [ ] API response time < 500ms (p95)
- [ ] Database queries optimized (no N+1 queries)
- [ ] Proper indexing on frequently queried columns
- [ ] Connection pooling configured
- [ ] Response caching configured where appropriate
- [ ] No memory leaks detected
- [ ] Background jobs running smoothly

### Load Testing
- [ ] System handles 100 concurrent users
- [ ] System handles 500 complaints/day
- [ ] Database performs well under load
- [ ] No deadlocks or timeout errors
- [ ] Email queue processes efficiently

---

## Monitoring & Logging Checklist

### Logging Configuration
- [ ] Application logs configured (Info level for production)
- [ ] Error logs captured and stored
- [ ] Database query logs enabled (for troubleshooting)
- [ ] IIS/Nginx access logs enabled
- [ ] Log rotation configured (daily/weekly)
- [ ] Log retention policy set (30-90 days)

### Monitoring Setup
- [ ] Server resource monitoring enabled (CPU, RAM, Disk)
- [ ] Application uptime monitoring configured
- [ ] Database performance monitoring enabled
- [ ] Email queue monitoring enabled
- [ ] SLA breach alerts configured
- [ ] Error rate alerts configured
- [ ] API endpoint monitoring enabled

### Alert Configuration
- [ ] Critical error alerts to dev team
- [ ] Server down alerts to ops team
- [ ] Database connection failure alerts
- [ ] High memory usage alerts (> 80%)
- [ ] High CPU usage alerts (> 80%)
- [ ] Disk space low alerts (< 20% free)

---

## Backup & Recovery Checklist

### Backup Configuration
- [ ] Database backup scheduled (daily full, hourly incremental)
- [ ] Application files backup configured
- [ ] Configuration files backup configured
- [ ] Upload directory backup configured
- [ ] Backup retention policy set (30 days minimum)
- [ ] Backup storage tested and accessible
- [ ] Off-site backup configured

### Recovery Testing
- [ ] Database restore tested successfully
- [ ] Application restore tested successfully
- [ ] Recovery Time Objective (RTO) documented
- [ ] Recovery Point Objective (RPO) documented
- [ ] Disaster recovery plan documented
- [ ] Team trained on recovery procedures

---

## Post-Deployment Tasks

### Immediate (Within 1 hour)
- [ ] Monitor server resources (CPU, RAM, Disk I/O)
- [ ] Check application logs for errors
- [ ] Test all critical functionality
- [ ] Verify email notifications working
- [ ] Check background services status
- [ ] Monitor API response times

### First 24 Hours
- [ ] Review error logs every 4 hours
- [ ] Monitor user activity and feedback
- [ ] Check database performance
- [ ] Verify backup completed successfully
- [ ] Monitor email queue processing
- [ ] Test SLA auto-escalation

### First Week
- [ ] Daily log reviews
- [ ] User feedback collection
- [ ] Performance optimization based on metrics
- [ ] Address any minor issues found
- [ ] Update documentation based on deployment experience

---

## Rollback Plan

If critical issues are found during deployment:

### Decision Criteria for Rollback
- [ ] Data corruption detected
- [ ] Critical functionality broken (login, complaint creation)
- [ ] Security vulnerability introduced
- [ ] Performance degradation > 50%
- [ ] Data loss risk identified

### Rollback Procedures
- [ ] Stop new application (IIS/systemd)
- [ ] Restore database from pre-deployment backup
- [ ] Restore previous application version
- [ ] Clear application cache
- [ ] Restart application
- [ ] Verify rollback successful
- [ ] Document issues encountered
- [ ] Plan fix for next deployment

---

## Sign-Off

### Deployment Team

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Project Manager | | | |
| Lead Developer | | | |
| Database Administrator | | | |
| System Administrator | | | |
| QA Lead | | | |

### Deployment Summary

**Deployment Date:** _______________
**Deployment Window:** _______________ to _______________
**Downtime Duration:** _______________
**Issues Encountered:** _______________
**Resolution Actions:** _______________

**Overall Deployment Status:**
- [ ] Successful - No Issues
- [ ] Successful - Minor Issues Resolved
- [ ] Partial - Major Issues Require Follow-up
- [ ] Failed - Rollback Initiated

**Next Steps:**
_______________________________________________________________
_______________________________________________________________
_______________________________________________________________

---

**Checklist Version:** 1.0
**Last Updated:** November 15, 2025
