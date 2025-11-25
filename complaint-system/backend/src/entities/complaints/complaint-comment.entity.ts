import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  UpdateDateColumn,
  ManyToOne,
  JoinColumn,
  Index,
} from 'typeorm';
import { Complaint } from './complaint.entity';
import { User } from '../master-data/user.entity';

export type CommentType = 'USER' | 'SYSTEM' | 'INTERNAL' | 'STATUS_CHANGE';

@Entity('complaint_comments')
@Index(['complaint_id', 'created_at'])
@Index(['created_by_user_id'])
@Index(['comment_type'])
export class ComplaintComment {
  @PrimaryGeneratedColumn('uuid')
  comment_id: string;

  @Column({ type: 'uuid' })
  complaint_id: string;

  @Column({
    type: 'enum',
    enum: ['USER', 'SYSTEM', 'INTERNAL', 'STATUS_CHANGE'],
    default: 'USER',
  })
  comment_type: CommentType;

  @Column({ type: 'text' })
  comment_text: string;

  @Column({ type: 'uuid', nullable: true })
  created_by_user_id: string; // Nullable for system-generated comments

  @Column({ type: 'boolean', default: false })
  is_internal: boolean; // Internal notes visible only to staff

  @Column({ type: 'jsonb', nullable: true })
  metadata: {
    old_status?: string;
    new_status?: string;
    old_assignee?: string;
    new_assignee?: string;
    escalation_level?: number;
    [key: string]: any;
  };

  @Column({ type: 'timestamp', nullable: true })
  edited_at: Date;

  @Column({ type: 'uuid', nullable: true })
  edited_by_user_id: string;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => Complaint, (complaint) => complaint.comments, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'complaint_id' })
  complaint: Complaint;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'created_by_user_id' })
  created_by: User;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'edited_by_user_id' })
  edited_by: User;
}
