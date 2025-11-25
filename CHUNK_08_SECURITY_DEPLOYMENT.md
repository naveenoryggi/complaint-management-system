# CHUNK 8: Security & Deployment Strategy

**Part of**: Master Planning Document
**Module**: Security, Infrastructure & Deployment
**Status**: Production-Ready Specifications

---

## Overview

Enterprise-grade security architecture and deployment strategy for the Complaint Management System with focus on data protection, compliance, scalability, and high availability.

---

## 8.1 Security Architecture

### Defense-in-Depth Strategy

```
┌─────────────────────────────────────────────────────────┐
│                     Security Layers                      │
├─────────────────────────────────────────────────────────┤
│ Layer 1: Network Security                               │
│   - Firewall rules                                       │
│   - DDoS protection (CloudFlare / AWS Shield)           │
│   - VPN for database access                             │
│                                                          │
│ Layer 2: Application Security                           │
│   - WAF (Web Application Firewall)                      │
│   - Rate limiting                                        │
│   - Input validation & sanitization                     │
│                                                          │
│ Layer 3: Authentication & Authorization                 │
│   - JWT with short expiry (15 min access, 7 day refresh)│
│   - Multi-factor authentication (MFA)                   │
│   - Role-based access control (RBAC)                    │
│                                                          │
│ Layer 4: Data Security                                  │
│   - Encryption at rest (AES-256)                        │
│   - Encryption in transit (TLS 1.3)                     │
│   - Database encryption                                 │
│                                                          │
│ Layer 5: Monitoring & Auditing                          │
│   - Audit logs for all actions                          │
│   - Anomaly detection                                   │
│   - Security incident response                          │
└─────────────────────────────────────────────────────────┘
```

---

## 8.2 Authentication & Authorization

### Authentication Flow

```typescript
// JWT Token Structure
interface JWTPayload {
  user_id: string;
  employee_code: string;
  email: string;
  tenant_id: string;
  roles: string[];              // Complaint system roles
  scopes: {                     // Organization scopes
    company_id?: string;
    branch_ids?: string[];
    department_ids?: string[];
  };
  iat: number;                  // Issued at
  exp: number;                  // Expiry (15 minutes)
}

interface RefreshToken {
  user_id: string;
  token_family: string;         // For token rotation
  exp: number;                  // Expiry (7 days)
}
```

### Authentication Methods

1. **SSO Integration (Preferred)**
   - SAML 2.0 support
   - OAuth 2.0 / OpenID Connect
   - Active Directory integration
   - Automatic user provisioning from Oryggi

2. **Username/Password (Fallback)**
   - Bcrypt password hashing (cost factor: 12)
   - Password complexity requirements:
     - Minimum 10 characters
     - Upper + lowercase + numbers + special chars
     - No common passwords (check against breach database)
   - Password expiry: 90 days
   - Account lockout: 5 failed attempts, 30-minute lockout

3. **Multi-Factor Authentication (MFA)**
   - TOTP (Time-based One-Time Password)
   - SMS OTP (optional)
   - Email verification
   - Backup codes

### Authorization Model

```typescript
// Permission Check Example
async function checkPermission(
  user: User,
  action: string,
  resource: string,
  context?: ComplaintContext
): Promise<boolean> {
  // 1. Check if user has required role
  const userRoles = await getUserRoles(user.user_id);

  // 2. Check role permissions
  for (const role of userRoles) {
    const permission = await db.complaint_role_permissions.findOne({
      where: {
        role_id: role.role_id,
        module: 'complaints',
        resource: resource,
        action: action,
        is_allowed: true
      }
    });

    if (!permission) continue;

    // 3. Check organizational scope
    if (context?.complaint) {
      const hasScope = await checkOrganizationalScope(
        role,
        context.complaint
      );
      if (!hasScope) continue;
    }

    return true;
  }

  return false;
}

// Organizational Scope Check
async function checkOrganizationalScope(
  userRole: UserComplaintRole,
  complaint: Complaint
): Promise<boolean> {
  // Global scope - access everything
  if (userRole.scope === 'GLOBAL') return true;

  // Company scope
  if (userRole.scope === 'COMPANY') {
    return userRole.company_id === complaint.company_id;
  }

  // Branch scope
  if (userRole.scope === 'BRANCH') {
    return userRole.branch_id === complaint.branch_id;
  }

  // Department scope
  if (userRole.scope === 'DEPARTMENT') {
    return userRole.department_id === complaint.department_id;
  }

  // Section scope
  if (userRole.scope === 'SECTION') {
    return userRole.section_id === complaint.section_id;
  }

  return false;
}
```

---

## 8.3 Data Protection & Privacy

### Encryption Strategy

1. **Data at Rest**
   ```sql
   -- PostgreSQL encryption
   -- Enable pgcrypto extension
   CREATE EXTENSION IF NOT EXISTS pgcrypto;

   -- Encrypt sensitive fields
   CREATE TABLE complaint_sensitive_data (
     complaint_id UUID PRIMARY KEY,
     encrypted_data BYTEA,  -- Encrypted JSON with AES-256
     encryption_key_id VARCHAR(50),
     created_at TIMESTAMP DEFAULT NOW()
   );

   -- Encryption function
   CREATE OR REPLACE FUNCTION encrypt_complaint_data(
     data JSONB,
     key TEXT
   ) RETURNS BYTEA AS $$
   BEGIN
     RETURN pgp_sym_encrypt(data::TEXT, key);
   END;
   $$ LANGUAGE plpgsql;
   ```

2. **Data in Transit**
   - TLS 1.3 for all API endpoints
   - HTTPS only (HSTS enabled)
   - Certificate pinning for mobile apps

3. **Database Encryption**
   - PostgreSQL: Transparent Data Encryption (TDE)
   - SQL Server (Oryggi): Read-only access, no encryption changes
   - Encrypted database backups

### Personal Data Protection (GDPR Compliance)

1. **Data Minimization**
   - Collect only necessary employee information
   - Auto-delete resolved complaints after retention period (7 years)
   - Anonymize data for analytics

2. **Right to Access**
   - Employee portal: "Download My Data" feature
   - Export in JSON/PDF format
   - Include all complaints and comments

3. **Right to Erasure**
   - Admin tool: Anonymize employee data on request
   - Replace personal info with "REDACTED"
   - Keep complaint for audit but remove PII

4. **Data Breach Response**
   - Automated detection of suspicious access patterns
   - Incident response team notification
   - User notification within 72 hours (GDPR requirement)

---

## 8.4 API Security

### Rate Limiting

```typescript
// Rate limit configuration
const rateLimitConfig = {
  // Per IP address
  global: {
    windowMs: 15 * 60 * 1000,  // 15 minutes
    max: 1000                   // 1000 requests per window
  },

  // Per authenticated user
  authenticated: {
    windowMs: 15 * 60 * 1000,  // 15 minutes
    max: 5000                   // 5000 requests per window
  },

  // Specific endpoints
  login: {
    windowMs: 15 * 60 * 1000,  // 15 minutes
    max: 5                      // 5 attempts
  },

  fileUpload: {
    windowMs: 60 * 60 * 1000,  // 1 hour
    max: 50                     // 50 uploads per hour
  }
};
```

### Input Validation

```typescript
// Zod schema for complaint creation
import { z } from 'zod';

const CreateComplaintSchema = z.object({
  category_id: z.string().uuid(),
  subject: z.string()
    .min(5, 'Subject must be at least 5 characters')
    .max(200, 'Subject must not exceed 200 characters')
    .regex(/^[a-zA-Z0-9\s\-_.,!?]+$/, 'Invalid characters in subject'),

  description: z.string()
    .min(20, 'Description must be at least 20 characters')
    .max(2000, 'Description must not exceed 2000 characters'),

  priority: z.enum(['LOW', 'MEDIUM', 'HIGH', 'CRITICAL']),

  attachments: z.array(z.object({
    filename: z.string().max(255),
    filesize: z.number().max(10 * 1024 * 1024), // 10MB max
    mimetype: z.enum([
      'application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'image/png',
      'image/jpeg',
      'image/jpg'
    ])
  })).max(5)  // Max 5 attachments
});
```

### SQL Injection Prevention

```typescript
// Always use parameterized queries
// BAD - Vulnerable to SQL injection
const query = `SELECT * FROM complaints WHERE subject LIKE '%${userInput}%'`;

// GOOD - Using TypeORM parameterized query
const complaints = await db.complaints.createQueryBuilder('complaint')
  .where('complaint.subject LIKE :search', { search: `%${userInput}%` })
  .getMany();

// GOOD - Using Prisma (automatically parameterized)
const complaints = await prisma.complaints.findMany({
  where: {
    subject: {
      contains: userInput
    }
  }
});
```

### XSS Protection

```typescript
// Frontend: DOMPurify for sanitizing HTML
import DOMPurify from 'dompurify';

function renderComplaintDescription(html: string) {
  const clean = DOMPurify.sanitize(html, {
    ALLOWED_TAGS: ['b', 'i', 'u', 'p', 'br', 'ul', 'ol', 'li'],
    ALLOWED_ATTR: []
  });
  return <div dangerouslySetInnerHTML={{ __html: clean }} />;
}

// Backend: helmet.js for HTTP headers
import helmet from 'helmet';

app.use(helmet({
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      styleSrc: ["'self'", "'unsafe-inline'"],
      scriptSrc: ["'self'"],
      imgSrc: ["'self'", "data:", "https://cdn.company.com"],
      connectSrc: ["'self'", "https://api.company.com"]
    }
  },
  xssFilter: true,
  noSniff: true,
  referrerPolicy: { policy: 'strict-origin-when-cross-origin' }
}));
```

### CORS Configuration

```typescript
// Strict CORS policy
const corsOptions = {
  origin: function (origin, callback) {
    const allowedOrigins = [
      'https://complaints.company.com',
      'https://complaints-staging.company.com',
      'http://localhost:3000'  // Development only
    ];

    if (!origin || allowedOrigins.indexOf(origin) !== -1) {
      callback(null, true);
    } else {
      callback(new Error('Not allowed by CORS'));
    }
  },
  credentials: true,
  methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE'],
  allowedHeaders: ['Content-Type', 'Authorization'],
  exposedHeaders: ['Content-Range', 'X-Total-Count'],
  maxAge: 600  // 10 minutes
};

app.use(cors(corsOptions));
```

---

## 8.5 File Upload Security

### Virus Scanning

```typescript
// ClamAV integration for virus scanning
import NodeClam from 'clamscan';

const clamscan = new NodeClam().init({
  clamdscan: {
    host: process.env.CLAMAV_HOST,
    port: 3310
  }
});

async function scanUploadedFile(file: Express.Multer.File): Promise<void> {
  const { isInfected, viruses } = await clamscan.isInfected(file.path);

  if (isInfected) {
    // Delete infected file
    fs.unlinkSync(file.path);

    // Log security incident
    await securityLogger.log({
      event: 'VIRUS_DETECTED',
      filename: file.originalname,
      viruses: viruses,
      user_id: currentUser.id,
      ip: req.ip
    });

    throw new Error('File contains virus and was rejected');
  }
}
```

### File Type Validation

```typescript
// Validate file type by magic numbers, not just extension
import fileType from 'file-type';

async function validateFileType(file: Express.Multer.File): Promise<boolean> {
  const type = await fileType.fromFile(file.path);

  const allowedTypes = [
    'application/pdf',
    'image/png',
    'image/jpeg',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
  ];

  if (!type || !allowedTypes.includes(type.mime)) {
    throw new Error('Invalid file type');
  }

  return true;
}
```

### S3 Upload with Security

```typescript
import { S3Client, PutObjectCommand } from '@aws-sdk/client-s3';

async function uploadToS3(file: Express.Multer.File, userId: string) {
  const s3Client = new S3Client({ region: 'us-east-1' });

  // Generate secure filename
  const fileExt = path.extname(file.originalname);
  const secureFilename = `${userId}/${uuidv4()}${fileExt}`;

  const command = new PutObjectCommand({
    Bucket: process.env.S3_BUCKET,
    Key: `complaints/attachments/${secureFilename}`,
    Body: fs.createReadStream(file.path),
    ContentType: file.mimetype,
    ServerSideEncryption: 'AES256',  // Server-side encryption
    Metadata: {
      'uploaded-by': userId,
      'original-filename': file.originalname
    },
    // Private by default
    ACL: 'private'
  });

  await s3Client.send(command);

  return {
    key: secureFilename,
    url: `https://${process.env.S3_BUCKET}.s3.amazonaws.com/complaints/attachments/${secureFilename}`
  };
}
```

---

## 8.6 Audit Logging

### Comprehensive Audit Trail

```typescript
// Audit log schema
interface AuditLog {
  log_id: string;
  tenant_id: string;
  user_id: string;
  action: string;              // CREATE, UPDATE, DELETE, VIEW, ESCALATE, etc.
  resource_type: string;       // COMPLAINT, USER, ROLE, ESCALATION_MATRIX, etc.
  resource_id: string;
  old_value?: any;             // Before state
  new_value?: any;             // After state
  ip_address: string;
  user_agent: string;
  timestamp: Date;
  context?: {                  // Additional context
    complaint_number?: string;
    escalation_level?: number;
    role_name?: string;
  };
}

// Audit logging middleware
async function auditLog(req: Request, action: string, resource: any) {
  await db.audit_logs.create({
    tenant_id: req.user.tenant_id,
    user_id: req.user.user_id,
    action: action,
    resource_type: resource.type,
    resource_id: resource.id,
    old_value: resource.oldValue,
    new_value: resource.newValue,
    ip_address: req.ip,
    user_agent: req.headers['user-agent'],
    timestamp: new Date()
  });
}
```

### Security Event Monitoring

```typescript
// Security events to monitor
const SECURITY_EVENTS = {
  FAILED_LOGIN: 'User login failed',
  ACCOUNT_LOCKED: 'Account locked due to multiple failed attempts',
  UNAUTHORIZED_ACCESS: 'User attempted to access unauthorized resource',
  PRIVILEGE_ESCALATION: 'User attempted privilege escalation',
  DATA_EXPORT: 'Large data export performed',
  ROLE_CHANGE: 'User role modified',
  PERMISSION_CHANGE: 'Permission settings changed',
  FILE_VIRUS_DETECTED: 'Virus detected in uploaded file',
  SQL_INJECTION_ATTEMPT: 'Possible SQL injection attempt detected',
  XSS_ATTEMPT: 'Possible XSS attempt detected'
};

// Alerting on security events
async function handleSecurityEvent(event: string, details: any) {
  // Log to security log
  await securityLogger.error(event, details);

  // Send alert to security team for critical events
  if (CRITICAL_EVENTS.includes(event)) {
    await emailService.send({
      to: 'security@company.com',
      subject: `[SECURITY ALERT] ${event}`,
      body: JSON.stringify(details, null, 2)
    });
  }

  // Automated response for certain events
  if (event === 'ACCOUNT_LOCKED') {
    await notifyUser(details.user_id, 'Your account has been locked');
  }
}
```

---

## 8.7 Deployment Architecture

### Infrastructure Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         Internet                             │
└──────────────────────────┬──────────────────────────────────┘
                           │
                    ┌──────▼──────┐
                    │  CloudFlare │ (CDN + DDoS Protection)
                    │     WAF     │
                    └──────┬──────┘
                           │
            ┌──────────────▼──────────────┐
            │   AWS Application           │
            │   Load Balancer (ALB)       │
            │   - SSL Termination         │
            │   - Health Checks           │
            └──────────────┬──────────────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
    ┌────▼────┐      ┌────▼────┐      ┌────▼────┐
    │ ECS Task│      │ ECS Task│      │ ECS Task│
    │ (API 1) │      │ (API 2) │      │ (API 3) │
    └────┬────┘      └────┬────┘      └────┬────┘
         │                │                 │
         └────────────────┼─────────────────┘
                          │
         ┌────────────────┼────────────────┐
         │                │                │
    ┌────▼─────┐    ┌────▼─────┐    ┌────▼─────┐
    │ RDS      │    │ElastiCache│    │   S3     │
    │PostgreSQL│    │  Redis    │    │  Files   │
    │(Multi-AZ)│    │           │    │          │
    └──────────┘    └──────────┘    └──────────┘
         │
         │ (Read-Only Connection)
         │
    ┌────▼─────────────────┐
    │  SQL Server Express  │
    │  (Oryggi HRMS)       │
    │  On-Premise          │
    └──────────────────────┘
```

### Deployment Environments

1. **Development**
   - Local Docker Compose setup
   - Mock Oryggi database with sample data
   - Hot reload enabled
   - Debug logging

2. **Staging**
   - AWS ECS Fargate
   - RDS PostgreSQL (db.t3.small)
   - ElastiCache (cache.t3.micro)
   - Connects to test Oryggi instance
   - Daily automated testing

3. **Production**
   - AWS ECS Fargate (auto-scaling 3-10 tasks)
   - RDS PostgreSQL Multi-AZ (db.r6g.xlarge)
   - ElastiCache Redis Cluster (3 nodes)
   - S3 with CloudFront CDN
   - VPN connection to production Oryggi database
   - 99.9% uptime SLA

---

## 8.8 CI/CD Pipeline

### GitHub Actions Workflow

```yaml
# .github/workflows/deploy.yml
name: Deploy Complaint Management System

on:
  push:
    branches: [main, staging]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
          cache: 'npm'

      - name: Install dependencies
        run: |
          cd backend && npm ci
          cd ../frontend && npm ci

      - name: Run linting
        run: |
          cd backend && npm run lint
          cd ../frontend && npm run lint

      - name: Run unit tests
        run: |
          cd backend && npm run test:unit
          cd ../frontend && npm run test

      - name: Run integration tests
        run: cd backend && npm run test:integration
        env:
          DATABASE_URL: postgresql://test:test@localhost:5432/testdb

      - name: Upload coverage
        uses: codecov/codecov-action@v3

  security-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Run Snyk security scan
        uses: snyk/actions/node@master
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}

      - name: OWASP Dependency Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: 'complaint-management-system'
          path: '.'

  build:
    needs: [test, security-scan]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v1

      - name: Build and push backend image
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: complaint-system-backend
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG ./backend
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG

      - name: Build and push frontend image
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: complaint-system-frontend
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG ./frontend
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG

  deploy-staging:
    needs: build
    if: github.ref == 'refs/heads/staging'
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to ECS Staging
        uses: aws-actions/amazon-ecs-deploy-task-definition@v1
        with:
          task-definition: ecs-task-staging.json
          service: complaint-system-staging
          cluster: staging-cluster
          wait-for-service-stability: true

      - name: Run E2E tests
        run: npm run test:e2e
        env:
          TEST_URL: https://staging.complaints.company.com

  deploy-production:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production
    steps:
      - name: Deploy to ECS Production
        uses: aws-actions/amazon-ecs-deploy-task-definition@v1
        with:
          task-definition: ecs-task-production.json
          service: complaint-system-production
          cluster: production-cluster
          wait-for-service-stability: true

      - name: Notify team
        uses: slackapi/slack-github-action@v1.24.0
        with:
          payload: |
            {
              "text": "🚀 Production deployment completed: ${{ github.sha }}"
            }
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK }}
```

### Database Migration Strategy

```typescript
// Zero-downtime database migrations using TypeORM

// Migration: Add new column with default value
export class AddComplaintPriorityScore1234567890 implements MigrationInterface {
  public async up(queryRunner: QueryRunner): Promise<void> {
    // Step 1: Add column with default
    await queryRunner.query(`
      ALTER TABLE complaints
      ADD COLUMN priority_score INTEGER DEFAULT 0
    `);

    // Step 2: Backfill existing data
    await queryRunner.query(`
      UPDATE complaints
      SET priority_score = CASE
        WHEN priority = 'CRITICAL' THEN 4
        WHEN priority = 'HIGH' THEN 3
        WHEN priority = 'MEDIUM' THEN 2
        ELSE 1
      END
    `);

    // Step 3: Make NOT NULL after backfill
    await queryRunner.query(`
      ALTER TABLE complaints
      ALTER COLUMN priority_score SET NOT NULL
    `);
  }

  public async down(queryRunner: QueryRunner): Promise<void> {
    await queryRunner.query(`
      ALTER TABLE complaints DROP COLUMN priority_score
    `);
  }
}
```

---

## 8.9 Monitoring & Observability

### Prometheus Metrics

```typescript
// Custom metrics for monitoring
import { Registry, Counter, Histogram, Gauge } from 'prom-client';

const register = new Registry();

// Complaint creation metrics
const complaintCreationCounter = new Counter({
  name: 'complaints_created_total',
  help: 'Total number of complaints created',
  labelNames: ['category', 'priority', 'branch'],
  registers: [register]
});

// Response time metrics
const complaintResolutionTime = new Histogram({
  name: 'complaint_resolution_time_hours',
  help: 'Time taken to resolve complaints in hours',
  labelNames: ['category', 'priority'],
  buckets: [1, 6, 12, 24, 48, 72, 168],  // 1h to 1 week
  registers: [register]
});

// SLA breach metrics
const slaBreachCounter = new Counter({
  name: 'complaints_sla_breached_total',
  help: 'Total number of SLA breaches',
  labelNames: ['level', 'category'],
  registers: [register]
});

// Active complaints gauge
const activeComplaintsGauge = new Gauge({
  name: 'complaints_active_count',
  help: 'Number of currently active complaints',
  labelNames: ['status', 'priority'],
  registers: [register]
});

// Oryggi sync health
const oryggiSyncHealth = new Gauge({
  name: 'oryggi_sync_last_success_timestamp',
  help: 'Timestamp of last successful Oryggi sync',
  registers: [register]
});
```

### Grafana Dashboards

```yaml
# Dashboard: Complaint Management Overview
panels:
  - title: "Complaints Created (24h)"
    query: 'rate(complaints_created_total[24h])'
    type: graph

  - title: "Active Complaints"
    query: 'complaints_active_count'
    type: stat

  - title: "Average Resolution Time"
    query: 'avg(complaint_resolution_time_hours)'
    type: stat

  - title: "SLA Breaches (7d)"
    query: 'increase(complaints_sla_breached_total[7d])'
    type: graph

  - title: "API Latency (p95)"
    query: 'histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))'
    type: graph

  - title: "Oryggi Sync Status"
    query: 'time() - oryggi_sync_last_success_timestamp < 1800'
    type: stat
    alert: 'Oryggi sync failed (no success in 30 min)'
```

### ELK Stack Logging

```typescript
// Structured logging with Winston
import winston from 'winston';

const logger = winston.createLogger({
  level: process.env.LOG_LEVEL || 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.errors({ stack: true }),
    winston.format.json()
  ),
  defaultMeta: {
    service: 'complaint-management-api',
    environment: process.env.NODE_ENV
  },
  transports: [
    // Console for development
    new winston.transports.Console({
      format: winston.format.simple()
    }),

    // Elasticsearch for production
    new winston.transports.Http({
      host: process.env.ELASTICSEARCH_HOST,
      port: 9200,
      path: '/logs/_doc',
      ssl: true,
      auth: {
        username: process.env.ELASTICSEARCH_USER,
        password: process.env.ELASTICSEARCH_PASSWORD
      }
    })
  ]
});

// Log with context
logger.info('Complaint created', {
  complaint_id: complaint.id,
  complaint_number: complaint.complaint_number,
  user_id: user.id,
  category: complaint.category,
  priority: complaint.priority,
  duration_ms: Date.now() - startTime
});
```

---

## 8.10 Backup & Disaster Recovery

### Backup Strategy

1. **Database Backups**
   ```bash
   # Automated daily backups with retention

   # PostgreSQL (Complaint System)
   # - Full backup: Daily at 2 AM UTC
   # - Incremental: Every 6 hours
   # - Point-in-Time Recovery (PITR): Enabled
   # - Retention: 30 days
   # - Cross-region replication to us-west-2

   # Backup script
   #!/bin/bash
   TIMESTAMP=$(date +%Y%m%d_%H%M%S)
   BACKUP_FILE="complaint-db-backup-$TIMESTAMP.sql.gz"

   pg_dump -h $DB_HOST -U $DB_USER $DB_NAME | gzip > /backups/$BACKUP_FILE

   # Upload to S3
   aws s3 cp /backups/$BACKUP_FILE s3://company-db-backups/complaint-system/

   # Verify backup integrity
   gunzip -c /backups/$BACKUP_FILE | pg_restore --list > /dev/null
   ```

2. **File Storage Backups**
   - S3 versioning enabled
   - Cross-region replication
   - Lifecycle policy: Move to Glacier after 90 days
   - Retention: 7 years (compliance requirement)

3. **Configuration Backups**
   - Infrastructure as Code (Terraform)
   - Version controlled in Git
   - Environment variables in AWS Secrets Manager

### Disaster Recovery Plan

**RPO (Recovery Point Objective)**: 1 hour
**RTO (Recovery Time Objective)**: 4 hours

```yaml
# Disaster Recovery Runbook

Scenario 1: Database Failure
  Detection: Automated health checks + Prometheus alerts
  Steps:
    1. Promote read replica to primary (automated failover)
    2. Update DNS records to new primary
    3. Verify application connectivity
    4. Restore from backup if corruption detected
  Expected Downtime: 15 minutes

Scenario 2: Complete Region Failure
  Detection: Multi-region health checks
  Steps:
    1. Activate DR region (us-west-2)
    2. Restore latest database backup to DR region RDS
    3. Deploy application containers to DR region ECS
    4. Update Route53 to point to DR region ALB
    5. Verify Oryggi connectivity from DR region
  Expected Downtime: 2-4 hours

Scenario 3: Data Corruption
  Detection: Data integrity checks + user reports
  Steps:
    1. Identify corruption timestamp
    2. Restore from point-in-time backup
    3. Replay transaction logs from backup point
    4. Verify data integrity
  Expected Downtime: 1-3 hours

Scenario 4: Oryggi Database Unavailable
  Detection: Sync health monitoring
  Steps:
    1. Switch to read-only mode using cached Oryggi data
    2. Display banner: "Limited functionality - HRMS integration unavailable"
    3. Queue sync operations for later execution
    4. Resume normal operations when Oryggi restored
  Expected Downtime: 0 (degraded mode)
```

---

## 8.11 Scalability & Performance

### Horizontal Scaling

```yaml
# Auto-scaling configuration
ECS Service:
  scaling:
    min_tasks: 3
    max_tasks: 10
    target_cpu_utilization: 70%
    target_memory_utilization: 80%

    scale_up:
      metric: CPUUtilization > 70%
      duration: 2 minutes
      cooldown: 5 minutes
      adjustment: +2 tasks

    scale_down:
      metric: CPUUtilization < 30%
      duration: 10 minutes
      cooldown: 10 minutes
      adjustment: -1 task

Database:
  read_replicas: 2
  connection_pooling:
    max: 100
    min: 10
    idle_timeout: 30000

Redis:
  cluster_mode: enabled
  nodes: 3
  read_endpoints: 2
```

### Caching Strategy

```typescript
// Multi-layer caching

// Layer 1: Application-level cache (in-memory)
import NodeCache from 'node-cache';
const appCache = new NodeCache({ stdTTL: 300 }); // 5 minutes

// Layer 2: Redis cache
import { Redis } from 'ioredis';
const redis = new Redis(process.env.REDIS_URL);

// Caching wrapper
async function getCachedData<T>(
  key: string,
  fetchFn: () => Promise<T>,
  ttl: number = 300
): Promise<T> {
  // Check app cache first
  let data = appCache.get<T>(key);
  if (data) return data;

  // Check Redis
  const cached = await redis.get(key);
  if (cached) {
    data = JSON.parse(cached);
    appCache.set(key, data, ttl);
    return data;
  }

  // Fetch from database
  data = await fetchFn();

  // Store in both caches
  await redis.setex(key, ttl, JSON.stringify(data));
  appCache.set(key, data, ttl);

  return data;
}

// Example usage: Cache user roles
async function getUserRoles(userId: string) {
  return getCachedData(
    `user:${userId}:roles`,
    async () => {
      return await db.user_complaint_roles.findAll({
        where: { user_id: userId, is_active: true }
      });
    },
    600  // 10 minutes TTL
  );
}

// Cache invalidation on role change
async function assignUserRole(userId: string, roleId: string) {
  await db.user_complaint_roles.create({ user_id: userId, role_id: roleId });

  // Invalidate caches
  appCache.del(`user:${userId}:roles`);
  await redis.del(`user:${userId}:roles`);
}
```

### Database Query Optimization

```sql
-- Essential indexes for performance

-- Complaints table
CREATE INDEX idx_complaints_status ON complaints(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_complaints_created_at ON complaints(created_at DESC);
CREATE INDEX idx_complaints_assigned_to ON complaints(assigned_to_user_id);
CREATE INDEX idx_complaints_created_by ON complaints(created_by_user_id);
CREATE INDEX idx_complaints_company_branch ON complaints(company_id, branch_id);
CREATE INDEX idx_complaints_category ON complaints(category_id);

-- Composite index for common queries
CREATE INDEX idx_complaints_lookup ON complaints(tenant_id, status, priority, created_at DESC)
  WHERE deleted_at IS NULL;

-- Full-text search index
CREATE INDEX idx_complaints_search ON complaints
  USING gin(to_tsvector('english', subject || ' ' || description));

-- Escalation history
CREATE INDEX idx_escalation_history_complaint ON escalation_history(complaint_id, escalated_at DESC);

-- Audit logs (partitioned by month)
CREATE INDEX idx_audit_logs_user_action ON audit_logs(user_id, action, timestamp DESC);
CREATE INDEX idx_audit_logs_resource ON audit_logs(resource_type, resource_id);

-- Query optimization example
EXPLAIN ANALYZE
SELECT c.*, u.first_name, u.last_name, cat.name as category_name
FROM complaints c
INNER JOIN users u ON c.created_by_user_id = u.user_id
INNER JOIN complaint_categories cat ON c.category_id = cat.category_id
WHERE c.tenant_id = 'xxx'
  AND c.status IN ('OPEN', 'IN_PROGRESS')
  AND c.deleted_at IS NULL
ORDER BY c.created_at DESC
LIMIT 50;

-- Expected execution time: < 50ms with indexes
```

---

## 8.12 Security Compliance Checklist

### Pre-Production Security Audit

```yaml
Authentication & Authorization:
  - [ ] JWT tokens with proper expiry implemented
  - [ ] Refresh token rotation enabled
  - [ ] MFA available for admin roles
  - [ ] Password complexity requirements enforced
  - [ ] Account lockout after failed attempts
  - [ ] Session timeout configured (15 min inactivity)
  - [ ] RBAC permissions tested for all roles

Data Protection:
  - [ ] Database encryption at rest enabled
  - [ ] TLS 1.3 enforced for all API endpoints
  - [ ] Sensitive fields encrypted in database
  - [ ] PII data identified and protected
  - [ ] GDPR compliance verified (right to access, erasure)
  - [ ] Data retention policies configured

API Security:
  - [ ] Rate limiting configured per endpoint
  - [ ] Input validation with Zod schemas
  - [ ] SQL injection prevention (parameterized queries)
  - [ ] XSS protection with DOMPurify
  - [ ] CORS policy restricted to allowed origins
  - [ ] CSRF protection enabled
  - [ ] API authentication required for all endpoints

File Upload Security:
  - [ ] File type validation (magic numbers)
  - [ ] File size limits enforced (10MB max)
  - [ ] Virus scanning with ClamAV
  - [ ] Files stored in S3 with private ACL
  - [ ] Signed URLs for file downloads (1-hour expiry)

Infrastructure:
  - [ ] WAF configured with OWASP rules
  - [ ] DDoS protection enabled
  - [ ] Security groups with least privilege
  - [ ] VPN required for database access
  - [ ] Secrets stored in AWS Secrets Manager
  - [ ] IAM roles with least privilege
  - [ ] CloudTrail logging enabled

Monitoring & Logging:
  - [ ] Audit logs for all sensitive actions
  - [ ] Security event alerting configured
  - [ ] Failed login attempt monitoring
  - [ ] Anomaly detection rules defined
  - [ ] Log retention: 1 year
  - [ ] Centralized logging in ELK Stack

Compliance:
  - [ ] GDPR compliance documentation
  - [ ] Data processing agreement (DPA) signed
  - [ ] Privacy policy published
  - [ ] Security incident response plan documented
  - [ ] Employee security training completed
  - [ ] Third-party security audit completed

Testing:
  - [ ] Penetration testing completed
  - [ ] OWASP Top 10 vulnerabilities checked
  - [ ] Dependency vulnerability scan (Snyk)
  - [ ] Static code analysis (SonarQube)
  - [ ] Security headers verified (helmet.js)
```

---

## Summary

**Security & Deployment Highlights**:
- ✅ Defense-in-depth security architecture
- ✅ JWT authentication with MFA support
- ✅ Comprehensive RBAC with organizational scopes
- ✅ Data encryption at rest and in transit
- ✅ Complete audit logging and monitoring
- ✅ Automated CI/CD pipeline with security scanning
- ✅ High availability with auto-scaling (99.9% uptime)
- ✅ Disaster recovery plan (RPO: 1h, RTO: 4h)
- ✅ Performance optimization with multi-layer caching
- ✅ GDPR compliant data protection

**Production Readiness**: Enterprise-grade deployment with comprehensive security, monitoring, and disaster recovery.

---

**Integration Note**: This chunk completes the master planning document. All architecture, database schemas, integration strategies, technology stack, UI/UX design, and deployment strategies are now fully documented.
