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
import { Company } from './company.entity';
import { Department } from './department.entity';
import { User } from './user.entity';

@Entity('branches')
@Index(['company_id'])
@Index(['oryggi_branch_id'], { unique: true })
@Index(['code'])
@Index(['is_active'])
@Unique(['company_id', 'code'])
export class Branch {
  @PrimaryGeneratedColumn('uuid')
  branch_id: string;

  @Column({ type: 'uuid' })
  company_id: string;

  // Synced from Oryggi.BranchMaster
  @Column({ type: 'int', unique: true })
  oryggi_branch_id: number; // Maps to Oryggi.BranchMaster.BranchCode

  @Column({ type: 'varchar', length: 255 })
  name: string; // From Oryggi.BranchMaster.BranchName

  @Column({ type: 'varchar', length: 50 })
  code: string; // From Oryggi.BranchMaster.BranchCode

  @Column({ type: 'varchar', length: 255, nullable: true })
  location: string; // From Oryggi.BranchMaster.Location

  @Column({ type: 'varchar', length: 50, default: 'Asia/Kolkata' })
  timezone: string;

  @Column({ type: 'boolean', default: true })
  is_active: boolean;

  @Column({ type: 'timestamp', nullable: true })
  last_synced_at: Date;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => Company, (company) => company.branches, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'company_id' })
  company: Company;

  @OneToMany(() => Department, (department) => department.branch)
  departments: Department[];

  @OneToMany(() => User, (user) => user.branch)
  users: User[];
}
