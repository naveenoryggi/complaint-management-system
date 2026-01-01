import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../services/auth.service';
import { CompanyService } from '../../../services/company.service';
import { AdminMenuConfigService, MenuCategory } from '../../../services/admin-menu-config.service';
import { NotificationService } from '../../../services/notification.service';
import { User } from '../../../models/user.model';
import { Company } from '../../../models/company.model';
import { AppNotification, getNotificationIcon, getNotificationColor, NotificationType } from '../../../models/notification.model';
import { ModuleSwitcherComponent } from '../module-switcher/module-switcher.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule, ModuleSwitcherComponent],
  templateUrl: './app-header.component.html',
  styleUrls: ['./app-header.component.scss']
})
export class AppHeaderComponent implements OnInit, OnDestroy {
  // Inputs for customization
  @Input() showSearch: boolean = false;
  @Input() showNotifications: boolean = false;
  @Input() showAdminPanel: boolean = true;
  @Input() showModuleSwitcher: boolean = true;
  @Input() activeNavLink: string = '';
  @Input() notificationCount: number = 0;

  // Outputs for parent communication
  @Output() searchChange = new EventEmitter<string>();
  @Output() notificationClick = new EventEmitter<void>();

  // State
  currentUser: User | null = null;
  company: Company | null = null;
  companyLogoUrl: string | null = null;
  showAdminMenu = false;
  showUserMenu = false;
  showNotificationDropdown = false;
  searchTerm: string = '';
  menuCategories: MenuCategory[] = [];

  // Notification state
  notifications: AppNotification[] = [];
  unreadNotificationCount = 0;

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private companyService: CompanyService,
    private adminMenuConfig: AdminMenuConfigService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.menuCategories = this.adminMenuConfig.getMenuCategories();
  }

  ngOnInit(): void {
    this.authService.currentUser
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        if (user && user.companyId) {
          this.loadCompanyDetails(user.companyId);
        }
        // Initialize notifications when user is available
        if (user && user.id) {
          this.initializeNotifications(user.id);
        }
      });
  }

  /**
   * Initialize notification service and subscriptions
   */
  private initializeNotifications(userId: string): void {
    // Initialize the notification service
    this.notificationService.initialize(userId);

    // Subscribe to notifications
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications.slice(0, 10); // Show max 10 in dropdown
      });

    // Subscribe to unread count
    this.notificationService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadNotificationCount = count;
        // Also update the input binding for backward compatibility
        this.notificationCount = count;
      });

    // Subscribe to new notifications for toast/alerts
    this.notificationService.newNotification$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notification => {
        // Could show a toast notification here
        console.log('New notification received:', notification);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Close menus when clicking outside
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-avatar-dropdown') && !target.closest('.nav-dropdown')) {
      this.showUserMenu = false;
      this.showAdminMenu = false;
    }
    if (!target.closest('.notification-dropdown-wrapper')) {
      this.showNotificationDropdown = false;
    }
  }

  loadCompanyDetails(companyId: string): void {
    this.companyService.getCompanyById(companyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.company = response.data;
            this.companyLogoUrl = this.companyService.getLogoUrl(response.data.logoUrl);
          }
        },
        error: (error) => {
          console.error('Error loading company details:', error);
        }
      });
  }

  // User Role Checks
  isAdmin(): boolean {
    if (!this.currentUser || !this.currentUser.roles) return false;
    return this.currentUser.roles.some(role =>
      role.roleCode === 'SYSTEM_ADMIN' ||
      role.roleCode === 'ADMIN' ||
      role.roleName.toLowerCase().includes('admin')
    );
  }

  getRoleNames(): string {
    if (!this.currentUser || !this.currentUser.roles || this.currentUser.roles.length === 0) {
      return 'No roles';
    }
    return this.currentUser.roles.map(r => r.roleName).join(', ');
  }

  // Menu Toggles
  toggleAdminMenu(): void {
    this.showAdminMenu = !this.showAdminMenu;
    this.showUserMenu = false;
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
    this.showAdminMenu = false;
  }

  toggleMenuCategory(categoryId: string): void {
    this.adminMenuConfig.toggleCategory(categoryId);
    this.menuCategories = this.adminMenuConfig.getMenuCategories();
  }

  // Navigation
  navigateToPage(path: string): void {
    this.closeAllMenus();
    this.router.navigate([path]);
  }

  navigateToAdmin(page: string): void {
    this.closeAllMenus();
    // Split the page path to handle nested routes like 'products/brands'
    const segments = page.split('/').filter(s => s);
    this.router.navigate(['/admin', ...segments]);
  }

  goHome(): void {
    this.navigateToPage('/dashboard');
  }

  // Search
  onSearch(): void {
    this.searchChange.emit(this.searchTerm);
  }

  // Notifications
  toggleNotificationDropdown(): void {
    this.showNotificationDropdown = !this.showNotificationDropdown;
    this.showUserMenu = false;
    this.showAdminMenu = false;

    // Load notifications when opening dropdown
    if (this.showNotificationDropdown) {
      this.notificationService.loadNotifications({ pageSize: 10 });
    }
  }

  onNotificationClick(): void {
    this.notificationClick.emit();
  }

  onNotificationItemClick(notification: AppNotification): void {
    // Mark as read
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }

    // Close dropdown
    this.showNotificationDropdown = false;

    // Navigate to related entity
    const url = this.notificationService.getNotificationUrl(notification);
    this.router.navigate([url]);
  }

  markAllNotificationsAsRead(): void {
    this.notificationService.markAllAsRead().subscribe();
  }

  viewAllNotifications(): void {
    this.showNotificationDropdown = false;
    this.router.navigate(['/notifications']);
  }

  getNotificationIcon(type: NotificationType): string {
    return getNotificationIcon(type);
  }

  getNotificationColor(type: NotificationType): string {
    return getNotificationColor(type);
  }

  getTimeAgo(date: Date | string): string {
    const now = new Date();
    const notificationDate = new Date(date);
    const diffMs = now.getTime() - notificationDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return notificationDate.toLocaleDateString();
  }

  // Auth
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private closeAllMenus(): void {
    this.showUserMenu = false;
    this.showAdminMenu = false;
  }
}
