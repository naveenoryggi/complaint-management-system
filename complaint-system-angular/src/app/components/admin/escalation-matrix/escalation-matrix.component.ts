import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { EscalationService } from '../../../services/escalation.service';
import {
  EscalationMatrix,
  CreateEscalationMatrixRequest,
  UpdateEscalationMatrixRequest,
  CreateEscalationLevelRequest,
  TimeUnit,
  AssignmentStrategy
} from '../../../models/escalation.model';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-escalation-matrix',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, UtcToLocalPipe],
  templateUrl: './escalation-matrix.component.html',
  styleUrls: ['./escalation-matrix.component.scss']
})
export class EscalationMatrixComponent implements OnInit {
  matrices: EscalationMatrix[] = [];
  selectedMatrix: EscalationMatrix | null = null;
  matrixForm!: FormGroup;
  showForm = false;
  isEditMode = false;
  loading = false;
  errorMessage = '';
  successMessage = '';

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
        this.matrices = data;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load escalation matrices. Please try again.';
        this.loading = false;
        console.error('Error loading matrices:', error);
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
        this.successMessage = `Escalation matrix ${this.isEditMode ? 'updated' : 'created'} successfully!`;
        this.loadMatrices();
        setTimeout(() => {
          this.cancelForm();
        }, 2000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || `Failed to ${this.isEditMode ? 'update' : 'create'} escalation matrix. Please try again.`;
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
        this.successMessage = 'Escalation matrix deleted successfully!';
        this.loadMatrices();
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete escalation matrix. Please try again.';
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
