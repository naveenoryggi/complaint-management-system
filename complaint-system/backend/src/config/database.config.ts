import { registerAs } from '@nestjs/config';

export const databaseConfig = registerAs('database', () => ({
  // PostgreSQL (Complaint System)
  postgres: {
    host: process.env.DB_HOST || 'localhost',
    port: parseInt(process.env.DB_PORT, 10) || 5432,
    username: process.env.DB_USERNAME || 'complaint_user',
    password: process.env.DB_PASSWORD || 'complaint_pass',
    database: process.env.DB_DATABASE || 'complaint_management',
  },

  // SQL Server (Oryggi HRMS - Read Only)
  oryggi: {
    host: process.env.ORYGGI_DB_HOST || 'localhost',
    port: parseInt(process.env.ORYGGI_DB_PORT, 10) || 1433,
    username: process.env.ORYGGI_DB_USERNAME || 'sa',
    password: process.env.ORYGGI_DB_PASSWORD,
    database: process.env.ORYGGI_DB_DATABASE || 'Oryggi',
    options: {
      encrypt: process.env.ORYGGI_DB_ENCRYPT === 'true',
      trustServerCertificate: process.env.ORYGGI_DB_TRUST_SERVER_CERTIFICATE === 'true',
      enableArithAbort: true,
    },
  },
}));
