import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ComplaintService } from '../../services/complaint.service';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { StatusMasterService, ComplaintStatusMaster } from '../../services/status-master.service';
import { User } from '../../models/user.model';

export interface StatusCount {
  statusId: string;
  statusName: string;
  statusCode: string;
  colorCode: string;
  iconClass: string;
  count: number;
  isFinal: boolean;
  displayOrder: number;
}

export interface TeamMemberStats {
  user: User;
  totalAssigned: number;
  statusCounts: StatusCount[];
  overdueComplaints: number;
  waitingForResponse: number;
  avgResolutionTime?: number;
  slaComplianceRate: number;
  lastActivityDate?: Date;
}

export interface TeamSummary {
  totalTeamMembers: number;
  totalAssignedComplaints: number;
  statusTotals: StatusCount[];
  avgSlaCompliance: number;
  topPerformer?: TeamMemberStats;
  mostWorkload?: TeamMemberStats;
}

@Component({
  selector: 'app-team-performance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './team-performance.component.html',
  styleUrls: ['./team-performance.component.scss']
})
export class TeamPerformanceComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  teamMembers: User[] = [];
  teamMemberStats: TeamMemberStats[] = [];
  teamSummary: TeamSummary | null = null;
  currentUser: User | null = null;
  statusMasters: ComplaintStatusMaster[] = [];

  // Loading states
  loading = true;
  loadingStats = false;

  // Filters
  searchTerm = '';
  sortBy: 'name' | 'assigned' | 'open' | 'sla' | 'overdue' = 'assigned';
  sortDirection: 'asc' | 'desc' = 'desc';
  departmentFilter = '';
  departments: string[] = [];

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;

  constructor(
    private router: Router,
    private complaintService: ComplaintService,
    private userService: UserService,
    private authService: AuthService,
    private statusMasterService: StatusMasterService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.currentUserValue;

    // Check if user is admin
    if (!this.isAdmin()) {
      this.router.navigate(['/dashboard']);
      return;
    }

    this.loadInitialData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  isAdmin(): boolean {
    if (!this.currentUser || !this.currentUser.roles) return false;
    return this.currentUser.roles.some(role =>
      role.roleCode === 'SYSTEM_ADMIN' ||
      role.roleCode === 'ADMIN' ||
      role.roleName?.toLowerCase().includes('admin')
    );
  }

  loadInitialData(): void {
    this.loading = true;

    // Load status masters and team members in parallel
    forkJoin({
      statuses: this.statusMasterService.getStatuses(undefined, true),
      users: this.userService.getUsers()
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: ({ statuses, users }) => {
        // Process status masters
        if (statuses.isSuccess && statuses.data) {
          this.statusMasters = statuses.data
            .filter(s => s.isActive)
            .sort((a, b) => a.displayOrder - b.displayOrder);
        }

        // Process users
        if (users.isSuccess && users.data) {
          this.teamMembers = users.data.filter(user => {
            if (user.id === this.currentUser?.id) return false;
            return user.isActive !== false;
          });

          this.departments = [...new Set(
            this.teamMembers
              .map(u => u.departmentName)
              .filter((d): d is string => !!d)
          )].sort();

          this.loadTeamMemberStats();
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading initial data:', error);
        this.loading = false;
      }
    });
  }

  loadTeamMemberStats(): void {
    this.loadingStats = true;
    this.teamMemberStats = [];

    const filteredMembers = this.getFilteredTeamMembers();
    const startIdx = (this.currentPage - 1) * this.pageSize;
    const endIdx = startIdx + this.pageSize;
    const pageMembers = filteredMembers.slice(startIdx, endIdx);

    if (pageMembers.length === 0) {
      this.loadingStats = false;
      this.calculateTeamSummary();
      return;
    }

    const statsRequests = pageMembers.map(member =>
      this.complaintService.getComplaints(1, 1000, undefined, undefined, undefined, member.id)
    );

    forkJoin(statsRequests).pipe(takeUntil(this.destroy$)).subscribe({
      next: (responses) => {
        this.teamMemberStats = pageMembers.map((member, index) => {
          const response = responses[index];
          const complaints = response.isSuccess && response.data ? response.data.items : [];

          // Build dynamic status counts
          const statusCounts: StatusCount[] = this.statusMasters.map(status => ({
            statusId: status.id,
            statusName: status.name,
            statusCode: status.code,
            colorCode: status.colorCode || '#6b7280',
            iconClass: status.iconClass || 'fa-circle',
            isFinal: status.isFinal,
            displayOrder: status.displayOrder,
            count: complaints.filter(c =>
              c.status?.toLowerCase() === status.name.toLowerCase() ||
              c.status?.toLowerCase() === status.code.toLowerCase()
            ).length
          }));

          // Calculate open complaints (non-final statuses)
          const openComplaints = statusCounts
            .filter(sc => !sc.isFinal)
            .reduce((sum, sc) => sum + sc.count, 0);

          // Calculate overdue
          const finalStatuses = this.statusMasters.filter(s => s.isFinal).map(s => s.name.toLowerCase());
          const overdueComplaints = complaints.filter(c => {
            if (finalStatuses.includes(c.status?.toLowerCase())) return false;
            const createdDate = new Date(c.submittedAt);
            const daysDiff = (Date.now() - createdDate.getTime()) / (1000 * 60 * 60 * 24);
            return daysDiff > 7;
          }).length;

          // Waiting for response
          const waitingForResponse = complaints.filter(c =>
            c.hasCustomerResponse && !finalStatuses.includes(c.status?.toLowerCase())
          ).length;

          // SLA compliance
          const slaComplianceRate = openComplaints > 0
            ? Math.round(((openComplaints - overdueComplaints) / openComplaints) * 100)
            : 100;

          // Last activity date
          const sortedByActivity = [...complaints].sort((a, b) =>
            new Date(b.lastResponseAt || b.submittedAt).getTime() - new Date(a.lastResponseAt || a.submittedAt).getTime()
          );
          const lastActivityDate = sortedByActivity[0]?.lastResponseAt || sortedByActivity[0]?.submittedAt;

          return {
            user: member,
            totalAssigned: complaints.length,
            statusCounts,
            overdueComplaints,
            waitingForResponse,
            slaComplianceRate,
            lastActivityDate: lastActivityDate ? new Date(lastActivityDate) : undefined
          };
        });

        this.sortTeamMemberStats();
        this.calculateTeamSummary();
        this.loadingStats = false;
      },
      error: (error) => {
        console.error('Error loading team member stats:', error);
        this.loadingStats = false;
      }
    });
  }

  getFilteredTeamMembers(): User[] {
    let filtered = [...this.teamMembers];

    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(m =>
        m.fullName?.toLowerCase().includes(search) ||
        m.email?.toLowerCase().includes(search) ||
        m.employeeCode?.toLowerCase().includes(search)
      );
    }

    if (this.departmentFilter) {
      filtered = filtered.filter(m => m.departmentName === this.departmentFilter);
    }

    return filtered;
  }

  sortTeamMemberStats(): void {
    this.teamMemberStats.sort((a, b) => {
      let comparison = 0;

      switch (this.sortBy) {
        case 'name':
          comparison = (a.user.fullName || '').localeCompare(b.user.fullName || '');
          break;
        case 'assigned':
          comparison = a.totalAssigned - b.totalAssigned;
          break;
        case 'open':
          const aOpen = a.statusCounts.filter(sc => !sc.isFinal).reduce((sum, sc) => sum + sc.count, 0);
          const bOpen = b.statusCounts.filter(sc => !sc.isFinal).reduce((sum, sc) => sum + sc.count, 0);
          comparison = aOpen - bOpen;
          break;
        case 'sla':
          comparison = a.slaComplianceRate - b.slaComplianceRate;
          break;
        case 'overdue':
          comparison = a.overdueComplaints - b.overdueComplaints;
          break;
      }

      return this.sortDirection === 'asc' ? comparison : -comparison;
    });
  }

  calculateTeamSummary(): void {
    if (this.teamMemberStats.length === 0) {
      this.teamSummary = null;
      return;
    }

    const totalAssignedComplaints = this.teamMemberStats.reduce((sum, m) => sum + m.totalAssigned, 0);

    // Aggregate status totals
    const statusTotals: StatusCount[] = this.statusMasters.map(status => {
      const total = this.teamMemberStats.reduce((sum, m) => {
        const sc = m.statusCounts.find(s => s.statusId === status.id);
        return sum + (sc?.count || 0);
      }, 0);

      return {
        statusId: status.id,
        statusName: status.name,
        statusCode: status.code,
        colorCode: status.colorCode || '#6b7280',
        iconClass: status.iconClass || 'fa-circle',
        isFinal: status.isFinal,
        displayOrder: status.displayOrder,
        count: total
      };
    });

    const avgSlaCompliance = Math.round(
      this.teamMemberStats.reduce((sum, m) => sum + m.slaComplianceRate, 0) / this.teamMemberStats.length
    );

    // Find top performer
    const eligiblePerformers = this.teamMemberStats.filter(m => m.totalAssigned > 0);
    const topPerformer = eligiblePerformers.length > 0
      ? eligiblePerformers.reduce((best, current) =>
          current.slaComplianceRate > best.slaComplianceRate ? current : best
        )
      : undefined;

    // Find most workload (open complaints)
    const mostWorkload = eligiblePerformers.length > 0
      ? eligiblePerformers.reduce((most, current) => {
          const currentOpen = current.statusCounts.filter(sc => !sc.isFinal).reduce((sum, sc) => sum + sc.count, 0);
          const mostOpen = most.statusCounts.filter(sc => !sc.isFinal).reduce((sum, sc) => sum + sc.count, 0);
          return currentOpen > mostOpen ? current : most;
        })
      : undefined;

    this.teamSummary = {
      totalTeamMembers: this.teamMemberStats.length,
      totalAssignedComplaints,
      statusTotals,
      avgSlaCompliance,
      topPerformer,
      mostWorkload
    };

    const filteredMembers = this.getFilteredTeamMembers();
    this.totalPages = Math.ceil(filteredMembers.length / this.pageSize);
  }

  getOpenCount(stat: TeamMemberStats): number {
    return stat.statusCounts.filter(sc => !sc.isFinal).reduce((sum, sc) => sum + sc.count, 0);
  }

  getClosedCount(stat: TeamMemberStats): number {
    return stat.statusCounts.filter(sc => sc.isFinal).reduce((sum, sc) => sum + sc.count, 0);
  }

  // Get visible statuses for card display (show all statuses with counts or non-final)
  getVisibleStatuses(statusCounts: StatusCount[]): StatusCount[] {
    // Show all statuses that have counts or are non-final, sorted by display order
    return statusCounts
      .filter(sc => sc.count > 0 || !sc.isFinal) // Always show non-final, or any with counts
      .sort((a, b) => a.displayOrder - b.displayOrder);
    // No slice limit - show all statuses dynamically
  }

  onSortChange(sortBy: 'name' | 'assigned' | 'open' | 'sla' | 'overdue'): void {
    if (this.sortBy === sortBy) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = sortBy;
      this.sortDirection = 'desc';
    }
    this.sortTeamMemberStats();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadTeamMemberStats();
  }

  onDepartmentChange(): void {
    this.currentPage = 1;
    this.loadTeamMemberStats();
  }

  onPageChange(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadTeamMemberStats();
  }

  viewMemberDashboard(member: User): void {
    this.router.navigate(['/dashboard'], {
      queryParams: { teamMemberId: member.id, teamMemberName: member.fullName }
    });
  }

  viewMemberComplaints(member: User): void {
    this.router.navigate(['/complaints'], {
      queryParams: { assignedToId: member.id }
    });
  }

  getSlaStatusClass(slaRate: number): string {
    if (slaRate >= 90) return 'sla-excellent';
    if (slaRate >= 70) return 'sla-good';
    if (slaRate >= 50) return 'sla-warning';
    return 'sla-critical';
  }

  getWorkloadClass(openComplaints: number): string {
    if (openComplaints === 0) return 'workload-none';
    if (openComplaints <= 5) return 'workload-light';
    if (openComplaints <= 15) return 'workload-moderate';
    if (openComplaints <= 30) return 'workload-heavy';
    return 'workload-overloaded';
  }

  formatLastActivity(date?: Date): string {
    if (!date) return 'No activity';

    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const minutes = Math.floor(diff / (1000 * 60));

    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;
    return date.toLocaleDateString();
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  refreshData(): void {
    this.loadInitialData();
  }

  exportReport(): void {
    console.log('Export feature coming soon');
  }
}
