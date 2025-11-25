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
} from 'typeorm';
import { Tenant } from './tenant.entity';
import { Company } from './company.entity';
import { Branch } from './branch.entity';
import { Department } from './department.entity';
import { Section } from './section.entity';

@Entity('users')
@Index(['tenant_id'])
@Index(['oryggi_employee_id'], { unique: true })
@Index(['company_id'])
@Index(['branch_id'])
@Index(['department_id'])
@Index(['section_id'])
@Index(['manager_id'])
@Index(['email'], { unique: true })
@Index(['employee_code'], { unique: true })
@Index(['is_active'])
export class User {
  @PrimaryGeneratedColumn('uuid')
  user_id: string;

  @Column({ type: 'uuid' })
  tenant_id: string;

  // Organization Hierarchy (Synced from Oryggi)
  @Column({ type: 'uuid' })
  company_id: string;

  @Column({ type: 'uuid', nullable: true })
  branch_id: string;

  @Column({ type: 'uuid', nullable: true })
  department_id: string;

  @Column({ type: 'uuid', nullable: true })
  section_id: string;

  // Synced from Oryggi.EmployeeMaster
  @Column({ type: 'int', unique: true })
  oryggi_employee_id: number; // Maps to Oryggi.EmployeeMaster.Ecode

  @Column({ type: 'varchar', length: 50, unique: true })
  employee_code: string; // From Oryggi.EmployeeMaster.CorpEmpCode

  @Column({ type: 'varchar', length: 255, unique: true })
  email: string; // From Oryggi.EmployeeMaster.E_mail

  @Column({ type: 'varchar', length: 50, nullable: true })
  phone: string; // From Oryggi.EmployeeMaster.Telephone1

  @Column({ type: 'varchar', length: 50, nullable: true })
  phone_secondary: string; // From Oryggi.EmployeeMaster.Telephone2

  @Column({ type: 'varchar', length: 100 })
  first_name: string; // From Oryggi.EmployeeMaster.FName

  @Column({ type: 'varchar', length: 100 })
  last_name: string; // From Oryggi.EmployeeMaster.LName

  @Column({ type: 'varchar', length: 255, nullable: true })
  full_name: string; // From Oryggi.EmployeeMaster.EmpName

  // Reporting Structure (Synced from Oryggi)
  @Column({ type: 'uuid', nullable: true })
  manager_id: string; // From Oryggi.EmployeeMaster.ReportingHeadEcode

  // Additional Oryggi Fields
  @Column({ type: 'int', nullable: true })
  oryggi_designation_id: number; // From Oryggi.EmployeeMaster.DesCode

  @Column({ type: 'int', nullable: true })
  oryggi_grade_id: number; // From Oryggi.EmployeeMaster.Gcode

  @Column({ type: 'int', nullable: true })
  oryggi_category_id: number; // From Oryggi.EmployeeMaster.Catcode

  @Column({ type: 'varchar', length: 50, nullable: true })
  oryggi_role: string; // From Oryggi.EmployeeMaster.Role

  @Column({ type: 'date', nullable: true })
  date_of_joining: Date; // From Oryggi.EmployeeMaster.DateofJoin

  @Column({ type: 'date', nullable: true })
  date_of_birth: Date; // From Oryggi.EmployeeMaster.DateofBirth

  // Authentication (for complaint system only)
  @Column({ type: 'varchar', length: 255, nullable: true, select: false })
  password_hash: string; // For local authentication if SSO not available

  @Column({ type: 'varchar', length: 255, nullable: true })
  avatar_url: string;

  @Column({ type: 'boolean', default: true })
  is_active: boolean; // From Oryggi.EmployeeMaster.Active

  @Column({ type: 'timestamp', nullable: true })
  last_synced_at: Date;

  @Column({ type: 'timestamp', nullable: true })
  last_login_at: Date;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => Tenant, (tenant) => tenant.users, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'tenant_id' })
  tenant: Tenant;

  @ManyToOne(() => Company, (company) => company.users, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'company_id' })
  company: Company;

  @ManyToOne(() => Branch, (branch) => branch.users, {
    nullable: true,
    onDelete: 'SET NULL',
  })
  @JoinColumn({ name: 'branch_id' })
  branch: Branch;

  @ManyToOne(() => Department, (department) => department.users, {
    nullable: true,
    onDelete: 'SET NULL',
  })
  @JoinColumn({ name: 'department_id' })
  department: Department;

  @ManyToOne(() => Section, (section) => section.users, {
    nullable: true,
    onDelete: 'SET NULL',
  })
  @JoinColumn({ name: 'section_id' })
  section: Section;

  @ManyToOne(() => User, { nullable: true })
  @JoinColumn({ name: 'manager_id' })
  manager: User;

  @OneToMany(() => User, (user) => user.manager)
  subordinates: User[];

  // Complaint relations will be added when creating Complaint entity
  // @OneToMany(() => Complaint, (complaint) => complaint.created_by)
  // created_complaints: Complaint[];

  // @OneToMany(() => Complaint, (complaint) => complaint.assigned_to)
  // assigned_complaints: Complaint[];

  // Role relations will be added when creating UserComplaintRole entity
  // @OneToMany(() => UserComplaintRole, (userRole) => userRole.user)
  // complaint_roles: UserComplaintRole[];
}
