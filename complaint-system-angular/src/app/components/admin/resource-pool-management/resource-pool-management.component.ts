import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ResourcePoolService } from '../../../services/resource-pool.service';
import { UserService } from '../../../services/user.service';
import { BranchService } from '../../../services/branch.service';
import { DepartmentService } from '../../../services/department.service';
import { SectionService } from '../../../services/section.service';
import { AuthService } from '../../../services/auth.service';
import { UserAutocompleteComponent, UserSearchResult } from '../shared/user-autocomplete.component';
import {
  ResourcePool,
  ResourcePoolType,
  CreateResourcePoolRequest,
  UpdateResourcePoolRequest
} from '../../../models/escalation.model';
import { User } from '../../../models/user.model';
import { Branch } from '../../../models/branch.model';
import { Department } from '../../../models/department.model';
import { Section } from '../../../models/section.model';

@Component({
  selector: 'app-resource-pool-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UserAutocompleteComponent],
  templateUrl: './resource-pool-management.component.html',
  styleUrls: ['./resource-pool-management.component.scss']
})
export class ResourcePoolManagementComponent implements OnInit {
  pools: ResourcePool[] = [];
  filteredPools: ResourcePool[] = [];
  users: User[] = [];
  branches: Branch[] = [];
  departments: Department[] = [];
  sections: Section[] = [];

  loading = false;
  errorMessage = '';
  successMessage = '';
  searchTerm = '';
  showActiveOnly = true;

  // Modal state
  showModal = false;
  modalMode: 'create' | 'edit' = 'create';
  modalTitle = '';

  // Form model
  poolForm!: CreateResourcePoolRequest | (UpdateResourcePoolRequest & { id?: string });
  poolFormIsActive = false;

  // Member management
  showMemberModal = false;
  selectedPool: ResourcePool | null = null;
  selectedUsersToAdd: UserSearchResult[] = [];

  // Delete confirmation
  showDeleteConfirm = false;
  poolToDelete: ResourcePool | null = null;
  deleteLoading = false;

  // Pool Types for dropdown
  poolTypes = [
    { value: ResourcePoolType.Branch, label: 'Branch' },
    { value: ResourcePoolType.Department, label: 'Department' },
    { value: ResourcePoolType.Section, label: 'Section' },
    { value: ResourcePoolType.Custom, label: 'Custom' }
  ];

  constructor(
    private resourcePoolService: ResourcePoolService,
    private userService: UserService,
    private branchService: BranchService,
    private departmentService: DepartmentService,
    private sectionService: SectionService,
    private authService: AuthService
  ) {
    this.poolForm = this.getEmptyForm();
  }

  ngOnInit(): void {
    this.loadPools();
    // Removed loadUsers() - we use search autocomplete instead to avoid loading 10k+ users
    this.loadBranches();
    this.loadDepartments();
    this.loadSections();
  }

  getEmptyForm(): CreateResourcePoolRequest {
    return {
      name: '',
      description: '',
      poolType: ResourcePoolType.Custom,
      memberUserIds: []
    };
  }

  loadPools(): void {
    this.loading = true;
    this.errorMessage = '';

    const currentUser = this.authService.currentUserValue;
    const companyId = currentUser?.companyId;

    this.resourcePoolService.getAllPools(companyId).subscribe({
      next: (pools) => {
        this.pools = pools;
        this.filterPools();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load resource pools. Please try again.';
        this.loading = false;
        console.error('Error loading resource pools:', error);
      }
    });
  }

  // Removed loadUsers() - using search autocomplete instead for performance

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

  loadSections(): void {
    const currentUser = this.authService.currentUserValue;
    if (currentUser?.companyId) {
      this.sectionService.getSections(currentUser.companyId).subscribe({
        next: (sections) => {
          this.sections = sections;
        },
        error: (error) => {
          console.error('Error loading sections:', error);
        }
      });
    }
  }

  filterPools(): void {
    let filtered = this.pools;

    // Filter by active status
    if (this.showActiveOnly) {
      filtered = filtered.filter(p => p.isActive);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(p =>
        p.name.toLowerCase().includes(search) ||
        (p.description && p.description.toLowerCase().includes(search)) ||
        p.poolType.toLowerCase().includes(search)
      );
    }

    this.filteredPools = filtered.sort((a, b) =>
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );
  }

  onSearchChange(): void {
    this.filterPools();
  }

  onFilterChange(): void {
    this.filterPools();
  }

  openCreateModal(): void {
    this.modalMode = 'create';
    this.modalTitle = 'Create New Resource Pool';
    this.poolForm = this.getEmptyForm();
    this.showModal = true;
    this.errorMessage = '';
  }

  openEditModal(pool: ResourcePool): void {
    this.modalMode = 'edit';
    this.modalTitle = 'Edit Resource Pool';
    this.poolForm = {
      id: pool.id,
      name: pool.name,
      description: pool.description || '',
      poolType: pool.poolType,
      branchId: pool.branchId,
      departmentId: pool.departmentId,
      sectionId: pool.sectionId,
      isActive: pool.isActive
    };
    this.poolFormIsActive = pool.isActive;
    this.showModal = true;
    this.errorMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.poolForm = this.getEmptyForm();
    this.poolFormIsActive = false;
    this.errorMessage = '';
  }

  onPoolTypeChange(): void {
    // Clear org unit fields when pool type changes
    this.poolForm.branchId = undefined;
    this.poolForm.departmentId = undefined;
    this.poolForm.sectionId = undefined;
  }

  savePool(): void {
    // Validation
    if (!this.poolForm.name) {
      this.errorMessage = 'Pool name is required';
      return;
    }

    // Validate org unit based on pool type
    if (this.poolForm.poolType === ResourcePoolType.Branch && !this.poolForm.branchId) {
      this.errorMessage = 'Branch is required for Branch pool type';
      return;
    }
    if (this.poolForm.poolType === ResourcePoolType.Department && !this.poolForm.departmentId) {
      this.errorMessage = 'Department is required for Department pool type';
      return;
    }
    if (this.poolForm.poolType === ResourcePoolType.Section && !this.poolForm.sectionId) {
      this.errorMessage = 'Section is required for Section pool type';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.modalMode === 'create') {
      this.resourcePoolService.createPool(this.poolForm as CreateResourcePoolRequest).subscribe({
        next: (pool) => {
          this.successMessage = 'Resource pool created successfully';
          this.closeModal();
          this.loadPools();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create resource pool. Please try again.';
          this.loading = false;
          console.error('Error creating resource pool:', error);
        }
      });
    } else {
      const updateForm = this.poolForm as UpdateResourcePoolRequest & { id: string };
      updateForm.isActive = this.poolFormIsActive;
      this.resourcePoolService.updatePool(updateForm.id, updateForm).subscribe({
        next: (pool) => {
          this.successMessage = 'Resource pool updated successfully';
          this.closeModal();
          this.loadPools();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update resource pool. Please try again.';
          this.loading = false;
          console.error('Error updating resource pool:', error);
        }
      });
    }
  }

  openMemberModal(pool: ResourcePool): void {
    this.selectedPool = pool;
    this.selectedUsersToAdd = [];
    this.showMemberModal = true;
  }

  closeMemberModal(): void {
    this.showMemberModal = false;
    this.selectedPool = null;
    this.selectedUsersToAdd = [];
  }

  onUserSelected(user: UserSearchResult | null): void {
    if (!user || !this.selectedPool) return;

    // Check if user is already in the pool
    const isAlreadyMember = this.selectedPool.members.some(m => m.userId === user.id);
    if (isAlreadyMember) {
      this.errorMessage = `${user.fullName} is already a member of this pool`;
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    // Check if user is already selected to be added
    const isAlreadySelected = this.selectedUsersToAdd.some(u => u.id === user.id);
    if (isAlreadySelected) {
      this.errorMessage = `${user.fullName} is already selected`;
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    // Add to selection
    this.selectedUsersToAdd.push(user);
  }

  removeSelectedUser(userId: string): void {
    this.selectedUsersToAdd = this.selectedUsersToAdd.filter(u => u.id !== userId);
  }

  addMembers(): void {
    if (!this.selectedPool || this.selectedUsersToAdd.length === 0) {
      return;
    }

    this.loading = true;
    let completed = 0;
    const total = this.selectedUsersToAdd.length;

    this.selectedUsersToAdd.forEach(user => {
      this.resourcePoolService.addMember(this.selectedPool!.id, { userId: user.id }).subscribe({
        next: () => {
          completed++;
          if (completed === total) {
            this.successMessage = `Successfully added ${total} member(s)`;
            this.closeMemberModal();
            this.loadPools();
            this.loading = false;
            setTimeout(() => this.successMessage = '', 3000);
          }
        },
        error: (error) => {
          console.error('Error adding member:', error);
          this.errorMessage = `Failed to add some members`;
          this.loading = false;
        }
      });
    });
  }

  removeMember(pool: ResourcePool, userId: string): void {
    if (!confirm('Are you sure you want to remove this member from the pool?')) {
      return;
    }

    this.resourcePoolService.removeMember(pool.id, userId).subscribe({
      next: () => {
        this.successMessage = 'Member removed successfully';
        this.loadPools();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to remove member';
        console.error('Error removing member:', error);
      }
    });
  }

  openDeleteConfirm(pool: ResourcePool): void {
    this.poolToDelete = pool;
    this.showDeleteConfirm = true;
    this.errorMessage = '';
  }

  closeDeleteConfirm(): void {
    this.showDeleteConfirm = false;
    this.poolToDelete = null;
  }

  confirmDelete(): void {
    if (!this.poolToDelete) return;

    this.deleteLoading = true;
    this.errorMessage = '';

    this.resourcePoolService.deletePool(this.poolToDelete.id).subscribe({
      next: () => {
        this.successMessage = 'Resource pool deleted successfully';
        this.closeDeleteConfirm();
        this.loadPools();
        setTimeout(() => this.successMessage = '', 3000);
        this.deleteLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete resource pool. Please try again.';
        this.deleteLoading = false;
        console.error('Error deleting resource pool:', error);
      }
    });
  }

  getPoolTypeLabel(type: ResourcePoolType): string {
    return this.poolTypes.find(pt => pt.value === type)?.label || type;
  }

  getPoolTypeBadgeClass(type: ResourcePoolType): string {
    switch (type) {
      case ResourcePoolType.Branch: return 'badge-primary';
      case ResourcePoolType.Department: return 'badge-success';
      case ResourcePoolType.Section: return 'badge-info';
      case ResourcePoolType.Custom: return 'badge-secondary';
      default: return 'badge-secondary';
    }
  }
}
