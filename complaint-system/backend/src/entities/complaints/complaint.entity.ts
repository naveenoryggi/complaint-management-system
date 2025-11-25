import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  UpdateDateColumn,
  ManyToOne,
  OneToMany,
  JoinColumn,
  Index,
  BeforeInsert,
} from 'typeorm';
import { User } from '../master-data/user.entity';
import { Company } from '../master-data/company.entity';
import { Branch } from '../master-data/branch.entity';
import { Department } from '../master-data/department.entity';
import { Section } from '../master-data/section.entity';
import { ComplaintCategory } from './complaint-category.entity';
import { ComplaintComment } from './complaint-comment.entity';
import { ComplaintAttachment } from './complaint-attachment.entity';

export type ComplaintStatus =
  | 'DRAFT'
  | 'OPEN'
  | 'IN_PROGRESS'
  | 'PENDING_INFO'
  | 'ESCALATED'
  | 'RESOLVED'
  | 'CLOSED'
  | 'REJECTED';

export type ComplaintPriority = 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';

@Entity('complaints')
@Index(['tenant_id', 'status', 'priority', 'created_at'])
@Index(['status'])
@Index(['created_at'])
@Index(['assigned_to_user_id'])
@Index(['created_by_user_id'])
@Index(['company_id', 'branch_id'])
@Index(['category_id'])
@Index(['complaint_number'], { unique: true })
export class Complaint {
  @PrimaryGeneratedColumn('uuid')
  complaint_id: string;

  @Column({ type: 'uuid' })
  tenant_id: string;

  // Auto-generated complaint number (CMP-2025-000001)
  @Column({ type: 'varchar', length: 50, unique: true })
  complaint_number: string;

  // Organizational Context
  @Column({ type: 'uuid' })
  company_id: string;

  @Column({ type: 'uuid', nullable: true })
  branch_id: string;

  @Column({ type: 'uuid', nullable: true })
  department_id: string;

  @Column({ type: 'uuid', nullable: true })
  section_id: string;

  // Complaint Details
  @Column({ type: 'uuid' })
  category_id: string;

  @Column({ type: 'varchar', length: 200 })
  subject: string;

  @Column({ type: 'text' })
  description: string;

  @Column({
    type: 'enum',
    enum: ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'],
    default: 'MEDIUM',
  })
  priority: ComplaintPriority;

  @Column({
    type: 'enum',
    enum: [
      'DRAFT',
      'OPEN',
      'IN_PROGRESS',
      'PENDING_INFO',
      'ESCALATED',
      'RESOLVED',
      'CLOSED',
      'REJECTED',
    ],
    default: 'OPEN',
  })
  status: ComplaintStatus;

  // Assignment
  @Column({ type: 'uuid' })
  created_by_user_id: string;

  @Column({ type: 'uuid', nullable: true })
  assigned_to_user_id: string;

  @Column({ type: 'uuid', nullable: true })
  assigned_to_role_id: string;

  @Column({ type: 'timestamp', nullable: true })
  assigned_at: Date;

  // Escalation
  @Column({ type: 'int', default: 0 })
  current_escalation_level: number;

  @Column({ type: 'uuid', nullable: true })
  escalation_matrix_id: string;

  @Column({ type: 'boolean', default: false })
  is_escalated: boolean;

  @Column({ type: 'timestamp', nullable: true })
  last_escalated_at: Date;

  // SLA Tracking
  @Column({ type: 'timestamp', nullable: true })
  sla_due_date: Date;

  @Column({ type: 'boolean', default: false })
  sla_breached: boolean;

  @Column({ type: 'timestamp', nullable: true })
  sla_breach_at: Date;

  // Resolution
  @Column({ type: 'text', nullable: true })
  resolution: string;

  @Column({ type: 'uuid', nullable: true })
  resolved_by_user_id: string;

  @Column({ type: 'timestamp', nullable: true })
  resolved_at: Date;

  @Column({ type: 'uuid', nullable: true })
  closed_by_user_id: string;

  @Column({ type: 'timestamp', nullable: true })
  closed_at: Date;

  // Additional Metadata
  @Column({ type: 'jsonb', nullable: true })
  custom_fields: Record<string, any>;

  @Column({ type: 'text', array: true, default: [] })
  tags: string[];

  // Soft Delete
  @Column({ type: 'timestamp', nullable: true })
  deleted_at: Date;

  @Column({ type: 'uuid', nullable: true })
  deleted_by_user_id: string;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Auto-generate complaint number before insert
  @BeforeInsert()
  async generateComplaintNumber() {
    if (!this.complaint_number) {
      const year = new Date().getFullYear();
      // This will be replaced with actual sequence logic in the service
      this.complaint_number = `CMP-${year}-TEMP`;
    }
  }

  // Relations
  @ManyToOne(() => ComplaintCategory, (category) => category.complaints)
  @JoinColumn({ name: 'category_id' })
  category: ComplaintCategory;

  @ManyToOne(() => Company)
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
  @JoinColumn({ name: 'created_by_user_id' })
  created_by: User;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'assigned_to_user_id' })
  assigned_to: User;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'resolved_by_user_id' })
  resolved_by: User;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'closed_by_user_id' })
  closed_by: User;

  @OneToMany(() => ComplaintComment, (comment) => comment.complaint)
  comments: ComplaintComment[];

  @OneToMany(() => ComplaintAttachment, (attachment) => attachment.complaint)
  attachments: ComplaintAttachment[];

  // Escalation history will be added when creating EscalationHistory entity
  // @OneToMany(() => EscalationHistory, (history) => history.complaint)
  // escalation_history: EscalationHistory[];
}
