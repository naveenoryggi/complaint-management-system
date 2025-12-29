import { InjectionToken } from '@angular/core';

/**
 * Injection token for the API base URL.
 * This allows runtime configuration of the API URL via config.json
 */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');
