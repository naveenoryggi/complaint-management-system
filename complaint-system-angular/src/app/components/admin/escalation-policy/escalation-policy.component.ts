import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EscalationService } from '../../../services/escalation.service';
import { AuthService } from '../../../services/auth.service';
import {
  EscalationPolicy,
  CreateEscalationPolicyRequest,
  UpdateEscalationPolicyRequest,
  EscalationMatrix,
  PolicyResolutionTestRequest,
  PolicyResolutionResult
} from '../../../models/escalation.model';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

interface OrganizationalUnit {
  id: string;
  name: string;
}

@Component({
  selector: 'app-escalation-policy',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, UtcToLocalPipe],
  templateUrl: './escalation-policy.component.html',
  styleUrls: ['./escalation-policy.component.scss']
})
export class EscalationPolicyComponent implements OnInit {
  policies: EscalationPolicy[] = [];
  matrices: EscalationMatrix[] = [];
  selectedPolicy: EscalationPolicy | null = null;
  policyForm!: FormGroup;
  testForm!: FormGroup;

  showForm = false;
  showTestPanel = false;
  isEditMode = false;
  loading = false;
  testLoading = false;
  errorMessage = '';
  successMessage = '';

  testResult: PolicyResolutionResult | null = null;

  // Mock data - in real app, fetch from API
  branches: OrganizationalUnit[] = [];
  departments: OrganizationalUnit[] = [];
  sections: OrganizationalUnit[] = [];
  categories: OrganizationalUnit[] = [];

  severityLevels = [
    { value: 1, label: 'Low' },
    { value: 2, label: 'Medium' },
    { value: 3, label: 'High' },
    { value: 4, label: 'Critical' }
  ];

  // For filtering and grouping
  selectedViewFilter: 'all' | 'active' | 'inactive' = 'all';
  selectedScopeFilter: 'all' | 'company' | 'branch' | 'department' | 'section' | 'category' = 'all';

  constructor(
    private escalationService: EscalationService,
    private authService: AuthService,
    private fb: FormBuilder
  ) {
    this.initForms();
  }

  ngOnInit(): void {
    this.loadPolicies();
    this.loadMatrices();
    this.loadOrganizationalUnits();
  }

  initForms(): void {
    this.policyForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(1000)],
      branchId: [''],
      departmentId: [''],
      sectionId: [''],
      categoryId: [''],
      enableAutoEscalation: [true],
      requireManualApproval: [false],
      defaultEscalationMatrixId: [''],
      minimumSeverityForAutoEscalation: [''],
      maxAutoEscalationLevels: [''],
      priority: [0, [Validators.min(0), Validators.max(100)]],
      effectiveFrom: [''],
      effectiveTo: ['']
    });

    this.testForm = this.fb.group({
      branchId: [''],
      departmentId: [''],
      sectionId: [''],
      categoryId: ['']
    });
  }

  loadPolicies(): void {
    this.loading = true;
    this.errorMessage = '';

    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'User not authenticated. Please login again.';
      this.loading = false;
      return;
    }

    this.escalationService.getPolicies(currentUser.companyId).subscribe({
      next: (data) => {
        this.policies = data.sort((a, b) => b.specificityScore - a.specificityScore);
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load escalation policies. Please try again.';
        this.loading = false;
        console.error('Error loading policies:', error);
      }
    });
  }

  loadMatrices(): void {
    this.escalationService.getMatrices().subscribe({
      next: (data) => {
        this.matrices = data.filter(m => m.isActive);
      },
      error: (error) => {
        console.error('Error loading matrices:', error);
      }
    });
  }

  loadOrganizationalUnits(): void {
    // Mock data - in real app, fetch from API
    this.branches = [
      { id: '1', name: 'Main Branch' },
      { id: '2', name: 'North Branch' }
    ];
    this.departments = [
      { id: '1', name: 'IT Department' },
      { id: '2', name: 'HR Department' },
      { id: '3', name: 'Finance Department' }
    ];
    this.sections = [
      { id: '1', name: 'Development' },
      { id: '2', name: 'Support' },
      { id: '3', name: 'Operations' }
    ];
    this.categories = [
      { id: '1', name: 'Technical Issue' },
      { id: '2', name: 'Service Request' },
      { id: '3', name: 'Bug Report' },
      { id: '4', name: 'Feature Request' }
    ];
  }

  get filteredPolicies(): EscalationPolicy[] {
    return this.policies.filter(policy => {
      // View filter
      if (this.selectedViewFilter === 'active' && !policy.isActive) return false;
      if (this.selectedViewFilter === 'inactive' && policy.isActive) return false;

      // Scope filter
      if (this.selectedScopeFilter === 'company' &&
          (policy.branchId || policy.departmentId || policy.sectionId || policy.categoryId)) return false;
      if (this.selectedScopeFilter === 'branch' && !policy.branchId) return false;
      if (this.selectedScopeFilter === 'department' && !policy.departmentId) return false;
      if (this.selectedScopeFilter === 'section' && !policy.sectionId) return false;
      if (this.selectedScopeFilter === 'category' && !policy.categoryId) return false;

      return true;
    });
  }

  get groupedPolicies(): { [key: string]: EscalationPolicy[] } {
    const grouped: { [key: string]: EscalationPolicy[] } = {
      'Company-wide': [],
      'Branch-specific': [],
      'Department-specific': [],
      'Section-specific': [],
      'Category-specific': []
    };

    this.filteredPolicies.forEach(policy => {
      if (policy.categoryId) {
        grouped['Category-specific'].push(policy);
      } else if (policy.sectionId) {
        grouped['Section-specific'].push(policy);
      } else if (policy.departmentId) {
        grouped['Department-specific'].push(policy);
      } else if (policy.branchId) {
        grouped['Branch-specific'].push(policy);
      } else {
        grouped['Company-wide'].push(policy);
      }
    });

    // Remove empty groups
    Object.keys(grouped).forEach(key => {
      if (grouped[key].length === 0) {
        delete grouped[key];
      }
    });

    return grouped;
  }

  createNewPolicy(): void {
    this.showForm = true;
    this.isEditMode = false;
    this.selectedPolicy = null;
    this.initForms();
  }

  editPolicy(policy: EscalationPolicy): void {
    this.showForm = true;
    this.isEditMode = true;
    this.selectedPolicy = policy;

    this.policyForm.patchValue({
      name: policy.name,
      description: policy.description,
      branchId: policy.branchId || '',
      departmentId: policy.departmentId || '',
      sectionId: policy.sectionId || '',
      categoryId: policy.categoryId || '',
      enableAutoEscalation: policy.enableAutoEscalation,
      requireManualApproval: policy.requireManualApproval,
      defaultEscalationMatrixId: policy.defaultEscalationMatrixId || '',
      minimumSeverityForAutoEscalation: policy.minimumSeverityForAutoEscalation || '',
      maxAutoEscalationLevels: policy.maxAutoEscalationLevels || '',
      priority: policy.priority,
      effectiveFrom: policy.effectiveFrom ? new Date(policy.effectiveFrom).toISOString().slice(0, 16) : '',
      effectiveTo: policy.effectiveTo ? new Date(policy.effectiveTo).toISOString().slice(0, 16) : ''
    });
  }

  cancelForm(): void {
    this.showForm = false;
    this.selectedPolicy = null;
    this.initForms();
    this.errorMessage = '';
    this.successMessage = '';
  }

  savePolicy(): void {
    if (this.policyForm.invalid) {
      this.errorMessage = 'Please fill in all required fields correctly.';
      Object.keys(this.policyForm.controls).forEach(key => {
        const control = this.policyForm.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'User not authenticated. Please login again.';
      this.loading = false;
      return;
    }

    const formValue = this.policyForm.value;

    if (this.isEditMode && this.selectedPolicy) {
      const updateRequest: UpdateEscalationPolicyRequest = {
        name: formValue.name,
        description: formValue.description,
        enableAutoEscalation: formValue.enableAutoEscalation,
        requireManualApproval: formValue.requireManualApproval,
        defaultEscalationMatrixId: formValue.defaultEscalationMatrixId || undefined,
        minimumSeverityForAutoEscalation: formValue.minimumSeverityForAutoEscalation || undefined,
        maxAutoEscalationLevels: formValue.maxAutoEscalationLevels || undefined,
        priority: formValue.priority,
        isActive: true,
        effectiveFrom: formValue.effectiveFrom ? new Date(formValue.effectiveFrom) : undefined,
        effectiveTo: formValue.effectiveTo ? new Date(formValue.effectiveTo) : undefined
      };

      this.escalationService.updatePolicy(this.selectedPolicy.id, updateRequest).subscribe({
        next: () => {
          this.successMessage = 'Escalation policy updated successfully!';
          this.loadPolicies();
          setTimeout(() => this.cancelForm(), 2000);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update escalation policy.';
          this.loading = false;
        }
      });
    } else {
      const createRequest: CreateEscalationPolicyRequest = {
        companyId: currentUser.companyId,
        name: formValue.name,
        description: formValue.description,
        branchId: formValue.branchId || undefined,
        departmentId: formValue.departmentId || undefined,
        sectionId: formValue.sectionId || undefined,
        categoryId: formValue.categoryId || undefined,
        enableAutoEscalation: formValue.enableAutoEscalation,
        requireManualApproval: formValue.requireManualApproval,
        defaultEscalationMatrixId: formValue.defaultEscalationMatrixId || undefined,
        minimumSeverityForAutoEscalation: formValue.minimumSeverityForAutoEscalation || undefined,
        maxAutoEscalationLevels: formValue.maxAutoEscalationLevels || undefined,
        priority: formValue.priority,
        effectiveFrom: formValue.effectiveFrom ? new Date(formValue.effectiveFrom) : undefined,
        effectiveTo: formValue.effectiveTo ? new Date(formValue.effectiveTo) : undefined
      };

      this.escalationService.createPolicy(createRequest).subscribe({
        next: () => {
          this.successMessage = 'Escalation policy created successfully!';
          this.loadPolicies();
          setTimeout(() => this.cancelForm(), 2000);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create escalation policy.';
          this.loading = false;
        }
      });
    }
  }

  deletePolicy(policy: EscalationPolicy): void {
    if (!confirm(`Are you sure you want to delete "${policy.name}"? This action cannot be undone.`)) {
      return;
    }

    this.loading = true;
    this.escalationService.deletePolicy(policy.id).subscribe({
      next: () => {
        this.successMessage = 'Escalation policy deleted successfully!';
        this.loadPolicies();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete escalation policy.';
        this.loading = false;
      }
    });
  }

  toggleTestPanel(): void {
    this.showTestPanel = !this.showTestPanel;
    this.testResult = null;
  }

  testPolicyResolution(): void {
    this.testLoading = true;

    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'User not authenticated. Please login again.';
      this.testLoading = false;
      return;
    }

    const formValue = this.testForm.value;

    const testRequest: PolicyResolutionTestRequest = {
      companyId: currentUser.companyId,
      branchId: formValue.branchId || undefined,
      departmentId: formValue.departmentId || undefined,
      sectionId: formValue.sectionId || undefined,
      categoryId: formValue.categoryId || undefined
    };

    this.escalationService.testPolicyResolution(testRequest).subscribe({
      next: (result) => {
        this.testResult = result;
        this.testLoading = false;
      },
      error: (error) => {
        console.error('Error testing policy resolution:', error);
        this.testLoading = false;
      }
    });
  }

  getScopeIcon(policy: EscalationPolicy): string {
    if (policy.categoryId) return 'fa-tag';
    if (policy.sectionId) return 'fa-sitemap';
    if (policy.departmentId) return 'fa-building';
    if (policy.branchId) return 'fa-map-marker-alt';
    return 'fa-globe';
  }

  getScopeColor(policy: EscalationPolicy): string {
    if (policy.categoryId) return 'purple';
    if (policy.sectionId) return 'orange';
    if (policy.departmentId) return 'blue';
    if (policy.branchId) return 'green';
    return 'gray';
  }

  getSeverityLabel(level: number): string {
    return this.severityLevels.find(s => s.value === level)?.label || 'Any';
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.policyForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  // TrackBy functions for *ngFor optimization
  trackByPolicyId(index: number, policy: EscalationPolicy): string {
    return policy.id;
  }

  trackByBranchId(index: number, branch: OrganizationalUnit): string {
    return branch.id;
  }

  trackByDepartmentId(index: number, dept: OrganizationalUnit): string {
    return dept.id;
  }

  trackBySectionId(index: number, section: OrganizationalUnit): string {
    return section.id;
  }

  trackByCategoryId(index: number, category: OrganizationalUnit): string {
    return category.id;
  }

  trackByMatrixId(index: number, matrix: EscalationMatrix): string {
    return matrix.id;
  }

  trackBySeverity(index: number, severity: any): number {
    return severity.value;
  }

  trackByGroupKey(index: number, item: any): string {
    return item.key;
  }
}
