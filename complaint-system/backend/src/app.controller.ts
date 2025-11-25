import { Controller, Get } from '@nestjs/common';
import { ApiTags, ApiOperation, ApiResponse } from '@nestjs/swagger';
import { AppService } from './app.service';

@ApiTags('Health Check')
@Controller()
export class AppController {
  constructor(private readonly appService: AppService) {}

  @Get()
  @ApiOperation({ summary: 'Health check endpoint' })
  @ApiResponse({ status: 200, description: 'API is running' })
  getHello(): object {
    return this.appService.getHealthCheck();
  }

  @Get('health')
  @ApiOperation({ summary: 'Detailed health status' })
  @ApiResponse({ status: 200, description: 'Health status with details' })
  getHealth(): object {
    return this.appService.getHealthStatus();
  }
}
