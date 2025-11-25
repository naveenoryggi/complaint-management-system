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
import { Branch } from './branch.entity';
import { Section } from './section.entity';
import { User } from './user.entity';

@Entity('departments')
@Index(['branch_id'])
@Index(['oryggi_dept_id'], { unique: true })
@Index(['code'])
@Index(['is_active'])
@Unique(['branch_id', 'code'])
export class Department {
  @PrimaryGeneratedColumn('uuid')
  department_id: string;

  @Column({ type: 'uuid' })
  branch_id: string;

  // Synced from Oryggi.DeptMaster
  @Column({ type: 'int', unique: true })
  oryggi_dept_id: number; // Maps to Oryggi.DeptMaster.Dcode

  @Column({ type: 'varchar', length: 255 })
  name: string; // From Oryggi.DeptMaster.Dname

  @Column({ type: 'varchar', length: 50 })
  code: string; // From Oryggi.DeptMaster.Dcode

  @Column({ type: 'uuid', nullable: true })
  head_user_id: string; // References users table

  @Column({ type: 'boolean', default: true })
  is_active: boolean;

  @Column({ type: 'timestamp', nullable: true })
  last_synced_at: Date;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => Branch, (branch) => branch.departments, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'branch_id' })
  branch: Branch;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'head_user_id' })
  head: User;

  @OneToMany(() => Section, (section) => section.department)
  sections: Section[];

  @OneToMany(() => User, (user) => user.department)
  users: User[];
}
