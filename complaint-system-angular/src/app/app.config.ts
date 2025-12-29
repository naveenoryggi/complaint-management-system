import { ApplicationConfig, provideZoneChangeDetection, LOCALE_ID, APP_INITIALIZER, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideServiceWorker } from '@angular/service-worker';
import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';
import { errorLoggingInterceptor } from './core/interceptors/error-logging.interceptor';
import { registerLocaleData } from '@angular/common';
import localeEnIn from '@angular/common/locales/en-IN';
import { environment } from '../environments/environment';
import { AppConfigService } from './services/app-config.service';

// Register India locale
registerLocaleData(localeEnIn);

// Factory function to load app configuration at startup
function initializeApp(configService: AppConfigService) {
  return () => configService.loadConfig();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimations(),
    { provide: LOCALE_ID, useValue: 'en-IN' },
    // Load configuration before app starts
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AppConfigService],
      multi: true
    },
    provideServiceWorker('ngsw-worker.js', {
      enabled: environment.production,
      registrationStrategy: 'registerWhenStable:30000'
    })
  ]
};
