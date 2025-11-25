import { DataSource } from 'typeorm';
import { config } from 'dotenv';
import { join } from 'path';

// Load environment variables
config();

export const AppDataSource = new DataSource({
  type: 'postgres',
  host: process.env.DB_HOST || 'localhost',
  port: parseInt(process.env.DB_PORT, 10) || 5432,
  username: process.env.DB_USERNAME || 'complaint_user',
  password: process.env.DB_PASSWORD || 'complaint_pass_dev',
  database: process.env.DB_DATABASE || 'complaint_management',
  entities: [join(__dirname, '..', 'entities', '**', '*.entity{.ts,.js}')],
  migrations: [join(__dirname, '..', 'database', 'migrations', '*{.ts,.js}')],
  synchronize: false, // Never use synchronize in production
  logging: process.env.NODE_ENV === 'development',
  migrationsTableName: 'migrations_history',
});
