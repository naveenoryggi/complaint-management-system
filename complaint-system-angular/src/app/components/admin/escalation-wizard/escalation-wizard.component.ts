import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { EscalationService } from '../../../services/escalation.service';
import { AuthService } from '../../../services/auth.service';
import { UserService } from '../../../services/user.service';
import { BranchService } from '../../../services/branch.service';
import { DepartmentService } from '../../../services/department.service';
import { ResourcePoolService } from '../../../services/resource-pool.service';
import { UserAutocompleteComponent, UserSearchResult } from '../shared/user-autocomplete.component';
import {
  CreateEscalationMatrixRequest,
  CreateEscalationLevelRequest,
  CreateEscalationPolicyRequest,
  TimeUnit,
  AssignmentStrategy,
  ResourcePoolAssignmentMethod
} from '../../../models/escalation.model';
import { User } from '../../../models/user.model';
import { Branch } from '../../../models/branch.model';
import { Department } from '../../../models/department.model';
import { ResourcePool } from '../../../models/escalation.model';

interface WizardTemplate {
  id: string;
  name: string;
  description: string;
  icon: string;
  levels: TemplateLevel[];
}

interface TemplateLevel {
  level: number;
  name: string;
  triggerAfterValue: number;
  triggerTimeUnit: TimeUnit;
  assignmentStrategy: AssignmentStrategy;
}

@Component({
  selector: 'app-escalation-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, UserAutocompleteComponent],
  templateUrl: './escalation-wizard.component.html',
  styleUrls: ['./escalation-wizard.component.scss']
})
export class EscalationWizardComponent implements OnInit {
  currentStep = 1;
  totalSteps = 4;

  // Step 1: Template Selection
  selectedTemplate: WizardTemplate | null = null;
  templates: WizardTemplate[] = [];

  // Step 2: Levels Configuration
  matrixForm!: FormGroup;

  // Step 3: Scope & Policy
  policyForm!: FormGroup;

  // Step 4: Review
  previewData: any = null;

  // Loading & Messages
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

  poolAssignmentMethods = [
    { value: ResourcePoolAssignmentMethod.Manual, label: 'Manual Selection' },
    { value: ResourcePoolAssignmentMethod.RoundRobin, label: 'Round Robin' },
    { value: ResourcePoolAssignmentMethod.LeastBusy, label: 'Least Busy' }
  ];

  // Data for dropdowns
  // Removed: users: User[] = [];  - using search autocomplete instead
  branches: Branch[] = [];
  departments: Department[] = [];
  resourcePools: ResourcePool[] = [];

  // Selected users for contacts
  selectedPrimaryContact: UserSearchResult | null = null;
  selectedSecondaryContact: UserSearchResult | null = null;
  selectedHRContact: UserSearchResult | null = null;

  constructor(
    private fb: FormBuilder,
    private escalationService: EscalationService,
    private authService: AuthService,
    private userService: UserService,
    private branchService: BranchService,
    private departmentService: DepartmentService,
    private resourcePoolService: ResourcePoolService,
    private router: Router
  ) {
    this.initializeForms();
    this.initializeTemplates();
  }

  ngOnInit(): void {
    // Removed loadUsers() - using search autocomplete instead
    this.loadBranches();
    this.loadDepartments();
    this.loadResourcePools();
  }

  // Removed loadUsers() - using search autocomplete instead for performance

  // User selection handlers for autocomplete
  onPrimaryContactSelected(user: UserSearchResult | null): void {
    this.selectedPrimaryContact = user;
    this.policyForm.patchValue({ primaryContactId: user?.id || '' });
  }

  onSecondaryContactSelected(user: UserSearchResult | null): void {
    this.selectedSecondaryContact = user;
    this.policyForm.patchValue({ secondaryContactId: user?.id || '' });
  }

  onHRContactSelected(user: UserSearchResult | null): void {
    this.selectedHRContact = user;
    this.policyForm.patchValue({ hrContactId: user?.id || '' });
  }

  loadBranches(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser?.companyId) {
      this.branchService.getBranches(currentUser.companyId, true).subscribe({
        next: (branches) => {
          this.branches = branches;
        },
        error: (error) => {
          console.error('Error loading branches:', error);
        }
      });
    }
  }

  loadDepartments(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser?.companyId) {
      this.departmentService.getDepartments(currentUser.companyId).subscribe({
        next: (departments) => {
          this.departments = departments;
        },
        error: (error) => {
          console.error('Error loading departments:', error);
        }
      });
    }
  }

  loadResourcePools(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser?.companyId) {
      this.resourcePoolService.getAllPools(currentUser.companyId).subscribe({
        next: (pools) => {
          this.resourcePools = pools.filter(p => p.isActive);
        },
        error: (error) => {
          console.error('Error loading resource pools:', error);
        }
      });
    }
  }

  initializeForms(): void {
    // Matrix Form
    this.matrixForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(500)],
      isDefault: [false],
      levels: this.fb.array([])
    });

    // Policy Form
    this.policyForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(1000)],
      scope: ['company'], // company, branch, department, section, category
      branchId: [''],
      departmentId: [''],
      sectionId: [''],
      categoryId: [''],
      enableAutoEscalation: [true],
      requireManualApproval: [false],
      minimumSeverityForAutoEscalation: [null],
      maxAutoEscalationLevels: [null],
      priority: [50, [Validators.min(0), Validators.max(100)]]
    });
  }

  initializeTemplates(): void {
    this.templates = [
      {
        id: 'fast',
        name: 'Fast Response',
        description: 'Quick escalation for urgent issues. Escalates within hours.',
        icon: 'fa-bolt',
        levels: [
          {
            level: 1,
            name: 'First Response',
            triggerAfterValue: 2,
            triggerTimeUnit: TimeUnit.Hours,
            assignmentStrategy: AssignmentStrategy.ReportingManager
          },
          {
            level: 2,
            name: 'Manager Review',
            triggerAfterValue: 4,
            triggerTimeUnit: TimeUnit.Hours,
            assignmentStrategy: AssignmentStrategy.SectionIncharge
          },
          {
            level: 3,
            name: 'Executive Attention',
            triggerAfterValue: 8,
            triggerTimeUnit: TimeUnit.Hours,
            assignmentStrategy: AssignmentStrategy.BranchContacts
          }
        ]
      },
      {
        id: 'standard',
        name: 'Standard Process',
        description: 'Balanced escalation timeline for most complaints. Escalates over days.',
        icon: 'fa-clock',
        levels: [
          {
            level: 1,
            name: 'Initial Assignment',
            triggerAfterValue: 24,
            triggerTimeUnit: TimeUnit.Hours,
            assignmentStrategy: AssignmentStrategy.ReportingManager
          },
          {
            level: 2,
            name: 'Team Lead Review',
            triggerAfterValue: 2,
            triggerTimeUnit: TimeUnit.Days,
            assignmentStrategy: AssignmentStrategy.SectionIncharge
          },
          {
            level: 3,
            name: 'Manager Escalation',
            triggerAfterValue: 5,
            triggerTimeUnit: TimeUnit.Days,
            assignmentStrategy: AssignmentStrategy.DepartmentContacts
          }
        ]
      },
      {
        id: 'slow',
        name: 'Extended Timeline',
        description: 'Slower escalation for low-priority items. Escalates over weeks.',
        icon: 'fa-hourglass-half',
        levels: [
          {
            level: 1,
            name: 'Queue Assignment',
            triggerAfterValue: 3,
            triggerTimeUnit: TimeUnit.Days,
            assignmentStrategy: AssignmentStrategy.ResourcePool
          },
          {
            level: 2,
            name: 'Supervisor Review',
            triggerAfterValue: 1,
            triggerTimeUnit: TimeUnit.Weeks,
            assignmentStrategy: AssignmentStrategy.SectionIncharge
          },
          {
            level: 3,
            name: 'Final Review',
            triggerAfterValue: 2,
            triggerTimeUnit: TimeUnit.Weeks,
            assignmentStrategy: AssignmentStrategy.AdminEscalation
          }
        ]
      },
      {
        id: 'custom',
        name: 'Custom Setup',
        description: 'Start from scratch and configure your own escalation levels.',
        icon: 'fa-cog',
        levels: [
          {
            level: 1,
            name: 'Level 1',
            triggerAfterValue: 24,
            triggerTimeUnit: TimeUnit.Hours,
            assignmentStrategy: AssignmentStrategy.ReportingManager
          }
        ]
      }
    ];
  }

  get levels(): FormArray {
    return this.matrixForm.get('levels') as FormArray;
  }

  // Step Navigation
  nextStep(): void {
    if (this.currentStep === 1 && !this.selectedTemplate) {
      this.errorMessage = 'Please select a template to continue';
      return;
    }

    if (this.currentStep === 2 && this.matrixForm.invalid) {
      this.errorMessage = 'Please fill in all required fields correctly';
      this.markFormGroupTouched(this.matrixForm);
      return;
    }

    if (this.currentStep === 3 && this.policyForm.invalid) {
      this.errorMessage = 'Please fill in all required fields correctly';
      this.markFormGroupTouched(this.policyForm);
      return;
    }

    this.errorMessage = '';

    if (this.currentStep === 1) {
      this.loadTemplateToForm();
    }

    if (this.currentStep === 3) {
      this.generatePreview();
    }

    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
      this.errorMessage = '';
    }
  }

  goToStep(step: number): void {
    if (step >= 1 && step <= this.currentStep) {
      this.currentStep = step;
      this.errorMessage = '';
    }
  }

  // Step 1: Template Selection
  selectTemplate(template: WizardTemplate): void {
    this.selectedTemplate = template;
    this.errorMessage = '';
  }

  // Step 2: Load Template to Form
  loadTemplateToForm(): void {
    if (!this.selectedTemplate) return;

    // Set default names based on template
    this.matrixForm.patchValue({
      name: `${this.selectedTemplate.name} Matrix`,
      description: this.selectedTemplate.description,
      isDefault: false
    });

    // Clear existing levels
    this.levels.clear();

    // Add levels from template
    this.selectedTemplate.levels.forEach(level => {
      this.levels.push(this.fb.group({
        level: [level.level, Validators.required],
        name: [level.name, [Validators.required, Validators.maxLength(100)]],
        description: ['', Validators.maxLength(500)],
        triggerAfterValue: [level.triggerAfterValue, [Validators.required, Validators.min(1)]],
        triggerTimeUnit: [level.triggerTimeUnit, Validators.required],
        assignmentStrategy: [level.assignmentStrategy, Validators.required],
        // Contact Hierarchy fields
        primaryContactId: [''],
        secondaryContactId: [''],
        hrContactId: [''],
        // Branch/Department fields
        branchId: [''],
        departmentId: [''],
        // Resource Pool fields
        resourcePoolId: [''],
        resourcePoolAssignmentMethod: [ResourcePoolAssignmentMethod.Manual],
        // Legacy fields
        assignToUserId: [''],
        assignToRole: [''],
        assignToUserIds: [''],
        sendNotification: [true],
        notifyPreviousHandler: [true],
        escalationMessage: ['']
      }));
    });

    // Set default policy name
    this.policyForm.patchValue({
      name: `${this.selectedTemplate.name} Policy`,
      description: `Auto-created policy for ${this.selectedTemplate.name.toLowerCase()} escalation`
    });
  }

  // Step 2: Level Management
  addLevel(): void {
    const levelNumber = this.levels.length + 1;
    this.levels.push(this.fb.group({
      level: [levelNumber, Validators.required],
      name: [`Level ${levelNumber}`, [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      triggerAfterValue: [24, [Validators.required, Validators.min(1)]],
      triggerTimeUnit: [TimeUnit.Hours, Validators.required],
      assignmentStrategy: [AssignmentStrategy.ReportingManager, Validators.required],
      // Contact Hierarchy fields
      primaryContactId: [''],
      secondaryContactId: [''],
      hrContactId: [''],
      // Branch/Department fields
      branchId: [''],
      departmentId: [''],
      // Resource Pool fields
      resourcePoolId: [''],
      resourcePoolAssignmentMethod: [ResourcePoolAssignmentMethod.Manual],
      // Legacy fields
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

  // Step 3: Scope handling
  onScopeChange(): void {
    const scope = this.policyForm.get('scope')?.value;

    // Clear fields based on scope
    this.policyForm.patchValue({
      branchId: '',
      departmentId: '',
      sectionId: '',
      categoryId: ''
    });
  }

  // Helper methods for dynamic form fields
  onAssignmentStrategyChange(levelIndex: number): void {
    const levelFormGroup = this.levels.at(levelIndex);
    const strategy = levelFormGroup.get('assignmentStrategy')?.value;

    // Clear all conditional fields when strategy changes
    levelFormGroup.patchValue({
      primaryContactId: '',
      secondaryContactId: '',
      hrContactId: '',
      branchId: '',
      departmentId: '',
      resourcePoolId: '',
      resourcePoolAssignmentMethod: ResourcePoolAssignmentMethod.Manual
    });
  }

  needsContactHierarchy(strategy: AssignmentStrategy): boolean {
    return strategy === AssignmentStrategy.SectionIncharge ||
           strategy === AssignmentStrategy.BranchContacts ||
           strategy === AssignmentStrategy.DepartmentContacts;
  }

  needsBranchSelector(strategy: AssignmentStrategy): boolean {
    return strategy === AssignmentStrategy.BranchContacts;
  }

  needsDepartmentSelector(strategy: AssignmentStrategy): boolean {
    return strategy === AssignmentStrategy.DepartmentContacts;
  }

  needsResourcePool(strategy: AssignmentStrategy): boolean {
    return strategy === AssignmentStrategy.ResourcePool;
  }

  getLevelStrategy(levelIndex: number): AssignmentStrategy {
    return this.levels.at(levelIndex).get('assignmentStrategy')?.value;
  }

  // Step 4: Generate Preview
  generatePreview(): void {
    this.previewData = {
      matrix: {
        name: this.matrixForm.value.name,
        description: this.matrixForm.value.description,
        isDefault: this.matrixForm.value.isDefault,
        levels: this.matrixForm.value.levels.map((level: any) => ({
          level: level.level,
          name: level.name,
          triggerDisplay: `${level.triggerAfterValue} ${this.getTimeUnitLabel(level.triggerTimeUnit)}`,
          assignmentDisplay: this.getAssignmentStrategyLabel(level.assignmentStrategy)
        }))
      },
      policy: {
        name: this.policyForm.value.name,
        description: this.policyForm.value.description,
        scope: this.policyForm.value.scope,
        enableAutoEscalation: this.policyForm.value.enableAutoEscalation,
        requireManualApproval: this.policyForm.value.requireManualApproval,
        priority: this.policyForm.value.priority
      }
    };
  }

  // Step 4: Final Creation
  createEscalation(): void {
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'User not authenticated. Please login again.';
      this.loading = false;
      return;
    }

    // Step 1: Create Matrix
    const matrixRequest: CreateEscalationMatrixRequest = {
      name: this.matrixForm.value.name,
      description: this.matrixForm.value.description,
      isDefault: this.matrixForm.value.isDefault,
      escalationLevels: this.matrixForm.value.levels.map((level: any) => ({
        level: level.level,
        name: level.name,
        description: level.description || undefined,
        triggerAfterValue: level.triggerAfterValue,
        triggerTimeUnit: level.triggerTimeUnit,
        assignmentStrategy: level.assignmentStrategy,
        // Contact Hierarchy fields
        primaryContactId: level.primaryContactId || undefined,
        secondaryContactId: level.secondaryContactId || undefined,
        hrContactId: level.hrContactId || undefined,
        // Branch/Department fields
        branchId: level.branchId || undefined,
        departmentId: level.departmentId || undefined,
        // Resource Pool fields
        resourcePoolId: level.resourcePoolId || undefined,
        resourcePoolAssignmentMethod: level.resourcePoolAssignmentMethod || undefined,
        // Legacy fields
        assignToUserId: level.assignToUserId || undefined,
        assignToRole: level.assignToRole || undefined,
        assignToUserIds: level.assignToUserIds || undefined,
        sendNotification: level.sendNotification,
        notifyPreviousHandler: level.notifyPreviousHandler,
        escalationMessage: level.escalationMessage || undefined
      }))
    };

    // Create matrix first
    this.escalationService.createMatrix(matrixRequest).subscribe({
      next: (matrixResponse) => {
        // Step 2: Create Policy linked to the matrix
        const policyRequest: CreateEscalationPolicyRequest = {
          companyId: currentUser.companyId,
          name: this.policyForm.value.name,
          description: this.policyForm.value.description,
          branchId: this.policyForm.value.scope === 'branch' ? this.policyForm.value.branchId : undefined,
          departmentId: this.policyForm.value.scope === 'department' ? this.policyForm.value.departmentId : undefined,
          sectionId: this.policyForm.value.scope === 'section' ? this.policyForm.value.sectionId : undefined,
          categoryId: this.policyForm.value.scope === 'category' ? this.policyForm.value.categoryId : undefined,
          enableAutoEscalation: this.policyForm.value.enableAutoEscalation,
          requireManualApproval: this.policyForm.value.requireManualApproval,
          defaultEscalationMatrixId: matrixResponse.id,
          minimumSeverityForAutoEscalation: this.policyForm.value.minimumSeverityForAutoEscalation || undefined,
          maxAutoEscalationLevels: this.policyForm.value.maxAutoEscalationLevels || undefined,
          priority: this.policyForm.value.priority
        };

        this.escalationService.createPolicy(policyRequest).subscribe({
          next: () => {
            this.successMessage = 'Escalation setup completed successfully!';
            this.loading = false;
            setTimeout(() => {
              this.router.navigate(['/admin/escalation-matrix']);
            }, 2000);
          },
          error: (error) => {
            this.errorMessage = error.error?.message || 'Failed to create escalation policy. Matrix was created successfully.';
            this.loading = false;
          }
        });
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to create escalation matrix. Please try again.';
        this.loading = false;
      }
    });
  }

  // Helper methods
  getTimeUnitLabel(unit: TimeUnit): string {
    return this.timeUnits.find(u => u.value === unit)?.label || 'Hours';
  }

  getAssignmentStrategyLabel(strategy: AssignmentStrategy): string {
    return this.assignmentStrategies.find(s => s.value === strategy)?.label || 'Unknown';
  }

  isLevelFieldInvalid(levelIndex: number, fieldName: string): boolean {
    const field = this.levels.at(levelIndex).get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  cancel(): void {
    if (confirm('Are you sure you want to cancel? All progress will be lost.')) {
      this.router.navigate(['/admin/escalation-matrix']);
    }
  }
}
