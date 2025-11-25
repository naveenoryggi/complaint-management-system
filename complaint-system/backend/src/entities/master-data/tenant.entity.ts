import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  UpdateDateColumn,
  OneToMany,
  Index,
} from 'typeorm';
import { Company } from './company.entity';
import { User } from './user.entity';

@Entity('tenants')
export class Tenant {
  @PrimaryGeneratedColumn('uuid')
  tenant_id: string;

  @Column({ type: 'varchar', length: 255, unique: true })
  name: string;

  @Column({ type: 'varchar', length: 100, unique: true, nullable: true })
  subdomain: string;

  @Column({
    type: 'varchar',
    length: 20,
    default: 'ACTIVE',
  })
  @Index()
  status: 'ACTIVE' | 'TRIAL' | 'SUSPENDED' | 'INACTIVE';

  // White-labeling
  @Column({ type: 'jsonb', default: {} })
  branding: {
    logo_url?: string;
    primary_color?: string;
    secondary_color?: string;
    company_name?: string;
  };

  // Feature flags
  @Column({ type: 'jsonb', default: {} })
  features: {
    webhooks_enabled?: boolean;
    sso_enabled?: boolean;
    mfa_enabled?: boolean;
    custom_roles?: boolean;
  };

  // Compliance
  @Column({ type: 'varchar', length: 50, nullable: true })
  data_residency: string;

  @Column({ type: 'text', array: true, default: [] })
  compliance_tags: string[];

  @CreateDateColumn()
  created_at: Date;

  @UpdateDateColumn()
  updated_at: Date;

  // Relations
  @OneToMany(() => Company, (company) => company.tenant)
  companies: Company[];

  @OneToMany(() => User, (user) => user.tenant)
  users: User[];
}
