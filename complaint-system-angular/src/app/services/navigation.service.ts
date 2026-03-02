import { Injectable } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { Location } from '@angular/common';

export interface Breadcrumb {
  label: string;
  url: string;
  icon?: string;
}

export interface NavigationConfig {
  title: string;
  showBackButton: boolean;
  showHomeButton: boolean;
  breadcrumbs: Breadcrumb[];
}

@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  private navigationHistory: string[] = [];
  private currentNavigationConfig = new BehaviorSubject<NavigationConfig>({
    title: '',
    showBackButton: false,
    showHomeButton: true,
    breadcrumbs: []
  });

  public navigationConfig$: Observable<NavigationConfig> = this.currentNavigationConfig.asObservable();

  // Route to title mapping
  private routeTitles: { [key: string]: string } = {
    '/dashboard': 'Dashboard',
    '/complaints': 'All Complaints',
    '/complaints/new': 'New Complaint',
    '/admin/company-settings': 'Company Settings',
    '/admin/users': 'User Management',
    '/admin/roles': 'Roles & Permissions',
    '/admin/employee-types': 'Employee Types',
    '/admin/resource-pools': 'Resource Pools',
    '/admin/branches': 'Branch Management',
    '/admin/departments': 'Department Management',
    '/admin/sections': 'Section Management',
    '/admin/categories': 'Category Management',
    '/admin/status-masters': 'Status Masters',
    '/admin/priority-masters': 'Priority Masters',
    '/admin/sla-management': 'SLA Management',
    '/admin/complaint-info-settings': 'Complaint Settings',
    '/admin/email-settings': 'Email Settings',
    '/admin/sms-gateway': 'SMS Gateway Settings',
    '/admin/whatsapp-settings': 'WhatsApp Settings',
    '/admin/templates': 'Communication Templates',
    '/admin/event-types': 'Event Types',
    '/admin/notification-rules': 'Notification Rules',
    '/admin/oryggi-sync': 'Oryggi Sync',
    '/admin/escalation-matrix': 'Escalation Matrix',
    '/admin/escalation-policy': 'Escalation Policy',
    '/admin/escalation-wizard': 'Escalation Wizard',
    '/tenders': 'All Tenders',
    '/tenders/new': 'New Tender',
    '/tender-dashboard': 'Tender Dashboard',
    '/company-profile': 'Company Profile',
    '/reference-bundles': 'Reference Bundles',
    '/oem-management': 'OEM Management',
    '/portal-registrations': 'Portal Registrations',
    '/emd-tracker': 'EMD & Fees Tracker',
    '/ai-generator': 'AI Generator'
  };

  // Route to icon mapping
  private routeIcons: { [key: string]: string } = {
    '/dashboard': 'bi-speedometer2',
    '/complaints': 'bi-list-ul',
    '/complaints/new': 'bi-plus-circle',
    '/admin/company-settings': 'bi-building',
    '/admin/users': 'bi-people',
    '/admin/roles': 'bi-shield-lock',
    '/admin/employee-types': 'bi-person-badge',
    '/admin/resource-pools': 'bi-people-fill',
    '/admin/branches': 'bi-geo-alt',
    '/admin/departments': 'bi-building-gear',
    '/admin/sections': 'bi-boxes',
    '/admin/categories': 'bi-tags',
    '/admin/status-masters': 'bi-circle',
    '/admin/priority-masters': 'bi-flag',
    '/admin/sla-management': 'bi-clock-history',
    '/admin/complaint-info-settings': 'bi-sliders',
    '/admin/email-settings': 'bi-envelope-at',
    '/admin/sms-gateway': 'bi-phone',
    '/admin/whatsapp-settings': 'bi-whatsapp',
    '/admin/templates': 'bi-file-earmark-text',
    '/admin/event-types': 'bi-calendar-event',
    '/admin/notification-rules': 'bi-bell',
    '/admin/oryggi-sync': 'bi-arrow-repeat',
    '/admin/escalation-matrix': 'bi-diagram-3',
    '/admin/escalation-policy': 'bi-shield-check',
    '/tenders': 'bi-file-text',
    '/tenders/new': 'bi-plus-circle',
    '/tender-dashboard': 'bi-speedometer2',
    '/company-profile': 'bi-building',
    '/reference-bundles': 'bi-folder2-open',
    '/oem-management': 'bi-building-gear',
    '/portal-registrations': 'bi-globe',
    '/emd-tracker': 'bi-bank',
    '/ai-generator': 'bi-robot'
  };

  // Parent route mapping for breadcrumbs
  private routeParents: { [key: string]: string } = {
    '/complaints/new': '/complaints',
    '/admin/company-settings': '/dashboard',
    '/admin/users': '/dashboard',
    '/admin/roles': '/dashboard',
    '/admin/employee-types': '/dashboard',
    '/admin/resource-pools': '/dashboard',
    '/admin/branches': '/dashboard',
    '/admin/departments': '/dashboard',
    '/admin/sections': '/dashboard',
    '/admin/categories': '/dashboard',
    '/admin/status-masters': '/dashboard',
    '/admin/priority-masters': '/dashboard',
    '/admin/sla-management': '/dashboard',
    '/admin/complaint-info-settings': '/dashboard',
    '/admin/email-settings': '/dashboard',
    '/admin/sms-gateway': '/dashboard',
    '/admin/whatsapp-settings': '/dashboard',
    '/admin/templates': '/dashboard',
    '/admin/event-types': '/dashboard',
    '/admin/notification-rules': '/dashboard',
    '/admin/oryggi-sync': '/dashboard',
    '/admin/escalation-matrix': '/dashboard',
    '/admin/escalation-policy': '/dashboard',
    '/admin/escalation-wizard': '/dashboard',
    '/tenders': '/tender-dashboard',
    '/tenders/new': '/tenders',
    '/company-profile': '/tender-dashboard',
    '/reference-bundles': '/tender-dashboard',
    '/oem-management': '/tender-dashboard',
    '/portal-registrations': '/tender-dashboard',
    '/emd-tracker': '/tender-dashboard',
    '/ai-generator': '/tenders'
  };

  constructor(
    private router: Router,
    private location: Location
  ) {
    this.initializeNavigationTracking();
  }

  private initializeNavigationTracking(): void {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.trackNavigation(event.urlAfterRedirects);
      this.updateNavigationConfig(event.urlAfterRedirects);
    });
  }

  private trackNavigation(url: string): void {
    // Don't track login page
    if (url === '/login') {
      this.navigationHistory = [];
      return;
    }

    // Remove the current URL if it already exists in history to avoid duplicates
    const existingIndex = this.navigationHistory.indexOf(url);
    if (existingIndex > -1) {
      this.navigationHistory.splice(existingIndex, 1);
    }

    // Add to history
    this.navigationHistory.push(url);

    // Keep only last 20 entries
    if (this.navigationHistory.length > 20) {
      this.navigationHistory.shift();
    }

    console.log('Navigation history:', this.navigationHistory);
  }

  private updateNavigationConfig(url: string): void {
    // Extract base route (without query params or fragments)
    const baseUrl = url.split('?')[0].split('#')[0];

    // Check if it's a complaint detail page (dynamic route)
    let title = this.routeTitles[baseUrl];
    let isComplaintDetail = false;

    if (!title) {
      if (baseUrl.startsWith('/complaints/') && baseUrl !== '/complaints/new') {
        title = 'Complaint Details';
        isComplaintDetail = true;
      } else {
        title = 'Page';
      }
    }

    const breadcrumbs = this.buildBreadcrumbs(baseUrl, isComplaintDetail);
    const showBackButton = this.navigationHistory.length > 1 && baseUrl !== '/dashboard';
    const showHomeButton = baseUrl !== '/dashboard';

    this.currentNavigationConfig.next({
      title,
      showBackButton,
      showHomeButton,
      breadcrumbs
    });
  }

  private buildBreadcrumbs(url: string, isComplaintDetail: boolean = false): Breadcrumb[] {
    const breadcrumbs: Breadcrumb[] = [];

    // Always start with dashboard (home)
    if (url !== '/dashboard') {
      breadcrumbs.push({
        label: 'Home',
        url: '/dashboard',
        icon: 'bi-house-door'
      });
    }

    // For complaint detail pages, build a specific trail
    if (isComplaintDetail) {
      breadcrumbs.push({
        label: 'All Complaints',
        url: '/complaints',
        icon: 'bi-list-ul'
      });

      // Extract complaint ID from URL
      const complaintId = url.split('/').pop();
      breadcrumbs.push({
        label: `Complaint #${complaintId}`,
        url: url,
        icon: 'bi-file-text'
      });

      return breadcrumbs;
    }

    // Build breadcrumb trail for regular routes
    let currentUrl = url;
    const trail: string[] = [];

    while (currentUrl && currentUrl !== '/dashboard') {
      trail.unshift(currentUrl);
      const parent = this.routeParents[currentUrl];
      if (parent && parent !== '/dashboard') {
        currentUrl = parent;
      } else {
        break;
      }
    }

    // Add breadcrumbs for the trail
    for (const route of trail) {
      let label = this.routeTitles[route];

      // If no title is found, try to extract from route
      if (!label) {
        const routePart = route.split('/').pop();
        label = routePart ? routePart.charAt(0).toUpperCase() + routePart.slice(1) : 'Page';
      }

      const icon = this.routeIcons[route];

      breadcrumbs.push({
        label,
        url: route,
        icon
      });
    }

    return breadcrumbs;
  }

  public goBack(): void {
    if (this.navigationHistory.length > 1) {
      // Remove current page
      this.navigationHistory.pop();
      // Get previous page
      const previousUrl = this.navigationHistory[this.navigationHistory.length - 1];
      if (previousUrl) {
        this.router.navigateByUrl(previousUrl);
      } else {
        this.goHome();
      }
    } else {
      this.location.back();
    }
  }

  public goHome(): void {
    this.router.navigate(['/dashboard']);
  }

  public navigateTo(url: string): void {
    this.router.navigate([url]);
  }

  public getNavigationHistory(): string[] {
    return [...this.navigationHistory];
  }

  public canGoBack(): boolean {
    return this.navigationHistory.length > 1;
  }

  public getCurrentPageTitle(): string {
    return this.currentNavigationConfig.value.title;
  }

  public getBreadcrumbs(): Breadcrumb[] {
    return this.currentNavigationConfig.value.breadcrumbs;
  }

  // Set custom title for dynamic pages
  public setPageTitle(title: string): void {
    const config = this.currentNavigationConfig.value;
    this.currentNavigationConfig.next({
      ...config,
      title
    });
  }

  // Update the last breadcrumb (for dynamic pages like complaint details)
  public updateLastBreadcrumb(label: string, icon?: string): void {
    const config = this.currentNavigationConfig.value;
    const breadcrumbs = [...config.breadcrumbs];

    if (breadcrumbs.length > 0) {
      // Update the last breadcrumb
      breadcrumbs[breadcrumbs.length - 1] = {
        ...breadcrumbs[breadcrumbs.length - 1],
        label,
        icon: icon || breadcrumbs[breadcrumbs.length - 1].icon
      };

      this.currentNavigationConfig.next({
        ...config,
        breadcrumbs
      });
    }
  }

  // Add custom breadcrumb
  public addBreadcrumb(label: string, url: string, icon?: string): void {
    const config = this.currentNavigationConfig.value;
    const breadcrumbs = [...config.breadcrumbs];

    // Check if breadcrumb already exists
    const existingIndex = breadcrumbs.findIndex(b => b.url === url);
    if (existingIndex > -1) {
      // Update existing breadcrumb
      breadcrumbs[existingIndex] = { label, url, icon };
    } else {
      breadcrumbs.push({ label, url, icon });
    }

    this.currentNavigationConfig.next({
      ...config,
      breadcrumbs
    });
  }
}
