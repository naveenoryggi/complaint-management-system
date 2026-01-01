import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { ProjectService } from '../../../services/project.service';
import { CustomerService } from '../../../services/customer.service';
import {
  Project,
  ProjectSummary,
  CreateProjectRequest,
  UpdateProjectRequest,
  ProjectMilestone,
  CreateMilestoneRequest,
  UpdateMilestoneRequest,
  ProjectTask,
  CreateTaskRequest,
  UpdateTaskRequest,
  ProjectTeamMember,
  AddTeamMemberRequest,
  ProjectDocument,
  ProjectStatus,
  MilestoneStatus,
  ProjectTaskStatus,
  TaskPriority,
  DocumentCategory,
  ProjectStatusLabels,
  MilestoneStatusLabels,
  ProjectTaskStatusLabels,
  TaskPriorityLabels,
  DocumentCategoryLabels,
  ProjectStatusOptions,
  MilestoneStatusOptions,
  ProjectTaskStatusOptions,
  TaskPriorityOptions,
  DocumentCategoryOptions
} from '../../../models/project.model';

interface CustomerLookup {
  id: string;
  code: string;
  name: string;
}

@Component({
  selector: 'app-project-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './project-management.component.html',
  styleUrls: ['./project-management.component.scss']
})
export class ProjectManagementComponent implements OnInit, OnDestroy {
  projects: ProjectSummary[] = [];
  selectedProject: Project | null = null;
  loading = false;
  detailLoading = false;
  errorMessage = '';
  successMessage = '';
  searchTerm = '';

  Math = Math;

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalProjects = 0;
  totalPages = 1;

  // Filters
  filterStatus: ProjectStatus | null = null;
  filterCustomerId: string | null = null;

  // Customer lookup for filter and form
  customers: CustomerLookup[] = [];

  // Modal state
  showModal = false;
  isEditMode = false;
  activeTab: 'basic' | 'timeline' | 'milestones' | 'tasks' | 'team' | 'documents' = 'basic';

  // Project Form data
  projectForm: CreateProjectRequest = this.getEmptyProjectForm();

  // Sub-entity forms
  showMilestoneModal = false;
  isEditMilestone = false;
  milestoneForm: CreateMilestoneRequest = this.getEmptyMilestoneForm();
  editingMilestoneId: string | null = null;

  showTaskModal = false;
  isEditTask = false;
  taskForm: CreateTaskRequest = this.getEmptyTaskForm();
  editingTaskId: string | null = null;

  showTeamModal = false;
  teamForm: AddTeamMemberRequest = this.getEmptyTeamForm();

  // Enum options for templates
  projectStatuses = ProjectStatusOptions;
  milestoneStatuses = MilestoneStatusOptions;
  taskStatuses = ProjectTaskStatusOptions;
  taskPriorities = TaskPriorityOptions;
  documentCategories = DocumentCategoryOptions;

  // Status labels
  ProjectStatusLabels = ProjectStatusLabels;
  MilestoneStatusLabels = MilestoneStatusLabels;
  ProjectTaskStatusLabels = ProjectTaskStatusLabels;
  TaskPriorityLabels = TaskPriorityLabels;

  constructor(
    private projectService: ProjectService,
    private customerService: CustomerService
  ) {}

  ngOnInit(): void {
    this.loadProjects();
    this.loadCustomers();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.searchTerm = searchTerm;
      this.currentPage = 1;
      this.loadProjects();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProjects(): void {
    this.loading = true;
    this.errorMessage = '';

    this.projectService.getProjects({
      searchTerm: this.searchTerm || undefined,
      status: this.filterStatus ?? undefined,
      customerId: this.filterCustomerId ?? undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.projects = response.data.items;
          this.totalProjects = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        } else {
          this.errorMessage = response.message || 'Failed to load projects';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while loading projects';
        this.loading = false;
      }
    });
  }

  loadCustomers(): void {
    this.customerService.getCustomerLookup().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.customers = response.data;
        }
      },
      error: () => {
        console.error('Failed to load customers');
      }
    });
  }

  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadProjects();
  }

  clearFilters(): void {
    this.filterStatus = null;
    this.filterCustomerId = null;
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadProjects();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadProjects();
    }
  }

  get visiblePages(): number[] {
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

  // Project Detail
  viewProject(project: ProjectSummary): void {
    this.detailLoading = true;
    this.projectService.getProjectById(project.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.selectedProject = response.data;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load project details';
      }
    });
  }

  closeDetail(): void {
    this.selectedProject = null;
  }

  // Modal Operations
  openAddModal(): void {
    this.projectForm = this.getEmptyProjectForm();
    this.isEditMode = false;
    this.activeTab = 'basic';
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(project: ProjectSummary): void {
    this.detailLoading = true;
    this.projectService.getProjectById(project.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const p = response.data;
          this.projectForm = {
            projectCode: p.projectCode,
            name: p.name,
            description: p.description,
            status: p.status,
            customerId: p.customerId,
            plannedStartDate: p.plannedStartDate,
            plannedEndDate: p.plannedEndDate,
            budget: p.budget,
            currency: p.currency || 'INR',
            projectManagerId: p.projectManagerId,
            priority: p.priority,
            tags: p.tags,
            notes: p.notes,
            customFields: p.customFields
          };
          this.selectedProject = response.data;
          this.isEditMode = true;
          this.activeTab = 'basic';
          this.showModal = true;
        }
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        this.errorMessage = 'Failed to load project details';
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.projectForm = this.getEmptyProjectForm();
    this.errorMessage = '';
  }

  saveProject(): void {
    // Validate required fields
    if (!this.projectForm.projectCode?.trim()) {
      this.errorMessage = 'Project code is required';
      return;
    }
    if (!this.projectForm.name?.trim()) {
      this.errorMessage = 'Project name is required';
      return;
    }
    if (!this.projectForm.customerId) {
      this.errorMessage = 'Customer is required';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.selectedProject) {
      const updateRequest: UpdateProjectRequest = {
        name: this.projectForm.name,
        description: this.projectForm.description,
        status: this.projectForm.status || ProjectStatus.Draft,
        plannedStartDate: this.projectForm.plannedStartDate,
        plannedEndDate: this.projectForm.plannedEndDate,
        actualStartDate: this.selectedProject.actualStartDate,
        actualEndDate: this.selectedProject.actualEndDate,
        progressPercentage: this.selectedProject.progressPercentage,
        budget: this.projectForm.budget,
        actualCost: this.selectedProject.actualCost,
        currency: this.projectForm.currency || 'INR',
        projectManagerId: this.projectForm.projectManagerId,
        priority: this.projectForm.priority,
        tags: this.projectForm.tags,
        notes: this.projectForm.notes,
        customFields: this.projectForm.customFields
      };

      this.projectService.updateProject(this.selectedProject.id, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Project updated successfully';
            this.closeModal();
            this.loadProjects();
          } else {
            this.errorMessage = response.message || 'Failed to update project';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.projectService.createProject(this.projectForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Project created successfully';
            this.closeModal();
            this.loadProjects();
          } else {
            this.errorMessage = response.message || 'Failed to create project';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  deleteProject(project: ProjectSummary): void {
    if (confirm(`Are you sure you want to delete project "${project.name}"?`)) {
      this.loading = true;
      this.projectService.deleteProject(project.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Project deleted successfully';
            this.loadProjects();
          } else {
            this.errorMessage = response.message || 'Failed to delete project';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  // ============ Milestones ============

  openAddMilestoneModal(): void {
    if (!this.selectedProject) return;
    this.milestoneForm = this.getEmptyMilestoneForm();
    this.isEditMilestone = false;
    this.editingMilestoneId = null;
    this.showMilestoneModal = true;
  }

  openEditMilestoneModal(milestone: ProjectMilestone): void {
    this.milestoneForm = {
      name: milestone.name,
      description: milestone.description,
      dueDate: milestone.dueDate,
      sortOrder: milestone.sortOrder,
      notes: milestone.notes
    };
    this.isEditMilestone = true;
    this.editingMilestoneId = milestone.id;
    this.showMilestoneModal = true;
  }

  closeMilestoneModal(): void {
    this.showMilestoneModal = false;
    this.milestoneForm = this.getEmptyMilestoneForm();
    this.editingMilestoneId = null;
  }

  saveMilestone(): void {
    if (!this.selectedProject) return;

    if (!this.milestoneForm.name?.trim()) {
      this.errorMessage = 'Milestone name is required';
      return;
    }

    this.loading = true;

    if (this.isEditMilestone && this.editingMilestoneId) {
      const updateRequest: UpdateMilestoneRequest = {
        name: this.milestoneForm.name,
        description: this.milestoneForm.description,
        status: MilestoneStatus.Pending, // Keep existing status when editing
        dueDate: this.milestoneForm.dueDate,
        sortOrder: this.milestoneForm.sortOrder || 0,
        notes: this.milestoneForm.notes
      };

      this.projectService.updateMilestone(this.editingMilestoneId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Milestone updated successfully';
            this.closeMilestoneModal();
            this.refreshProjectDetail();
          } else {
            this.errorMessage = response.message || 'Failed to update milestone';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.projectService.createMilestone(this.selectedProject.id, this.milestoneForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Milestone created successfully';
            this.closeMilestoneModal();
            this.refreshProjectDetail();
          } else {
            this.errorMessage = response.message || 'Failed to create milestone';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  deleteMilestone(milestone: ProjectMilestone): void {
    if (confirm(`Are you sure you want to delete milestone "${milestone.name}"?`)) {
      this.loading = true;
      this.projectService.deleteMilestone(milestone.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Milestone deleted successfully';
            this.refreshProjectDetail();
          } else {
            this.errorMessage = response.message || 'Failed to delete milestone';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  // ============ Tasks ============

  openAddTaskModal(): void {
    if (!this.selectedProject) return;
    this.taskForm = this.getEmptyTaskForm();
    this.isEditTask = false;
    this.editingTaskId = null;
    this.showTaskModal = true;
  }

  openEditTaskModal(task: ProjectTask): void {
    this.taskForm = {
      milestoneId: task.milestoneId,
      title: task.title,
      description: task.description,
      priority: task.priority,
      assigneeId: task.assigneeId,
      dueDate: task.dueDate,
      estimatedHours: task.estimatedHours,
      tags: task.tags,
      notes: task.notes
    };
    this.isEditTask = true;
    this.editingTaskId = task.id;
    this.showTaskModal = true;
  }

  closeTaskModal(): void {
    this.showTaskModal = false;
    this.taskForm = this.getEmptyTaskForm();
    this.editingTaskId = null;
  }

  saveTask(): void {
    if (!this.selectedProject) return;

    if (!this.taskForm.title?.trim()) {
      this.errorMessage = 'Task title is required';
      return;
    }

    this.loading = true;

    if (this.isEditTask && this.editingTaskId) {
      const updateRequest: UpdateTaskRequest = {
        milestoneId: this.taskForm.milestoneId,
        title: this.taskForm.title,
        description: this.taskForm.description,
        status: ProjectTaskStatus.ToDo,
        priority: this.taskForm.priority || TaskPriority.Medium,
        assigneeId: this.taskForm.assigneeId,
        dueDate: this.taskForm.dueDate,
        estimatedHours: this.taskForm.estimatedHours,
        tags: this.taskForm.tags,
        notes: this.taskForm.notes
      };

      this.projectService.updateTask(this.editingTaskId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Task updated successfully';
            this.closeTaskModal();
            this.refreshProjectDetail();
          } else {
            this.errorMessage = response.message || 'Failed to update task';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    } else {
      this.projectService.createTask(this.selectedProject.id, this.taskForm).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Task created successfully';
            this.closeTaskModal();
            this.refreshProjectDetail();
          } else {
            this.errorMessage = response.message || 'Failed to create task';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  deleteTask(task: ProjectTask): void {
    if (confirm(`Are you sure you want to delete task "${task.title}"?`)) {
      this.loading = true;
      this.projectService.deleteTask(task.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Task deleted successfully';
            this.refreshProjectDetail();
          } else {
            this.errorMessage = response.message || 'Failed to delete task';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  // ============ Team Members ============

  openAddTeamModal(): void {
    if (!this.selectedProject) return;
    this.teamForm = this.getEmptyTeamForm();
    this.showTeamModal = true;
  }

  closeTeamModal(): void {
    this.showTeamModal = false;
    this.teamForm = this.getEmptyTeamForm();
  }

  saveTeamMember(): void {
    if (!this.selectedProject) return;

    if (!this.teamForm.employeeId) {
      this.errorMessage = 'Employee is required';
      return;
    }

    this.loading = true;

    this.projectService.addTeamMember(this.selectedProject.id, this.teamForm).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Team member added successfully';
          this.closeTeamModal();
          this.refreshProjectDetail();
        } else {
          this.errorMessage = response.message || 'Failed to add team member';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred';
        this.loading = false;
      }
    });
  }

  removeTeamMember(member: ProjectTeamMember): void {
    if (confirm(`Are you sure you want to remove "${member.employeeName}" from the project?`)) {
      this.loading = true;
      this.projectService.removeTeamMember(member.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Team member removed successfully';
            this.refreshProjectDetail();
          } else {
            this.errorMessage = response.message || 'Failed to remove team member';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'An error occurred';
          this.loading = false;
        }
      });
    }
  }

  // ============ Helper Methods ============

  private refreshProjectDetail(): void {
    if (this.selectedProject) {
      this.projectService.getProjectById(this.selectedProject.id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.selectedProject = response.data;
          }
        }
      });
    }
  }

  private getEmptyProjectForm(): CreateProjectRequest {
    return {
      projectCode: '',
      name: '',
      description: '',
      status: ProjectStatus.Draft,
      customerId: '',
      currency: 'INR'
    };
  }

  private getEmptyMilestoneForm(): CreateMilestoneRequest {
    return {
      name: '',
      description: '',
      dueDate: new Date(),
      sortOrder: 0,
      notes: ''
    };
  }

  private getEmptyTaskForm(): CreateTaskRequest {
    return {
      title: '',
      description: '',
      priority: TaskPriority.Medium
    };
  }

  private getEmptyTeamForm(): AddTeamMemberRequest {
    return {
      employeeId: '',
      role: '',
      allocationPercentage: 100
    };
  }

  getStatusClass(status: ProjectStatus): string {
    switch (status) {
      case ProjectStatus.Draft: return 'status-draft';
      case ProjectStatus.Planning: return 'status-planning';
      case ProjectStatus.InProgress: return 'status-inprogress';
      case ProjectStatus.OnHold: return 'status-onhold';
      case ProjectStatus.Completed: return 'status-completed';
      case ProjectStatus.Cancelled: return 'status-cancelled';
      default: return '';
    }
  }

  getMilestoneStatusClass(status: MilestoneStatus): string {
    switch (status) {
      case MilestoneStatus.Pending: return 'status-pending';
      case MilestoneStatus.InProgress: return 'status-inprogress';
      case MilestoneStatus.Completed: return 'status-completed';
      case MilestoneStatus.Delayed: return 'status-delayed';
      default: return '';
    }
  }

  getTaskStatusClass(status: ProjectTaskStatus): string {
    switch (status) {
      case ProjectTaskStatus.ToDo: return 'status-todo';
      case ProjectTaskStatus.InProgress: return 'status-inprogress';
      case ProjectTaskStatus.InReview: return 'status-review';
      case ProjectTaskStatus.Completed: return 'status-completed';
      case ProjectTaskStatus.Blocked: return 'status-blocked';
      default: return '';
    }
  }

  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'priority-low';
      case TaskPriority.Medium: return 'priority-medium';
      case TaskPriority.High: return 'priority-high';
      case TaskPriority.Critical: return 'priority-critical';
      default: return '';
    }
  }

  dismissSuccess(): void {
    this.successMessage = '';
  }

  dismissError(): void {
    this.errorMessage = '';
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  formatCurrency(amount: number | undefined): string {
    if (amount === undefined || amount === null) return '-';
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(amount);
  }
}
