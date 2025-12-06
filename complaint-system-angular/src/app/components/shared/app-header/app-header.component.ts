import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../services/auth.service';
import { CompanyService } from '../../../services/company.service';
import { AdminMenuConfigService, MenuCategory } from '../../../services/admin-menu-config.service';
import { User } from '../../../models/user.model';
import { Company } from '../../../models/company.model';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app-header.component.html',
  styleUrls: ['./app-header.component.scss']
})
export class AppHeaderComponent implements OnInit, OnDestroy {
  // Inputs for customization
  @Input() showSearch: boolean = false;
  @Input() showNotifications: boolean = false;
  @Input() showAdminPanel: boolean = true;
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
  searchTerm: string = '';
  menuCategories: MenuCategory[] = [];

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private companyService: CompanyService,
    private adminMenuConfig: AdminMenuConfigService,
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
    this.router.navigate(['/admin', page]);
  }

  goHome(): void {
    this.navigateToPage('/dashboard');
  }

  // Search
  onSearch(): void {
    this.searchChange.emit(this.searchTerm);
  }

  // Notifications
  onNotificationClick(): void {
    this.notificationClick.emit();
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
