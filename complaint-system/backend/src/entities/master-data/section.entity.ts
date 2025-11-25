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
  Unique,
} from 'typeorm';
import { Department } from './department.entity';
import { User } from './user.entity';

@Entity('sections')
@Index(['department_id'])
@Index(['oryggi_section_id'], { unique: true })
@Index(['code'])
@Index(['is_active'])
@Unique(['department_id', 'code'])
export class Section {
  @PrimaryGeneratedColumn('uuid')
  section_id: string;

  @Column({ type: 'uuid' })
  department_id: string;

  // Synced from Oryggi.SectionMaster
  @Column({ type: 'int', unique: true })
  oryggi_section_id: number; // Maps to Oryggi.SectionMaster.SecCode

  @Column({ type: 'varchar', length: 255 })
  name: string; // From Oryggi.SectionMaster.SecName

  @Column({ type: 'varchar', length: 50 })
  code: string; // From Oryggi.SectionMaster.SecCode

  @Column({ type: 'uuid', nullable: true })
  supervisor_user_id: string; // References users table

  @Column({ type: 'boolean', default: true })
  is_active: boolean;

  @Column({ type: 'timestamp', nullable: true })
  last_synced_at: Date;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => Department, (department) => department.sections, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'department_id' })
  department: Department;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'supervisor_user_id' })
  supervisor: User;

  @OneToMany(() => User, (user) => user.section)
  users: User[];
}
