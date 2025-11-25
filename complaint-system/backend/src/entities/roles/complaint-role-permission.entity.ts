import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  UpdateDateColumn,
  ManyToOne,
  JoinColumn,
  Index,
  Unique,
} from 'typeorm';
import { ComplaintRole } from './complaint-role.entity';

@Entity('complaint_role_permissions')
@Index(['role_id'])
@Index(['module', 'resource', 'action'])
@Unique(['role_id', 'module', 'resource', 'action'])
export class ComplaintRolePermission {
  @PrimaryGeneratedColumn('uuid')
  permission_id: string;

  @Column({ type: 'uuid' })
  role_id: string;

  @Column({ type: 'varchar', length: 50 })
  module: string; // e.g., 'complaints', 'escalation', 'email_alerts', 'roles', 'users'

  @Column({ type: 'varchar', length: 50 })
  resource: string; // e.g., 'complaint', 'category', 'comment', 'attachment'

  @Column({ type: 'varchar', length: 50 })
  action: string; // e.g., 'create', 'read', 'update', 'delete', 'assign', 'escalate'

  @Column({ type: 'boolean', default: true })
  is_allowed: boolean;

  @Column({ type: 'jsonb', nullable: true })
  conditions: {
    own_only?: boolean; // Can only perform action on own resources
    department_only?: boolean; // Can only perform action on department resources
    branch_only?: boolean; // Can only perform action on branch resources
    company_only?: boolean; // Can only perform action on company resources
    status_restriction?: string[]; // Can only perform action on specific statuses
    priority_restriction?: string[]; // Can only perform action on specific priorities
    [key: string]: any;
  };

  @Column({ type: 'text', nullable: true })
  description: string;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => ComplaintRole, (role) => role.permissions, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'role_id' })
  role: ComplaintRole;
}
