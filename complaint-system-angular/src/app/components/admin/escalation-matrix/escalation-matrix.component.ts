import { Component, OnInit } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { EscalationService } from '../../../services/escalation.service';
import { AuthService } from '../../../services/auth.service';
import {
  EscalationMatrix,
  CreateEscalationMatrixRequest,
  UpdateEscalationMatrixRequest,
  TimeUnit,
  AssignmentStrategy
} from '../../../models/escalation.model';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

interface SimulationResult {
  matched: boolean;
  matchedRule?: string;
}

interface EscalationStats {
  triggeredToday: number;
  triggeredPercent: number;
  slaBreaches: number;
  slaBreachPercent: number;
  activeRules: number;
  activePercent: number;
}

@Component({
  selector: 'app-escalation-matrix',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, UtcToLocalPipe, TitleCasePipe],
  templateUrl: './escalation-matrix.component.html',
  styleUrls: ['./escalation-matrix.component.scss']
})
export class EscalationMatrixComponent implements OnInit {
  matrices: EscalationMatrix[] = [];
  filteredMatrices: EscalationMatrix[] = [];
  selectedMatrix: EscalationMatrix | null = null;
  matrixForm!: FormGroup;
  showForm = false;
  isEditMode = false;
  loading = false;
  errorMessage = '';
  successMessage = '';

  // Search and Filter
  searchTerm = '';
  statusFilter: 'all' | 'active' | 'inactive' = 'all';
  priorityFilter: 'all' | 'high' | 'medium' | 'low' = 'all';

  // Expanded matrices tracking
  expandedMatrices = new Set<string>();

  // Dropdown control
  activeDropdownId: string | null = null;

  // Simulator
  simulatorPriorityValue = 3;
  simulatorPriority = 'high';
  simulatorIdleTime = 4;
  simulationResult: SimulationResult | null = null;

  // Stats
  stats: EscalationStats = {
    triggeredToday: 0,
    triggeredPercent: 0,
    slaBreaches: 0,
    slaBreachPercent: 0,
    activeRules: 0,
    activePercent: 0
  };

  // Enums for template
  timeUnits = [
    { value: TimeUnit.Minutes, label: 'Minutes' },
    { value: TimeUnit.Hours, label: 'Hours' },
    { value: TimeUnit.Days, label: 'Days' },
    { value: TimeUnit.Weeks, label: 'Weeks' }
  ];

  assignmentStrategies = [
    { value: AssignmentStrategy.ReportingManager, label: 'Reporting Manager' },
    { value: AssignmentStrategy.SectionIncharge, label: 'Section Incharge' },
    { value: AssignmentStrategy.BranchContacts, label: 'Branch Contacts' },
    { value: AssignmentStrategy.DepartmentContacts, label: 'Department Contacts' },
    { value: AssignmentStrategy.AdminEscalation, label: 'Admin Escalation' },
    { value: AssignmentStrategy.ResourcePool, label: 'Resource Pool' }
  ];

  constructor(
    private escalationService: EscalationService,
    private authService: AuthService,
    private fb: FormBuilder
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.loadMatrices();
  }

  initForm(): void {
    this.matrixForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(500)],
      isDefault: [false],
      levels: this.fb.array([])
    });
  }

  get levels(): FormArray {
    return this.matrixForm.get('levels') as FormArray;
  }

  loadMatrices(): void {
    this.loading = true;
    this.errorMessage = '';

    this.escalationService.getMatrices().subscribe({
      next: (data) => {
        this.matrices = data || [];
        this.filterMatrices();
        this.calculateStats();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load escalation rules. Please try again.';
        this.loading = false;
        console.error('Error loading matrices:', error);
      }
    });
  }

  filterMatrices(): void {
    let filtered = [...this.matrices];

    // Filter by status
    if (this.statusFilter === 'active') {
      filtered = filtered.filter(m => m.isActive);
    } else if (this.statusFilter === 'inactive') {
      filtered = filtered.filter(m => !m.isActive);
    }

    // Filter by priority
    if (this.priorityFilter !== 'all') {
      filtered = filtered.filter(m => {
        const priorityLevel = this.getPriorityLevel(m.priority);
        return priorityLevel === this.priorityFilter;
      });
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(m =>
        m.name.toLowerCase().includes(search) ||
        (m.description && m.description.toLowerCase().includes(search)) ||
        m.id.toLowerCase().includes(search)
      );
    }

    // Sort by priority (high first)
    filtered.sort((a, b) => (b.priority || 0) - (a.priority || 0));

    this.filteredMatrices = filtered;
  }

  calculateStats(): void {
    const activeRules = this.matrices.filter(m => m.isActive).length;
    const totalRules = this.matrices.length;

    this.stats = {
      triggeredToday: Math.floor(Math.random() * 150), // Mock data - replace with API call
      triggeredPercent: 65,
      slaBreaches: Math.floor(Math.random() * 20),
      slaBreachPercent: 15,
      activeRules: activeRules,
      activePercent: totalRules > 0 ? (activeRules / totalRules) * 100 : 0
    };
  }

  onSearchChange(): void {
    this.filterMatrices();
  }

  setStatusFilter(filter: 'all' | 'active' | 'inactive'): void {
    // Cycle through filters
    if (this.statusFilter === 'all') {
      this.statusFilter = 'active';
    } else if (this.statusFilter === 'active') {
      this.statusFilter = 'inactive';
    } else {
      this.statusFilter = 'all';
    }
    this.filterMatrices();
  }

  togglePriorityFilter(): void {
    if (this.priorityFilter === 'all') {
      this.priorityFilter = 'high';
    } else if (this.priorityFilter === 'high') {
      this.priorityFilter = 'medium';
    } else if (this.priorityFilter === 'medium') {
      this.priorityFilter = 'low';
    } else {
      this.priorityFilter = 'all';
    }
    this.filterMatrices();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.statusFilter = 'all';
    this.priorityFilter = 'all';
    this.filterMatrices();
  }

  toggleExpand(matrixId: string): void {
    if (this.expandedMatrices.has(matrixId)) {
      this.expandedMatrices.delete(matrixId);
    } else {
      this.expandedMatrices.add(matrixId);
    }
  }

  toggleDropdown(matrixId: string): void {
    if (this.activeDropdownId === matrixId) {
      this.activeDropdownId = null;
    } else {
      this.activeDropdownId = matrixId;
    }
  }

  closeDropdown(): void {
    this.activeDropdownId = null;
  }

  getPriorityLevel(priority: number | undefined): string {
    if (!priority || priority <= 1) return 'low';
    if (priority <= 2) return 'medium';
    return 'high';
  }

  getPriorityClass(priority: number | undefined): string {
    const level = this.getPriorityLevel(priority);
    return `priority-${level}`;
  }

  getMatrixIcon(matrix: EscalationMatrix): string {
    const priority = matrix.priority || 0;
    if (priority >= 3) return 'timer_off';
    if (priority >= 2) return 'autorenew';
    return 'mark_email_read';
  }

  getLevelSummary(matrix: EscalationMatrix): string {
    if (!matrix.escalationLevels || matrix.escalationLevels.length === 0) {
      return 'No triggers configured';
    }
    const firstLevel = matrix.escalationLevels[0];
    return `Time > ${firstLevel.triggerTimeDisplay || firstLevel.triggerAfterValue + ' ' + this.getTimeUnitLabel(firstLevel.triggerTimeUnit)}`;
  }

  getActionSummary(matrix: EscalationMatrix): string {
    if (!matrix.escalationLevels || matrix.escalationLevels.length === 0) {
      return 'No actions configured';
    }
    const firstLevel = matrix.escalationLevels[0];
    return `Notify: ${this.getAssignmentStrategyLabel(firstLevel.assignmentStrategy)}`;
  }

  getInitials(name: string): string {
    if (!name) return '?';
    const parts = name.trim().split(' ').filter(p => p.length > 0);
    if (parts.length === 0) return '?';
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  // Simulator Methods
  updateSimulatorPriority(): void {
    switch (this.simulatorPriorityValue) {
      case 1: this.simulatorPriority = 'low'; break;
      case 2: this.simulatorPriority = 'medium'; break;
      case 3: this.simulatorPriority = 'high'; break;
      case 4: this.simulatorPriority = 'critical'; break;
      default: this.simulatorPriority = 'medium';
    }
  }

  runSimulation(): void {
    // Find matching rule based on priority and idle time
    const matchingRule = this.matrices.find(m => {
      if (!m.isActive || !m.escalationLevels || m.escalationLevels.length === 0) return false;

      const firstLevel = m.escalationLevels[0];
      const triggerHours = this.convertToHours(firstLevel.triggerAfterValue, firstLevel.triggerTimeUnit);

      return this.simulatorIdleTime >= triggerHours;
    });

    this.simulationResult = {
      matched: !!matchingRule,
      matchedRule: matchingRule?.name
    };
  }

  convertToHours(value: number, unit: TimeUnit): number {
    switch (unit) {
      case TimeUnit.Minutes: return value / 60;
      case TimeUnit.Hours: return value;
      case TimeUnit.Days: return value * 24;
      case TimeUnit.Weeks: return value * 24 * 7;
      default: return value;
    }
  }

  testRule(matrix: EscalationMatrix): void {
    // Set simulator to test this rule
    if (matrix.escalationLevels && matrix.escalationLevels.length > 0) {
      const firstLevel = matrix.escalationLevels[0];
      this.simulatorIdleTime = this.convertToHours(firstLevel.triggerAfterValue, firstLevel.triggerTimeUnit) + 1;
      this.runSimulation();
    }
  }

  // Toggle Active Status
  toggleMatrixActive(matrix: EscalationMatrix): void {
    const updateRequest: UpdateEscalationMatrixRequest = {
      name: matrix.name,
      description: matrix.description,
      categoryId: matrix.categoryId,
      branchId: matrix.branchId,
      departmentId: matrix.departmentId,
      isActive: !matrix.isActive,
      priority: matrix.priority,
      enableAutoEscalation: matrix.enableAutoEscalation,
      sendEmailNotifications: matrix.sendEmailNotifications,
      escalationLevels: matrix.escalationLevels.map(level => ({
        level: level.level,
        name: level.name,
        description: level.description,
        triggerAfterValue: level.triggerAfterValue,
        triggerTimeUnit: level.triggerTimeUnit,
        assignmentStrategy: level.assignmentStrategy,
        assignToUserId: level.assignToUserId,
        assignToRole: level.assignToRole,
        sendNotification: level.sendNotification,
        notifyPreviousHandler: level.notifyPreviousHandler,
        escalationMessage: level.escalationMessage
      }))
    };

    this.escalationService.updateMatrix(matrix.id, updateRequest).subscribe({
      next: () => {
        matrix.isActive = !matrix.isActive;
        this.successMessage = `Rule ${matrix.isActive ? 'activated' : 'deactivated'} successfully!`;
        this.calculateStats();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = 'Failed to update rule status. Please try again.';
        console.error('Error toggling matrix:', error);
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  duplicateMatrix(matrix: EscalationMatrix): void {
    const createRequest: CreateEscalationMatrixRequest = {
      name: `${matrix.name} (Copy)`,
      description: matrix.description,
      isDefault: false,
      escalationLevels: matrix.escalationLevels.map(level => ({
        level: level.level,
        name: level.name,
        description: level.description,
        triggerAfterValue: level.triggerAfterValue,
        triggerTimeUnit: level.triggerTimeUnit,
        assignmentStrategy: level.assignmentStrategy,
        assignToUserId: level.assignToUserId,
        assignToRole: level.assignToRole,
        sendNotification: level.sendNotification,
        notifyPreviousHandler: level.notifyPreviousHandler,
        escalationMessage: level.escalationMessage
      }))
    };

    this.loading = true;
    this.escalationService.createMatrix(createRequest).subscribe({
      next: () => {
        this.successMessage = 'Rule duplicated successfully!';
        this.loadMatrices();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = 'Failed to duplicate rule. Please try again.';
        this.loading = false;
        console.error('Error duplicating matrix:', error);
      }
    });
  }

  createNewMatrix(): void {
    this.showForm = true;
    this.isEditMode = false;
    this.selectedMatrix = null;
    this.initForm();
    this.addLevel(); // Add first level by default
  }

  editMatrix(matrix: EscalationMatrix): void {
    this.showForm = true;
    this.isEditMode = true;
    this.selectedMatrix = matrix;

    this.matrixForm.patchValue({
      name: matrix.name,
      description: matrix.description,
      isDefault: matrix.isDefault
    });

    // Clear and rebuild levels
    this.levels.clear();
    matrix.escalationLevels.forEach(level => {
      this.levels.push(this.fb.group({
        level: [level.level, Validators.required],
        name: [level.name, [Validators.required, Validators.maxLength(100)]],
        description: [level.description, Validators.maxLength(500)],
        triggerAfterValue: [level.triggerAfterValue, [Validators.required, Validators.min(1)]],
        triggerTimeUnit: [level.triggerTimeUnit, Validators.required],
        assignmentStrategy: [level.assignmentStrategy, Validators.required],
        assignToUserId: [level.assignToUserId],
        assignToRole: [level.assignToRole],
        assignToUserIds: [level.assignToUserIds],
        sendNotification: [level.sendNotification],
        notifyPreviousHandler: [level.notifyPreviousHandler],
        escalationMessage: [level.escalationMessage, Validators.maxLength(1000)]
      }));
    });
  }

  addLevel(): void {
    const levelNumber = this.levels.length + 1;
    this.levels.push(this.fb.group({
      level: [levelNumber, Validators.required],
      name: [`Level ${levelNumber}`, [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      triggerAfterValue: [24, [Validators.required, Validators.min(1)]],
      triggerTimeUnit: [TimeUnit.Hours, Validators.required],
      assignmentStrategy: [AssignmentStrategy.ReportingManager, Validators.required],
      assignToUserId: [''],
      assignToRole: [''],
      assignToUserIds: [''],
      sendNotification: [true],
      notifyPreviousHandler: [true],
      escalationMessage: ['']
    }));
  }

  removeLevel(index: number): void {
    if (this.levels.length > 1) {
      this.levels.removeAt(index);
      // Update level numbers
      this.levels.controls.forEach((control, i) => {
        control.patchValue({ level: i + 1 });
      });
    }
  }

  cancelForm(): void {
    this.showForm = false;
    this.selectedMatrix = null;
    this.initForm();
    this.errorMessage = '';
    this.successMessage = '';
  }

  saveMatrix(): void {
    if (this.matrixForm.invalid) {
      this.errorMessage = 'Please fill in all required fields correctly.';
      Object.keys(this.matrixForm.controls).forEach(key => {
        const control = this.matrixForm.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.matrixForm.value;
    const escalationLevels = formValue.levels.map((level: any) => ({
      level: level.level,
      name: level.name,
      description: level.description,
      triggerAfterValue: level.triggerAfterValue,
      triggerTimeUnit: level.triggerTimeUnit,
      assignmentStrategy: level.assignmentStrategy,
      assignToUserId: level.assignToUserId || undefined,
      assignToRole: level.assignToRole || undefined,
      assignToUserIds: level.assignToUserIds || undefined,
      sendNotification: level.sendNotification,
      notifyPreviousHandler: level.notifyPreviousHandler,
      escalationMessage: level.escalationMessage || undefined
    }));

    let operation;

    if (this.isEditMode && this.selectedMatrix) {
      const updateRequest: UpdateEscalationMatrixRequest = {
        name: formValue.name,
        description: formValue.description,
        categoryId: this.selectedMatrix.categoryId,
        branchId: this.selectedMatrix.branchId,
        departmentId: this.selectedMatrix.departmentId,
        isActive: this.selectedMatrix.isActive,
        priority: this.selectedMatrix.priority,
        enableAutoEscalation: this.selectedMatrix.enableAutoEscalation,
        sendEmailNotifications: this.selectedMatrix.sendEmailNotifications,
        escalationLevels: escalationLevels
      };
      operation = this.escalationService.updateMatrix(this.selectedMatrix.id, updateRequest);
    } else {
      const createRequest: CreateEscalationMatrixRequest = {
        name: formValue.name,
        description: formValue.description,
        isDefault: formValue.isDefault,
        escalationLevels: escalationLevels
      };
      operation = this.escalationService.createMatrix(createRequest);
    }

    operation.subscribe({
      next: () => {
        this.successMessage = `Escalation rule ${this.isEditMode ? 'updated' : 'created'} successfully!`;
        this.loadMatrices();
        setTimeout(() => {
          this.cancelForm();
        }, 2000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || `Failed to ${this.isEditMode ? 'update' : 'create'} escalation rule. Please try again.`;
        this.loading = false;
        console.error('Error saving matrix:', error);
      }
    });
  }

  deleteMatrix(matrix: EscalationMatrix): void {
    if (!confirm(`Are you sure you want to delete "${matrix.name}"? This action cannot be undone.`)) {
      return;
    }

    this.loading = true;
    this.escalationService.deleteMatrix(matrix.id).subscribe({
      next: () => {
        this.successMessage = 'Escalation rule deleted successfully!';
        this.loadMatrices();
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete escalation rule. Please try again.';
        this.loading = false;
        console.error('Error deleting matrix:', error);
      }
    });
  }

  getTimeUnitLabel(unit: TimeUnit): string {
    return this.timeUnits.find(u => u.value === unit)?.label || 'Hours';
  }

  getAssignmentStrategyLabel(strategy: AssignmentStrategy): string {
    return this.assignmentStrategies.find(s => s.value === strategy)?.label || 'Unknown';
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.matrixForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  isLevelFieldInvalid(levelIndex: number, fieldName: string): boolean {
    const field = this.levels.at(levelIndex).get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  // TrackBy functions for *ngFor optimization
  trackByMatrixId(index: number, matrix: EscalationMatrix): string {
    return matrix.id;
  }

  trackByEscalationLevel(index: number, level: any): number {
    return level.level;
  }

  trackByTimeUnit(index: number, item: any): number {
    return item.value;
  }

  trackByStrategy(index: number, item: any): number {
    return item.value;
  }

  trackByLevel(index: number, item: any): number {
    return index;
  }
}
