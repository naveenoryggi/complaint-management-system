import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { UserService } from '../../../services/user.service';
import { BranchService } from '../../../services/branch.service';
import { DepartmentService } from '../../../services/department.service';
import { SectionService } from '../../../services/section.service';
import { EmployeeTypeService } from '../../../services/employee-type.service';
import { AuthService } from '../../../services/auth.service';
import { OryggiSyncService } from '../../../services/oryggi-sync.service';
import { User, CreateUserRequest, UpdateUserRequest } from '../../../models/user.model';
import { Branch } from '../../../models/branch.model';
import { Department } from '../../../models/department.model';
import { Section } from '../../../models/section.model';
import { EmployeeType } from '../../../models/employee-type.model';
import { UserSearchAutocompleteComponent } from '../../shared/user-search-autocomplete/user-search-autocomplete.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UserSearchAutocompleteComponent],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit, OnDestroy {
  users: User[] = [];
  filteredUsers: User[] = [];
  paginatedUsers: User[] = [];
  selectedUser: User | null = null;
  loading = false;
  detailLoading = false;
  errorMessage = '';
  successMessage = '';
  searchTerm = '';

  // Make Math available in template for calculations
  Math = Math;

  // Search debouncing
  private searchSubject = new Subject<string>();

  // Pagination
  currentPage = 1;
  pageSize = 50;
  totalUsers = 0;
  totalPages = 1;

  // Modal state
  showModal = false;
  isEditMode = false;
  showImportModal = false;
  importEmployeeCode = '';

  // Form data
  userForm: (CreateUserRequest & { id?: string }) | null = null;

  // Organizational structure data
  branches: Branch[] = [];
  departments: Department[] = [];
  sections: Section[] = [];
  employeeTypes: EmployeeType[] = [];
  managers: User[] = [];
  selectedManager: User | null = null;

  constructor(
    private userService: UserService,
    private branchService: BranchService,
    private departmentService: DepartmentService,
    private sectionService: SectionService,
    private employeeTypeService: EmployeeTypeService,
    private authService: AuthService,
    private oryggiSyncService: OryggiSyncService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
    this.loadOrganizationalData();

    // Setup debounced search
    this.searchSubject.pipe(
      debounceTime(300), // Wait 300ms after user stops typing
      distinctUntilChanged() // Only emit if value changes
    ).subscribe(searchTerm => {
      this.searchTerm = searchTerm;
      this.filterUsers();
    });
  }

  ngOnDestroy(): void {
    this.searchSubject.complete();
  }

  loadUsers(): void {
    this.loading = true;
    this.errorMessage = '';

    this.userService.getUsers().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.users = response.data;
          this.managers = response.data; // For manager dropdown
          this.filterUsers();
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load users. Please try again.';
        this.loading = false;
        console.error('Error loading users:', error);
      }
    });
  }

  loadOrganizationalData(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) return;

    const companyId = currentUser.companyId;

    // Load branches
    this.branchService.getBranches(companyId, false).subscribe({
      next: (data) => {
        this.branches = data;
      },
      error: (error) => {
        console.error('Error loading branches:', error);
      }
    });

    // Load employee types
    this.employeeTypeService.getEmployeeTypes(companyId, false).subscribe({
      next: (data) => {
        this.employeeTypes = data;
      },
      error: (error) => {
        console.error('Error loading employee types:', error);
      }
    });
  }

  onBranchChange(branchId: string | undefined): void {
    if (!branchId) {
      this.departments = [];
      this.sections = [];
      if (this.userForm) {
        this.userForm.departmentId = undefined;
        this.userForm.sectionId = undefined;
      }
      return;
    }

    this.departmentService.getDepartments(branchId, false).subscribe({
      next: (data) => {
        this.departments = data;
        this.sections = [];
        if (this.userForm) {
          this.userForm.departmentId = undefined;
          this.userForm.sectionId = undefined;
        }
      },
      error: (error) => {
        console.error('Error loading departments:', error);
      }
    });
  }

  onDepartmentChange(departmentId: string | undefined): void {
    if (!departmentId) {
      this.sections = [];
      if (this.userForm) {
        this.userForm.sectionId = undefined;
      }
      return;
    }

    this.sectionService.getSections(departmentId, false).subscribe({
      next: (data) => {
        this.sections = data;
        if (this.userForm) {
          this.userForm.sectionId = undefined;
        }
      },
      error: (error) => {
        console.error('Error loading sections:', error);
      }
    });
  }

  filterUsers(): void {
    let filtered = this.users;

    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(u =>
        u.fullName.toLowerCase().includes(search) ||
        u.email.toLowerCase().includes(search) ||
        u.employeeCode.toLowerCase().includes(search) ||
        (u.jobTitle && u.jobTitle.toLowerCase().includes(search))
      );
    }

    this.filteredUsers = filtered.sort((a, b) => a.fullName.localeCompare(b.fullName));
    this.totalUsers = this.filteredUsers.length;
    this.totalPages = Math.ceil(this.totalUsers / this.pageSize);

    // Reset to page 1 when filtering
    this.currentPage = 1;
    this.updatePaginatedUsers();
  }

  onSearchChange(): void {
    // Use debounced search
    this.searchSubject.next(this.searchTerm);
  }

  updatePaginatedUsers(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedUsers = this.filteredUsers.slice(startIndex, endIndex);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePaginatedUsers();
      // Scroll to top of table
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  nextPage(): void {
    this.goToPage(this.currentPage + 1);
  }

  previousPage(): void {
    this.goToPage(this.currentPage - 1);
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  openCreateModal(): void {
    const currentUser = this.authService.currentUserValue;
    this.userForm = {
      companyId: currentUser?.companyId || '',
      employeeCode: '',
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      jobTitle: '',
      branchId: undefined,
      departmentId: undefined,
      sectionId: undefined,
      employeeTypeId: undefined,
      managerId: undefined
    };
    this.isEditMode = false;
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(user: User): void {
    this.userForm = {
      id: user.id,
      companyId: user.companyId,
      employeeCode: user.employeeCode,
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      phone: user.phone || '',
      jobTitle: user.jobTitle || '',
      branchId: user.branchId,
      departmentId: user.departmentId,
      sectionId: user.sectionId,
      employeeTypeId: user.employeeTypeId,
      managerId: user.managerId
    };

    // Set selected manager if managerId exists
    if (user.managerId) {
      // Find the manager from the users list
      this.selectedManager = this.users.find(u => u.id === user.managerId) || null;
    } else {
      this.selectedManager = null;
    }

    // Load cascading dropdowns
    if (user.branchId) {
      this.onBranchChange(user.branchId);
      if (user.departmentId) {
        // Wait for departments to load, then load sections
        setTimeout(() => {
          this.onDepartmentChange(user.departmentId);
        }, 300);
      }
    }

    this.isEditMode = true;
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.userForm = null;
    this.selectedManager = null;
    this.errorMessage = '';
    this.departments = [];
    this.sections = [];
  }

  saveUser(): void {
    if (!this.userForm || !this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.userForm.id) {
      // Update existing user
      const updateRequest: UpdateUserRequest = {
        firstName: this.userForm.firstName,
        lastName: this.userForm.lastName,
        email: this.userForm.email,
        phone: this.userForm.phone,
        jobTitle: this.userForm.jobTitle,
        branchId: this.userForm.branchId,
        departmentId: this.userForm.departmentId,
        sectionId: this.userForm.sectionId,
        employeeTypeId: this.userForm.employeeTypeId,
        managerId: this.userForm.managerId,
        isActive: true
      };

      this.userService.updateUser(this.userForm.id, updateRequest).subscribe({
        next: (response) => {
          this.successMessage = 'User updated successfully!';
          this.loading = false;
          this.closeModal();
          this.loadUsers();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update user';
          this.loading = false;
          console.error('Error updating user:', error);
        }
      });
    } else {
      // Create new user
      const createRequest: CreateUserRequest = {
        companyId: this.userForm.companyId,
        employeeCode: this.userForm.employeeCode,
        firstName: this.userForm.firstName,
        lastName: this.userForm.lastName,
        email: this.userForm.email,
        phone: this.userForm.phone,
        jobTitle: this.userForm.jobTitle,
        branchId: this.userForm.branchId,
        departmentId: this.userForm.departmentId,
        sectionId: this.userForm.sectionId,
        employeeTypeId: this.userForm.employeeTypeId,
        managerId: this.userForm.managerId
      };

      this.userService.createUser(createRequest).subscribe({
        next: (response) => {
          this.successMessage = 'User created successfully!';
          this.loading = false;
          this.closeModal();
          this.loadUsers();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create user';
          this.loading = false;
          console.error('Error creating user:', error);
        }
      });
    }
  }

  validateForm(): boolean {
    if (!this.userForm) return false;

    if (!this.userForm.firstName.trim()) {
      this.errorMessage = 'First name is required';
      return false;
    }
    if (!this.userForm.lastName.trim()) {
      this.errorMessage = 'Last name is required';
      return false;
    }
    if (!this.userForm.email.trim()) {
      this.errorMessage = 'Email is required';
      return false;
    }
    if (!this.isEditMode && !this.userForm.employeeCode.trim()) {
      this.errorMessage = 'Employee code is required';
      return false;
    }
    return true;
  }

  viewUserDetails(userId: string): void {
    this.detailLoading = true;
    this.errorMessage = '';

    this.userService.getUserById(userId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.selectedUser = response.data;
        }
        this.detailLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load user details. Please try again.';
        this.detailLoading = false;
        console.error('Error loading user details:', error);
      }
    });
  }

  closeUserDetails(): void {
    this.selectedUser = null;
  }

  getRoleNames(user: User): string {
    if (!user.roles || user.roles.length === 0) {
      return 'No roles assigned';
    }
    return user.roles.map(r => r.roleName).join(', ');
  }

  getPrimaryRole(user: User): string {
    if (!user.roles || user.roles.length === 0) {
      return 'No Role';
    }
    const primaryRole = user.roles.find(r => r.isPrimary);
    return primaryRole ? primaryRole.roleName : user.roles[0].roleName;
  }

  deleteUser(user: User): void {
    if (!confirm(`Are you sure you want to delete user "${user.fullName}"? This action cannot be undone.`)) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.userService.deleteUser(user.id).subscribe({
      next: (response) => {
        this.successMessage = `User "${user.fullName}" deleted successfully!`;
        this.loading = false;
        this.loadUsers();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete user';
        this.loading = false;
        console.error('Error deleting user:', error);
      }
    });
  }

  syncFromOryggi(user: User): void {
    if (!user.employeeCode) {
      this.errorMessage = 'Cannot sync: User has no employee code';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    if (!confirm(`Sync user "${user.fullName}" from Oryggi database? This will update the user's information with the latest data from Oryggi.`)) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.oryggiSyncService.syncSingleEmployee(user.employeeCode).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = `User "${user.fullName}" synced successfully from Oryggi!`;
          this.loadUsers(); // Reload to show updated data
        } else {
          this.errorMessage = response.message || 'Sync failed';
        }
        this.loading = false;
        setTimeout(() => {
          this.successMessage = '';
          this.errorMessage = '';
        }, 5000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to sync user from Oryggi';
        this.loading = false;
        console.error('Error syncing user:', error);
        setTimeout(() => this.errorMessage = '', 5000);
      }
    });
  }

  openImportModal(): void {
    this.showImportModal = true;
    this.importEmployeeCode = '';
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeImportModal(): void {
    this.showImportModal = false;
    this.importEmployeeCode = '';
    this.errorMessage = '';
  }

  importUserFromOryggi(): void {
    if (!this.importEmployeeCode.trim()) {
      this.errorMessage = 'Please enter an employee code';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.oryggiSyncService.syncSingleEmployee(this.importEmployeeCode.trim()).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = `Employee "${this.importEmployeeCode}" imported successfully from Oryggi!`;
          this.closeImportModal();
          this.loadUsers(); // Reload to show the new user
          setTimeout(() => this.successMessage = '', 5000);
        } else {
          this.errorMessage = response.message || 'Import failed';
        }
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to import user from Oryggi';
        this.loading = false;
        console.error('Error importing user:', error);
      }
    });
  }

  /**
   * Handle manager selection from autocomplete component
   */
  onManagerSelected(manager: User): void {
    if (this.userForm) {
      this.userForm.managerId = manager.id;
      this.selectedManager = manager;
    }
  }

  /**
   * Handle manager search cleared
   */
  onManagerSearchCleared(): void {
    if (this.userForm) {
      this.userForm.managerId = undefined;
      this.selectedManager = null;
    }
  }
}
