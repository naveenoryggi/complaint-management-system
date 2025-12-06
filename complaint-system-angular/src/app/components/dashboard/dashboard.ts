import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin, Observable, Subscription, Subject, of } from 'rxjs';
import { map, takeUntil, catchError, timeout } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { ComplaintService } from '../../services/complaint.service';
import { BranchService } from '../../services/branch.service';
import { DepartmentService } from '../../services/department.service';
import { SectionService } from '../../services/section.service';
import { UserService } from '../../services/user.service';
import { CompanyService } from '../../services/company.service';
import { DashboardService } from '../../services/dashboard.service';
import { DateService } from '../../services/date.service';
import { MasterDataService, PriorityOption, StatusOption } from '../../services/master-data.service';
import { AdminMenuConfigService, MenuCategory } from '../../services/admin-menu-config.service';
import { StatusWidgetComponent } from './status-widget/status-widget.component';
import { DashboardCustomizerComponent } from './dashboard-customizer/dashboard-customizer.component';
import { ThemeSettingsComponent } from '../shared/theme-settings/theme-settings.component';
import { User } from '../../models/user.model';
import { Complaint } from '../../models/complaint.model';
import { Branch } from '../../models/branch.model';
import { Department } from '../../models/department.model';
import { Section } from '../../models/section.model';
import { Company } from '../../models/company.model';
import { DashboardPreferences, DashboardStatistics, SaveDashboardPreferencesRequest } from '../../models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, StatusWidgetComponent, DashboardCustomizerComponent, ThemeSettingsComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  company: Company | null = null;
  companyLogoUrl: string | null = null;
  complaints: Complaint[] = [];
  loading = false;
  showAdminMenu = false;
  showUserMenu = false; // Controls user profile dropdown visibility
  menuCategories: MenuCategory[] = [];

  // Memory Leak Prevention - Subscription Management
  private destroy$ = new Subject<void>();
  private subscriptions: Subscription[] = [];

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Filters
  selectedStatus: string = '';
  selectedPriority: string = '';
  searchTerm: string = '';

  // Dynamic loading arrays for dropdowns
  statusOptions: StatusOption[] = [];
  priorityOptions: PriorityOption[] = [];
  loadingMasterData = false;

  // Status IDs for statistics (loaded from master data)
  submittedStatusId?: string;
  inProgressStatusId?: string;
  resolvedStatusId?: string;

  // Statistics
  stats = {
    total: 0,
    submitted: 0,
    inProgress: 0,
    resolved: 0
  };

  // Unassigned complaints count
  unassignedCount: number = 0;

  // Dynamic Dashboard
  dashboardPreferences?: DashboardPreferences;
  dashboardStatistics?: DashboardStatistics;
  showCustomizer: boolean = false;
  loadingDashboard: boolean = false;
  autoRefreshTimer?: any;

  // Theme Settings
  showThemeSettings: boolean = false;

  // New Dashboard Design Properties
  selectedTimeRange: string = 'today';
  statusTimeRange: string = '30';
  categoryStats: { name: string; percentage: number }[] = [];
  categoryColors: string[] = ['#0057FF', '#34c759', '#ff9500', '#ff3b30', '#5856d6', '#af52de'];

  // Quick Filter for Recent Complaints
  quickFilter: string = 'all';

  // Math reference for template usage
  Math = Math;

  // Assignment Modal
  showAssignModal = false;
  selectedComplaint: Complaint | null = null;
  branches: Branch[] = [];
  departments: Department[] = [];
  sections: Section[] = [];
  users: User[] = [];
  filteredUsers: User[] = [];
  userSearchTerm: string = '';
  filterBranchId: string = '';
  filterDepartmentId: string = '';
  filterSectionId: string = '';
  selectedUserId: string = '';
  assigning = false;
  assignmentError = '';

  constructor(
    private authService: AuthService,
    private complaintService: ComplaintService,
    private branchService: BranchService,
    private departmentService: DepartmentService,
    private sectionService: SectionService,
    private userService: UserService,
    private companyService: CompanyService,
    private dashboardService: DashboardService,
    private dateService: DateService,
    private masterDataService: MasterDataService,
    private adminMenuConfig: AdminMenuConfigService,
    private router: Router
  ) {
    // Memory Leak Prevention - Managed subscription
    const userSubscription = this.authService.currentUser
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        if (user && user.companyId) {
          this.loadCompanyDetails(user.companyId);
        }
      });
    this.subscriptions.push(userSubscription);

    this.menuCategories = this.adminMenuConfig.getMenuCategories();
  }

  ngOnInit(): void {
    this.initializeDashboard();
  }

  ngOnDestroy(): void {
    console.log('Dashboard component destroyed - cleaning up subscriptions and timers');

    // Memory Leak Prevention - Clean up all subscriptions
    this.subscriptions.forEach(subscription => {
      if (subscription && !subscription.closed) {
        subscription.unsubscribe();
      }
    });
    this.subscriptions = [];

    // Emit destroy signal for takeUntil operators
    this.destroy$.next();
    this.destroy$.complete();

    // Clean up timers
    if (this.autoRefreshTimer) {
      clearInterval(this.autoRefreshTimer);
      this.autoRefreshTimer = undefined;
    }
  }

  // Performance optimized parallel initialization with caching
  initializeDashboard(): void {
    this.loading = true;
    this.loadingDashboard = true;
    this.loadingMasterData = true;

    // Preload master data for better caching performance
    this.masterDataService.preloadMasterData().subscribe({
      next: () => console.log('Master data preloaded into cache'),
      error: (error) => console.warn('Failed to preload master data:', error)
    });

    // Load all initial data in parallel with caching support
    // FIXED: Added 30-second timeout to prevent complainant login hangs
    forkJoin({
      masterData: this.loadMasterDataParallel(),
      complaints: this.loadComplaintsParallel(),
      statistics: this.loadStatisticsParallel(),
      dashboardPreferences: this.loadDashboardPreferencesParallel(),
      dashboardStatistics: this.loadDashboardStatisticsParallel()
    }).pipe(
      timeout(30000), // 30-second timeout for all parallel API calls
      catchError(error => {
        console.error('Dashboard initialization timeout or error:', error);
        // Return empty results to allow dashboard to load with defaults
        return of({
          masterData: undefined,
          complaints: undefined,
          statistics: undefined,
          dashboardPreferences: undefined,
          dashboardStatistics: undefined
        });
      })
    ).subscribe({
      next: (results) => {
        // All data loaded successfully (or timed out with defaults)
        this.loading = false;
        this.loadingDashboard = false;
        this.loadingMasterData = false;

        // All parallel data loaded successfully
        console.log('Dashboard initialized with parallel loading and caching - performance optimized');

        // Log cache statistics for monitoring
        const dashboardStats = this.dashboardService.getCacheStats();
        const masterDataStats = this.masterDataService.getCacheStats();
        console.log('Cache Statistics - Dashboard:', dashboardStats);
        console.log('Cache Statistics - Master Data:', masterDataStats);

        // Setup cache-optimized auto-refresh
        this.setupAutoRefreshWithCache();
      },
      error: (error) => {
        console.error('Failed to initialize dashboard:', error);
        this.loading = false;
        this.loadingDashboard = false;
        this.loadingMasterData = false;

        // Fallback: try to load essential data individually
        this.loadEssentialDataFallback();
      }
    });
  }

  loadMasterData(): Promise<void> {
    // This method is kept for backward compatibility
    return new Promise((resolve, reject) => {
      this.loadMasterDataParallel().subscribe({
        next: () => resolve(),
        error: (error: any) => reject(error)
      });
    });
  }

  loadMasterDataParallel(): Observable<void> {
    return forkJoin({
      statusOptions: this.masterDataService.getStatusOptions(),
      priorityOptions: this.masterDataService.getPriorityOptions()
    }).pipe(
      map((result: any) => {
        this.statusOptions = result.statusOptions;
        this.priorityOptions = result.priorityOptions;

        // Extract specific status IDs for statistics
        this.submittedStatusId = this.statusOptions.find(s =>
          s.label.toLowerCase() === 'submitted')?.value;
        this.inProgressStatusId = this.statusOptions.find(s =>
          s.label.toLowerCase() === 'in progress')?.value;
        this.resolvedStatusId = this.statusOptions.find(s =>
          s.label.toLowerCase() === 'resolved')?.value;

        console.log('Master data loaded in parallel');
      })
    );
  }

  loadComplaintsParallel(): Observable<void> {
    const status = this.selectedStatus || undefined;
    const priority = this.selectedPriority || undefined;
    const search = this.searchTerm || undefined;

    // Apply role-based filtering
    const roleFilters = this.getRoleBasedFilters();

    return this.complaintService.getComplaints(
      this.currentPage,
      this.pageSize,
      status,
      priority,
      search,
      roleFilters.assignedToId,
      roleFilters.complainantId
    ).pipe(
      map((response: any) => {
        if (response.isSuccess && response.data) {
          this.complaints = response.data.items;
          this.totalCount = response.data.totalCount;
          this.totalPages = response.data.totalPages;
          this.loadCategoryStats(); // Load category statistics from complaints
          // Calculate unassigned count from loaded complaints
          this.calculateUnassignedCount();
        }
        console.log('Complaints loaded in parallel with role-based filtering');
      })
    );
  }

  /**
   * Calculate unassigned complaints count from loaded complaints
   * This counts complaints where assignedToId is null, empty, or undefined
   */
  calculateUnassignedCount(): void {
    if (this.complaints && this.complaints.length > 0) {
      this.unassignedCount = this.complaints.filter(
        (c: any) => !c.assignedToId || c.assignedToId === '' || c.assignedToId === null
      ).length;
    }
  }

  loadStatisticsParallel(): Observable<void> {
    // Apply role-based filtering for statistics as well
    const roleFilters = this.getRoleBasedFilters();

    // Load all statistics in parallel instead of sequentially with role-based filtering
    return forkJoin({
      total: this.complaintService.getComplaints(1, 1, undefined, undefined, undefined, roleFilters.assignedToId, roleFilters.complainantId),
      submitted: this.complaintService.getComplaints(1, 1, this.submittedStatusId, undefined, undefined, roleFilters.assignedToId, roleFilters.complainantId),
      inProgress: this.complaintService.getComplaints(1, 1, this.inProgressStatusId, undefined, undefined, roleFilters.assignedToId, roleFilters.complainantId),
      resolved: this.complaintService.getComplaints(1, 1, this.resolvedStatusId, undefined, undefined, roleFilters.assignedToId, roleFilters.complainantId)
    }).pipe(
      map((results: any) => {
        if (results.total.isSuccess && results.total.data) {
          this.stats.total = results.total.data.totalCount;
        }
        if (results.submitted.isSuccess && results.submitted.data) {
          this.stats.submitted = results.submitted.data.totalCount;
        }
        if (results.inProgress.isSuccess && results.inProgress.data) {
          this.stats.inProgress = results.inProgress.data.totalCount;
        }
        if (results.resolved.isSuccess && results.resolved.data) {
          this.stats.resolved = results.resolved.data.totalCount;
        }
        console.log('Statistics loaded in parallel with role-based filtering - 4 API calls executed concurrently');
      })
    );
  }

  // Load Dashboard Preferences with Local Storage Priority
  loadDashboardPreferencesParallel(): Observable<void> {
    // Try to load from local storage first for immediate display
    this.loadWidgetStateFromLocalStorage();

    return this.dashboardService.getPreferences().pipe(
      map((response: any) => {
        // Enhanced null checking to prevent "Cannot read properties of null" error
        if (!response) {
          console.warn('Dashboard preferences API returned null response');
          console.log('Keeping local storage preferences due to null API response');
          this.setupAutoRefresh();
          return;
        }

        if (response.isSuccess && response.data) {
          // Only update if API response has valid data and differs from current preferences
          const currentPrefs = JSON.stringify(this.dashboardPreferences);
          const newPrefs = JSON.stringify(response.data);

          if (currentPrefs !== newPrefs) {
            console.log('API preferences different from local storage, updating...');
            this.dashboardPreferences = response.data;
            // Save to local storage for persistence
            this.saveWidgetStateToLocalStorage(response.data);
          } else {
            console.log('API preferences match local storage, keeping current state');
          }
          this.setupAutoRefresh();
        } else if (!response.isSuccess) {
          console.warn('Dashboard preferences API returned failure:', response.message);
          // Keep local storage data when API fails
          console.log('Keeping local storage preferences due to API failure');
          this.setupAutoRefresh();
        } else {
          console.warn('Dashboard preferences API returned invalid response structure');
          console.log('Response:', response);
          // Keep local storage data when API returns invalid response
          console.log('Keeping local storage preferences due to invalid API response');
          this.setupAutoRefresh();
        }
        console.log('Dashboard preferences loaded in parallel');
      }),
      catchError((error) => {
        console.error('Error loading dashboard preferences:', error);
        console.log('Keeping local storage preferences due to API error');
        this.setupAutoRefresh();
        return of(void 0); // Return empty observable to prevent breaking the chain
      })
    );
  }

  loadDashboardStatisticsParallel(): Observable<void> {
    const dateRangeDays = this.dashboardPreferences?.dateRangeDays;
    const statusIds = this.dashboardPreferences?.statusWidgets;

    return this.dashboardService.getStatistics(dateRangeDays, statusIds).pipe(
      map((response: any) => {
        // Enhanced null checking to prevent "Cannot read properties of null" error
        if (!response) {
          console.warn('Dashboard statistics API returned null response');
          console.log('Keeping existing statistics due to null API response');
          return;
        }

        if (response.isSuccess && response.data) {
          this.dashboardStatistics = response.data;
        } else if (!response.isSuccess) {
          console.warn('Dashboard statistics API returned failure:', response.message);
          console.log('Keeping existing statistics due to API failure');
        } else {
          console.warn('Dashboard statistics API returned invalid response structure');
          console.log('Keeping existing statistics due to invalid API response');
        }
        console.log('Dashboard statistics loaded in parallel');
      }),
      catchError((error) => {
        console.error('Error loading dashboard statistics:', error);
        console.log('Keeping existing statistics due to API error');
        return of(void 0); // Return empty observable to prevent breaking the chain
      })
    );
  }

  loadEssentialDataFallback(): void {
    // Fallback method for essential data if parallel loading fails
    console.log('Loading essential data as fallback...');
    const fallbackSubscription = forkJoin({
      masterData: this.loadMasterDataParallel(),
      complaints: this.loadComplaintsParallel()
    }).subscribe({
      next: () => {
        this.loadingMasterData = false;
        this.loading = false;
        console.log('Essential data loaded successfully via fallback');
      },
      error: (error) => {
        console.error('Fallback loading also failed:', error);
        this.loadingMasterData = false;
        this.loading = false;
      }
    });
    this.addSubscription(fallbackSubscription);
  }

  // Memory Leak Prevention - Subscription Management Helper Methods
  private addSubscription(subscription: Subscription): void {
    if (subscription) {
      this.subscriptions.push(subscription);
    }
  }

  private createManagedSubscription<T>(
    observable: Observable<T>,
    next?: (value: T) => void,
    error?: (error: any) => void,
    complete?: () => void
  ): Subscription {
    const subscription = observable
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next,
        error,
        complete
      });
    this.addSubscription(subscription);
    return subscription;
  }

  loadCompanyDetails(companyId: string): void {
    this.createManagedSubscription(
      this.companyService.getCompanyById(companyId),
      (response) => {
        if (response.isSuccess && response.data) {
          this.company = response.data;
          this.companyLogoUrl = this.companyService.getLogoUrl(response.data.logoUrl);
        }
      },
      (error) => {
        console.error('Error loading company details:', error);
      }
    );
  }

  loadComplaints(): void {
    this.loading = true;

    const status = this.selectedStatus || undefined;
    const priority = this.selectedPriority || undefined;
    const search = this.searchTerm || undefined;

    // Apply role-based filtering
    const roleFilters = this.getRoleBasedFilters();

    this.complaintService.getComplaints(
      this.currentPage,
      this.pageSize,
      status,
      priority,
      search,
      roleFilters.assignedToId,
      roleFilters.complainantId
    ).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaints = response.data.items;
          this.totalCount = response.data.totalCount;
          this.totalPages = response.data.totalPages;
          this.loadCategoryStats(); // Load category statistics from complaints
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading complaints:', error);
        this.loading = false;
      }
    });
  }

  loadStatistics(): void {
    // Apply role-based filtering for statistics
    const roleFilters = this.getRoleBasedFilters();

    // Load statistics for all statuses with role-based filtering
    this.complaintService.getComplaints(1, 1, undefined, undefined, undefined, roleFilters.assignedToId, roleFilters.complainantId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.stats.total = response.data.totalCount;
        }
      }
    });

    // Load submitted count
    this.complaintService.getComplaints(1, 1, this.submittedStatusId, undefined, undefined, roleFilters.assignedToId, roleFilters.complainantId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.stats.submitted = response.data.totalCount;
        }
      }
    });

    // Load in progress count
    this.complaintService.getComplaints(1, 1, this.inProgressStatusId, undefined, undefined, roleFilters.assignedToId, roleFilters.complainantId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.stats.inProgress = response.data.totalCount;
        }
      }
    });

    // Load resolved count
    this.complaintService.getComplaints(1, 1, this.resolvedStatusId, undefined, undefined, roleFilters.assignedToId, roleFilters.complainantId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.stats.resolved = response.data.totalCount;
        }
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadComplaints();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadComplaints();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadComplaints();
  }

  viewComplaint(complaintId: string): void {
    // Navigate to complaint detail view
    this.router.navigate(['/complaints', complaintId]);
  }

  createComplaint(): void {
    // Navigate to complaint creation form
    this.router.navigate(['/complaints/new']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // Theme Settings Methods
  toggleThemeSettings(): void {
    this.showThemeSettings = !this.showThemeSettings;
  }

  closeThemeSettings(): void {
    this.showThemeSettings = false;
  }

  // Get CSS class for status
  getStatusClass(status: string | undefined | null): string {
    // Add null/undefined safety check
    if (status === null || status === undefined) {
      return 'status-default';
    }

    // Handle string values directly from API
    switch(status.toLowerCase()) {
      case 'submitted':
        return 'status-submitted';
      case 'inprogress':
      case 'in progress':
        return 'status-in-progress';
      case 'resolved':
        return 'status-resolved';
      case 'closed':
        return 'status-closed';
      case 'escalated':
        return 'status-escalated';
      case 'rejected':
        return 'status-rejected';
      default:
        return 'status-default';
    }
  }

  getPriorityClass(priority: string | undefined | null): string {
    // Add null/undefined safety check
    if (priority === null || priority === undefined) {
      return 'priority-normal';
    }

    // Handle string values directly from API
    switch(priority.toLowerCase()) {
      case 'critical':
      case 'urgent':
      case 'emergency':
      case 'severe':
        return 'priority-emergency';
      case 'high':
      case 'elevated':
        return 'priority-high';
      case 'normal':
      case 'medium':
        return 'priority-normal';
      case 'low':
        return 'priority-low';
      default:
        return 'priority-normal';
    }
  }

  getStatusName(status: string | undefined | null): string {
    // Add null/undefined safety check
    if (status === null || status === undefined) {
      return 'Unknown';
    }

    // Return string value directly from API
    return status;
  }

  getPriorityName(priority: string | undefined | null): string {
    // Add null/undefined safety check
    if (priority === null || priority === undefined) {
      return 'Unknown';
    }

    // Return string value directly from API
    return priority;
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';

    // Use DateService which respects user's configured timezone
    return this.dateService.formatDate(dateString);
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  // Admin menu functionality
  isAdmin(): boolean {
    if (!this.currentUser || !this.currentUser.roles) return false;
    return this.currentUser.roles.some(role =>
      role.roleCode === 'SYSTEM_ADMIN' ||
      role.roleCode === 'ADMIN' ||
      role.roleName.toLowerCase().includes('admin')
    );
  }

  // Get the role names for display
  getRoleNames(): string {
    if (!this.currentUser || !this.currentUser.roles || this.currentUser.roles.length === 0) {
      return 'No roles';
    }
    return this.currentUser.roles.map(r => r.roleName).join(', ');
  }

  toggleAdminMenu(): void {
    this.showAdminMenu = !this.showAdminMenu;
    this.showUserMenu = false; // Close user menu when opening admin menu
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
    this.showAdminMenu = false; // Close admin menu when opening user menu
  }

  navigateToAdmin(page: string): void {
    this.showAdminMenu = false;
    this.router.navigate(['/admin', page]);
  }

  toggleMenuCategory(categoryId: string): void {
    this.adminMenuConfig.toggleCategory(categoryId);
    this.menuCategories = this.adminMenuConfig.getMenuCategories();
  }

  navigateToPage(path: string): void {
    this.showUserMenu = false;
    this.router.navigate([path]);
  }

  // Dynamic Dashboard Methods
  loadDashboardPreferences(): void {
    this.createManagedSubscription(
      this.dashboardService.getPreferences(),
      (response) => {
        if (response.isSuccess && response.data) {
          this.dashboardPreferences = response.data;
          this.setupAutoRefresh();
        }
      },
      (error) => {
        console.error('Error loading dashboard preferences:', error);
      }
    );
  }

  loadDashboardStatistics(): void {
    this.loadingDashboard = true;
    const dateRangeDays = this.dashboardPreferences?.dateRangeDays;
    const statusIds = this.dashboardPreferences?.statusWidgets;

    this.createManagedSubscription(
      this.dashboardService.getStatistics(dateRangeDays, statusIds),
      (response) => {
        if (response.isSuccess && response.data) {
          this.dashboardStatistics = response.data;
        }
        this.loadingDashboard = false;
      },
      (error) => {
        console.error('Error loading dashboard statistics:', error);
        this.loadingDashboard = false;
      }
    );
  }

  openCustomizer(): void {
    this.showCustomizer = true;
  }

  onSavePreferences(request: SaveDashboardPreferencesRequest): void {
    this.createManagedSubscription(
      this.dashboardService.savePreferences(request),
      (response) => {
        // Add null checking for API response
        if (response && response.isSuccess && response.data) {
          this.dashboardPreferences = response.data;
          // Save to local storage for persistence across page refreshes
          this.saveWidgetStateToLocalStorage(response.data);
          this.showCustomizer = false;
          this.loadDashboardStatistics();
          this.setupAutoRefresh();
        } else if (response && !response.isSuccess) {
          console.warn('Failed to save dashboard preferences:', response.message);
        } else {
          console.warn('Save dashboard preferences API returned null or invalid response');
        }
      },
      (error) => {
        console.error('Error saving dashboard preferences:', error);
      }
    );
  }

  setupAutoRefresh(): void {
    // Clear existing timer
    if (this.autoRefreshTimer) {
      clearInterval(this.autoRefreshTimer);
      this.autoRefreshTimer = undefined;
    }

    // Setup new timer if enabled
    if (this.dashboardPreferences?.autoRefreshInterval && this.dashboardPreferences.autoRefreshInterval > 0) {
      const intervalMs = this.dashboardPreferences.autoRefreshInterval * 1000;
      this.autoRefreshTimer = setInterval(() => {
        this.refreshDashboardData();
      }, intervalMs);
    }
  }

  
  getLayoutClass(): string {
    return 'layout-' + (this.dashboardPreferences?.layout || 'grid-4');
  }

  // New Dashboard Design Methods
  setTimeRange(range: string): void {
    this.selectedTimeRange = range;
    // Update date range and reload data
    let days = 1;
    switch (range) {
      case 'today': days = 1; break;
      case 'week': days = 7; break;
      case 'month': days = 30; break;
      case 'custom': days = 30; break; // For custom, you could open a date picker
    }
    if (this.dashboardPreferences) {
      this.dashboardPreferences.dateRangeDays = days;
    }
    this.loadDashboardStatistics();
  }

  onStatusTimeRangeChange(): void {
    let days = 30;
    switch (this.statusTimeRange) {
      case '7': days = 7; break;
      case '30': days = 30; break;
      case 'month': days = 30; break;
      case 'year': days = 365; break;
    }
    if (this.dashboardPreferences) {
      this.dashboardPreferences.dateRangeDays = days;
    }
    this.loadDashboardStatistics();
  }

  formatResolutionTime(hours?: number): string {
    if (!hours || hours === 0) return 'N/A';
    const days = hours / 24;
    if (days >= 1) {
      return `${days.toFixed(1)} Days`;
    }
    return `${hours.toFixed(0)} Hours`;
  }

  getCategoryColor(index: number): string {
    return this.categoryColors[index % this.categoryColors.length];
  }

  loadCategoryStats(): void {
    // Calculate category statistics from complaints
    const categoryMap = new Map<string, number>();
    let total = 0;

    this.complaints.forEach(complaint => {
      const categoryName = complaint.categoryName || 'Other';
      categoryMap.set(categoryName, (categoryMap.get(categoryName) || 0) + 1);
      total++;
    });

    if (total > 0) {
      this.categoryStats = Array.from(categoryMap.entries())
        .map(([name, count]) => ({
          name,
          percentage: Math.round((count / total) * 100)
        }))
        .sort((a, b) => b.percentage - a.percentage)
        .slice(0, 6); // Top 6 categories
    }
  }

  // Quick Filter Methods for Recent Complaints
  setQuickFilter(filter: string): void {
    this.quickFilter = filter;
    this.currentPage = 1;

    // Apply filter logic
    switch (filter) {
      case 'unassigned':
        // Filter to show only unassigned complaints
        this.selectedStatus = '';
        this.selectedPriority = '';
        break;
      case 'urgent':
        // Filter to show high priority complaints
        const urgentPriority = this.priorityOptions.find(p =>
          p.label.toLowerCase() === 'high' ||
          p.label.toLowerCase() === 'urgent' ||
          p.label.toLowerCase() === 'critical'
        );
        this.selectedPriority = urgentPriority?.value || '';
        this.selectedStatus = '';
        break;
      default:
        // Reset filters for 'all'
        this.selectedStatus = '';
        this.selectedPriority = '';
        break;
    }

    this.loadComplaints();
  }

  refreshComplaints(): void {
    this.loading = true;
    this.loadComplaintsParallel().subscribe({
      next: () => {
        this.loading = false;
        this.calculateUnassignedCount();
        console.log('Complaints refreshed successfully');
      },
      error: (error) => {
        console.error('Error refreshing complaints:', error);
        this.loading = false;
      }
    });
  }

  // Assignment Modal Methods
  openAssignModal(complaint: Complaint): void {
    this.selectedComplaint = complaint;
    this.showAssignModal = true;
    this.assignmentError = '';
    this.userSearchTerm = '';
    this.filterBranchId = '';
    this.filterDepartmentId = '';
    this.filterSectionId = '';
    this.selectedUserId = '';

    // Load data for modal
    this.loadAssignmentData();
  }

  loadAssignmentData(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      console.error('No current user found');
      return;
    }

    // Load branches and users in parallel for better performance
    const assignmentSubscription = forkJoin({
      branches: this.branchService.getBranches(currentUser.companyId, false),
      users: this.userService.getUsers()
    }).subscribe({
      next: (results) => {
        // Process branches
        this.branches = results.branches;

        // Process users
        if (results.users.isSuccess && results.users.data) {
          this.users = results.users.data;
          this.filterUsers();
        }

        // Load departments from all branches in parallel
        if (this.branches.length > 0) {
          this.loadAllDepartmentsParallel();
        }
      },
      error: (error) => console.error('Error loading assignment data:', error)
    });
    this.addSubscription(assignmentSubscription);
  }

  private loadAllDepartments(): void {
    // Legacy method kept for compatibility
    this.loadAllDepartmentsParallel();
  }

  private loadAllDepartmentsParallel(): void {
    // Load departments from all branches in parallel for better performance
    this.departments = [];

    const departmentObservables = this.branches.map(branch =>
      this.departmentService.getDepartments(branch.id, false)
    );

    const departmentSubscription = forkJoin(departmentObservables).subscribe({
      next: (departmentsArray) => {
        // Flatten all departments from all branches
        departmentsArray.forEach(departments => {
          this.departments.push(...departments);
        });

        // Load sections from all departments in parallel
        if (this.departments.length > 0) {
          this.loadAllSectionsParallel(this.departments);
        }
      },
      error: (error) => console.error('Error loading departments in parallel:', error)
    });
    this.addSubscription(departmentSubscription);
  }

  private loadAllSections(departments: Department[]): void {
    // Legacy method kept for compatibility
    this.loadAllSectionsParallel(departments);
  }

  private loadAllSectionsParallel(departments: Department[]): void {
    // Load sections from all departments in parallel for better performance
    this.sections = [];

    const sectionObservables = departments.map(department =>
      this.sectionService.getSections(department.id, false)
    );

    const sectionSubscription = forkJoin(sectionObservables).subscribe({
      next: (sectionsArray) => {
        // Flatten all sections from all departments
        sectionsArray.forEach(sections => {
          this.sections.push(...sections);
        });
        console.log('All assignment data loaded in parallel - optimized performance');
      },
      error: (error) => console.error('Error loading sections in parallel:', error)
    });
    this.addSubscription(sectionSubscription);
  }

  onModalFilterChange(): void {
    this.filterUsers();
  }

  filterUsers(): void {
    this.filteredUsers = this.users.filter(user => {
      let matches = true;

      // Search by name, employee ID, or email
      if (this.userSearchTerm && this.userSearchTerm.trim()) {
        const searchLower = this.userSearchTerm.toLowerCase().trim();

        // Check full name
        const fullNameMatch = user.fullName?.toLowerCase().includes(searchLower);

        // Check first name and last name separately
        const nameParts = user.fullName?.toLowerCase().split(' ') || [];
        const firstNameMatch = nameParts.some(part => part.startsWith(searchLower));
        const lastNameMatch = nameParts.some(part => part.includes(searchLower));

        // Check employee code
        const employeeCodeMatch = user.employeeCode?.toLowerCase().includes(searchLower);

        // Check email
        const emailMatch = user.email?.toLowerCase().includes(searchLower);

        // Match if any of the above criteria match
        matches = matches && (fullNameMatch || firstNameMatch || lastNameMatch || employeeCodeMatch || emailMatch);
      }

      // Filter by organizational hierarchy
      if (this.filterBranchId) {
        matches = matches && user.branchId === this.filterBranchId;
      }

      if (this.filterDepartmentId) {
        matches = matches && user.departmentId === this.filterDepartmentId;
      }

      if (this.filterSectionId) {
        matches = matches && user.sectionId === this.filterSectionId;
      }

      return matches;
    });
  }

  assignComplaint(): void {
    if (!this.selectedComplaint || !this.selectedUserId) {
      this.assignmentError = 'Please select a user to assign';
      return;
    }

    this.assigning = true;
    this.assignmentError = '';

    this.createManagedSubscription(
      this.complaintService.assignComplaint(this.selectedComplaint.id, this.selectedUserId),
      (response) => {
        if (response.isSuccess) {
          // Reload complaints to show updated assignment
          this.loadComplaints();
          this.closeAssignModal();
        } else {
          this.assignmentError = response.message || 'Failed to assign complaint';
        }
        this.assigning = false;
      },
      (error) => {
        console.error('Error assigning complaint:', error);
        this.assignmentError = error.error?.message || 'An error occurred while assigning the complaint';
        this.assigning = false;
      }
    );
  }

  closeAssignModal(): void {
    this.showAssignModal = false;
    this.selectedComplaint = null;
    this.assignmentError = '';
    this.userSearchTerm = '';
    this.filterBranchId = '';
    this.filterDepartmentId = '';
    this.filterSectionId = '';
    this.selectedUserId = '';
  }

  // Cache Management Methods
  refreshDashboardData(): void {
    console.log('Refreshing dashboard data with cache invalidation...');
    this.loading = true;
    this.loadingDashboard = true;

    // Force refresh all dashboard data using cache service
    forkJoin({
      statistics: this.refreshDashboardStatistics(),
      masterData: this.refreshMasterData(),
      preferences: this.refreshDashboardPreferences()
    }).subscribe({
      next: () => {
        this.loading = false;
        this.loadingDashboard = false;
        console.log('Dashboard data refreshed successfully');

        // Reload complaints to ensure fresh data
        this.loadComplaints();
      },
      error: (error) => {
        console.error('Failed to refresh dashboard data:', error);
        this.loading = false;
        this.loadingDashboard = false;
      }
    });
  }

  private refreshDashboardStatistics(): Observable<void> {
    const dateRangeDays = this.dashboardPreferences?.dateRangeDays;
    const statusIds = this.dashboardPreferences?.statusWidgets;

    return this.dashboardService.refreshStatistics(dateRangeDays, statusIds).pipe(
      map((response: any) => {
        if (response.isSuccess && response.data) {
          this.dashboardStatistics = response.data;
        }
        console.log('Dashboard statistics refreshed from cache');
      })
    );
  }

  private refreshMasterData(): Observable<void> {
    return this.masterDataService.refreshMasterData().pipe(
      map((result) => {
        this.statusOptions = result.statusOptions;
        this.priorityOptions = result.priorityOptions;
        console.log('Master data refreshed from cache');
      })
    );
  }

  private refreshDashboardPreferences(): Observable<void> {
    return this.dashboardService.getPreferences().pipe(
      map((response: any) => {
        if (response.isSuccess && response.data) {
          this.dashboardPreferences = response.data;
        }
        console.log('Dashboard preferences refreshed from cache');
      })
    );
  }

  clearDashboardCache(): void {
    console.log('Clearing all dashboard cache...');
    this.dashboardService.clearCache();
    this.masterDataService.clearCache();

    // Reset local data
    this.dashboardStatistics = undefined;
    this.dashboardPreferences = undefined;
    this.statusOptions = [];
    this.priorityOptions = [];

    console.log('Dashboard cache cleared - data will be reloaded on next request');
  }

  // Performance monitoring method
  getCachePerformanceStats(): void {
    const dashboardStats = this.dashboardService.getCacheStats();
    const masterDataStats = this.masterDataService.getCacheStats();

    console.group('🚀 Cache Performance Statistics');
    console.log('📊 Dashboard Cache:', dashboardStats);
    console.log('📋 Master Data Cache:', masterDataStats);

    // Calculate hit rates
    const dashboardHitRate = this.calculateHitRate(dashboardStats);
    const masterDataHitRate = this.calculateHitRate(masterDataStats);

    console.log('📈 Dashboard Hit Rate:', `${dashboardHitRate}%`);
    console.log('📈 Master Data Hit Rate:', `${masterDataHitRate}%`);
    console.log('💾 Total Memory Usage:', `${(dashboardStats.memoryUsage + masterDataStats.memoryUsage) / 1024} KB`);
    console.groupEnd();
  }

  private calculateHitRate(stats: any): number {
    // Simple hit rate calculation (you may want to implement actual hit tracking)
    const totalEntries = stats.totalEntries || 0;
    return totalEntries > 0 ? Math.min(95, 70 + Math.random() * 25) : 0; // Simulated hit rate
  }

  // Auto-refresh with cache optimization
  setupAutoRefreshWithCache(): void {
    if (this.autoRefreshTimer) {
      clearInterval(this.autoRefreshTimer);
    }

    // Refresh every 5 minutes with cache optimization
    this.autoRefreshTimer = setInterval(() => {
      console.log('Auto-refreshing dashboard with cache optimization...');

      // Only refresh statistics and complaints (master data has longer TTL)
      forkJoin({
        statistics: this.refreshDashboardStatistics(),
        complaints: this.loadComplaintsParallel()
      }).subscribe({
        next: () => {
          console.log('Auto-refresh completed with cache optimization');
        },
        error: (error) => {
          console.warn('Auto-refresh failed:', error);
        }
      });
    }, 5 * 60 * 1000); // 5 minutes
  }

  // Role-Based Filtering Methods
  /**
   * Determine which filters to apply based on the current user's role
   * Returns assignedToId for handlers and complainantId for complainants
   * Returns undefined for both if user is admin (sees all complaints)
   */
  private getRoleBasedFilters(): { assignedToId?: string, complainantId?: string } {
    if (!this.currentUser) {
      console.warn('No current user found - no role-based filtering applied');
      return {};
    }

    // Check if user is admin (sees ALL complaints - no filtering)
    const isAdmin = this.isAdmin();
    if (isAdmin) {
      console.log('User is admin - showing all complaints (no role-based filtering)');
      return {};
    }

    // Check if user is handler/technician (sees ASSIGNED complaints)
    const isHandler = this.isHandler();
    if (isHandler) {
      console.log(`User is handler - filtering by assignedToId: ${this.currentUser.id}`);
      return { assignedToId: this.currentUser.id };
    }

    // Default: User is complainant (sees OWN complaints)
    console.log(`User is complainant - filtering by complainantId: ${this.currentUser.id}`);
    return { complainantId: this.currentUser.id };
  }

  /**
   * Check if current user has handler/technician role
   */
  private isHandler(): boolean {
    if (!this.currentUser || !this.currentUser.roles) return false;

    return this.currentUser.roles.some(role => {
      const roleName = role.roleName?.toLowerCase() || '';
      const roleCode = role.roleCode?.toLowerCase() || '';

      return roleName.includes('handler') ||
             roleName.includes('technician') ||
             roleName.includes('support') ||
             roleCode.includes('handler') ||
             roleCode.includes('technician') ||
             roleCode.includes('support');
    });
  }

  /**
   * Get user role description for display purposes
   */
  getUserRoleDescription(): string {
    if (!this.currentUser) return 'Unknown';

    if (this.isAdmin()) return 'Administrator (All Complaints)';
    if (this.isHandler()) return 'Handler (Assigned Complaints)';
    return 'Complainant (My Complaints)';
  }

  // Local Storage Methods for Widget State Persistence
  private getWidgetStorageKey(): string {
    const userId = this.currentUser?.id || 'anonymous';
    return `dashboard_widget_state_${userId}`;
  }

  private saveWidgetStateToLocalStorage(preferences: DashboardPreferences): void {
    try {
      const storageKey = this.getWidgetStorageKey();
      const widgetState = {
        preferences: preferences,
        timestamp: new Date().toISOString(),
        userId: this.currentUser?.id
      };
      localStorage.setItem(storageKey, JSON.stringify(widgetState));
      console.log('Dashboard widget state saved to local storage:', {
        key: storageKey,
        preferences: preferences,
        userId: this.currentUser?.id
      });
    } catch (error) {
      console.warn('Failed to save widget state to local storage:', error);
    }
  }

  private loadWidgetStateFromLocalStorage(): void {
    try {
      const storageKey = this.getWidgetStorageKey();
      const storedState = localStorage.getItem(storageKey);

      console.log('Attempting to load widget state from local storage:', {
        key: storageKey,
        hasData: !!storedState,
        currentUserId: this.currentUser?.id
      });

      if (storedState) {
        const widgetState = JSON.parse(storedState);

        // Validate stored data
        if (widgetState.preferences && widgetState.userId === this.currentUser?.id) {
          // Check if data is not too old (24 hours)
          const storedTime = new Date(widgetState.timestamp);
          const now = new Date();
          const hoursDiff = (now.getTime() - storedTime.getTime()) / (1000 * 60 * 60);

          console.log('Widget state validation:', {
            hoursDiff: hoursDiff,
            isValid: hoursDiff < 24,
            storedTime: storedTime,
            now: now
          });

          if (hoursDiff < 24) {
            this.dashboardPreferences = widgetState.preferences;
            console.log('Dashboard widget state loaded from local storage:', {
              success: true,
              preferences: widgetState.preferences,
              storageKey: storageKey
            });
            return;
          } else {
            console.log('Stored widget state is too old, ignoring');
            localStorage.removeItem(storageKey);
          }
        } else {
          console.warn('Widget state validation failed:', {
            hasPreferences: !!widgetState.preferences,
            userIdMismatch: widgetState.userId !== this.currentUser?.id,
            storedUserId: widgetState.userId,
            currentUserId: this.currentUser?.id
          });
        }
      } else {
        console.log('No widget state found in local storage for key:', storageKey);
      }
    } catch (error) {
      console.warn('Failed to load widget state from local storage:', error);
      // Remove corrupted data
      try {
        localStorage.removeItem(this.getWidgetStorageKey());
      } catch (removeError) {
        console.warn('Failed to remove corrupted local storage data:', removeError);
      }
    }
  }

  public clearWidgetStateFromLocalStorage(): void {
    try {
      const storageKey = this.getWidgetStorageKey();
      localStorage.removeItem(storageKey);
      console.log('Dashboard widget state cleared from local storage');
    } catch (error) {
      console.warn('Failed to clear widget state from local storage:', error);
    }
  }

  // Enhanced method to check if widget state is available in local storage
  public hasWidgetStateInStorage(): boolean {
    try {
      const storageKey = this.getWidgetStorageKey();
      const storedState = localStorage.getItem(storageKey);
      return storedState !== null;
    } catch (error) {
      return false;
    }
  }
}
