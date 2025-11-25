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
import { User } from '../master-data/user.entity';
import { ComplaintRole } from './complaint-role.entity';
import { Company } from '../master-data/company.entity';
import { Branch } from '../master-data/branch.entity';
import { Department } from '../master-data/department.entity';
import { Section } from '../master-data/section.entity';

export type OrganizationalScope = 'GLOBAL' | 'COMPANY' | 'BRANCH' | 'DEPARTMENT' | 'SECTION';

@Entity('user_complaint_roles')
@Index(['user_id', 'role_id'])
@Index(['role_id'])
@Index(['scope'])
@Index(['is_active'])
@Unique(['user_id', 'role_id', 'company_id', 'branch_id', 'department_id', 'section_id'])
export class UserComplaintRole {
  @PrimaryGeneratedColumn('uuid')
  user_role_id: string;

  @Column({ type: 'uuid' })
  user_id: string;

  @Column({ type: 'uuid' })
  role_id: string;

  // Organizational Scope
  @Column({
    type: 'enum',
    enum: ['GLOBAL', 'COMPANY', 'BRANCH', 'DEPARTMENT', 'SECTION'],
    default: 'GLOBAL',
  })
  scope: OrganizationalScope;

  @Column({ type: 'uuid', nullable: true })
  company_id: string; // Required if scope is COMPANY or below

  @Column({ type: 'uuid', nullable: true })
  branch_id: string; // Required if scope is BRANCH or below

  @Column({ type: 'uuid', nullable: true })
  department_id: string; // Required if scope is DEPARTMENT or below

  @Column({ type: 'uuid', nullable: true })
  section_id: string; // Required if scope is SECTION

  @Column({ type: 'uuid' })
  assigned_by_user_id: string;

  @Column({ type: 'timestamp', nullable: true })
  valid_from: Date;

  @Column({ type: 'timestamp', nullable: true })
  valid_until: Date;

  @Column({ type: 'boolean', default: true })
  is_active: boolean;

  @Column({ type: 'text', nullable: true })
  notes: string;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => User)
  @JoinColumn({ name: 'user_id' })
  user: User;

  @ManyToOne(() => ComplaintRole, (role) => role.user_assignments)
  @JoinColumn({ name: 'role_id' })
  role: ComplaintRole;

  @ManyToOne(() => Company, { nullable: true })
  @JoinColumn({ name: 'company_id' })
  company: Company;

  @ManyToOne(() => Branch, { nullable: true })
  @JoinColumn({ name: 'branch_id' })
  branch: Branch;

  @ManyToOne(() => Department, { nullable: true })
  @JoinColumn({ name: 'department_id' })
  department: Department;

  @ManyToOne(() => Section, { nullable: true })
  @JoinColumn({ name: 'section_id' })
  section: Section;

  @ManyToOne(() => User)
  @JoinColumn({ name: 'assigned_by_user_id' })
  assigned_by: User;
}
