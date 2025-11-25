import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  UpdateDateColumn,
  OneToMany,
  Index,
} from 'typeorm';
import { UserComplaintRole } from './user-complaint-role.entity';
import { ComplaintRolePermission } from './complaint-role-permission.entity';

export type RoleType = 'SYSTEM' | 'CUSTOM';

@Entity('complaint_roles')
@Index(['code'], { unique: true })
@Index(['is_active'])
export class ComplaintRole {
  @PrimaryGeneratedColumn('uuid')
  role_id: string;

  @Column({ type: 'varchar', length: 100, unique: true })
  name: string;

  @Column({ type: 'varchar', length: 50, unique: true })
  code: string; // e.g., SYSTEM_ADMIN, HR_MANAGER, EMPLOYEE

  @Column({ type: 'text', nullable: true })
  description: string;

  @Column({
    type: 'enum',
    enum: ['SYSTEM', 'CUSTOM'],
    default: 'CUSTOM',
  })
  role_type: RoleType;

  @Column({ type: 'int', default: 0 })
  priority_level: number; // Higher number = higher priority

  @Column({ type: 'boolean', default: true })
  can_view_all_complaints: boolean;

  @Column({ type: 'boolean', default: false })
  can_view_department_complaints: boolean;

  @Column({ type: 'boolean', default: false })
  can_view_branch_complaints: boolean;

  @Column({ type: 'boolean', default: false })
  can_assign_complaints: boolean;

  @Column({ type: 'boolean', default: false })
  can_escalate_complaints: boolean;

  @Column({ type: 'boolean', default: false })
  can_resolve_complaints: boolean;

  @Column({ type: 'boolean', default: false })
  can_close_complaints: boolean;

  @Column({ type: 'boolean', default: false })
  can_delete_complaints: boolean;

  @Column({ type: 'boolean', default: false })
  can_configure_escalation: boolean;

  @Column({ type: 'boolean', default: false })
  can_manage_roles: boolean;

  @Column({ type: 'boolean', default: true })
  is_active: boolean;

  @Column({ type: 'boolean', default: false })
  is_system_role: boolean; // Cannot be deleted

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @OneToMany(() => UserComplaintRole, (userRole) => userRole.role)
  user_assignments: UserComplaintRole[];

  @OneToMany(() => ComplaintRolePermission, (permission) => permission.role)
  permissions: ComplaintRolePermission[];
}
