import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeTypeService } from '../../../services/employee-type.service';
import { AuthService } from '../../../services/auth.service';
import { EmployeeType, CreateEmployeeTypeRequest, UpdateEmployeeTypeRequest } from '../../../models/employee-type.model';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-employee-type-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UtcToLocalPipe],
  templateUrl: './employee-type-management.component.html',
  styleUrls: ['./employee-type-management.component.scss']
})
export class EmployeeTypeManagementComponent implements OnInit {
  employeeTypes: EmployeeType[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  // Modal states
  showModal = false;
  isEditMode = false;
  showDeleteModal = false;

  // Form data
  employeeTypeForm!: CreateEmployeeTypeRequest & { id?: string };
  employeeTypeToDelete: EmployeeType | null = null;

  // Search and filter
  searchTerm = '';
  showInactive = false;

  constructor(
    private employeeTypeService: EmployeeTypeService,
    private authService: AuthService
  ) {
    // Initialize form in constructor after authService is injected
    this.employeeTypeForm = this.getEmptyForm();
  }

  ngOnInit(): void {
    this.loadEmployeeTypes();
  }

  getEmptyForm(): CreateEmployeeTypeRequest & { id?: string } {
    const currentUser = this.authService.currentUserValue;
    return {
      companyId: currentUser?.companyId || '',
      name: '',
      code: '',
      description: '',
      isActive: true
    };
  }

  loadEmployeeTypes(): void {
    this.loading = true;
    this.errorMessage = '';

    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'User not authenticated';
      this.loading = false;
      return;
    }

    this.employeeTypeService.getEmployeeTypes(currentUser.companyId, false).subscribe({
      next: (data) => {
        this.employeeTypes = data;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load employee types. Please try again.';
        this.loading = false;
        console.error('Error loading employee types:', error);
      }
    });
  }

  get filteredEmployeeTypes(): EmployeeType[] {
    let filtered = this.employeeTypes;

    // Filter by active status
    if (!this.showInactive) {
      filtered = filtered.filter(e => e.isActive);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(e =>
        e.name.toLowerCase().includes(search) ||
        e.code.toLowerCase().includes(search) ||
        (e.description && e.description.toLowerCase().includes(search))
      );
    }

    return filtered;
  }

  openCreateModal(): void {
    this.employeeTypeForm = this.getEmptyForm();
    this.isEditMode = false;
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(employeeType: EmployeeType): void {
    this.employeeTypeForm = {
      id: employeeType.id,
      companyId: employeeType.companyId,
      name: employeeType.name,
      code: employeeType.code,
      description: employeeType.description || '',
      isActive: employeeType.isActive
    };
    this.isEditMode = true;
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.employeeTypeForm = this.getEmptyForm();
    this.errorMessage = '';
  }

  saveEmployeeType(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.employeeTypeForm.id) {
      // Update existing employee type
      const updateRequest: UpdateEmployeeTypeRequest = {
        name: this.employeeTypeForm.name,
        code: this.employeeTypeForm.code,
        description: this.employeeTypeForm.description,
        isActive: this.employeeTypeForm.isActive
      };

      this.employeeTypeService.updateEmployeeType(this.employeeTypeForm.id, updateRequest).subscribe({
        next: (response) => {
          this.successMessage = 'Employee type updated successfully!';
          this.loading = false;
          this.closeModal();
          this.loadEmployeeTypes();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update employee type';
          this.loading = false;
          console.error('Error updating employee type:', error);
        }
      });
    } else {
      // Create new employee type
      const createRequest: CreateEmployeeTypeRequest = {
        companyId: this.employeeTypeForm.companyId,
        name: this.employeeTypeForm.name,
        code: this.employeeTypeForm.code,
        description: this.employeeTypeForm.description,
        isActive: this.employeeTypeForm.isActive
      };

      this.employeeTypeService.createEmployeeType(createRequest).subscribe({
        next: (response) => {
          this.successMessage = 'Employee type created successfully!';
          this.loading = false;
          this.closeModal();
          this.loadEmployeeTypes();
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create employee type';
          this.loading = false;
          console.error('Error creating employee type:', error);
        }
      });
    }
  }

  validateForm(): boolean {
    if (!this.employeeTypeForm.name.trim()) {
      this.errorMessage = 'Employee type name is required';
      return false;
    }
    if (!this.employeeTypeForm.code.trim()) {
      this.errorMessage = 'Employee type code is required';
      return false;
    }
    if (this.employeeTypeForm.code.length > 20) {
      this.errorMessage = 'Employee type code must be 20 characters or less';
      return false;
    }
    return true;
  }

  openDeleteModal(employeeType: EmployeeType): void {
    this.employeeTypeToDelete = employeeType;
    this.showDeleteModal = true;
    this.errorMessage = '';
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.employeeTypeToDelete = null;
    this.errorMessage = '';
  }

  confirmDelete(): void {
    if (!this.employeeTypeToDelete) return;

    this.loading = true;
    this.errorMessage = '';

    this.employeeTypeService.deleteEmployeeType(this.employeeTypeToDelete.id).subscribe({
      next: (response) => {
        this.successMessage = 'Employee type deleted successfully!';
        this.loading = false;
        this.closeDeleteModal();
        this.loadEmployeeTypes();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to delete employee type';
        this.loading = false;
        console.error('Error deleting employee type:', error);
      }
    });
  }

  toggleEmployeeTypeStatus(employeeType: EmployeeType): void {
    const updateRequest: UpdateEmployeeTypeRequest = {
      name: employeeType.name,
      code: employeeType.code,
      description: employeeType.description,
      isActive: !employeeType.isActive
    };

    this.employeeTypeService.updateEmployeeType(employeeType.id, updateRequest).subscribe({
      next: () => {
        employeeType.isActive = !employeeType.isActive;
        this.successMessage = `Employee type ${employeeType.isActive ? 'activated' : 'deactivated'} successfully!`;
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = 'Failed to update employee type status';
        console.error('Error toggling employee type status:', error);
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  getActiveEmployeeTypesCount(): number {
    return this.employeeTypes.filter(e => e.isActive).length;
  }

  getInactiveEmployeeTypesCount(): number {
    return this.employeeTypes.filter(e => !e.isActive).length;
  }

  // TrackBy function for *ngFor optimization
  trackByEmployeeTypeId(index: number, employeeType: EmployeeType): string {
    return employeeType.id;
  }
}
