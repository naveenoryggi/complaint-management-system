// Master Data Entities
export * from './master-data';

// Complaint Entities
export * from './complaints';

// Role & Permission Entities
export * from './roles';

// Escalation Entities (to be created)
// export * from './escalation';

// Email Alert Entities (to be created)
// export * from './email-alerts';

// Consolidated entity list for TypeORM configuration
import { Tenant, Company, Branch, Department, Section, User } from './master-data';
import {
  ComplaintCategory,
  Complaint,
  ComplaintComment,
  ComplaintAttachment,
} from './complaints';
import {
  ComplaintRole,
  UserComplaintRole,
  ComplaintRolePermission,
} from './roles';

export const entities = [
  // Master Data (6 entities)
  Tenant,
  Company,
  Branch,
  Department,
  Section,
  User,

  // Complaints (4 entities)
  ComplaintCategory,
  Complaint,
  ComplaintComment,
  ComplaintAttachment,

  // Roles & Permissions (3 entities)
  ComplaintRole,
  UserComplaintRole,
  ComplaintRolePermission,

  // Total: 13 entities created
  // Remaining: Escalation (3), Email Alerts (5), Audit (1) = 9 entities to be added
];
