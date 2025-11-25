import { Injectable } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';

@Injectable()
export class AppService {
  constructor(private configService: ConfigService) {}

  getHealthCheck(): object {
    return {
      status: 'ok',
      message: 'Complaint Management System API is running',
      timestamp: new Date().toISOString(),
    };
  }

  getHealthStatus(): object {
    return {
      status: 'ok',
      timestamp: new Date().toISOString(),
      environment: this.configService.get<string>('NODE_ENV'),
      version: '1.0.0',
      uptime: process.uptime(),
      services: {
        database: 'connected', // Will be updated with actual check
        redis: 'connected', // Will be updated with actual check
        oryggi: 'not_configured', // Will be updated with actual check
      },
    };
  }
}
