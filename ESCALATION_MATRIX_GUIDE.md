# Escalation Matrix Configuration Guide

**Quick Reference for Administrators**

---

## Table of Contents

1. [Introduction](#introduction)
2. [Configuration Examples](#configuration-examples)
3. [Assignment Strategy Patterns](#assignment-strategy-patterns)
4. [Scope Mapping Examples](#scope-mapping-examples)
5. [Common Scenarios](#common-scenarios)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

---

## Introduction

This guide provides practical examples and patterns for configuring escalation matrices in the Complaint Management System. Use this as a quick reference when setting up new escalation workflows.

### Key Concepts

- **Escalation Matrix**: A configured workflow defining how complaints escalate through different levels
- **Level**: Each step in the escalation process (1-5 levels supported)
- **Assignment Strategy**: How the system determines who handles the complaint at each level
- **Scope**: Where the matrix applies (Company → Branch → Department → Section)
- **SLA**: Service Level Agreement - time limits for each escalation level

---

## Configuration Examples

### Example 1: Simple 2-Level Escalation for Attendance Issues

**Scenario**: Attendance complaints should go to the employee's manager first, then to Branch HR.

```json
{
  "matrix_name": "Attendance - 2 Level",
  "description": "Simple escalation for attendance-related complaints",
  "total_levels": 2,
  "scope": {
    "company_ids": ["all"],
    "branch_ids": ["all"],
    "department_ids": [],
    "section_ids": []
  },
  "categories": ["Attendance Issues", "Biometric Issues"],
  "sla_config": {
    "level_1_hours": 24,
    "level_2_hours": 48
  },
  "levels": [
    {
      "level": 1,
      "name": "Reporting Manager",
      "assignment_strategy": "REPORTING_CHAIN",
      "allowed_actions": ["RESOLVE", "ESCALATE", "REQUEST_INFO"],
      "auto_escalate_hours": 20,
      "notification_config": {
        "immediate": true,
        "reminder_hours": 12,
        "warning_hours": 2
      }
    },
    {
      "level": 2,
      "name": "Branch HR Manager",
      "assignment_strategy": "ROLE",
      "assignees": [
        {
          "assignee_type": "ROLE",
          "role": "BRANCH_HR",
          "scope_filter": {
            "branch_id": "from_complaint"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "REJECT", "REASSIGN"],
      "auto_escalate_hours": null,
      "notification_config": {
        "immediate": true,
        "reminder_hours": 24,
        "warning_hours": 4
      }
    }
  ]
}
```

---

### Example 2: 3-Level Escalation for Salary Disputes

**Scenario**: Salary complaints are more critical and require manager → Branch HR → Central HR escalation.

```json
{
  "matrix_name": "Salary Disputes - 3 Level",
  "description": "Critical escalation path for salary and payroll issues",
  "total_levels": 3,
  "scope": {
    "company_ids": ["company-abc-123"],
    "branch_ids": ["all"],
    "department_ids": [],
    "section_ids": []
  },
  "categories": ["Salary & Payroll", "Bonus Issues"],
  "sla_config": {
    "level_1_hours": 12,
    "level_2_hours": 24,
    "level_3_hours": 48
  },
  "levels": [
    {
      "level": 1,
      "name": "Reporting Manager",
      "assignment_strategy": "REPORTING_CHAIN",
      "allowed_actions": ["RESOLVE", "ESCALATE", "REQUEST_INFO"],
      "auto_escalate_hours": 8,
      "notification_config": {
        "immediate": true,
        "reminder_hours": 6,
        "warning_hours": 1
      }
    },
    {
      "level": 2,
      "name": "Branch HR Manager",
      "assignment_strategy": "ROLE",
      "assignees": [
        {
          "assignee_type": "ROLE",
          "role": "BRANCH_HR",
          "scope_filter": {
            "branch_id": "from_complaint"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "ESCALATE", "REASSIGN"],
      "auto_escalate_hours": 16,
      "notification_config": {
        "immediate": true,
        "reminder_hours": 8,
        "warning_hours": 2
      }
    },
    {
      "level": 3,
      "name": "Central HR Head",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "assignee_type": "USER",
          "user_id": "user-hr-head-001",
          "fallback_assignee": {
            "assignee_type": "USER",
            "user_id": "user-hr-deputy-001"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "REJECT", "REASSIGN"],
      "auto_escalate_hours": null,
      "notification_config": {
        "immediate": true,
        "reminder_hours": 12,
        "warning_hours": 4
      }
    }
  ]
}
```

---

### Example 3: 4-Level Escalation with Branch-Specific Routing

**Scenario**: Different branches have different HR structures, requiring branch-specific assignees at each level.

```json
{
  "matrix_name": "Multi-Branch - 4 Level",
  "description": "Branch-specific escalation for large organizations",
  "total_levels": 4,
  "scope": {
    "company_ids": ["company-xyz-456"],
    "branch_ids": ["branch-mumbai", "branch-bangalore", "branch-delhi"],
    "department_ids": [],
    "section_ids": []
  },
  "categories": ["HR Issues", "Workplace Issues", "Policy Queries"],
  "sla_config": {
    "level_1_hours": 24,
    "level_2_hours": 48,
    "level_3_hours": 72,
    "level_4_hours": 96
  },
  "levels": [
    {
      "level": 1,
      "name": "Reporting Manager",
      "assignment_strategy": "REPORTING_CHAIN",
      "allowed_actions": ["RESOLVE", "ESCALATE"],
      "auto_escalate_hours": 20
    },
    {
      "level": 2,
      "name": "Branch HR",
      "assignment_strategy": "ROLE",
      "assignees": [
        {
          "assignee_type": "ROLE",
          "role": "BRANCH_HR",
          "scope_filter": {
            "branch_id": "branch-mumbai"
          }
        },
        {
          "assignee_type": "ROLE",
          "role": "BRANCH_HR",
          "scope_filter": {
            "branch_id": "branch-bangalore"
          }
        },
        {
          "assignee_type": "ROLE",
          "role": "BRANCH_HR",
          "scope_filter": {
            "branch_id": "branch-delhi"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "ESCALATE", "REASSIGN"],
      "auto_escalate_hours": 36
    },
    {
      "level": 3,
      "name": "Regional HR Head",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "assignee_type": "USER",
          "user_id": "user-regional-hr-west",
          "scope_filter": {
            "branch_id": "branch-mumbai"
          }
        },
        {
          "assignee_type": "USER",
          "user_id": "user-regional-hr-south",
          "scope_filter": {
            "branch_id": "branch-bangalore"
          }
        },
        {
          "assignee_type": "USER",
          "user_id": "user-regional-hr-north",
          "scope_filter": {
            "branch_id": "branch-delhi"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "ESCALATE", "REASSIGN"],
      "auto_escalate_hours": 60
    },
    {
      "level": 4,
      "name": "Chief HR Officer",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "assignee_type": "USER",
          "user_id": "user-chro-001",
          "fallback_assignee": {
            "assignee_type": "ROLE",
            "role": "CENTRAL_HR"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "REJECT"],
      "auto_escalate_hours": null
    }
  ]
}
```

---

### Example 4: Department-Specific 3-Level Escalation

**Scenario**: Engineering department has specific HR requirements different from other departments.

```json
{
  "matrix_name": "Engineering Dept - 3 Level",
  "description": "Specialized escalation for engineering department",
  "total_levels": 3,
  "scope": {
    "company_ids": ["company-abc-123"],
    "branch_ids": ["all"],
    "department_ids": ["dept-engineering"],
    "section_ids": []
  },
  "categories": ["All Categories"],
  "sla_config": {
    "level_1_hours": 24,
    "level_2_hours": 48,
    "level_3_hours": 72
  },
  "levels": [
    {
      "level": 1,
      "name": "Engineering Manager",
      "assignment_strategy": "REPORTING_CHAIN",
      "allowed_actions": ["RESOLVE", "ESCALATE"],
      "auto_escalate_hours": 20
    },
    {
      "level": 2,
      "name": "Engineering HR Specialist",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "assignee_type": "USER",
          "user_id": "user-eng-hr-specialist",
          "scope_filter": {
            "department_id": "dept-engineering"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "ESCALATE", "REASSIGN"],
      "auto_escalate_hours": 36
    },
    {
      "level": 3,
      "name": "Engineering Head + Central HR",
      "assignment_strategy": "ROUND_ROBIN",
      "assignees": [
        {
          "assignee_type": "USER",
          "user_id": "user-vp-engineering"
        },
        {
          "assignee_type": "USER",
          "user_id": "user-central-hr-head"
        }
      ],
      "allowed_actions": ["RESOLVE", "REJECT"],
      "auto_escalate_hours": null
    }
  ]
}
```

---

### Example 5: Load-Balanced 5-Level Escalation for Large Organizations

**Scenario**: Large organization with multiple HR specialists at each level, using load balancing.

```json
{
  "matrix_name": "Enterprise - 5 Level Load Balanced",
  "description": "Full escalation path with load balancing for large scale",
  "total_levels": 5,
  "scope": {
    "company_ids": ["enterprise-corp-001"],
    "branch_ids": ["all"],
    "department_ids": [],
    "section_ids": []
  },
  "categories": ["All Categories"],
  "sla_config": {
    "level_1_hours": 24,
    "level_2_hours": 48,
    "level_3_hours": 72,
    "level_4_hours": 96,
    "level_5_hours": 120
  },
  "levels": [
    {
      "level": 1,
      "name": "Direct Manager",
      "assignment_strategy": "REPORTING_CHAIN",
      "allowed_actions": ["RESOLVE", "ESCALATE"],
      "auto_escalate_hours": 20
    },
    {
      "level": 2,
      "name": "Branch HR Team",
      "assignment_strategy": "LEAST_LOADED",
      "assignees": [
        {
          "assignee_type": "GROUP",
          "group_id": "group-branch-hr-team",
          "scope_filter": {
            "branch_id": "from_complaint"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "ESCALATE", "REASSIGN"],
      "auto_escalate_hours": 36
    },
    {
      "level": 3,
      "name": "Senior HR Specialist",
      "assignment_strategy": "ROUND_ROBIN",
      "assignees": [
        {
          "assignee_type": "USER",
          "user_id": "user-senior-hr-1"
        },
        {
          "assignee_type": "USER",
          "user_id": "user-senior-hr-2"
        },
        {
          "assignee_type": "USER",
          "user_id": "user-senior-hr-3"
        }
      ],
      "allowed_actions": ["RESOLVE", "ESCALATE", "REASSIGN"],
      "auto_escalate_hours": 60
    },
    {
      "level": 4,
      "name": "Regional HR Director",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "assignee_type": "USER",
          "user_id": "user-regional-director",
          "fallback_assignee": {
            "assignee_type": "USER",
            "user_id": "user-deputy-director"
          }
        }
      ],
      "allowed_actions": ["RESOLVE", "ESCALATE", "REASSIGN"],
      "auto_escalate_hours": 80
    },
    {
      "level": 5,
      "name": "Chief People Officer",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "assignee_type": "USER",
          "user_id": "user-cpo-001"
        }
      ],
      "allowed_actions": ["RESOLVE", "REJECT"],
      "auto_escalate_hours": null
    }
  ]
}
```

---

## Assignment Strategy Patterns

### 1. REPORTING_CHAIN

**When to Use**: Always use for Level 1 when complaints should first go to the employee's direct manager.

**Configuration**:
```json
{
  "level": 1,
  "assignment_strategy": "REPORTING_CHAIN",
  "assignees": []  // No assignees needed - uses HRMS data
}
```

**How it Works**:
- System looks up employee's `manager_id` from HRMS
- Automatically assigns to that manager
- If manager not found, uses fallback logic

---

### 2. SPECIFIC_USER

**When to Use**: When you want a specific person to handle complaints at this level (e.g., HR Head, Department Head).

**Configuration**:
```json
{
  "level": 2,
  "assignment_strategy": "SPECIFIC_USER",
  "assignees": [
    {
      "assignee_type": "USER",
      "user_id": "user-hr-manager-001",
      "scope_filter": {
        "branch_id": "branch-mumbai"
      },
      "fallback_assignee": {
        "assignee_type": "USER",
        "user_id": "user-hr-deputy-001"
      }
    }
  ]
}
```

**Best Practices**:
- Always configure a fallback assignee
- Use for senior roles (HR Head, Regional Director, etc.)
- Specify scope to route branch-specific complaints to branch-specific users

---

### 3. ROLE

**When to Use**: When any user with a specific role can handle complaints (e.g., "BRANCH_HR", "DEPARTMENT_HEAD").

**Configuration**:
```json
{
  "level": 2,
  "assignment_strategy": "ROLE",
  "assignees": [
    {
      "assignee_type": "ROLE",
      "role": "BRANCH_HR",
      "scope_filter": {
        "branch_id": "from_complaint"  // Matches complaint's branch
      }
    }
  ]
}
```

**How it Works**:
- System finds users with role "BRANCH_HR" in the complaint's branch
- Assigns to one of them (first found, or can combine with ROUND_ROBIN/LEAST_LOADED)

---

### 4. ROUND_ROBIN

**When to Use**: When you have multiple people at the same level and want to distribute workload evenly.

**Configuration**:
```json
{
  "level": 3,
  "assignment_strategy": "ROUND_ROBIN",
  "assignees": [
    {
      "assignee_type": "USER",
      "user_id": "user-hr-specialist-1"
    },
    {
      "assignee_type": "USER",
      "user_id": "user-hr-specialist-2"
    },
    {
      "assignee_type": "USER",
      "user_id": "user-hr-specialist-3"
    }
  ]
}
```

**How it Works**:
- Complaint 1 → User 1
- Complaint 2 → User 2
- Complaint 3 → User 3
- Complaint 4 → User 1 (cycles back)

---

### 5. LEAST_LOADED

**When to Use**: When you want complaints to go to the person with the fewest active complaints.

**Configuration**:
```json
{
  "level": 2,
  "assignment_strategy": "LEAST_LOADED",
  "assignees": [
    {
      "assignee_type": "USER",
      "user_id": "user-hr-1",
      "weight": 2  // Can handle 2x workload
    },
    {
      "assignee_type": "USER",
      "user_id": "user-hr-2",
      "weight": 1
    },
    {
      "assignee_type": "USER",
      "user_id": "user-hr-3",
      "weight": 1
    }
  ]
}
```

**How it Works**:
- System counts active complaints per user
- Considers weight (user-hr-1 can have 2x complaints before being "loaded")
- Assigns to least loaded user

---

### 6. GROUP

**When to Use**: When you have a team/group that collectively handles complaints.

**Configuration**:
```json
{
  "level": 2,
  "assignment_strategy": "LEAST_LOADED",
  "assignees": [
    {
      "assignee_type": "GROUP",
      "group_id": "group-branch-hr-mumbai",
      "scope_filter": {
        "branch_id": "branch-mumbai"
      }
    }
  ]
}
```

**How it Works**:
- System looks up all members of the group
- Applies the assignment strategy (LEAST_LOADED, ROUND_ROBIN, etc.) within the group

---

## Scope Mapping Examples

### Scenario 1: Company-Wide Default Matrix

**Use Case**: Default escalation for all branches and departments.

```json
{
  "scope": {
    "company_ids": ["all"],
    "branch_ids": [],
    "department_ids": [],
    "section_ids": []
  }
}
```

**Priority**: Lowest (catches everything not caught by specific matrices)

---

### Scenario 2: Branch-Specific Matrix

**Use Case**: Mumbai branch has different HR structure.

```json
{
  "scope": {
    "company_ids": ["company-abc-123"],
    "branch_ids": ["branch-mumbai"],
    "department_ids": [],
    "section_ids": []
  }
}
```

**Priority**: Medium (overrides company-wide for Mumbai branch)

---

### Scenario 3: Department-Specific Matrix

**Use Case**: Engineering department needs special handling.

```json
{
  "scope": {
    "company_ids": ["company-abc-123"],
    "branch_ids": ["branch-bangalore"],
    "department_ids": ["dept-engineering"],
    "section_ids": []
  }
}
```

**Priority**: High (overrides branch-wide for engineering dept)

---

### Scenario 4: Section-Specific Matrix

**Use Case**: Security section has unique escalation path.

```json
{
  "scope": {
    "company_ids": ["company-abc-123"],
    "branch_ids": ["branch-delhi"],
    "department_ids": ["dept-operations"],
    "section_ids": ["section-security"]
  }
}
```

**Priority**: Highest (most specific, overrides all others)

---

## Common Scenarios

### Scenario 1: New Branch Setup

**Task**: Configure escalation for a new branch.

**Steps**:

1. **Identify Branch HR**
   - Who is the Branch HR Manager?
   - User ID: `user-hr-branch-pune`

2. **Create Branch-Specific Matrix**
```json
{
  "matrix_name": "Pune Branch - Standard Escalation",
  "scope": {
    "branch_ids": ["branch-pune"]
  },
  "total_levels": 3,
  "levels": [
    {
      "level": 1,
      "name": "Reporting Manager",
      "assignment_strategy": "REPORTING_CHAIN"
    },
    {
      "level": 2,
      "name": "Pune Branch HR",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "user_id": "user-hr-branch-pune",
          "scope_filter": {
            "branch_id": "branch-pune"
          }
        }
      ]
    },
    {
      "level": 3,
      "name": "Regional HR",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "user_id": "user-regional-hr-west"
        }
      ]
    }
  ]
}
```

---

### Scenario 2: High-Priority Category

**Task**: Harassment complaints need immediate attention and skip normal escalation.

**Steps**:

1. **Create Critical Priority Matrix**
```json
{
  "matrix_name": "Harassment - Critical Escalation",
  "categories": ["Workplace Harassment"],
  "total_levels": 2,
  "sla_config": {
    "level_1_hours": 4,  // Very short SLA
    "level_2_hours": 12
  },
  "levels": [
    {
      "level": 1,
      "name": "HR Compliance Officer",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "user_id": "user-compliance-officer"
        }
      ],
      "auto_escalate_hours": 2
    },
    {
      "level": 2,
      "name": "Chief HR Officer + Legal",
      "assignment_strategy": "SPECIFIC_USER",
      "assignees": [
        {
          "user_id": "user-chro"
        }
      ],
      "auto_escalate_hours": null
    }
  ]
}
```

---

### Scenario 3: Temporary Coverage

**Task**: Branch HR is on leave, complaints should go to deputy.

**Steps**:

1. **Update Assignee with Fallback**
```json
{
  "assignees": [
    {
      "user_id": "user-hr-branch-regular",
      "fallback_assignee": {
        "user_id": "user-hr-branch-deputy"
      }
    }
  ]
}
```

2. **OR Temporarily Deactivate Regular HR User**
   - System will automatically use fallback

---

### Scenario 4: Load Balancing Multiple HR Specialists

**Task**: Three HR specialists should share workload evenly.

**Configuration**:
```json
{
  "level": 2,
  "name": "HR Specialist Team",
  "assignment_strategy": "LEAST_LOADED",
  "assignees": [
    {
      "user_id": "user-hr-specialist-1",
      "weight": 1
    },
    {
      "user_id": "user-hr-specialist-2",
      "weight": 1
    },
    {
      "user_id": "user-hr-specialist-3",
      "weight": 2  // More experienced, can handle more
    }
  ]
}
```

---

## Best Practices

### 1. SLA Configuration

✅ **DO**:
- Set realistic SLAs based on complexity
- Lower SLAs for critical categories
- Auto-escalate before SLA breach (e.g., 80% of SLA)
- Consider business hours vs. 24/7

❌ **DON'T**:
- Set SLAs too tight (creates pressure)
- Make all levels same SLA
- Forget to account for weekends/holidays

**Recommended SLAs**:
```
Low Priority:    L1=48h, L2=96h, L3=120h
Medium Priority: L1=24h, L2=48h, L3=72h
High Priority:   L1=12h, L2=24h, L3=48h
Critical:        L1=4h,  L2=12h, L3=24h
```

---

### 2. Escalation Levels

✅ **DO**:
- Use 2-3 levels for most cases
- Reserve 4-5 levels for complex organizations
- Always have a final level (no auto-escalate)
- Name levels clearly (e.g., "Branch HR", not "Level 2")

❌ **DON'T**:
- Create too many levels (delays resolution)
- Have circular escalation
- Skip levels in the chain

---

### 3. Assignment Strategy

✅ **DO**:
- Use REPORTING_CHAIN for Level 1
- Use SPECIFIC_USER for senior roles
- Use LEAST_LOADED for teams
- Always configure fallback assignees

❌ **DON'T**:
- Mix strategies within a level (pick one)
- Forget to scope assignees by branch/department
- Leave assignees without backups

---

### 4. Scope Configuration

✅ **DO**:
- Start with company-wide default
- Add branch-specific as needed
- Use department/section for exceptions
- Test scope priority

❌ **DON'T**:
- Create overlapping scopes without clear priority
- Make everything branch-specific (hard to maintain)
- Forget to update scopes when org changes

---

### 5. Notifications

✅ **DO**:
- Send immediate notification on assignment
- Set reminders at 50% of SLA
- Send warning at 90% of SLA
- Notify all stakeholders on escalation

❌ **DON'T**:
- Spam with too many reminders
- Forget to notify employee of escalation
- Send notifications outside business hours (configure preferences)

---

## Troubleshooting

### Issue 1: Complaints Not Being Assigned

**Symptoms**: Complaint created but no assignee.

**Possible Causes**:
1. No matching escalation matrix found
2. Manager not configured in HRMS
3. Assignee user is inactive
4. Scope mismatch

**Solution**:
```sql
-- Check if matrix exists for this complaint
SELECT * FROM escalation_matrices
WHERE tenant_id = '<tenant_id>'
  AND complaint_category_ids @> ARRAY['<category_id>']::uuid[]
  AND (scope_branch_ids @> ARRAY['<branch_id>']::uuid[] OR scope_branch_ids IS NULL);

-- Check employee's manager
SELECT user_id, manager_id FROM users WHERE user_id = '<employee_id>';

-- Check assignee active status
SELECT user_id, is_active FROM users WHERE user_id = '<assignee_id>';
```

**Fix**:
1. Create a default (catch-all) escalation matrix
2. Ensure all employees have managers in HRMS
3. Configure fallback assignees

---

### Issue 2: Auto-Escalation Not Working

**Symptoms**: Complaints not escalating despite SLA breach.

**Possible Causes**:
1. Cron job not running
2. `auto_escalate_hours` not configured
3. Complaint already at max level
4. Complaint status is RESOLVED/CLOSED

**Solution**:
```sql
-- Check escalation state
SELECT * FROM complaint_escalation_state
WHERE complaint_id = '<complaint_id>';

-- Check if past deadline
SELECT
  complaint_id,
  current_level,
  level_deadline,
  NOW() > level_deadline AS is_past_deadline,
  next_escalation_at
FROM complaint_escalation_state
WHERE complaint_id = '<complaint_id>';
```

**Fix**:
1. Ensure cron job is running (`*/5 * * * *`)
2. Set `auto_escalate_hours` in level configuration
3. Check complaint hasn't reached max level

---

### Issue 3: Wrong Person Receiving Complaints

**Symptoms**: Complaints assigned to incorrect user.

**Possible Causes**:
1. Scope filter misconfigured
2. Multiple matrices with overlapping scope
3. HRMS data out of sync
4. Assignment strategy not matching intent

**Solution**:
```sql
-- Check which matrix was used
SELECT m.*, ces.complaint_id
FROM complaint_escalation_state ces
JOIN escalation_matrices m ON ces.matrix_id = m.matrix_id
WHERE ces.complaint_id = '<complaint_id>';

-- Check assignee scope
SELECT ea.*, el.level_number, el.assignment_strategy
FROM escalation_assignees ea
JOIN escalation_levels el ON ea.level_id = el.level_id
WHERE el.matrix_id = '<matrix_id>';
```

**Fix**:
1. Review and correct scope filters
2. Deactivate conflicting matrices
3. Resync HRMS data
4. Use more specific assignment strategy

---

### Issue 4: Notifications Not Sent

**Symptoms**: Users not receiving email/SMS notifications.

**Possible Causes**:
1. Notification service down
2. Invalid email/phone in user profile
3. Notification preferences disabled
4. Message queue backlog

**Solution**:
```sql
-- Check notification status
SELECT * FROM notifications
WHERE complaint_id = '<complaint_id>'
ORDER BY created_at DESC;

-- Check failed notifications
SELECT * FROM notifications
WHERE status = 'FAILED'
  AND created_at > NOW() - INTERVAL '1 hour';
```

**Fix**:
1. Check notification service health
2. Verify user contact information
3. Check notification preferences
4. Clear message queue backlog

---

## Quick Reference Table

| Assignment Strategy | Use Case | Config Complexity | Best For |
|---------------------|----------|-------------------|----------|
| REPORTING_CHAIN | Direct manager | Low | Level 1 |
| SPECIFIC_USER | Named person | Low | Senior roles |
| ROLE | Any user with role | Medium | HR teams |
| ROUND_ROBIN | Even distribution | Medium | Multiple specialists |
| LEAST_LOADED | Balance workload | High | Large teams |
| GROUP | Team-based | High | Shared responsibility |

---

## Escalation Level Guidelines

| Organization Size | Recommended Levels | Rationale |
|-------------------|-------------------|-----------|
| Small (<100 employees) | 2 levels | Manager → HR Head |
| Medium (100-500) | 3 levels | Manager → Branch HR → Central HR |
| Large (500-2000) | 3-4 levels | Manager → Branch HR → Regional HR → Central HR |
| Enterprise (2000+) | 4-5 levels | Full hierarchy with specialists |

---

## Email Alert Configuration for Escalation Levels

### Overview
Each escalation level can have customized email alerts with specific templates, recipients, and timing. This section provides examples of configuring email alerts for different escalation scenarios.

### Example 1: Basic Email Alert for Level 1 Assignment

**Scenario**: Send immediate email to manager when complaint is assigned at Level 1.

```json
{
  "escalation_level": 1,
  "alert_configuration": {
    "alert_type": "COMPLAINT_ASSIGNED",
    "template_id": "template-assignment-level1",
    "trigger_on_assignment": true,
    "trigger_on_reminder": true,
    "trigger_on_escalation": false,
    "trigger_on_sla_warning": true,

    "recipients": [
      {
        "type": "ASSIGNED_USER",
        "description": "Manager who received the assignment"
      },
      {
        "type": "EMPLOYEE",
        "description": "Employee who created the complaint",
        "include_as": "CC"
      }
    ],

    "email_template": {
      "subject": "[{{priority}}] Complaint #{{complaint_number}} assigned to you",
      "body_preview": "Dear {{assigned_to_name}}, A {{priority}} priority complaint has been assigned to you...",
      "include_attachments": false
    },

    "schedule": {
      "send_immediately": true,
      "send_reminder_at": "50% of SLA",
      "send_warning_at": "90% of SLA"
    }
  }
}
```

---

### Example 2: Custom Email for Critical Escalation (Level 2)

**Scenario**: When escalated to Level 2, send urgent notification to Branch HR with different template.

```json
{
  "escalation_level": 2,
  "alert_configuration": {
    "alert_type": "COMPLAINT_ESCALATED",
    "template_id": "template-escalation-level2-critical",
    "trigger_on_assignment": true,
    "trigger_on_escalation": true,
    "trigger_on_sla_warning": true,

    "recipients": [
      {
        "type": "ASSIGNED_USER",
        "description": "Branch HR Manager"
      },
      {
        "type": "MANAGER",
        "description": "Employee's direct manager",
        "include_as": "CC"
      },
      {
        "type": "EMAIL_LIST",
        "emails": ["hr-alerts@company.com"],
        "include_as": "BCC"
      }
    ],

    "email_template": {
      "subject": "🚨 ESCALATED: Complaint #{{complaint_number}} requires your attention",
      "body_preview": "URGENT: This complaint has been escalated to your level...",
      "email_priority": "HIGH",
      "include_attachments": true
    },

    "schedule": {
      "send_immediately": true,
      "send_reminder_at": "12 hours",
      "send_warning_at": "4 hours before SLA"
    },

    "conditional_sending": {
      "only_if_sla_breach": false,
      "only_if_priority": ["HIGH", "CRITICAL"],
      "only_during_business_hours": false
    }
  }
}
```

---

### Example 3: Different Alerts for Different Categories at Same Level

**Scenario**: Salary complaints at Level 2 get different template than attendance complaints.

```json
{
  "escalation_level": 2,
  "category_specific_alerts": [
    {
      "categories": ["Salary & Payroll", "Bonus Issues"],
      "alert_type": "COMPLAINT_ASSIGNED",
      "template_id": "template-salary-level2",

      "email_template": {
        "subject": "[SALARY] Complaint #{{complaint_number}} - {{complaint_subject}}",
        "custom_variables": {
          "salary_month": "{{salary_info.month}}",
          "expected_amount": "{{salary_info.expected}}",
          "received_amount": "{{salary_info.received}}"
        }
      },

      "recipients": [
        {
          "type": "ROLE",
          "role": "PAYROLL_MANAGER"
        },
        {
          "type": "ROLE",
          "role": "BRANCH_HR",
          "include_as": "CC"
        }
      ]
    },
    {
      "categories": ["Attendance & Timing"],
      "alert_type": "COMPLAINT_ASSIGNED",
      "template_id": "template-attendance-level2",

      "email_template": {
        "subject": "[ATTENDANCE] Complaint #{{complaint_number}} - {{complaint_subject}}",
        "custom_variables": {
          "attendance_date": "{{attendance_info.date}}",
          "punch_status": "{{attendance_info.status}}"
        }
      },

      "recipients": [
        {
          "type": "ROLE",
          "role": "BRANCH_HR"
        }
      ]
    }
  ]
}
```

---

### Example 4: SLA Breach Alert Configuration

**Scenario**: Send escalating warnings as SLA deadline approaches.

```json
{
  "escalation_level": 2,
  "sla_alerts": [
    {
      "alert_type": "SLA_WARNING_50",
      "trigger_at": "50% of SLA remaining",
      "template_id": "template-sla-warning",

      "recipients": [
        {
          "type": "ASSIGNED_USER",
          "description": "Current assignee"
        }
      ],

      "email_template": {
        "subject": "⏰ Reminder: Complaint #{{complaint_number}} - {{sla_hours_remaining}}h remaining",
        "body_preview": "This is a friendly reminder that the SLA deadline is approaching..."
      }
    },
    {
      "alert_type": "SLA_WARNING_90",
      "trigger_at": "90% of SLA expired (10% remaining)",
      "template_id": "template-sla-critical-warning",

      "recipients": [
        {
          "type": "ASSIGNED_USER"
        },
        {
          "type": "MANAGER",
          "description": "Assignee's manager",
          "include_as": "CC"
        }
      ],

      "email_template": {
        "subject": "🚨 URGENT: Complaint #{{complaint_number}} - SLA expires in {{sla_hours_remaining}}h",
        "email_priority": "HIGH"
      }
    },
    {
      "alert_type": "SLA_BREACH",
      "trigger_at": "When SLA is breached",
      "template_id": "template-sla-breach",

      "recipients": [
        {
          "type": "ASSIGNED_USER"
        },
        {
          "type": "ESCALATION_CHAIN",
          "description": "All users in escalation path",
          "include_as": "CC"
        },
        {
          "type": "EMAIL_LIST",
          "emails": ["sla-monitoring@company.com"],
          "include_as": "BCC"
        }
      ],

      "email_template": {
        "subject": "❌ SLA BREACHED: Complaint #{{complaint_number}} - Immediate action required",
        "email_priority": "HIGH",
        "include_attachments": true
      },

      "schedule": {
        "send_immediately": true
      }
    }
  ]
}
```

---

### Example 5: Daily Digest for Multiple Level Assignees

**Scenario**: Send daily summary of pending complaints to HR team.

```json
{
  "escalation_level": 2,
  "digest_configuration": {
    "alert_type": "DAILY_DIGEST",
    "template_id": "template-daily-digest-level2",

    "recipients": [
      {
        "type": "ROLE",
        "role": "BRANCH_HR"
      }
    ],

    "email_template": {
      "subject": "Daily Digest: {{pending_count}} pending complaints - {{current_date}}",
      "includes": [
        "Open complaints assigned to you",
        "Complaints nearing SLA breach",
        "Escalated complaints",
        "Resolved complaints (last 24h)"
      ]
    },

    "schedule": {
      "send_at": "08:00 AM",
      "timezone": "Asia/Kolkata",
      "days": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
      "batching": {
        "enabled": true,
        "group_by": "recipient"
      }
    },

    "conditional_sending": {
      "only_if_has_data": true,
      "skip_if_zero_complaints": true
    }
  }
}
```

---

### Example 6: Branch-Specific Email Configuration

**Scenario**: Different branches get different email templates and recipients.

```json
{
  "escalation_level": 2,
  "branch_specific_alerts": [
    {
      "branch_id": "branch-mumbai",
      "alert_type": "COMPLAINT_ASSIGNED",
      "template_id": "template-mumbai-branch",

      "email_template": {
        "header_html": "<div style='background-color: #1976d2;'>Mumbai Branch</div>",
        "footer_html": "<p>Contact Mumbai HR: +91-22-12345678</p>",
        "reply_to": "hr.mumbai@company.com"
      },

      "recipients": [
        {
          "type": "SPECIFIC_USER",
          "user_id": "user-mumbai-hr-head"
        },
        {
          "type": "EMAIL_LIST",
          "emails": ["mumbai-hr-team@company.com"],
          "include_as": "CC"
        }
      ]
    },
    {
      "branch_id": "branch-bangalore",
      "alert_type": "COMPLAINT_ASSIGNED",
      "template_id": "template-bangalore-branch",

      "email_template": {
        "header_html": "<div style='background-color: #dc004e;'>Bangalore Branch</div>",
        "footer_html": "<p>Contact Bangalore HR: +91-80-12345678</p>",
        "reply_to": "hr.bangalore@company.com"
      },

      "recipients": [
        {
          "type": "SPECIFIC_USER",
          "user_id": "user-bangalore-hr-head"
        }
      ]
    }
  ]
}
```

---

### Example 7: Auto-Escalation Email Notifications

**Scenario**: When auto-escalation occurs, notify all parties with context.

```json
{
  "escalation_level": 3,
  "auto_escalation_alert": {
    "alert_type": "COMPLAINT_ESCALATED",
    "template_id": "template-auto-escalation",
    "trigger_on": "AUTO_ESCALATION",

    "recipients": [
      {
        "type": "ASSIGNED_USER",
        "description": "New assignee at Level 3"
      },
      {
        "type": "PREVIOUS_ASSIGNEE",
        "description": "Previous assignee at Level 2",
        "include_as": "CC"
      },
      {
        "type": "EMPLOYEE",
        "description": "Original complaint creator",
        "include_as": "CC"
      },
      {
        "type": "MANAGER",
        "description": "Employee's manager",
        "include_as": "CC"
      }
    ],

    "email_template": {
      "subject": "⬆️ AUTO-ESCALATED: Complaint #{{complaint_number}} escalated to Level {{escalation_level}}",
      "body_variables": {
        "escalation_reason": "Auto-escalated due to SLA breach",
        "previous_assignee": "{{previous_assignee_name}}",
        "time_at_previous_level": "{{hours_at_level}} hours",
        "sla_breach_time": "{{sla_breach_duration}}",
        "escalation_history": "{{escalation_timeline}}"
      }
    },

    "schedule": {
      "send_immediately": true
    }
  }
}
```

---

### Example 8: Resolution Email with Feedback Survey

**Scenario**: When complaint is resolved, send confirmation email with survey link.

```json
{
  "escalation_level": "RESOLVED",
  "alert_configuration": {
    "alert_type": "COMPLAINT_RESOLVED",
    "template_id": "template-resolution-with-survey",
    "trigger_on": "STATUS_CHANGE_TO_RESOLVED",

    "recipients": [
      {
        "type": "EMPLOYEE",
        "description": "Original complaint creator"
      },
      {
        "type": "ASSIGNED_USER",
        "description": "User who resolved it",
        "include_as": "CC"
      }
    ],

    "email_template": {
      "subject": "✅ Resolved: Complaint #{{complaint_number}} has been resolved",
      "includes": [
        "Resolution summary",
        "Resolved by",
        "Resolution date",
        "Feedback survey link"
      ],
      "custom_variables": {
        "resolution_notes": "{{resolution.notes}}",
        "resolved_by": "{{resolution.resolved_by_name}}",
        "resolved_at": "{{resolution.resolved_at}}",
        "survey_url": "{{survey_link}}"
      },
      "call_to_action": {
        "text": "Rate Your Experience",
        "link": "{{survey_url}}"
      }
    },

    "schedule": {
      "send_immediately": true
    },

    "conditional_sending": {
      "skip_if_reopened": true
    }
  }
}
```

---

### Integrating Alerts with Escalation Matrix

When configuring an escalation matrix, include email alert settings for each level:

```json
{
  "matrix_name": "Standard 3-Level with Email Alerts",
  "total_levels": 3,
  "levels": [
    {
      "level": 1,
      "name": "Reporting Manager",
      "assignment_strategy": "REPORTING_CHAIN",

      // Email alert configuration for this level
      "email_alerts": {
        "on_assignment": {
          "template_id": "template-level1-assignment",
          "recipients": ["ASSIGNED_USER", "EMPLOYEE"]
        },
        "on_reminder": {
          "template_id": "template-level1-reminder",
          "send_after": "12 hours"
        },
        "on_sla_warning": {
          "template_id": "template-level1-sla-warning",
          "send_at": "90% SLA"
        }
      }
    },
    {
      "level": 2,
      "name": "Branch HR",
      "assignment_strategy": "ROLE",

      "email_alerts": {
        "on_assignment": {
          "template_id": "template-level2-assignment",
          "recipients": ["ASSIGNED_USER", "EMPLOYEE", "PREVIOUS_ASSIGNEE"]
        },
        "on_escalation": {
          "template_id": "template-level2-escalation",
          "recipients": ["ASSIGNED_USER", "ESCALATION_CHAIN"]
        },
        "on_sla_breach": {
          "template_id": "template-level2-sla-breach",
          "recipients": ["ASSIGNED_USER", "ROLE:CENTRAL_HR"],
          "email_priority": "HIGH"
        }
      }
    },
    {
      "level": 3,
      "name": "Central HR",
      "assignment_strategy": "SPECIFIC_USER",

      "email_alerts": {
        "on_assignment": {
          "template_id": "template-level3-assignment",
          "recipients": ["ASSIGNED_USER", "EMAIL:ceo@company.com"]
        },
        "on_resolution": {
          "template_id": "template-resolution-final-level",
          "recipients": ["EMPLOYEE", "ASSIGNED_USER", "ESCALATION_CHAIN"]
        }
      }
    }
  ]
}
```

---

### Best Practices for Email Alert Configuration

✅ **DO**:
- Use clear, action-oriented subject lines
- Customize templates for different escalation levels
- Include relevant context in email body
- Provide direct links to the complaint
- Configure different alerts for different priorities
- Test email templates before activating
- Monitor email delivery rates

❌ **DON'T**:
- Send too many alerts (causes alert fatigue)
- Use generic templates for all levels
- Forget to include SLA deadline information
- Send alerts at odd hours (respect user preferences)
- Use technical jargon in email content
- Forget to include unsubscribe options

---

## Support

For additional help with escalation matrix and email alert configuration:

1. **Documentation**: See main architecture document and Configuration Management Guide
2. **Examples**: Review configuration examples above
3. **Testing**: Use test mode to validate matrices and email templates before publishing
4. **Support**: Contact system administrator or technical team

---

**Last Updated**: October 11, 2025
**Version**: 2.0 (Added Email Alert Configuration)
