import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StoreService } from '../../../services/store.service';
import { UserService } from '../../../services/user.service';
import { BranchService } from '../../../services/branch.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import {
  Store,
  CreateStoreDto,
  UpdateStoreDto,
  StoreStaff,
  StoreRole,
  StoreRoleLabels,
  AssignStoreUserRoleDto
} from '../../../models/store.model';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-store-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UtcToLocalPipe],
  templateUrl: './store-management.component.html',
  styleUrls: ['./store-management.component.scss']
})
export class StoreManagementComponent implements OnInit {
  stores: Store[] = [];
  users: any[] = [];
  branches: any[] = [];
  storeStaff: StoreStaff[] = [];

  isLoading = false;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  showModal = false;
  showStaffModal = false;
  showDeleteConfirm = false;
  isEditMode = false;

  selectedStore: Store | null = null;
  storeToDelete: Store | null = null;

  // Form model
  form: CreateStoreDto & { id?: string; isActive?: boolean } = this.getEmptyForm();

  // Staff form
  staffForm: AssignStoreUserRoleDto = {
    userId: '',
    role: StoreRole.StoreKeeper,
    notes: ''
  };

  storeRoles = [
    { value: StoreRole.StoreKeeper, label: 'Store Keeper' },
    { value: StoreRole.StoreManager, label: 'Store Manager' },
    { value: StoreRole.StoreAdmin, label: 'Store Admin' }
  ];

  StoreRoleLabels = StoreRoleLabels;

  constructor(
    private storeService: StoreService,
    private userService: UserService,
    private branchService: BranchService,
    private authService: AuthService,
    private logger: LoggerService
  ) {}

  ngOnInit(): void {
    this.loadStores();
    this.loadUsers();
    this.loadBranches();
  }

  private getEmptyForm(): CreateStoreDto & { id?: string } {
    return {
      code: '',
      name: '',
      description: '',
      address: '',
      city: '',
      phone: '',
      email: '',
      primaryManagerId: undefined,
      secondaryManagerId: undefined,
      branchId: undefined,
      approvalTimeoutHours: 48,
      autoEscalateToSecondary: true,
      requireDeptApprovalForAssignment: false
    };
  }

  loadStores(): void {
    this.isLoading = true;
    this.storeService.getStores(true).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.stores = result.data;
        } else {
          this.errorMessage = result.message;
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.logger.error('Error loading stores', error);
        this.errorMessage = 'Failed to load stores';
        this.isLoading = false;
      }
    });
  }

  loadUsers(): void {
    this.userService.getUsers().subscribe({
      next: (result: any) => {
        if (result.isSuccess) {
          this.users = result.data || [];
        }
      },
      error: (error) => {
        this.logger.error('Error loading users', error);
      }
    });
  }

  loadBranches(): void {
    const companyId = this.authService.currentUserValue?.companyId;
    if (!companyId) {
      this.logger.warn('No companyId found, cannot load branches');
      return;
    }
    this.branchService.getBranches(companyId).subscribe({
      next: (branches: any) => {
        this.branches = branches || [];
      },
      error: (error) => {
        this.logger.error('Error loading branches', error);
      }
    });
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.form = this.getEmptyForm();
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(store: Store): void {
    this.isEditMode = true;
    this.selectedStore = store;
    this.form = {
      id: store.id,
      code: store.code,
      name: store.name,
      description: store.description || '',
      address: store.address || '',
      city: store.city || '',
      phone: store.phone || '',
      email: store.email || '',
      primaryManagerId: store.primaryManagerId,
      secondaryManagerId: store.secondaryManagerId,
      branchId: store.branchId,
      isActive: store.isActive,
      approvalTimeoutHours: store.approvalTimeoutHours,
      autoEscalateToSecondary: store.autoEscalateToSecondary,
      requireDeptApprovalForAssignment: store.requireDeptApprovalForAssignment
    };
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.selectedStore = null;
    this.form = this.getEmptyForm();
  }

  saveStore(): void {
    // Code is required only for new stores (immutable after creation)
    if (!this.isEditMode && !this.form.code) {
      this.errorMessage = 'Code is required';
      return;
    }
    if (!this.form.name) {
      this.errorMessage = 'Name is required';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    if (this.isEditMode && this.form.id) {
      const updateDto: UpdateStoreDto = {
        name: this.form.name,
        description: this.form.description,
        address: this.form.address,
        city: this.form.city,
        phone: this.form.phone,
        email: this.form.email,
        primaryManagerId: this.form.primaryManagerId,
        secondaryManagerId: this.form.secondaryManagerId,
        branchId: this.form.branchId,
        isActive: this.form.isActive,
        approvalTimeoutHours: this.form.approvalTimeoutHours,
        autoEscalateToSecondary: this.form.autoEscalateToSecondary,
        requireDeptApprovalForAssignment: this.form.requireDeptApprovalForAssignment
      };

      this.storeService.updateStore(this.form.id, updateDto).subscribe({
        next: (result) => {
          if (result.isSuccess) {
            this.successMessage = 'Store updated successfully';
            this.loadStores();
            this.closeModal();
          } else {
            this.errorMessage = result.message;
          }
          this.isSubmitting = false;
        },
        error: (error) => {
          this.logger.error('Error updating store', error);
          this.errorMessage = 'Failed to update store';
          this.isSubmitting = false;
        }
      });
    } else {
      const createDto: CreateStoreDto = {
        code: this.form.code,
        name: this.form.name,
        description: this.form.description,
        address: this.form.address,
        city: this.form.city,
        phone: this.form.phone,
        email: this.form.email,
        primaryManagerId: this.form.primaryManagerId,
        secondaryManagerId: this.form.secondaryManagerId,
        branchId: this.form.branchId,
        approvalTimeoutHours: this.form.approvalTimeoutHours,
        autoEscalateToSecondary: this.form.autoEscalateToSecondary,
        requireDeptApprovalForAssignment: this.form.requireDeptApprovalForAssignment
      };

      this.storeService.createStore(createDto).subscribe({
        next: (result) => {
          if (result.isSuccess) {
            this.successMessage = 'Store created successfully';
            this.loadStores();
            this.closeModal();
          } else {
            this.errorMessage = result.message;
          }
          this.isSubmitting = false;
        },
        error: (error) => {
          this.logger.error('Error creating store', error);
          this.errorMessage = 'Failed to create store';
          this.isSubmitting = false;
        }
      });
    }
  }

  confirmDelete(store: Store): void {
    this.storeToDelete = store;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.storeToDelete = null;
    this.showDeleteConfirm = false;
  }

  deleteStore(): void {
    if (!this.storeToDelete) return;

    this.isSubmitting = true;
    this.storeService.deleteStore(this.storeToDelete.id).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.successMessage = 'Store deleted successfully';
          this.loadStores();
        } else {
          this.errorMessage = result.message;
        }
        this.cancelDelete();
        this.isSubmitting = false;
      },
      error: (error) => {
        this.logger.error('Error deleting store', error);
        this.errorMessage = 'Failed to delete store';
        this.isSubmitting = false;
      }
    });
  }

  // Staff management
  openStaffModal(store: Store): void {
    this.selectedStore = store;
    this.loadStoreStaff(store.id);
    this.staffForm = {
      userId: '',
      role: StoreRole.StoreKeeper,
      notes: ''
    };
    this.showStaffModal = true;
  }

  closeStaffModal(): void {
    this.showStaffModal = false;
    this.selectedStore = null;
    this.storeStaff = [];
  }

  loadStoreStaff(storeId: string): void {
    this.storeService.getStoreStaff(storeId).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.storeStaff = result.data;
        }
      },
      error: (error) => {
        this.logger.error('Error loading store staff', error);
      }
    });
  }

  assignStaff(): void {
    if (!this.selectedStore || !this.staffForm.userId) {
      this.errorMessage = 'Please select a user';
      return;
    }

    this.isSubmitting = true;
    this.storeService.assignStoreRole(this.selectedStore.id, this.staffForm).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.successMessage = 'Staff assigned successfully';
          this.loadStoreStaff(this.selectedStore!.id);
          this.staffForm = {
            userId: '',
            role: StoreRole.StoreKeeper,
            notes: ''
          };
        } else {
          this.errorMessage = result.message;
        }
        this.isSubmitting = false;
      },
      error: (error) => {
        this.logger.error('Error assigning staff', error);
        this.errorMessage = 'Failed to assign staff';
        this.isSubmitting = false;
      }
    });
  }

  revokeStaff(staff: StoreStaff): void {
    if (!confirm(`Are you sure you want to revoke ${staff.userName}'s role?`)) {
      return;
    }

    this.storeService.revokeStoreRole(staff.id).subscribe({
      next: (result) => {
        if (result.isSuccess) {
          this.successMessage = 'Staff role revoked';
          this.loadStoreStaff(this.selectedStore!.id);
        } else {
          this.errorMessage = result.message;
        }
      },
      error: (error) => {
        this.logger.error('Error revoking staff role', error);
        this.errorMessage = 'Failed to revoke staff role';
      }
    });
  }

  getUserName(userId: string | undefined): string {
    if (!userId) return '-';
    const user = this.users.find(u => u.id === userId);
    return user ? user.fullName || user.email : '-';
  }

  getBranchName(branchId: string | undefined): string {
    if (!branchId) return '-';
    const branch = this.branches.find(b => b.id === branchId);
    return branch ? branch.name : '-';
  }
}
