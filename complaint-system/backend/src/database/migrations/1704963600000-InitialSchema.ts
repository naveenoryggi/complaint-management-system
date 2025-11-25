import { MigrationInterface, QueryRunner } from 'typeorm';

export class InitialSchema1704963600000 implements MigrationInterface {
  name = 'InitialSchema1704963600000';

  public async up(queryRunner: QueryRunner): Promise<void> {
    // Enable UUID extension
    await queryRunner.query(`CREATE EXTENSION IF NOT EXISTS "uuid-ossp"`);

    // Create ENUM types
    await queryRunner.query(`
      CREATE TYPE "complaint_status_enum" AS ENUM (
        'DRAFT', 'OPEN', 'IN_PROGRESS', 'PENDING_INFO',
        'ESCALATED', 'RESOLVED', 'CLOSED', 'REJECTED'
      )
    `);

    await queryRunner.query(`
      CREATE TYPE "complaint_priority_enum" AS ENUM (
        'LOW', 'MEDIUM', 'HIGH', 'CRITICAL'
      )
    `);

    await queryRunner.query(`
      CREATE TYPE "comment_type_enum" AS ENUM (
        'USER', 'SYSTEM', 'INTERNAL', 'STATUS_CHANGE'
      )
    `);

    await queryRunner.query(`
      CREATE TYPE "attachment_status_enum" AS ENUM (
        'UPLOADING', 'SCANNING', 'CLEAN', 'INFECTED', 'FAILED'
      )
    `);

    await queryRunner.query(`
      CREATE TYPE "role_type_enum" AS ENUM ('SYSTEM', 'CUSTOM')
    `);

    await queryRunner.query(`
      CREATE TYPE "organizational_scope_enum" AS ENUM (
        'GLOBAL', 'COMPANY', 'BRANCH', 'DEPARTMENT', 'SECTION'
      )
    `);

    await queryRunner.query(`
      CREATE TYPE "tenant_status_enum" AS ENUM (
        'ACTIVE', 'TRIAL', 'SUSPENDED', 'INACTIVE'
      )
    `);

    // Create tenants table
    await queryRunner.query(`
      CREATE TABLE "tenants" (
        "tenant_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "name" varchar(255) NOT NULL UNIQUE,
        "subdomain" varchar(100) UNIQUE,
        "status" tenant_status_enum NOT NULL DEFAULT 'ACTIVE',
        "branding" jsonb DEFAULT '{}',
        "features" jsonb DEFAULT '{}',
        "data_residency" varchar(50),
        "compliance_tags" text[] DEFAULT '{}',
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now()
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_tenants_status" ON "tenants" ("status")`);
    await queryRunner.query(`CREATE INDEX "idx_tenants_subdomain" ON "tenants" ("subdomain")`);

    // Create companies table
    await queryRunner.query(`
      CREATE TABLE "companies" (
        "company_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "tenant_id" uuid NOT NULL,
        "oryggi_company_id" int NOT NULL UNIQUE,
        "name" varchar(255) NOT NULL,
        "code" varchar(50) NOT NULL UNIQUE,
        "address" text,
        "email" varchar(255),
        "phone" varchar(50),
        "is_active" boolean DEFAULT true,
        "last_synced_at" timestamp,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_companies_tenant" FOREIGN KEY ("tenant_id")
          REFERENCES "tenants"("tenant_id") ON DELETE CASCADE
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_companies_tenant" ON "companies" ("tenant_id")`);
    await queryRunner.query(`CREATE INDEX "idx_companies_oryggi_id" ON "companies" ("oryggi_company_id")`);
    await queryRunner.query(`CREATE INDEX "idx_companies_code" ON "companies" ("code")`);
    await queryRunner.query(`CREATE INDEX "idx_companies_active" ON "companies" ("is_active")`);

    // Create branches table
    await queryRunner.query(`
      CREATE TABLE "branches" (
        "branch_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "company_id" uuid NOT NULL,
        "oryggi_branch_id" int NOT NULL UNIQUE,
        "name" varchar(255) NOT NULL,
        "code" varchar(50) NOT NULL,
        "location" varchar(255),
        "timezone" varchar(50) DEFAULT 'Asia/Kolkata',
        "is_active" boolean DEFAULT true,
        "last_synced_at" timestamp,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_branches_company" FOREIGN KEY ("company_id")
          REFERENCES "companies"("company_id") ON DELETE CASCADE,
        CONSTRAINT "unique_branch_code" UNIQUE ("company_id", "code")
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_branches_company" ON "branches" ("company_id")`);
    await queryRunner.query(`CREATE INDEX "idx_branches_oryggi_id" ON "branches" ("oryggi_branch_id")`);
    await queryRunner.query(`CREATE INDEX "idx_branches_code" ON "branches" ("code")`);
    await queryRunner.query(`CREATE INDEX "idx_branches_active" ON "branches" ("is_active")`);

    // Create departments table
    await queryRunner.query(`
      CREATE TABLE "departments" (
        "department_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "branch_id" uuid NOT NULL,
        "oryggi_dept_id" int NOT NULL UNIQUE,
        "name" varchar(255) NOT NULL,
        "code" varchar(50) NOT NULL,
        "head_user_id" uuid,
        "is_active" boolean DEFAULT true,
        "last_synced_at" timestamp,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_departments_branch" FOREIGN KEY ("branch_id")
          REFERENCES "branches"("branch_id") ON DELETE CASCADE,
        CONSTRAINT "unique_dept_code" UNIQUE ("branch_id", "code")
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_departments_branch" ON "departments" ("branch_id")`);
    await queryRunner.query(`CREATE INDEX "idx_departments_oryggi_id" ON "departments" ("oryggi_dept_id")`);
    await queryRunner.query(`CREATE INDEX "idx_departments_code" ON "departments" ("code")`);
    await queryRunner.query(`CREATE INDEX "idx_departments_active" ON "departments" ("is_active")`);

    // Create sections table
    await queryRunner.query(`
      CREATE TABLE "sections" (
        "section_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "department_id" uuid NOT NULL,
        "oryggi_section_id" int NOT NULL UNIQUE,
        "name" varchar(255) NOT NULL,
        "code" varchar(50) NOT NULL,
        "supervisor_user_id" uuid,
        "is_active" boolean DEFAULT true,
        "last_synced_at" timestamp,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_sections_department" FOREIGN KEY ("department_id")
          REFERENCES "departments"("department_id") ON DELETE CASCADE,
        CONSTRAINT "unique_section_code" UNIQUE ("department_id", "code")
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_sections_department" ON "sections" ("department_id")`);
    await queryRunner.query(`CREATE INDEX "idx_sections_oryggi_id" ON "sections" ("oryggi_section_id")`);
    await queryRunner.query(`CREATE INDEX "idx_sections_code" ON "sections" ("code")`);
    await queryRunner.query(`CREATE INDEX "idx_sections_active" ON "sections" ("is_active")`);

    // Create users table
    await queryRunner.query(`
      CREATE TABLE "users" (
        "user_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "tenant_id" uuid NOT NULL,
        "company_id" uuid NOT NULL,
        "branch_id" uuid,
        "department_id" uuid,
        "section_id" uuid,
        "oryggi_employee_id" int NOT NULL UNIQUE,
        "employee_code" varchar(50) NOT NULL UNIQUE,
        "email" varchar(255) NOT NULL UNIQUE,
        "phone" varchar(50),
        "phone_secondary" varchar(50),
        "first_name" varchar(100) NOT NULL,
        "last_name" varchar(100) NOT NULL,
        "full_name" varchar(255),
        "manager_id" uuid,
        "oryggi_designation_id" int,
        "oryggi_grade_id" int,
        "oryggi_category_id" int,
        "oryggi_role" varchar(50),
        "date_of_joining" date,
        "date_of_birth" date,
        "password_hash" varchar(255),
        "avatar_url" varchar(255),
        "is_active" boolean DEFAULT true,
        "last_synced_at" timestamp,
        "last_login_at" timestamp,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_users_tenant" FOREIGN KEY ("tenant_id")
          REFERENCES "tenants"("tenant_id") ON DELETE CASCADE,
        CONSTRAINT "fk_users_company" FOREIGN KEY ("company_id")
          REFERENCES "companies"("company_id") ON DELETE CASCADE,
        CONSTRAINT "fk_users_branch" FOREIGN KEY ("branch_id")
          REFERENCES "branches"("branch_id") ON DELETE SET NULL,
        CONSTRAINT "fk_users_department" FOREIGN KEY ("department_id")
          REFERENCES "departments"("department_id") ON DELETE SET NULL,
        CONSTRAINT "fk_users_section" FOREIGN KEY ("section_id")
          REFERENCES "sections"("section_id") ON DELETE SET NULL,
        CONSTRAINT "fk_users_manager" FOREIGN KEY ("manager_id")
          REFERENCES "users"("user_id") ON DELETE SET NULL
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_users_tenant" ON "users" ("tenant_id")`);
    await queryRunner.query(`CREATE INDEX "idx_users_oryggi_id" ON "users" ("oryggi_employee_id")`);
    await queryRunner.query(`CREATE INDEX "idx_users_company" ON "users" ("company_id")`);
    await queryRunner.query(`CREATE INDEX "idx_users_branch" ON "users" ("branch_id")`);
    await queryRunner.query(`CREATE INDEX "idx_users_department" ON "users" ("department_id")`);
    await queryRunner.query(`CREATE INDEX "idx_users_section" ON "users" ("section_id")`);
    await queryRunner.query(`CREATE INDEX "idx_users_manager" ON "users" ("manager_id")`);
    await queryRunner.query(`CREATE INDEX "idx_users_email" ON "users" ("email")`);
    await queryRunner.query(`CREATE INDEX "idx_users_employee_code" ON "users" ("employee_code")`);
    await queryRunner.query(`CREATE INDEX "idx_users_active" ON "users" ("is_active")`);

    // Add foreign key constraints for department and section heads
    await queryRunner.query(`
      ALTER TABLE "departments"
      ADD CONSTRAINT "fk_departments_head"
      FOREIGN KEY ("head_user_id") REFERENCES "users"("user_id") ON DELETE SET NULL
    `);

    await queryRunner.query(`
      ALTER TABLE "sections"
      ADD CONSTRAINT "fk_sections_supervisor"
      FOREIGN KEY ("supervisor_user_id") REFERENCES "users"("user_id") ON DELETE SET NULL
    `);

    // Create complaint_categories table
    await queryRunner.query(`
      CREATE TABLE "complaint_categories" (
        "category_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "name" varchar(100) NOT NULL UNIQUE,
        "code" varchar(50) NOT NULL UNIQUE,
        "description" text,
        "parent_category_id" uuid,
        "icon" varchar(50),
        "color" varchar(20),
        "sort_order" int DEFAULT 0,
        "is_active" boolean DEFAULT true,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now()
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_complaint_categories_active" ON "complaint_categories" ("is_active")`);
    await queryRunner.query(`CREATE INDEX "idx_complaint_categories_parent" ON "complaint_categories" ("parent_category_id")`);

    // Create complaints table
    await queryRunner.query(`
      CREATE TABLE "complaints" (
        "complaint_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "tenant_id" uuid NOT NULL,
        "complaint_number" varchar(50) NOT NULL UNIQUE,
        "company_id" uuid NOT NULL,
        "branch_id" uuid,
        "department_id" uuid,
        "section_id" uuid,
        "category_id" uuid NOT NULL,
        "subject" varchar(200) NOT NULL,
        "description" text NOT NULL,
        "priority" complaint_priority_enum NOT NULL DEFAULT 'MEDIUM',
        "status" complaint_status_enum NOT NULL DEFAULT 'OPEN',
        "created_by_user_id" uuid NOT NULL,
        "assigned_to_user_id" uuid,
        "assigned_to_role_id" uuid,
        "assigned_at" timestamp,
        "current_escalation_level" int DEFAULT 0,
        "escalation_matrix_id" uuid,
        "is_escalated" boolean DEFAULT false,
        "last_escalated_at" timestamp,
        "sla_due_date" timestamp,
        "sla_breached" boolean DEFAULT false,
        "sla_breach_at" timestamp,
        "resolution" text,
        "resolved_by_user_id" uuid,
        "resolved_at" timestamp,
        "closed_by_user_id" uuid,
        "closed_at" timestamp,
        "custom_fields" jsonb,
        "tags" text[] DEFAULT '{}',
        "deleted_at" timestamp,
        "deleted_by_user_id" uuid,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_complaints_category" FOREIGN KEY ("category_id")
          REFERENCES "complaint_categories"("category_id"),
        CONSTRAINT "fk_complaints_company" FOREIGN KEY ("company_id")
          REFERENCES "companies"("company_id"),
        CONSTRAINT "fk_complaints_branch" FOREIGN KEY ("branch_id")
          REFERENCES "branches"("branch_id"),
        CONSTRAINT "fk_complaints_department" FOREIGN KEY ("department_id")
          REFERENCES "departments"("department_id"),
        CONSTRAINT "fk_complaints_section" FOREIGN KEY ("section_id")
          REFERENCES "sections"("section_id"),
        CONSTRAINT "fk_complaints_created_by" FOREIGN KEY ("created_by_user_id")
          REFERENCES "users"("user_id"),
        CONSTRAINT "fk_complaints_assigned_to" FOREIGN KEY ("assigned_to_user_id")
          REFERENCES "users"("user_id"),
        CONSTRAINT "fk_complaints_resolved_by" FOREIGN KEY ("resolved_by_user_id")
          REFERENCES "users"("user_id"),
        CONSTRAINT "fk_complaints_closed_by" FOREIGN KEY ("closed_by_user_id")
          REFERENCES "users"("user_id")
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_complaints_tenant_status_priority_created" ON "complaints" ("tenant_id", "status", "priority", "created_at")`);
    await queryRunner.query(`CREATE INDEX "idx_complaints_status" ON "complaints" ("status")`);
    await queryRunner.query(`CREATE INDEX "idx_complaints_created_at" ON "complaints" ("created_at")`);
    await queryRunner.query(`CREATE INDEX "idx_complaints_assigned_to" ON "complaints" ("assigned_to_user_id")`);
    await queryRunner.query(`CREATE INDEX "idx_complaints_created_by" ON "complaints" ("created_by_user_id")`);
    await queryRunner.query(`CREATE INDEX "idx_complaints_company_branch" ON "complaints" ("company_id", "branch_id")`);
    await queryRunner.query(`CREATE INDEX "idx_complaints_category" ON "complaints" ("category_id")`);
    await queryRunner.query(`CREATE INDEX "idx_complaints_number" ON "complaints" ("complaint_number")`);

    // Create complaint_comments table
    await queryRunner.query(`
      CREATE TABLE "complaint_comments" (
        "comment_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "complaint_id" uuid NOT NULL,
        "comment_type" comment_type_enum NOT NULL DEFAULT 'USER',
        "comment_text" text NOT NULL,
        "created_by_user_id" uuid,
        "is_internal" boolean DEFAULT false,
        "metadata" jsonb,
        "edited_at" timestamp,
        "edited_by_user_id" uuid,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_comments_complaint" FOREIGN KEY ("complaint_id")
          REFERENCES "complaints"("complaint_id") ON DELETE CASCADE,
        CONSTRAINT "fk_comments_created_by" FOREIGN KEY ("created_by_user_id")
          REFERENCES "users"("user_id"),
        CONSTRAINT "fk_comments_edited_by" FOREIGN KEY ("edited_by_user_id")
          REFERENCES "users"("user_id")
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_comments_complaint_created" ON "complaint_comments" ("complaint_id", "created_at")`);
    await queryRunner.query(`CREATE INDEX "idx_comments_created_by" ON "complaint_comments" ("created_by_user_id")`);
    await queryRunner.query(`CREATE INDEX "idx_comments_type" ON "complaint_comments" ("comment_type")`);

    // Create complaint_attachments table
    await queryRunner.query(`
      CREATE TABLE "complaint_attachments" (
        "attachment_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "complaint_id" uuid NOT NULL,
        "original_filename" varchar(255) NOT NULL,
        "stored_filename" varchar(500) NOT NULL,
        "file_path" varchar(500) NOT NULL,
        "mime_type" varchar(100) NOT NULL,
        "file_size" bigint NOT NULL,
        "file_extension" varchar(10),
        "status" attachment_status_enum NOT NULL DEFAULT 'UPLOADING',
        "virus_scanned" boolean DEFAULT false,
        "virus_scan_result" varchar(255),
        "scanned_at" timestamp,
        "storage_url" varchar(500),
        "storage_provider" varchar(100),
        "uploaded_by_user_id" uuid NOT NULL,
        "deleted_at" timestamp,
        "deleted_by_user_id" uuid,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_attachments_complaint" FOREIGN KEY ("complaint_id")
          REFERENCES "complaints"("complaint_id") ON DELETE CASCADE,
        CONSTRAINT "fk_attachments_uploaded_by" FOREIGN KEY ("uploaded_by_user_id")
          REFERENCES "users"("user_id"),
        CONSTRAINT "fk_attachments_deleted_by" FOREIGN KEY ("deleted_by_user_id")
          REFERENCES "users"("user_id")
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_attachments_complaint" ON "complaint_attachments" ("complaint_id")`);
    await queryRunner.query(`CREATE INDEX "idx_attachments_uploaded_by" ON "complaint_attachments" ("uploaded_by_user_id")`);
    await queryRunner.query(`CREATE INDEX "idx_attachments_status" ON "complaint_attachments" ("status")`);

    // Create complaint_roles table
    await queryRunner.query(`
      CREATE TABLE "complaint_roles" (
        "role_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "name" varchar(100) NOT NULL UNIQUE,
        "code" varchar(50) NOT NULL UNIQUE,
        "description" text,
        "role_type" role_type_enum NOT NULL DEFAULT 'CUSTOM',
        "priority_level" int DEFAULT 0,
        "can_view_all_complaints" boolean DEFAULT true,
        "can_view_department_complaints" boolean DEFAULT false,
        "can_view_branch_complaints" boolean DEFAULT false,
        "can_assign_complaints" boolean DEFAULT false,
        "can_escalate_complaints" boolean DEFAULT false,
        "can_resolve_complaints" boolean DEFAULT false,
        "can_close_complaints" boolean DEFAULT false,
        "can_delete_complaints" boolean DEFAULT false,
        "can_configure_escalation" boolean DEFAULT false,
        "can_manage_roles" boolean DEFAULT false,
        "is_active" boolean DEFAULT true,
        "is_system_role" boolean DEFAULT false,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now()
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_complaint_roles_code" ON "complaint_roles" ("code")`);
    await queryRunner.query(`CREATE INDEX "idx_complaint_roles_active" ON "complaint_roles" ("is_active")`);

    // Create user_complaint_roles table
    await queryRunner.query(`
      CREATE TABLE "user_complaint_roles" (
        "user_role_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "user_id" uuid NOT NULL,
        "role_id" uuid NOT NULL,
        "scope" organizational_scope_enum NOT NULL DEFAULT 'GLOBAL',
        "company_id" uuid,
        "branch_id" uuid,
        "department_id" uuid,
        "section_id" uuid,
        "assigned_by_user_id" uuid NOT NULL,
        "valid_from" timestamp,
        "valid_until" timestamp,
        "is_active" boolean DEFAULT true,
        "notes" text,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_user_roles_user" FOREIGN KEY ("user_id")
          REFERENCES "users"("user_id"),
        CONSTRAINT "fk_user_roles_role" FOREIGN KEY ("role_id")
          REFERENCES "complaint_roles"("role_id"),
        CONSTRAINT "fk_user_roles_company" FOREIGN KEY ("company_id")
          REFERENCES "companies"("company_id"),
        CONSTRAINT "fk_user_roles_branch" FOREIGN KEY ("branch_id")
          REFERENCES "branches"("branch_id"),
        CONSTRAINT "fk_user_roles_department" FOREIGN KEY ("department_id")
          REFERENCES "departments"("department_id"),
        CONSTRAINT "fk_user_roles_section" FOREIGN KEY ("section_id")
          REFERENCES "sections"("section_id"),
        CONSTRAINT "fk_user_roles_assigned_by" FOREIGN KEY ("assigned_by_user_id")
          REFERENCES "users"("user_id"),
        CONSTRAINT "unique_user_role_scope" UNIQUE ("user_id", "role_id", "company_id", "branch_id", "department_id", "section_id")
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_user_roles_user_role" ON "user_complaint_roles" ("user_id", "role_id")`);
    await queryRunner.query(`CREATE INDEX "idx_user_roles_role" ON "user_complaint_roles" ("role_id")`);
    await queryRunner.query(`CREATE INDEX "idx_user_roles_scope" ON "user_complaint_roles" ("scope")`);
    await queryRunner.query(`CREATE INDEX "idx_user_roles_active" ON "user_complaint_roles" ("is_active")`);

    // Create complaint_role_permissions table
    await queryRunner.query(`
      CREATE TABLE "complaint_role_permissions" (
        "permission_id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
        "role_id" uuid NOT NULL,
        "module" varchar(50) NOT NULL,
        "resource" varchar(50) NOT NULL,
        "action" varchar(50) NOT NULL,
        "is_allowed" boolean DEFAULT true,
        "conditions" jsonb,
        "description" text,
        "created_at" timestamp NOT NULL DEFAULT now(),
        "updated_at" timestamp NOT NULL DEFAULT now(),
        CONSTRAINT "fk_permissions_role" FOREIGN KEY ("role_id")
          REFERENCES "complaint_roles"("role_id") ON DELETE CASCADE,
        CONSTRAINT "unique_role_permission" UNIQUE ("role_id", "module", "resource", "action")
      )
    `);

    await queryRunner.query(`CREATE INDEX "idx_permissions_role" ON "complaint_role_permissions" ("role_id")`);
    await queryRunner.query(`CREATE INDEX "idx_permissions_module_resource_action" ON "complaint_role_permissions" ("module", "resource", "action")`);
  }

  public async down(queryRunner: QueryRunner): Promise<void> {
    // Drop tables in reverse order of creation
    await queryRunner.query(`DROP TABLE IF EXISTS "complaint_role_permissions" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "user_complaint_roles" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "complaint_roles" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "complaint_attachments" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "complaint_comments" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "complaints" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "complaint_categories" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "users" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "sections" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "departments" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "branches" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "companies" CASCADE`);
    await queryRunner.query(`DROP TABLE IF EXISTS "tenants" CASCADE`);

    // Drop ENUM types
    await queryRunner.query(`DROP TYPE IF EXISTS "tenant_status_enum"`);
    await queryRunner.query(`DROP TYPE IF EXISTS "organizational_scope_enum"`);
    await queryRunner.query(`DROP TYPE IF EXISTS "role_type_enum"`);
    await queryRunner.query(`DROP TYPE IF EXISTS "attachment_status_enum"`);
    await queryRunner.query(`DROP TYPE IF EXISTS "comment_type_enum"`);
    await queryRunner.query(`DROP TYPE IF EXISTS "complaint_priority_enum"`);
    await queryRunner.query(`DROP TYPE IF EXISTS "complaint_status_enum"`);

    // Drop UUID extension
    await queryRunner.query(`DROP EXTENSION IF EXISTS "uuid-ossp"`);
  }
}
