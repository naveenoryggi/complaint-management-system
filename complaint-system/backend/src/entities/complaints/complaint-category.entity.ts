import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  UpdateDateColumn,
  OneToMany,
  Index,
} from 'typeorm';
import { Complaint } from './complaint.entity';

@Entity('complaint_categories')
@Index(['is_active'])
@Index(['parent_category_id'])
export class ComplaintCategory {
  @PrimaryGeneratedColumn('uuid')
  category_id: string;

  @Column({ type: 'varchar', length: 100, unique: true })
  name: string;

  @Column({ type: 'varchar', length: 50, unique: true })
  code: string;

  @Column({ type: 'text', nullable: true })
  description: string;

  @Column({ type: 'uuid', nullable: true })
  parent_category_id: string; // For hierarchical categories

  @Column({ type: 'varchar', length: 50, nullable: true })
  icon: string; // Icon name for UI

  @Column({ type: 'varchar', length: 20, nullable: true })
  color: string; // Hex color code for UI

  @Column({ type: 'int', default: 0 })
  sort_order: number;

  @Column({ type: 'boolean', default: true })
  is_active: boolean;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @OneToMany(() => Complaint, (complaint) => complaint.category)
  complaints: Complaint[];
}
