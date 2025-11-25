import { AppDataSource } from '../../config/typeorm.config';
import { seedTenant } from './01-tenant.seed';
import { seedComplaintRoles } from './02-roles.seed';
import { seedComplaintCategories } from './03-categories.seed';
import { seedRolePermissions } from './04-permissions.seed';

async function runSeeds() {
  try {
    console.log('🌱 Starting database seed...\n');

    // Initialize data source
    await AppDataSource.initialize();
    console.log('✅ Database connection established\n');

    // Run seeds in order
    console.log('📦 Seeding default tenant...');
    await seedTenant(AppDataSource);
    console.log('✅ Tenant seeded\n');

    console.log('📦 Seeding complaint roles...');
    await seedComplaintRoles(AppDataSource);
    console.log('✅ Roles seeded\n');

    console.log('📦 Seeding complaint categories...');
    await seedComplaintCategories(AppDataSource);
    console.log('✅ Categories seeded\n');

    console.log('📦 Seeding role permissions...');
    await seedRolePermissions(AppDataSource);
    console.log('✅ Permissions seeded\n');

    console.log('🎉 All seeds completed successfully!\n');

    await AppDataSource.destroy();
    process.exit(0);
  } catch (error) {
    console.error('❌ Error during seed:', error);
    process.exit(1);
  }
}

runSeeds();
