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

export type AttachmentStatus = 'UPLOADING' | 'SCANNING' | 'CLEAN' | 'INFECTED' | 'FAILED';

@Entity('complaint_attachments')
@Index(['complaint_id'])
@Index(['uploaded_by_user_id'])
@Index(['status'])
export class ComplaintAttachment {
  @PrimaryGeneratedColumn('uuid')
  attachment_id: string;

  @Column({ type: 'uuid' })
  complaint_id: string;

  @Column({ type: 'varchar', length: 255 })
  original_filename: string;

  @Column({ type: 'varchar', length: 500 })
  stored_filename: string; // UUID-based filename in storage

  @Column({ type: 'varchar', length: 500 })
  file_path: string; // Full path or S3 key

  @Column({ type: 'varchar', length: 100 })
  mime_type: string;

  @Column({ type: 'bigint' })
  file_size: number; // In bytes

  @Column({ type: 'varchar', length: 10, nullable: true })
  file_extension: string;

  @Column({
    type: 'enum',
    enum: ['UPLOADING', 'SCANNING', 'CLEAN', 'INFECTED', 'FAILED'],
    default: 'UPLOADING',
  })
  status: AttachmentStatus;

  @Column({ type: 'boolean', default: false })
  virus_scanned: boolean;

  @Column({ type: 'varchar', length: 255, nullable: true })
  virus_scan_result: string;

  @Column({ type: 'timestamp', nullable: true })
  scanned_at: Date;

  @Column({ type: 'varchar', length: 500, nullable: true })
  storage_url: string; // Public or signed URL

  @Column({ type: 'varchar', length: 100, nullable: true })
  storage_provider: string; // 'local', 's3', 'azure', etc.

  @Column({ type: 'uuid' })
  uploaded_by_user_id: string;

  @Column({ type: 'timestamp', nullable: true })
  deleted_at: Date;

  @Column({ type: 'uuid', nullable: true })
  deleted_by_user_id: string;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => Complaint, (complaint) => complaint.attachments, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'complaint_id' })
  complaint: Complaint;

  @ManyToOne(() => User)
  @JoinColumn({ name: 'uploaded_by_user_id' })
  uploaded_by: User;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'deleted_by_user_id' })
  deleted_by: User;
}
