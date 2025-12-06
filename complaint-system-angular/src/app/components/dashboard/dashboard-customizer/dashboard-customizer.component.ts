import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { trigger, transition, style, animate } from '@angular/animations';
import { DashboardPreferences, SaveDashboardPreferencesRequest } from '../../../models/dashboard.model';
import { StatusMasterService, ComplaintStatusMaster, ApiResponse } from '../../../services/status-master.service';

@Component({
  selector: 'app-dashboard-customizer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard-customizer.component.html',
  styleUrl: './dashboard-customizer.component.scss',
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
      ])
    ])
  ]
})
export class DashboardCustomizerComponent implements OnInit {
  @Input() show: boolean = false;
  @Input() currentPreferences?: DashboardPreferences;
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<SaveDashboardPreferencesRequest>();

  availableStatuses: ComplaintStatusMaster[] = [];
  selectedStatusIds: string[] = [];

  // Form fields
  layout: string = 'grid-4';
  showTrends: boolean = true;
  showPercentages: boolean = true;
  autoRefreshInterval: number = 0;
  dateRangeDays: number = 30;
  theme: string = 'auto';

  loading: boolean = false;
  error: string = '';

  layoutOptions = [
    { value: 'grid-2', label: '2 Columns', icon: 'fa-table-cells-large' },
    { value: 'grid-3', label: '3 Columns', icon: 'fa-table-cells' },
    { value: 'grid-4', label: '4 Columns', icon: 'fa-grip' },
    { value: 'grid-5', label: '5 Columns', icon: 'fa-grip-vertical' },
    { value: 'grid-6', label: '6 Columns', icon: 'fa-grip-vertical' },
    { value: 'list', label: 'List View', icon: 'fa-list-ul' }
  ];

  dateRangeOptions = [
    { value: 7, label: 'Last 7 days' },
    { value: 14, label: 'Last 14 days' },
    { value: 30, label: 'Last 30 days' },
    { value: 60, label: 'Last 60 days' },
    { value: 90, label: 'Last 90 days' },
    { value: 180, label: 'Last 6 months' },
    { value: 365, label: 'Last year' }
  ];

  refreshIntervalOptions = [
    { value: 0, label: 'Never' },
    { value: 30, label: '30 seconds' },
    { value: 60, label: '1 minute' },
    { value: 300, label: '5 minutes' },
    { value: 600, label: '10 minutes' }
  ];

  themeOptions = [
    { value: 'light', label: 'Light', icon: 'fa-sun' },
    { value: 'dark', label: 'Dark', icon: 'fa-moon' },
    { value: 'auto', label: 'Auto', icon: 'fa-circle-half-stroke' }
  ];

  constructor(private statusMasterService: StatusMasterService) {}

  ngOnInit(): void {
    this.loadStatuses();
    this.loadCurrentPreferences();
  }

  loadStatuses(): void {
    this.loading = true;
    this.statusMasterService.getStatuses(undefined, true, true).subscribe({
      next: (response: ApiResponse<ComplaintStatusMaster[]>) => {
        if (response.isSuccess && response.data) {
          this.availableStatuses = response.data;
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading statuses:', error);
        this.error = 'Failed to load available statuses';
        this.loading = false;
      }
    });
  }

  loadCurrentPreferences(): void {
    if (this.currentPreferences) {
      this.selectedStatusIds = [...this.currentPreferences.statusWidgets];
      this.layout = this.currentPreferences.layout;
      this.showTrends = this.currentPreferences.showTrends;
      this.showPercentages = this.currentPreferences.showPercentages;
      this.autoRefreshInterval = this.currentPreferences.autoRefreshInterval;
      this.dateRangeDays = this.currentPreferences.dateRangeDays;
      this.theme = this.currentPreferences.theme || 'auto';
    }
  }

  toggleStatus(statusId: string): void {
    const index = this.selectedStatusIds.indexOf(statusId);
    if (index > -1) {
      this.selectedStatusIds.splice(index, 1);
    } else {
      this.selectedStatusIds.push(statusId);
    }
  }

  isStatusSelected(statusId: string): boolean {
    return this.selectedStatusIds.includes(statusId);
  }

  selectAllStatuses(): void {
    this.selectedStatusIds = this.availableStatuses.map(s => s.id);
  }

  clearAllStatuses(): void {
    this.selectedStatusIds = [];
  }

  onClose(): void {
    this.close.emit();
  }

  onSave(): void {
    if (this.selectedStatusIds.length === 0) {
      this.error = 'Please select at least one status to display';
      return;
    }

    const request: SaveDashboardPreferencesRequest = {
      statusWidgets: this.selectedStatusIds,
      layout: this.layout,
      showTrends: this.showTrends,
      showPercentages: this.showPercentages,
      autoRefreshInterval: this.autoRefreshInterval,
      dateRangeDays: this.dateRangeDays,
      theme: this.theme
    };

    this.save.emit(request);
  }

  resetToDefaults(): void {
    this.selectedStatusIds = this.availableStatuses.slice(0, 4).map(s => s.id);
    this.layout = 'grid-4';
    this.showTrends = true;
    this.showPercentages = true;
    this.autoRefreshInterval = 0;
    this.dateRangeDays = 30;
    this.theme = 'auto';
  }

  getSelectedCount(): number {
    return this.selectedStatusIds.length;
  }
}
