import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { PwaService } from '../../../services/pwa.service';

@Component({
  selector: 'app-pwa-install-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pwa-install-banner.component.html',
  styleUrls: ['./pwa-install-banner.component.scss']
})
export class PwaInstallBannerComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  showInstallBanner = false;
  canInstall = false;
  isInstalled = false;
  isOnline = true;

  constructor(private pwaService: PwaService) {}

  ngOnInit(): void {
    this.setupPwaListeners();
  }

  private setupPwaListeners(): void {
    // Listen for install prompt events
    this.pwaService.getInstallPromptEvent$()
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.canInstall = true;
        this.showInstallBanner = true;
      });

    // Listen for app installation status
    this.pwaService.getAppInstalled$()
      .pipe(takeUntil(this.destroy$))
      .subscribe(installed => {
        this.isInstalled = installed;
        if (installed) {
          this.showInstallBanner = false;
          this.canInstall = false;
        }
      });

    // Listen for network status
    this.pwaService.getNetworkStatus$()
      .pipe(takeUntil(this.destroy$))
      .subscribe(online => {
        this.isOnline = online;
      });

    // Check if app can be installed on init
    this.canInstall = this.pwaService.canInstallApp();
    this.isInstalled = this.pwaService.isInstalled();
    this.showInstallBanner = this.canInstall && !this.isInstalled;
  }

  async installApp(): Promise<void> {
    try {
      await this.pwaService.installApp();
      this.showInstallBanner = false;
    } catch (error) {
      console.error('Error installing app:', error);
    }
  }

  dismissBanner(): void {
    this.showInstallBanner = false;
    // Store dismissal in localStorage to not show again for a while
    localStorage.setItem('pwa-install-dismissed', Date.now().toString());
  }

  get isMobileDevice(): boolean {
    const userAgent = navigator.userAgent.toLowerCase();
    return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/i.test(userAgent);
  }

  get deviceIcon(): string {
    if (this.isMobileDevice) {
      return 'bi-phone';
    }
    return 'bi-laptop';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}