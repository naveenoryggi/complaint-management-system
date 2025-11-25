import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

console.log('Starting Angular application bootstrap...');

bootstrapApplication(App, appConfig)
  .then(() => {
    console.log('Angular application bootstrapped successfully!');
  })
  .catch((err) => {
    console.error('Angular bootstrap failed:', err);
    console.error('Stack trace:', err.stack);
  });
