import { Module } from '@nestjs/common';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { TypeOrmModule } from '@nestjs/typeorm';
import { BullModule } from '@nestjs/bull';
import { ScheduleModule } from '@nestjs/schedule';
import { AppController } from './app.controller';
import { AppService } from './app.service';

// Configuration
import { databaseConfig } from './config/database.config';
import { redisConfig } from './config/redis.config';

// Modules (to be created)
// import { AuthModule } from './modules/auth/auth.module';
// import { UsersModule } from './modules/users/users.module';
// import { ComplaintsModule } from './modules/complaints/complaints.module';
// import { EscalationModule } from './modules/escalation/escalation.module';
// import { EmailAlertsModule } from './modules/email-alerts/email-alerts.module';
// import { RolesModule } from './modules/roles/roles.module';
// import { OryggiSyncModule } from './modules/oryggi-sync/oryggi-sync.module';

@Module({
  imports: [
    // Configuration Module
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: process.env.NODE_ENV === 'production' ? '.env.production' : '.env',
      load: [databaseConfig, redisConfig],
    }),

    // TypeORM for PostgreSQL (Complaint System Database)
    TypeOrmModule.forRootAsync({
      inject: [ConfigService],
      useFactory: (configService: ConfigService) => ({
        type: 'postgres',
        host: configService.get('DB_HOST'),
        port: configService.get<number>('DB_PORT'),
        username: configService.get('DB_USERNAME'),
        password: configService.get('DB_PASSWORD'),
        database: configService.get('DB_DATABASE'),
        entities: [__dirname + '/**/*.entity{.ts,.js}'],
        synchronize: process.env.NODE_ENV === 'development', // Only for dev
        logging: process.env.NODE_ENV === 'development',
        migrations: [__dirname + '/database/migrations/*{.ts,.js}'],
        migrationsRun: false,
      }),
    }),

    // Bull Queue for Redis
    BullModule.forRootAsync({
      inject: [ConfigService],
      useFactory: (configService: ConfigService) => ({
        redis: {
          host: configService.get('REDIS_HOST'),
          port: configService.get<number>('REDIS_PORT'),
          password: configService.get('REDIS_PASSWORD') || undefined,
        },
      }),
    }),

    // Schedule Module for Cron Jobs
    ScheduleModule.forRoot(),

    // Feature Modules (uncomment as you create them)
    // AuthModule,
    // UsersModule,
    // ComplaintsModule,
    // EscalationModule,
    // EmailAlertsModule,
    // RolesModule,
    // OryggiSyncModule,
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}
