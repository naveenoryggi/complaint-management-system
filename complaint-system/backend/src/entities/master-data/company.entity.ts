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
import { Branch } from './branch.entity';
import { User } from './user.entity';

@Entity('companies')
@Index(['tenant_id'])
@Index(['oryggi_company_id'], { unique: true })
@Index(['code'], { unique: true })
@Index(['is_active'])
export class Company {
  @PrimaryGeneratedColumn('uuid')
  company_id: string;

  @Column({ type: 'uuid' })
  tenant_id: string;

  // Synced from Oryggi.CompanyMaster
  @Column({ type: 'int', unique: true })
  oryggi_company_id: number; // Maps to Oryggi.CompanyMaster.Ccode

  @Column({ type: 'varchar', length: 255 })
  name: string; // From Oryggi.CompanyMaster.CName

  @Column({ type: 'varchar', length: 50, unique: true })
  code: string; // From Oryggi.CompanyMaster.Ccode

  @Column({ type: 'text', nullable: true })
  address: string; // From Oryggi.CompanyMaster.Address

  @Column({ type: 'varchar', length: 255, nullable: true })
  email: string; // From Oryggi.CompanyMaster.Email

  @Column({ type: 'varchar', length: 50, nullable: true })
  phone: string; // From Oryggi.CompanyMaster.TelephoneNo

  @Column({ type: 'boolean', default: true })
  is_active: boolean;

  @Column({ type: 'timestamp', nullable: true })
  last_synced_at: Date;

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @ManyToOne(() => Tenant, (tenant) => tenant.companies, {
    onDelete: 'CASCADE',
  })
  @JoinColumn({ name: 'tenant_id' })
  tenant: Tenant;

  @OneToMany(() => Branch, (branch) => branch.company)
  branches: Branch[];

  @OneToMany(() => User, (user) => user.company)
  users: User[];
}
