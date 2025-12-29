import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment, runtimeConfig } from '../../environments/environment';

export interface AppConfig {
  apiUrl: string;
  baseUrl?: string;
  hostname?: string;
  webPort?: string;
  apiPort?: string;
  isIIS?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AppConfigService {
  private static config: AppConfig | null = null;
  private static initialized = false;

  constructor(private http: HttpClient) {}

  /**
   * Loads configuration from config.json if available, otherwise uses environment defaults.
   * Call this method before app initialization.
   * Updates runtimeConfig.apiUrl which services can use.
   */
  async loadConfig(): Promise<AppConfig> {
    if (AppConfigService.initialized) {
      return AppConfigService.config!;
    }

    try {
      // Try to load config.json from assets
      const response = await firstValueFrom(
        this.http.get<AppConfig>('/assets/config.json')
      );
      AppConfigService.config = response;
      // Update runtime config so services can use it
      runtimeConfig.apiUrl = response.apiUrl;
      console.log('Loaded API configuration from config.json:', AppConfigService.config);
      console.log('API URL set to:', runtimeConfig.apiUrl);
    } catch (error) {
      // Fall back to environment settings if config.json doesn't exist
      console.log('config.json not found, using environment defaults');
      AppConfigService.config = {
        apiUrl: environment.apiUrl
      };
      runtimeConfig.apiUrl = environment.apiUrl;
    }

    AppConfigService.initialized = true;
    return AppConfigService.config;
  }

  /**
   * Gets the API URL. Returns environment default if config hasn't been loaded.
   * Use runtimeConfig.apiUrl directly in services for consistency.
   */
  get apiUrl(): string {
    return runtimeConfig.apiUrl;
  }

  /**
   * Static getter for API URL - can be called without injection
   */
  static getApiUrl(): string {
    return runtimeConfig.apiUrl;
  }

  /**
   * Gets the full configuration object.
   */
  getConfig(): AppConfig | null {
    return AppConfigService.config;
  }
}
