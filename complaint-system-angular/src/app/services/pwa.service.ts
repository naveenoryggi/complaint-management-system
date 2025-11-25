import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { SwUpdate } from '@angular/service-worker';
import { Subject, BehaviorSubject, interval } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PwaService {
  private destroy$ = new Subject<void>();
  private deferredPrompt: any = null;

  // Observable streams
  public installPromptEvent = new Subject<any>();
  public appInstalled = new BehaviorSubject<boolean>(false);
  public networkStatus = new BehaviorSubject<boolean>(navigator.onLine);
  public updateAvailable = new BehaviorSubject<boolean>(false);
  public appUpdateActivated = new Subject<void>();

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private swUpdate: SwUpdate
  ) {
    if (isPlatformBrowser(this.platformId)) {
      this.initializePwaFeatures();
    }
  }

  private initializePwaFeatures(): void {
    this.setupInstallPrompt();
    this.setupNetworkMonitoring();
    this.setupServiceWorker();
    this.checkAppInstallation();
  }

  private setupInstallPrompt(): void {
    window.addEventListener('beforeinstallprompt', (event) => {
      event.preventDefault();
      this.deferredPrompt = event;
      this.installPromptEvent.next(event);
    });

    window.addEventListener('appinstalled', () => {
      this.appInstalled.next(true);
      this.deferredPrompt = null;
    });
  }

  private setupNetworkMonitoring(): void {
    window.addEventListener('online', () => {
      this.networkStatus.next(true);
    });

    window.addEventListener('offline', () => {
      this.networkStatus.next(false);
    });
  }

  private setupServiceWorker(): void {
    if (!this.swUpdate.isEnabled) {
      return;
    }

    // Check for updates periodically
    interval(6 * 60 * 60) // Every 6 hours
      .pipe(
        takeUntil(this.destroy$),
        filter(() => this.networkStatus.value)
      )
      .subscribe(() => {
        this.swUpdate.checkForUpdate();
      });

    // Handle update available
    this.swUpdate.versionUpdates.subscribe(event => {
      if (event.type === 'VERSION_DETECTED') {
        this.updateAvailable.next(true);
      }
    });

    // Handle update activated
    this.swUpdate.versionUpdates.subscribe(event => {
      if (event.type === 'VERSION_READY') {
        this.appUpdateActivated.next();
      }
    });
  }

  private checkAppInstallation(): void {
    if ('standalone' in window.navigator && (window.navigator as any).standalone) {
      this.appInstalled.next(true);
    } else if (window.matchMedia('(display-mode: standalone)').matches) {
      this.appInstalled.next(true);
    }
  }

  // Public methods
  public canInstallApp(): boolean {
    return !!this.deferredPrompt;
  }

  public async installApp(): Promise<void> {
    if (!this.deferredPrompt) {
      return;
    }

    try {
      const result = await this.deferredPrompt.prompt();
      const { outcome } = await result.userChoice;

      if (outcome === 'accepted') {
        console.log('User accepted the install prompt');
      } else {
        console.log('User dismissed the install prompt');
      }

      this.deferredPrompt = null;
    } catch (error) {
      console.error('Error during app installation:', error);
    }
  }

  public async checkForUpdates(): Promise<boolean> {
    if (!this.swUpdate.isEnabled) {
      return false;
    }

    try {
      const result = await this.swUpdate.checkForUpdate();
      return result;
    } catch (error) {
      console.error('Error checking for updates:', error);
      return false;
    }
  }

  public async activateUpdate(): Promise<void> {
    if (!this.swUpdate.isEnabled) {
      return;
    }

    try {
      await this.swUpdate.activateUpdate();
    } catch (error) {
      console.error('Error activating update:', error);
    }
  }

  public isOnline(): boolean {
    return this.networkStatus.value;
  }

  public isInstalled(): boolean {
    return this.appInstalled.value;
  }

  public getInstallPromptEvent$() {
    return this.installPromptEvent.asObservable();
  }

  public getAppInstalled$() {
    return this.appInstalled.asObservable();
  }

  public getNetworkStatus$() {
    return this.networkStatus.asObservable();
  }

  public getUpdateAvailable$() {
    return this.updateAvailable.asObservable();
  }

  public getAppUpdateActivated$() {
    return this.appUpdateActivated.asObservable();
  }

  // Cleanup
  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // PWA Features
  public shareContent(data: ShareData): Promise<void> {
    if (!navigator.share) {
      return Promise.reject(new Error('Web Share API not supported'));
    }

    return navigator.share(data);
  }

  public async copyToClipboard(text: string): Promise<void> {
    if (!navigator.clipboard) {
      // Fallback for older browsers
      const textArea = document.createElement('textarea');
      textArea.value = text;
      textArea.style.position = 'fixed';
      textArea.style.left = '-999999px';
      textArea.style.top = '-999999px';
      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();
      const result = document.execCommand('copy');
      document.body.removeChild(textArea);

      return result ? Promise.resolve() : Promise.reject(new Error('Copy failed'));
    }

    return navigator.clipboard.writeText(text);
  }

  public getDisplayMode(): string {
    if ('standalone' in window.navigator && (window.navigator as any).standalone) {
      return 'standalone-ios';
    }

    if (window.matchMedia('(display-mode: standalone)').matches) {
      return 'standalone';
    }

    if (window.matchMedia('(display-mode: minimal-ui)').matches) {
      return 'minimal-ui';
    }

    if (window.matchMedia('(display-mode: browser)').matches) {
      return 'browser';
    }

    return 'unknown';
  }

  public requestPersistentStorage(): Promise<boolean> {
    if ('storage' in navigator && 'persist' in navigator.storage) {
      return navigator.storage.persist().then((persistent) => {
        console.log(`Persistent storage granted: ${persistent}`);
        return persistent;
      }).catch((error) => {
        console.error('Error requesting persistent storage:', error);
        return false;
      });
    }

    return Promise.resolve(false);
  }

  public getStorageEstimate(): Promise<StorageEstimate | null> {
    if ('storage' in navigator && 'estimate' in navigator.storage) {
      return navigator.storage.estimate();
    }

    return Promise.resolve(null);
  }
}