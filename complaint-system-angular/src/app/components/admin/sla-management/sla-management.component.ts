import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { SetupProgressService } from '../../../services/setup-progress.service';
import { SLAService, CategorySLA, PrioritySLA, CreateCategorySLARequest, CreatePrioritySLARequest } from '../../../services/sla.service';
import { CategoryService } from '../../../services/category.service';
import { PriorityMasterService } from '../../../services/priority-master.service';

interface SLALevel {
  id?: string;
  name: string;
  description: string;
  order: number;
  isActive: boolean;
  colorCode: string;
  defaultResponseTime: number;
  defaultResolutionTime: number;
  responseTimeUnit: 'minutes' | 'hours' | 'days';
  resolutionTimeUnit: 'minutes' | 'hours' | 'days';
}

interface GlobalSLASettings {
  enableSLA: boolean;
  workingHoursOnly: boolean;
  workingHoursStart: string;
  workingHoursEnd: string;
  workingDays: number[]; // 0=Sunday, 1=Monday, etc.
  autoEscalateOnBreach: boolean;
  escalationThresholdPercent: number;
  notifyBeforeBreach: boolean;
  notifyBeforeBreachMinutes: number;
  pauseSLAOnPendingInfo: boolean;
  excludeHolidays: boolean;
}

@Component({
  selector: 'app-sla-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './sla-management.component.html',
  styleUrls: ['./sla-management.component.scss']
})
export class SLAManagementComponent implements OnInit {
  activeTab = signal<'global' | 'levels' | 'category' | 'priority'>('global');

  globalSLAForm!: FormGroup;
  slaLevelForm!: FormGroup;
  categoryMappingForm!: FormGroup;
  priorityMappingForm!: FormGroup;

  slaLevels = signal<SLALevel[]>([]);
  categoryMappings = signal<CategorySLA[]>([]);
  priorityMappings = signal<PrioritySLA[]>([]);

  editingLevel = signal<SLALevel | null>(null);
  showLevelForm = signal(false);

  editingCategoryMapping = signal<CategorySLA | null>(null);
  editingPriorityMapping = signal<PrioritySLA | null>(null);
  showCategoryMappingForm = signal(false);
  showPriorityMappingForm = signal(false);

  weekDays = [
    { value: 1, label: 'Monday' },
    { value: 2, label: 'Tuesday' },
    { value: 3, label: 'Wednesday' },
    { value: 4, label: 'Thursday' },
    { value: 5, label: 'Friday' },
    { value: 6, label: 'Saturday' },
    { value: 0, label: 'Sunday' }
  ];

  timeUnits = [
    { value: 'minutes', label: 'Minutes' },
    { value: 'hours', label: 'Hours' },
    { value: 'days', label: 'Days' }
  ];

  predefinedColors = [
    '#4CAF50', '#2196F3', '#FF9800', '#F44336',
    '#9C27B0', '#00BCD4', '#FFC107', '#795548'
  ];

  loading = signal(false);
  savingSettings = signal(false);
  savingLevel = signal(false);
  savingCategoryMapping = signal(false);
  savingPriorityMapping = signal(false);

  // Available options for dropdowns
  categories = signal<any[]>([]);
  priorities = signal<any[]>([]);

  constructor(
    private fb: FormBuilder,
    public setupService: SetupProgressService,
    private slaService: SLAService,
    private categoryService: CategoryService,
    private priorityService: PriorityMasterService
  ) {}

  ngOnInit(): void {
    this.initializeGlobalSLAForm();
    this.initializeSLALevelForm();
    this.initializeCategoryMappingForm();
    this.initializePriorityMappingForm();
    this.loadGlobalSettings();
    this.loadSLALevels();
    this.loadCategories();
    this.loadPriorities();
    this.loadCategoryMappings();
    this.loadPriorityMappings();
  }

  initializeGlobalSLAForm(): void {
    this.globalSLAForm = this.fb.group({
      enableSLA: [true],
      workingHoursOnly: [false],
      workingHoursStart: ['09:00'],
      workingHoursEnd: ['17:00'],
      workingDays: [[1, 2, 3, 4, 5]], // Monday-Friday
      autoEscalateOnBreach: [true],
      escalationThresholdPercent: [80, [Validators.min(0), Validators.max(100)]],
      notifyBeforeBreach: [true],
      notifyBeforeBreachMinutes: [30, [Validators.min(1)]],
      pauseSLAOnPendingInfo: [true],
      excludeHolidays: [true]
    });

    // Subscribe to workingHoursOnly changes
    this.globalSLAForm.get('workingHoursOnly')?.valueChanges.subscribe(enabled => {
      const startControl = this.globalSLAForm.get('workingHoursStart');
      const endControl = this.globalSLAForm.get('workingHoursEnd');
      const daysControl = this.globalSLAForm.get('workingDays');

      if (enabled) {
        startControl?.enable();
        endControl?.enable();
        daysControl?.enable();
      } else {
        startControl?.disable();
        endControl?.disable();
        daysControl?.disable();
      }
    });
  }

  initializeSLALevelForm(): void {
    this.slaLevelForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', [Validators.maxLength(200)]],
      order: [1, [Validators.required, Validators.min(1)]],
      isActive: [true],
      colorCode: ['#4CAF50', [Validators.required]],
      defaultResponseTime: [4, [Validators.required, Validators.min(1)]],
      defaultResolutionTime: [24, [Validators.required, Validators.min(1)]],
      responseTimeUnit: ['hours', [Validators.required]],
      resolutionTimeUnit: ['hours', [Validators.required]]
    });
  }

  loadGlobalSettings(): void {
    this.loading.set(true);
    this.slaService.getSettings().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          const data = response.data;
          this.globalSLAForm.patchValue({
            enableSLA: data.isEnabled,
            workingHoursOnly: data.workingHoursOnly,
            workingHoursStart: data.workingHoursStart || '09:00',
            workingHoursEnd: data.workingHoursEnd || '17:00',
            workingDays: this.stringToWorkingDays(data.workingDays),
            autoEscalateOnBreach: data.autoEscalateOnBreach,
            escalationThresholdPercent: data.escalationThresholdPercent,
            notifyBeforeBreach: data.notifyBeforeBreach,
            notifyBeforeBreachMinutes: data.notifyBeforeBreachMinutes,
            pauseSLAOnPendingInfo: data.pauseSLAOnPendingInfo,
            excludeHolidays: data.excludeHolidays
          });
        }
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading SLA settings:', error);
        this.loading.set(false);
        this.handleError(error, 'loading SLA settings');
      }
    });
  }

  loadSLALevels(): void {
    this.loading.set(true);
    this.slaService.getLevels().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          const levels = response.data.map(level => ({
            id: level.id,
            name: level.name,
            description: level.description || '',
            order: level.order,
            isActive: level.isActive,
            colorCode: level.colorCode,
            defaultResponseTime: level.defaultResponseTime,
            defaultResolutionTime: level.defaultResolutionTime,
            responseTimeUnit: this.mapTimeUnit(level.responseTimeUnit),
            resolutionTimeUnit: this.mapTimeUnit(level.resolutionTimeUnit)
          }));
          this.slaLevels.set(levels);
        }
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading SLA levels:', error);
        this.loading.set(false);
        this.handleError(error, 'loading SLA levels');
      }
    });
  }

  saveGlobalSettings(): void {
    if (this.globalSLAForm.invalid) {
      alert('Please correct the errors in the form');
      return;
    }

    const formValue = this.globalSLAForm.value;
    const request = {
      isEnabled: formValue.enableSLA,
      workingHoursOnly: formValue.workingHoursOnly,
      workingHoursStart: formValue.workingHoursStart,
      workingHoursEnd: formValue.workingHoursEnd,
      workingDays: this.workingDaysToString(formValue.workingDays),
      excludeHolidays: formValue.excludeHolidays,
      autoEscalateOnBreach: formValue.autoEscalateOnBreach,
      escalationThresholdPercent: formValue.escalationThresholdPercent,
      notifyBeforeBreach: formValue.notifyBeforeBreach,
      notifyBeforeBreachMinutes: formValue.notifyBeforeBreachMinutes,
      pauseSLAOnPendingInfo: formValue.pauseSLAOnPendingInfo,
      timezone: 'UTC'
    };

    this.savingSettings.set(true);
    this.slaService.updateSettings(request).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.setupService.markStepCompleted('global-sla-settings');
          alert('Global SLA settings saved successfully!');
        } else {
          alert('Error saving settings: ' + (response.message || 'Unknown error'));
        }
        this.savingSettings.set(false);
      },
      error: (error) => {
        console.error('Error saving SLA settings:', error);
        this.savingSettings.set(false);
        this.handleError(error, 'saving SLA settings');
      }
    });
  }

  isWorkingDay(day: number): boolean {
    const selectedDays = this.globalSLAForm.get('workingDays')?.value || [];
    return selectedDays.includes(day);
  }

  toggleWorkingDay(day: number): void {
    const control = this.globalSLAForm.get('workingDays');
    const currentDays: number[] = control?.value || [];

    const index = currentDays.indexOf(day);
    if (index > -1) {
      currentDays.splice(index, 1);
    } else {
      currentDays.push(day);
    }

    control?.setValue([...currentDays]);
  }

  // SLA Level Management
  openLevelForm(level?: SLALevel): void {
    if (level) {
      this.editingLevel.set(level);
      this.slaLevelForm.patchValue(level);
    } else {
      this.editingLevel.set(null);
      this.slaLevelForm.reset({
        order: this.slaLevels().length + 1,
        isActive: true,
        colorCode: this.predefinedColors[0],
        responseTimeUnit: 'hours',
        resolutionTimeUnit: 'hours'
      });
    }
    this.showLevelForm.set(true);
  }

  closeLevelForm(): void {
    this.showLevelForm.set(false);
    this.editingLevel.set(null);
    this.slaLevelForm.reset();
  }

  saveSLALevel(): void {
    if (this.slaLevelForm.invalid) {
      alert('Please correct the errors in the form');
      return;
    }

    const formValue = this.slaLevelForm.value;
    const request = {
      name: formValue.name,
      description: formValue.description || '',
      order: formValue.order,
      isActive: formValue.isActive,
      colorCode: formValue.colorCode,
      defaultResponseTime: formValue.defaultResponseTime,
      responseTimeUnit: this.capitalizeFirst(formValue.responseTimeUnit),
      defaultResolutionTime: formValue.defaultResolutionTime,
      resolutionTimeUnit: this.capitalizeFirst(formValue.resolutionTimeUnit)
    };

    const editingLevel = this.editingLevel();
    const operation = editingLevel
      ? this.slaService.updateLevel(editingLevel.id!, request)
      : this.slaService.createLevel(request);

    this.savingLevel.set(true);
    operation.subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.setupService.markStepCompleted('sla-levels');
          this.loadSLALevels(); // Reload list
          this.closeLevelForm();
          alert(editingLevel ? 'SLA level updated successfully!' : 'SLA level created successfully!');
        } else {
          alert('Error saving SLA level: ' + (response.message || 'Unknown error'));
        }
        this.savingLevel.set(false);
      },
      error: (error) => {
        console.error('Error saving SLA level:', error);
        this.savingLevel.set(false);
        this.handleError(error, 'saving SLA level');
      }
    });
  }

  deleteSLALevel(level: SLALevel): void {
    if (!confirm(`Are you sure you want to delete "${level.name}"?`)) {
      return;
    }

    if (!level.id) {
      alert('Cannot delete level without ID');
      return;
    }

    this.slaService.deleteLevel(level.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadSLALevels(); // Reload list
          alert('SLA level deleted successfully!');
        } else {
          alert('Error deleting SLA level: ' + (response.message || 'Unknown error'));
        }
      },
      error: (error) => {
        console.error('Error deleting SLA level:', error);
        this.handleError(error, 'deleting SLA level');
      }
    });
  }

  moveLevelUp(level: SLALevel): void {
    const levels = this.slaLevels();
    const index = levels.findIndex(l => l.id === level.id);
    if (index > 0) {
      const newLevels = [...levels];
      [newLevels[index - 1], newLevels[index]] = [newLevels[index], newLevels[index - 1]];
      newLevels.forEach((l, i) => l.order = i + 1);
      this.slaLevels.set(newLevels);
    }
  }

  moveLevelDown(level: SLALevel): void {
    const levels = this.slaLevels();
    const index = levels.findIndex(l => l.id === level.id);
    if (index < levels.length - 1) {
      const newLevels = [...levels];
      [newLevels[index], newLevels[index + 1]] = [newLevels[index + 1], newLevels[index]];
      newLevels.forEach((l, i) => l.order = i + 1);
      this.slaLevels.set(newLevels);
    }
  }

  // Category Mapping Management

  initializeCategoryMappingForm(): void {
    this.categoryMappingForm = this.fb.group({
      categoryId: ['', [Validators.required]],
      slaLevelId: ['', [Validators.required]],
      overrideResponseTime: [null],
      overrideResolutionTime: [null],
      isActive: [true]
    });
  }

  loadCategoryMappings(): void {
    this.loading.set(true);
    this.slaService.getCategoryMappings().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.categoryMappings.set(response.data);
        }
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading category mappings:', error);
        this.loading.set(false);
        this.handleError(error, 'loading category mappings');
      }
    });
  }

  openCategoryMappingForm(mapping?: CategorySLA): void {
    if (mapping) {
      this.editingCategoryMapping.set(mapping);
      this.categoryMappingForm.patchValue({
        categoryId: mapping.categoryId,
        slaLevelId: mapping.slaLevelId,
        overrideResponseTime: mapping.overrideResponseTime,
        overrideResolutionTime: mapping.overrideResolutionTime,
        isActive: mapping.isActive
      });
    } else {
      this.editingCategoryMapping.set(null);
      this.categoryMappingForm.reset({ isActive: true });
    }
    this.showCategoryMappingForm.set(true);
  }

  closeCategoryMappingForm(): void {
    this.showCategoryMappingForm.set(false);
    this.editingCategoryMapping.set(null);
    this.categoryMappingForm.reset();
  }

  saveCategoryMapping(): void {
    if (this.categoryMappingForm.invalid) {
      alert('Please correct the errors in the form');
      return;
    }

    const formValue = this.categoryMappingForm.value;
    const request: CreateCategorySLARequest = {
      categoryId: formValue.categoryId,
      slaLevelId: formValue.slaLevelId,
      overrideResponseTime: formValue.overrideResponseTime || undefined,
      overrideResolutionTime: formValue.overrideResolutionTime || undefined,
      isActive: formValue.isActive
    };

    this.savingCategoryMapping.set(true);
    this.slaService.saveCategoryMapping(request).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadCategoryMappings();
          this.closeCategoryMappingForm();
          alert('Category mapping saved successfully!');
        } else {
          alert('Error saving category mapping: ' + (response.message || 'Unknown error'));
        }
        this.savingCategoryMapping.set(false);
      },
      error: (error) => {
        console.error('Error saving category mapping:', error);
        this.savingCategoryMapping.set(false);
        this.handleError(error, 'saving category mapping');
      }
    });
  }

  deleteCategoryMapping(mapping: CategorySLA): void {
    if (!confirm(`Are you sure you want to delete the mapping for "${mapping.categoryName}"?`)) {
      return;
    }

    this.slaService.deleteCategoryMapping(mapping.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadCategoryMappings();
          alert('Category mapping deleted successfully!');
        } else {
          alert('Error deleting category mapping: ' + (response.message || 'Unknown error'));
        }
      },
      error: (error) => {
        console.error('Error deleting category mapping:', error);
        this.handleError(error, 'deleting category mapping');
      }
    });
  }

  // Priority Mapping Management

  initializePriorityMappingForm(): void {
    this.priorityMappingForm = this.fb.group({
      priorityId: ['', [Validators.required]],
      slaLevelId: ['', [Validators.required]],
      overrideResponseTime: [null],
      overrideResolutionTime: [null],
      isActive: [true]
    });
  }

  loadPriorityMappings(): void {
    this.loading.set(true);
    this.slaService.getPriorityMappings().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.priorityMappings.set(response.data);
        }
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading priority mappings:', error);
        this.loading.set(false);
        this.handleError(error, 'loading priority mappings');
      }
    });
  }

  openPriorityMappingForm(mapping?: PrioritySLA): void {
    if (mapping) {
      this.editingPriorityMapping.set(mapping);
      this.priorityMappingForm.patchValue({
        priorityId: mapping.priorityId,
        slaLevelId: mapping.slaLevelId,
        overrideResponseTime: mapping.overrideResponseTime,
        overrideResolutionTime: mapping.overrideResolutionTime,
        isActive: mapping.isActive
      });
    } else {
      this.editingPriorityMapping.set(null);
      this.priorityMappingForm.reset({ isActive: true });
    }
    this.showPriorityMappingForm.set(true);
  }

  closePriorityMappingForm(): void {
    this.showPriorityMappingForm.set(false);
    this.editingPriorityMapping.set(null);
    this.priorityMappingForm.reset();
  }

  savePriorityMapping(): void {
    if (this.priorityMappingForm.invalid) {
      alert('Please correct the errors in the form');
      return;
    }

    const formValue = this.priorityMappingForm.value;
    const request: CreatePrioritySLARequest = {
      priorityId: formValue.priorityId,
      slaLevelId: formValue.slaLevelId,
      overrideResponseTime: formValue.overrideResponseTime || undefined,
      overrideResolutionTime: formValue.overrideResolutionTime || undefined,
      isActive: formValue.isActive
    };

    this.savingPriorityMapping.set(true);
    this.slaService.savePriorityMapping(request).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadPriorityMappings();
          this.closePriorityMappingForm();
          alert('Priority mapping saved successfully!');
        } else {
          alert('Error saving priority mapping: ' + (response.message || 'Unknown error'));
        }
        this.savingPriorityMapping.set(false);
      },
      error: (error) => {
        console.error('Error saving priority mapping:', error);
        this.savingPriorityMapping.set(false);
        this.handleError(error, 'saving priority mapping');
      }
    });
  }

  deletePriorityMapping(mapping: PrioritySLA): void {
    if (!confirm(`Are you sure you want to delete the mapping for "${mapping.priorityName}"?`)) {
      return;
    }

    this.slaService.deletePriorityMapping(mapping.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadPriorityMappings();
          alert('Priority mapping deleted successfully!');
        } else {
          alert('Error deleting priority mapping: ' + (response.message || 'Unknown error'));
        }
      },
      error: (error) => {
        console.error('Error deleting priority mapping:', error);
        this.handleError(error, 'deleting priority mapping');
      }
    });
  }

  // Load Helper Data

  loadCategories(): void {
    this.categoryService.getCategories(false).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.categories.set(response.data);
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.handleError(error, 'loading categories');
      }
    });
  }

  loadPriorities(): void {
    this.priorityService.getPriorities(undefined, true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.priorities.set(response.data);
        }
      },
      error: (error) => {
        console.error('Error loading priorities:', error);
        this.handleError(error, 'loading priorities');
      }
    });
  }

  formatTimeDisplay(time: number, unit: string): string {
    return `${time} ${unit}`;
  }

  getStatusDot(isActive: boolean): string {
    return isActive ? '🟢' : '⚫';
  }

  // Tab Navigation
  setActiveTab(tab: 'global' | 'levels' | 'category' | 'priority'): void {
    this.activeTab.set(tab);
  }

  // Validation helpers
  isFieldInvalid(formGroup: FormGroup, fieldName: string): boolean {
    const field = formGroup.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(formGroup: FormGroup, fieldName: string): string {
    const field = formGroup.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) return 'This field is required';
    if (field.errors['min']) return `Minimum value is ${field.errors['min'].min}`;
    if (field.errors['max']) return `Maximum value is ${field.errors['max'].max}`;
    if (field.errors['maxlength']) return `Maximum length is ${field.errors['maxlength'].requiredLength}`;

    return 'Invalid value';
  }

  // Helper Methods for Data Mapping

  /**
   * Convert backend time unit to frontend format
   * Backend: "Minutes", "Hours", "Days"
   * Frontend: "minutes", "hours", "days"
   */
  private mapTimeUnit(unit: string): 'minutes' | 'hours' | 'days' {
    return unit.toLowerCase() as 'minutes' | 'hours' | 'days';
  }

  /**
   * Convert frontend time unit to backend format
   * Frontend: "minutes", "hours", "days"
   * Backend: "Minutes", "Hours", "Days"
   */
  private capitalizeFirst(str: string): string {
    return str.charAt(0).toUpperCase() + str.slice(1);
  }

  /**
   * Convert working days array to comma-separated string
   * Frontend: [1, 2, 3, 4, 5]
   * Backend: "1,2,3,4,5"
   */
  private workingDaysToString(days: number[]): string {
    return days.join(',');
  }

  /**
   * Convert comma-separated string to working days array
   * Backend: "1,2,3,4,5"
   * Frontend: [1, 2, 3, 4, 5]
   */
  private stringToWorkingDays(str: string): number[] {
    if (!str) return [1, 2, 3, 4, 5]; // Default Monday-Friday
    return str.split(',').map(d => parseInt(d.trim()));
  }

  /**
   * Handle API errors with user-friendly messages
   */
  private handleError(error: any, operation: string): void {
    console.error(`Error ${operation}:`, error);

    if (error.error?.message) {
      alert(`Error: ${error.error.message}`);
    } else if (error.status === 403) {
      alert('You do not have permission to perform this action. Please contact your administrator.');
    } else if (error.status === 401) {
      alert('Your session has expired. Please log in again.');
      // Could redirect to login here
    } else if (error.status === 404) {
      alert('The requested resource was not found.');
    } else if (error.status === 500) {
      alert('A server error occurred. Please try again later.');
    } else {
      alert(`An error occurred while ${operation}. Please try again.`);
    }
  }
}
