import { DataSource } from 'typeorm';
import { Tenant } from '../../entities/master-data/tenant.entity';

export async function seedTenant(dataSource: DataSource): Promise<void> {
  const tenantRepository = dataSource.getRepository(Tenant);

  // Check if default tenant already exists
  const existingTenant = await tenantRepository.findOne({
    where: { name: 'Default Organization' },
  });

  if (existingTenant) {
    console.log('  ⏭️  Default tenant already exists, skipping...');
    return;
  }

  // Create default tenant
  const tenant = tenantRepository.create({
    name: 'Default Organization',
    subdomain: 'default',
    status: 'ACTIVE',
    branding: {
      logo_url: null,
      primary_color: '#1976d2',
      secondary_color: '#43a047',
      company_name: 'Default Organization',
    },
    features: {
      webhooks_enabled: true,
      sso_enabled: false,
      mfa_enabled: false,
      custom_roles: true,
    },
    data_residency: 'IN',
    compliance_tags: ['GDPR', 'ISO27001'],
  });

  await tenantRepository.save(tenant);
  console.log(`  ✓ Created default tenant: ${tenant.tenant_id}`);
}
