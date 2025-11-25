import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import {
  OryggiSyncService,
  SyncHistoryItem,
  SyncSchedule,
  SyncScheduleRequest,
  SqlHealthCheck,
  SqlDiagnostics,
  BlockedProcess,
  LongRunningQuery,
  OpenTransaction
} from '../../../services/oryggi-sync.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-oryggi-sync',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './oryggi-sync.component.html',
  styleUrls: ['./oryggi-sync.component.css']
})
export class OryggiSyncComponent implements OnInit, OnDestroy {
  loading = false;
  syncing = false;
  syncHistory: SyncHistoryItem[] = [];
  latestSync: SyncHistoryItem | null = null;
  error: string | null = null;
  successMessage: string | null = null;
  tenantId: string = '18910DFB-1F39-46F8-979C-D8B845A5388D'; // Default tenant

  // Progress tracking
  syncProgress = 0;
  syncProgressMessage = '';
  showProgress = false;
  private progressCheckInterval: any = null;

  // Detailed progress tracking
  employeesSynced = 0;
  totalEmployees = 0;
  currentBatch = 0;
  totalBatches = 0;

  // Schedule management
  schedules: SyncSchedule[] = [];
  showScheduleForm = false;
  editingSchedule: SyncSchedule | null = null;
  scheduleForm: SyncScheduleRequest = {
    scheduleType: 'Daily',
    timeOfDay: '02:00',
    isEnabled: true
  };

  // SQL Diagnostics
  showDiagnostics = false;
  diagnosticsLoading = false;
  healthCheck: SqlHealthCheck | null = null;
  diagnostics: SqlDiagnostics | null = null;
  diagnosticsError: string | null = null;
  autoRefreshEnabled = false;
  private diagnosticsRefreshInterval: any = null;
  killSessionReason = '';
  showKillSessionModal = false;
  sessionToKill: number | null = null;

  constructor(
    private syncService: OryggiSyncService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadSyncStatus();
    this.loadSyncHistory();
    this.loadSchedules();
  }

  ngOnDestroy(): void {
    // Clean up progress check interval
    if (this.progressCheckInterval) {
      clearInterval(this.progressCheckInterval);
    }
    // Clean up diagnostics refresh interval
    if (this.diagnosticsRefreshInterval) {
      clearInterval(this.diagnosticsRefreshInterval);
    }
  }

  loadSyncStatus(): void {
    this.loading = true;
    this.error = null;

    this.syncService.getSyncStatus(this.tenantId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.latestSync = response.data;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load sync status';
        console.error('Error loading sync status:', err);
        this.loading = false;
      }
    });
  }

  loadSyncHistory(count: number = 10): void {
    this.loading = true;
    this.error = null;

    this.syncService.getSyncHistory(this.tenantId, count).subscribe({
      next: (response) => {
        if (response.success) {
          this.syncHistory = response.data;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load sync history';
        console.error('Error loading sync history:', err);
        this.loading = false;
      }
    });
  }

  triggerSync(): void {
    if (this.syncing) return;

    this.syncing = true;
    this.showProgress = true;
    this.error = null;
    this.successMessage = null;
    this.syncProgress = 0;
    this.syncProgressMessage = 'Initializing sync...';

    // Simulate progress (5 stages: Companies, Branches, Departments, Sections, Employees)
    const stages = [
      { progress: 15, message: 'Syncing companies...' },
      { progress: 30, message: 'Syncing branches...' },
      { progress: 50, message: 'Syncing departments...' },
      { progress: 70, message: 'Syncing sections...' },
      { progress: 85, message: 'Syncing employees...' }
    ];

    let stageIndex = 0;
    this.progressCheckInterval = setInterval(() => {
      if (stageIndex < stages.length && this.syncing) {
        this.syncProgress = stages[stageIndex].progress;
        this.syncProgressMessage = stages[stageIndex].message;
        stageIndex++;
      }
    }, 1000); // Update every second

    // Don't pass tenant ID - backend will use default tenant
    // Add 10 minute timeout for large sync operations
    this.syncService.triggerSync().pipe(
      timeout(600000), // 10 minutes = 600000ms
      catchError((err) => {
        if (err.name === 'TimeoutError') {
          console.error('Sync operation timed out after 10 minutes');
          return throwError(() => new Error('Sync operation timed out. The sync may still be running in the background.'));
        }
        return throwError(() => err);
      })
    ).subscribe({
      next: (response) => {
        // Clear progress interval
        if (this.progressCheckInterval) {
          clearInterval(this.progressCheckInterval);
          this.progressCheckInterval = null;
        }

        this.syncing = false;
        this.syncProgress = 100;
        this.syncProgressMessage = 'Sync completed!';

        setTimeout(() => {
          this.showProgress = false;
        }, 2000);

        if (response.success && response.data) {
          const duration = response.data.duration ? response.data.duration.toFixed(2) : '0.00';
          this.successMessage = `✓ Sync completed successfully in ${duration} seconds.
            • Companies: ${response.data.companies.created} created, ${response.data.companies.updated} updated
            • Branches: ${response.data.branches.created} created, ${response.data.branches.updated} updated
            • Departments: ${response.data.departments.created} created, ${response.data.departments.updated} updated
            • Sections: ${response.data.sections.created} created, ${response.data.sections.updated} updated
            • Employees: ${response.data.employees.created} created, ${response.data.employees.updated} updated`;

          // Reload sync history and status
          this.loadSyncStatus();
          this.loadSyncHistory();
        } else {
          this.error = response.message || 'Sync failed';
          if (response.error) {
            this.error += ': ' + response.error;
          }
        }
      },
      error: (err) => {
        // Clear progress interval
        if (this.progressCheckInterval) {
          clearInterval(this.progressCheckInterval);
          this.progressCheckInterval = null;
        }

        this.syncing = false;
        this.showProgress = false;
        this.error = 'Failed to trigger sync: ' + (err.error?.message || err.message);
        console.error('Error triggering sync:', err);
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status?.toUpperCase()) {
      case 'SUCCESS':
        return 'status-success';
      case 'FAILED':
        return 'status-failed';
      case 'IN_PROGRESS':
        return 'status-in-progress';
      default:
        return 'status-unknown';
    }
  }

  formatDuration(seconds: number | null | undefined): string {
    if (seconds === null || seconds === undefined) {
      return 'N/A';
    }
    if (seconds < 60) {
      return `${seconds.toFixed(1)}s`;
    }
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}m ${remainingSeconds.toFixed(0)}s`;
  }

  formatDate(date: string | null | undefined): string {
    if (!date) return 'N/A';

    try {
      // Parse the UTC date string
      // If the date doesn't end with 'Z', it's assumed to be UTC from our API
      const dateStr = date.endsWith('Z') ? date : date + 'Z';
      const utcDate = new Date(dateStr);

      // Check if valid date
      if (isNaN(utcDate.getTime())) {
        return 'Invalid Date';
      }

      // Convert to local timezone and format
      return utcDate.toLocaleString();
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  dismissMessage(): void {
    this.error = null;
    this.successMessage = null;
  }

  // Schedule management methods
  loadSchedules(): void {
    this.syncService.getSchedules().subscribe({
      next: (response) => {
        if (response.success) {
          this.schedules = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading schedules:', err);
      }
    });
  }

  openScheduleForm(): void {
    this.showScheduleForm = true;
    this.editingSchedule = null;
    this.scheduleForm = {
      scheduleType: 'Daily',
      timeOfDay: '02:00',
      isEnabled: true
    };
    this.error = null;
    this.successMessage = null;
  }

  editSchedule(schedule: SyncSchedule): void {
    this.showScheduleForm = true;
    this.editingSchedule = schedule;
    this.scheduleForm = {
      scheduleType: schedule.scheduleType,
      timeOfDay: schedule.timeOfDay,
      dayValue: schedule.dayValue,
      isEnabled: schedule.isEnabled,
      description: schedule.description
    };
    this.error = null;
    this.successMessage = null;
  }

  cancelScheduleForm(): void {
    this.showScheduleForm = false;
    this.editingSchedule = null;
    this.scheduleForm = {
      scheduleType: 'Daily',
      timeOfDay: '02:00',
      isEnabled: true
    };
  }

  saveSchedule(): void {
    this.error = null;
    this.successMessage = null;

    // Validate form
    if (!this.scheduleForm.timeOfDay) {
      this.error = 'Time of day is required';
      return;
    }

    if (this.scheduleForm.scheduleType === 'Weekly' && (this.scheduleForm.dayValue === undefined || this.scheduleForm.dayValue < 0 || this.scheduleForm.dayValue > 6)) {
      this.error = 'For Weekly schedules, please select a day of week (Sunday-Saturday)';
      return;
    }

    if (this.scheduleForm.scheduleType === 'Monthly' && (this.scheduleForm.dayValue === undefined || this.scheduleForm.dayValue < 1 || this.scheduleForm.dayValue > 31)) {
      this.error = 'For Monthly schedules, please select a day (1-31)';
      return;
    }

    if (this.editingSchedule) {
      // Update existing schedule
      this.syncService.updateSchedule(this.editingSchedule.id, this.scheduleForm).subscribe({
        next: (response) => {
          if (response.success) {
            this.successMessage = 'Schedule updated successfully';
            this.showScheduleForm = false;
            this.loadSchedules();
          } else {
            this.error = response.message || 'Failed to update schedule';
          }
        },
        error: (err) => {
          this.error = 'Error updating schedule: ' + (err.error?.message || err.message);
          console.error('Error updating schedule:', err);
        }
      });
    } else {
      // Create new schedule
      this.syncService.createSchedule(this.scheduleForm).subscribe({
        next: (response) => {
          if (response.success) {
            this.successMessage = 'Schedule created successfully';
            this.showScheduleForm = false;
            this.loadSchedules();
          } else {
            this.error = response.message || 'Failed to create schedule';
          }
        },
        error: (err) => {
          this.error = 'Error creating schedule: ' + (err.error?.message || err.message);
          console.error('Error creating schedule:', err);
        }
      });
    }
  }

  deleteSchedule(schedule: SyncSchedule): void {
    if (!confirm(`Are you sure you want to delete this ${schedule.scheduleType} schedule?`)) {
      return;
    }

    this.syncService.deleteSchedule(schedule.id).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Schedule deleted successfully';
          this.loadSchedules();
        } else {
          this.error = response.message || 'Failed to delete schedule';
        }
      },
      error: (err) => {
        this.error = 'Error deleting schedule: ' + (err.error?.message || err.message);
        console.error('Error deleting schedule:', err);
      }
    });
  }

  toggleSchedule(schedule: SyncSchedule): void {
    const updatedSchedule: SyncScheduleRequest = {
      scheduleType: schedule.scheduleType,
      timeOfDay: schedule.timeOfDay,
      dayValue: schedule.dayValue,
      isEnabled: !schedule.isEnabled,
      description: schedule.description
    };

    this.syncService.updateSchedule(schedule.id, updatedSchedule).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = `Schedule ${!schedule.isEnabled ? 'enabled' : 'disabled'} successfully`;
          this.loadSchedules();
        } else {
          this.error = response.message || 'Failed to toggle schedule';
        }
      },
      error: (err) => {
        this.error = 'Error toggling schedule: ' + (err.error?.message || err.message);
        console.error('Error toggling schedule:', err);
      }
    });
  }

  getScheduleDescription(schedule: SyncSchedule): string {
    let desc = `${schedule.scheduleType} at ${schedule.timeOfDay}`;

    if (schedule.scheduleType === 'Weekly' && schedule.dayValue !== undefined) {
      const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
      desc += ` on ${days[schedule.dayValue]}`;
    } else if (schedule.scheduleType === 'Monthly' && schedule.dayValue !== undefined) {
      desc += ` on day ${schedule.dayValue}`;
    }

    return desc;
  }

  getDayOfWeekName(day: number): string {
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    return days[day] || '';
  }

  // SQL Diagnostics methods
  toggleDiagnostics(): void {
    this.showDiagnostics = !this.showDiagnostics;

    if (this.showDiagnostics) {
      this.loadDiagnostics();
      this.loadHealthCheck();
    } else {
      // Stop auto-refresh when closing diagnostics
      this.stopAutoRefresh();
    }
  }

  loadHealthCheck(): void {
    this.syncService.getHealthCheck().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.healthCheck = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading health check:', err);
      }
    });
  }

  loadDiagnostics(): void {
    this.diagnosticsLoading = true;
    this.diagnosticsError = null;

    this.syncService.getDiagnostics().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.diagnostics = response.data;
          this.healthCheck = response.data.healthCheck;
        }
        this.diagnosticsLoading = false;
      },
      error: (err) => {
        this.diagnosticsError = 'Failed to load diagnostics: ' + (err.error?.message || err.message);
        console.error('Error loading diagnostics:', err);
        this.diagnosticsLoading = false;
      }
    });
  }

  refreshDiagnostics(): void {
    this.loadDiagnostics();
  }

  toggleAutoRefresh(): void {
    this.autoRefreshEnabled = !this.autoRefreshEnabled;

    if (this.autoRefreshEnabled) {
      // Refresh every 30 seconds
      this.diagnosticsRefreshInterval = setInterval(() => {
        this.loadDiagnostics();
      }, 30000);
      this.successMessage = 'Auto-refresh enabled (30 seconds)';
    } else {
      this.stopAutoRefresh();
      this.successMessage = 'Auto-refresh disabled';
    }

    setTimeout(() => {
      this.successMessage = null;
    }, 3000);
  }

  private stopAutoRefresh(): void {
    if (this.diagnosticsRefreshInterval) {
      clearInterval(this.diagnosticsRefreshInterval);
      this.diagnosticsRefreshInterval = null;
    }
    this.autoRefreshEnabled = false;
  }

  getHealthStatusClass(status: string): string {
    switch (status) {
      case 'Healthy':
        return 'health-status-healthy';
      case 'Warning':
        return 'health-status-warning';
      case 'Critical':
        return 'health-status-critical';
      default:
        return 'health-status-error';
    }
  }

  openKillSessionModal(sessionId: number): void {
    this.sessionToKill = sessionId;
    this.killSessionReason = '';
    this.showKillSessionModal = true;
  }

  closeKillSessionModal(): void {
    this.showKillSessionModal = false;
    this.sessionToKill = null;
    this.killSessionReason = '';
  }

  confirmKillSession(): void {
    if (!this.sessionToKill) return;

    if (!this.killSessionReason.trim()) {
      this.diagnosticsError = 'Please provide a reason for killing this session';
      return;
    }

    this.diagnosticsLoading = true;
    this.diagnosticsError = null;

    this.syncService.killSession({
      sessionId: this.sessionToKill,
      reason: this.killSessionReason
    }).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = response.message || 'Session killed successfully';
          this.closeKillSessionModal();
          // Reload diagnostics to reflect changes
          setTimeout(() => {
            this.loadDiagnostics();
          }, 1000);
        } else {
          this.diagnosticsError = response.message || 'Failed to kill session';
        }
        this.diagnosticsLoading = false;
      },
      error: (err) => {
        this.diagnosticsError = 'Error killing session: ' + (err.error?.message || err.message);
        console.error('Error killing session:', err);
        this.diagnosticsLoading = false;
      }
    });
  }

  cleanupStuckSyncs(): void {
    if (!confirm('Are you sure you want to mark all stuck syncs as failed? This action cannot be undone.')) {
      return;
    }

    this.diagnosticsLoading = true;
    this.diagnosticsError = null;

    this.syncService.cleanupStuckSyncs().subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = response.message || 'Stuck syncs cleaned up successfully';
          // Reload diagnostics and sync history
          this.loadDiagnostics();
          this.loadSyncHistory();
        } else {
          this.diagnosticsError = 'Failed to cleanup stuck syncs';
        }
        this.diagnosticsLoading = false;
      },
      error: (err) => {
        this.diagnosticsError = 'Error cleaning up stuck syncs: ' + (err.error?.message || err.message);
        console.error('Error cleaning up stuck syncs:', err);
        this.diagnosticsLoading = false;
      }
    });
  }

  cleanupSingleSync(syncLogId: string): void {
    if (!confirm('Are you sure you want to mark this sync as failed? This action cannot be undone.')) {
      return;
    }

    this.loading = true;
    this.error = null;

    this.syncService.cleanupSingleSync(syncLogId).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = response.message || 'Sync cleaned up successfully';
          // Reload sync history and status
          this.loadSyncHistory();
          this.loadSyncStatus();
          // Reload diagnostics if panel is open
          if (this.showDiagnostics) {
            this.loadDiagnostics();
          } else {
            // Just reload health check to update stuck sync count
            this.loadHealthCheck();
          }
        } else {
          this.error = 'Failed to cleanup sync';
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error cleaning up sync: ' + (err.error?.message || err.message);
        console.error('Error cleaning up sync:', err);
        this.loading = false;
      }
    });
  }

  formatQueryText(query: string): string {
    return query.length > 100 ? query.substring(0, 100) + '...' : query;
  }

  formatTime(seconds: number): string {
    if (seconds < 60) {
      return `${seconds}s`;
    }
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}m ${remainingSeconds}s`;
  }
}
