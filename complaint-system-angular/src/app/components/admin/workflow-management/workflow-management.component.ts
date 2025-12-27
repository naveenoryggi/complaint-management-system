import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { WorkflowService } from '../../../services/workflow.service';
import { AuthService } from '../../../services/auth.service';
import { CategoryService } from '../../../services/category.service';
import { StatusMasterService } from '../../../services/status-master.service';
import { CategoryWorkflow, CreateWorkflowRequest, AddStatusRequest, AddTransitionRequest, CategoryWorkflowStatus, CategoryWorkflowTransition } from '../../../models/workflow.model';

@Component({
  selector: 'app-workflow-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './workflow-management.component.html',
  styleUrls: ['./workflow-management.component.scss']
})
export class WorkflowManagementComponent implements OnInit {
  @ViewChild('workflowsGrid') workflowsGrid!: ElementRef;

  workflows: CategoryWorkflow[] = [];
  categories: any[] = [];
  statusMasters: any[] = [];
  roles: any[] = [];

  selectedWorkflow: CategoryWorkflow | null = null;
  showCreateModal = false;
  showStatusModal = false;
  showTransitionModal = false;

  workflowForm: FormGroup;
  statusForm: FormGroup;
  transitionForm: FormGroup;

  loading = false;
  error: string = '';
  success: string = '';

  companyId: string = '';

  constructor(
    private workflowService: WorkflowService,
    private authService: AuthService,
    private categoryService: CategoryService,
    private statusMasterService: StatusMasterService,
    private fb: FormBuilder
  ) {
    this.workflowForm = this.fb.group({
      categoryId: ['', Validators.required],
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      isActive: [true],
      isDefault: [true]
    });

    this.statusForm = this.fb.group({
      statusMasterId: ['', Validators.required],
      displayOrder: [0, [Validators.required, Validators.min(0)]],
      isInitialStatus: [false],
      defaultSLAHours: [null],
      escalationHours: [null],
      requiresApproval: [false]
    });

    this.transitionForm = this.fb.group({
      fromStatusId: ['', Validators.required],
      toStatusId: ['', Validators.required],
      transitionName: ['', Validators.required],
      description: [''],
      requiresComment: [false],
      requiresApproval: [false],
      displayOrder: [0],
      buttonColor: ['#17a2b8'],
      iconClass: ['bi-arrow-right']
    });
  }

  ngOnInit(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser) {
      this.companyId = currentUser.companyId;
    }
    this.loadWorkflows();
    this.loadCategories();
    this.loadStatusMasters();
  }

  loadWorkflows(): void {
    this.loading = true;
    this.error = '';

    this.workflowService.getAllWorkflows(this.companyId).subscribe({
      next: (response) => {
        this.workflows = response.data || [];
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load workflows';
        this.loading = false;
        console.error('Error loading workflows:', error);
      }
    });
  }

  loadCategories(): void {
    this.categoryService.getCategories(true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.categories = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.error = 'Failed to load categories';
      }
    });
  }

  loadStatusMasters(): void {
    this.statusMasterService.getStatuses(this.companyId, true, true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.statusMasters = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading status masters:', error);
        this.error = 'Failed to load status masters';
      }
    });
  }

  // Workflow Card Scroll
  scrollWorkflows(direction: 'left' | 'right'): void {
    if (this.workflowsGrid) {
      const scrollAmount = 300;
      const container = this.workflowsGrid.nativeElement;
      if (direction === 'left') {
        container.scrollLeft -= scrollAmount;
      } else {
        container.scrollLeft += scrollAmount;
      }
    }
  }

  openCreateModal(): void {
    this.workflowForm.reset({
      isActive: true,
      isDefault: true,
      displayOrder: 0
    });
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.workflowForm.reset();
  }

  createWorkflow(): void {
    if (this.workflowForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';

    const request: CreateWorkflowRequest = {
      ...this.workflowForm.value,
      companyId: this.companyId
    };

    this.workflowService.createWorkflow(request).subscribe({
      next: (response) => {
        this.success = 'Workflow created successfully';
        this.closeCreateModal();
        this.loadWorkflows();
        this.loading = false;
        setTimeout(() => this.success = '', 3000);
      },
      error: (error) => {
        this.error = 'Failed to create workflow';
        this.loading = false;
        console.error('Error creating workflow:', error);
      }
    });
  }

  selectWorkflow(workflow: CategoryWorkflow): void {
    this.selectedWorkflow = workflow;
  }

  openEditWorkflowModal(workflow?: CategoryWorkflow): void {
    const wf = workflow || this.selectedWorkflow;
    if (!wf) {
      this.error = 'Please select a workflow first';
      return;
    }
    this.selectedWorkflow = wf;
    this.workflowForm.patchValue({
      categoryId: wf.categoryId,
      name: wf.name,
      description: wf.description,
      isActive: wf.isActive,
      isDefault: wf.isDefault
    });
    this.showCreateModal = true;
  }

  openStatusModal(): void {
    if (!this.selectedWorkflow) {
      this.error = 'Please select a workflow first';
      return;
    }
    this.statusForm.reset({
      displayOrder: this.selectedWorkflow.workflowStatuses.length,
      isInitialStatus: this.selectedWorkflow.workflowStatuses.length === 0,
      requiresApproval: false
    });
    this.showStatusModal = true;
  }

  closeStatusModal(): void {
    this.showStatusModal = false;
    this.statusForm.reset();
  }

  addStatus(): void {
    if (this.statusForm.invalid || !this.selectedWorkflow) {
      return;
    }

    this.loading = true;
    this.error = '';

    const request: AddStatusRequest = {
      workflowId: this.selectedWorkflow.id,
      ...this.statusForm.value
    };

    this.workflowService.addStatusToWorkflow(this.selectedWorkflow.id, request).subscribe({
      next: (response) => {
        this.success = 'Status added successfully';
        this.closeStatusModal();
        this.loadWorkflows();
        this.loading = false;
        setTimeout(() => this.success = '', 3000);
      },
      error: (error) => {
        this.error = 'Failed to add status';
        this.loading = false;
        console.error('Error adding status:', error);
      }
    });
  }

  editStatus(status: CategoryWorkflowStatus): void {
    if (!this.selectedWorkflow) {
      this.error = 'Please select a workflow first';
      return;
    }
    this.statusForm.patchValue({
      statusMasterId: status.statusMasterId,
      displayOrder: status.displayOrder,
      isInitialStatus: status.isInitialStatus,
      defaultSLAHours: status.defaultSLAHours,
      escalationHours: status.escalationHours,
      requiresApproval: status.requiresApproval
    });
    this.showStatusModal = true;
  }

  confirmDeleteStatus(status: CategoryWorkflowStatus): void {
    if (!this.selectedWorkflow) {
      this.error = 'Please select a workflow first';
      return;
    }
    if (confirm(`Are you sure you want to delete the status "${status.statusName}"?`)) {
      this.loading = true;
      this.workflowService.removeStatusFromWorkflow(this.selectedWorkflow.id, status.id).subscribe({
        next: () => {
          this.success = 'Status removed successfully';
          this.loadWorkflows();
          this.loading = false;
          setTimeout(() => this.success = '', 3000);
        },
        error: (error) => {
          this.error = 'Failed to remove status';
          this.loading = false;
          console.error('Error removing status:', error);
        }
      });
    }
  }

  openTransitionModal(): void {
    if (!this.selectedWorkflow) {
      this.error = 'Please select a workflow first';
      return;
    }
    if (this.selectedWorkflow.workflowStatuses.length < 2) {
      this.error = 'Workflow must have at least 2 statuses to create transitions';
      return;
    }
    this.transitionForm.reset({
      requiresComment: false,
      requiresApproval: false,
      displayOrder: this.selectedWorkflow.transitions.length,
      buttonColor: '#17a2b8',
      iconClass: 'bi-arrow-right'
    });
    this.showTransitionModal = true;
  }

  closeTransitionModal(): void {
    this.showTransitionModal = false;
    this.transitionForm.reset();
  }

  addTransition(): void {
    if (this.transitionForm.invalid || !this.selectedWorkflow) {
      return;
    }

    this.loading = true;
    this.error = '';

    const request: AddTransitionRequest = {
      workflowId: this.selectedWorkflow.id,
      ...this.transitionForm.value
    };

    this.workflowService.addTransitionRule(this.selectedWorkflow.id, request).subscribe({
      next: (response) => {
        this.success = 'Transition added successfully';
        this.closeTransitionModal();
        this.loadWorkflows();
        this.loading = false;
        setTimeout(() => this.success = '', 3000);
      },
      error: (error) => {
        this.error = 'Failed to add transition';
        this.loading = false;
        console.error('Error adding transition:', error);
      }
    });
  }

  editTransition(transition: CategoryWorkflowTransition): void {
    if (!this.selectedWorkflow) {
      this.error = 'Please select a workflow first';
      return;
    }
    this.transitionForm.patchValue({
      fromStatusId: transition.fromStatusId,
      toStatusId: transition.toStatusId,
      transitionName: transition.transitionName,
      description: transition.description,
      requiresComment: transition.requiresComment,
      requiresApproval: transition.requiresApproval,
      displayOrder: transition.displayOrder,
      buttonColor: transition.buttonColor,
      iconClass: transition.iconClass
    });
    this.showTransitionModal = true;
  }

  confirmDeleteTransition(transition: CategoryWorkflowTransition): void {
    if (!this.selectedWorkflow) {
      this.error = 'Please select a workflow first';
      return;
    }
    if (confirm(`Are you sure you want to delete the transition "${transition.transitionName}"?`)) {
      this.loading = true;
      this.workflowService.removeTransitionRule(this.selectedWorkflow.id, transition.id).subscribe({
        next: () => {
          this.success = 'Transition removed successfully';
          this.loadWorkflows();
          this.loading = false;
          setTimeout(() => this.success = '', 3000);
        },
        error: (error) => {
          this.error = 'Failed to remove transition';
          this.loading = false;
          console.error('Error removing transition:', error);
        }
      });
    }
  }

  getStatusName(statusId: string): string {
    if (!this.selectedWorkflow) return '';
    const status = this.selectedWorkflow.workflowStatuses.find(s => s.statusMasterId === statusId);
    return status ? status.statusName : '';
  }

  getCategoryName(categoryId: string): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category ? category.name : 'Unknown Category';
  }

  getTransitionFromStatusName(transition: CategoryWorkflowTransition): string {
    if (!this.selectedWorkflow) return '';
    const status = this.selectedWorkflow.workflowStatuses.find(s => s.id === transition.fromStatusId || s.statusMasterId === transition.fromStatusId);
    return status ? status.statusName : '';
  }

  getTransitionToStatusName(transition: CategoryWorkflowTransition): string {
    if (!this.selectedWorkflow) return '';
    const status = this.selectedWorkflow.workflowStatuses.find(s => s.id === transition.toStatusId || s.statusMasterId === transition.toStatusId);
    return status ? status.statusName : '';
  }

  getTransitionFromStatusColor(transition: CategoryWorkflowTransition): string {
    if (!this.selectedWorkflow) return '#6B7280';
    const status = this.selectedWorkflow.workflowStatuses.find(s => s.id === transition.fromStatusId || s.statusMasterId === transition.fromStatusId);
    return status?.statusColorCode || '#6B7280';
  }

  getTransitionToStatusColor(transition: CategoryWorkflowTransition): string {
    if (!this.selectedWorkflow) return '#3B82F6';
    const status = this.selectedWorkflow.workflowStatuses.find(s => s.id === transition.toStatusId || s.statusMasterId === transition.toStatusId);
    return status?.statusColorCode || '#3B82F6';
  }
}
