import { Component, signal, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { ThemeService } from './services/theme.service';
import { PwaService } from './services/pwa.service';
import { CommonModule, DOCUMENT, isPlatformBrowser } from '@angular/common';
import { Subscription } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { PwaInstallBannerComponent } from './components/shared/pwa-install-banner/pwa-install-banner.component';
import { BreadcrumbComponent } from './components/shared/breadcrumb/breadcrumb.component';
import { ThemeCustomizerComponent } from './components/shared/theme-customizer/theme-customizer.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, PwaInstallBannerComponent, BreadcrumbComponent, ThemeCustomizerComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('complaint-system-angular');
  private themeSubscription: Subscription | null = null;
  private destroy$ = new Subject<void>();
  private currentUrl = '';

  constructor(
    private themeService: ThemeService,
    private pwaService: PwaService,
    private router: Router,
    @Inject(DOCUMENT) private document: Document,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    console.log('App component initialized');

    // Track current URL for showing/hiding breadcrumb and managing body classes
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.currentUrl = event.urlAfterRedirects;
      this.updateBodyClasses();
    });
  }

  isLoginPage(): boolean {
    return this.currentUrl === '/login' || this.currentUrl === '/';
  }

  /**
   * Update body classes based on current route for proper theme application
   */
  private updateBodyClasses(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    try {
      const body = this.document.body;

      if (this.isLoginPage()) {
        // Add login-page class for special login background
        body.classList.add('login-page');
      } else {
        // Remove login-page class for glassmorphism background
        body.classList.remove('login-page');
      }
    } catch (error) {
      console.error('Error updating body classes:', error);
    }
  }

  ngOnInit(): void {
    console.log('App component ngOnInit called');

    // Set initial body class for proper theme
    this.updateBodyClasses();

    try {
      this.themeSubscription = this.themeService.config$.subscribe(config => {
        console.log('Theme configuration updated:', config);
      });
    } catch (error) {
      console.error('Error in theme subscription:', error);
    }

    // Initialize PWA features with error handling
    try {
      this.initializePwaFeatures();
    } catch (error) {
      console.error('Error initializing PWA features:', error);
    }
  }

  private initializePwaFeatures(): void {
    console.log('Initializing PWA features...');

    try {
      // Listen for app updates
      this.pwaService.getUpdateAvailable$()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updateAvailable) => {
            if (updateAvailable) {
              this.handleAppUpdate();
            }
          },
          error: (error) => {
            console.error('Error in update available subscription:', error);
          }
        });

      // Listen for network status changes
      this.pwaService.getNetworkStatus$()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (isOnline) => {
            console.log('Network status changed:', isOnline ? 'online' : 'offline');
          },
          error: (error) => {
            console.error('Error in network status subscription:', error);
          }
        });
    } catch (error) {
      console.error('Error setting up PWA listeners:', error);
    }
  }

  private handleAppUpdate(): void {
    try {
      // Create a notification for app update
      if ('Notification' in window && Notification.permission === 'granted') {
        new Notification('App Update Available', {
          body: 'A new version of the Complaint Management System is available.',
          icon: '/assets/icons/icon-192x192.png'
        });
      }
    } catch (error) {
      console.error('Error handling app update:', error);
    }
  }

  ngOnDestroy(): void {
    console.log('App component ngOnDestroy called');
    if (this.themeSubscription) {
      this.themeSubscription.unsubscribe();
    }
    this.destroy$.next();
    this.destroy$.complete();
  }
}
