import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SectionService } from '../../../services/section.service';
import { DepartmentService } from '../../../services/department.service';
import { BranchService } from '../../../services/branch.service';
import { AuthService } from '../../../services/auth.service';
import { LoggerService } from '../../../core/services/logger.service';
import { Section, CreateSectionRequest, UpdateSectionRequest } from '../../../models/section.model';
import { Department } from '../../../models/department.model';
import { Branch } from '../../../models/branch.model';
import { BaseMasterManagementComponent, ApiResponse } from '../shared/base-master-management.component';
import { UserAutocompleteComponent, UserSearchResult } from '../shared/user-autocomplete.component';
import { Observable } from 'rxjs';
import { UtcToLocalPipe } from '../../../pipes/utc-to-local.pipe';

@Component({
  selector: 'app-section-management',
  standalone: true,
  imports: [CommonModule, FormsModule, UserAutocompleteComponent, UtcToLocalPipe],
  templateUrl: './section-management.component.html',
  styleUrls: ['./section-management.component.scss']
})
export class SectionManagementComponent
  extends BaseMasterManagementComponent<Section, CreateSectionRequest, UpdateSectionRequest>
  implements OnInit {

  // Form model - union type to handle both Create and Update
  form!: (CreateSectionRequest & { id?: string }) | (UpdateSectionRequest & { code?: string });

  // User display names for autocomplete
  headName?: string;
  secondaryHeadName?: string;
  hrResponsibleName?: string;

  // Section-specific properties
  branches: Branch[] = [];
  departments: Department[] = [];
  selectedBranchId = '';
  selectedDepartmentId = '';

  protected override get entityName(): string {
    return 'Section';
  }

  // Getter/setter for code property (for backward compatibility)
  get sectionCode(): string {
    return this.formCode;
  }

  set sectionCode(value: string) {
    this.formCode = value;
  }

  // Getters/setters for section-specific fields
  get departmentId(): string {
    return (this.form as any).departmentId || '';
  }

  set departmentId(value: string) {
    (this.form as any).departmentId = value;
  }

  get headId(): string | undefined {
    return (this.form as any).headId;
  }

  set headId(value: string | undefined) {
    (this.form as any).headId = value;
  }

  get secondaryHeadId(): string | undefined {
    return (this.form as any).secondaryHeadId;
  }

  set secondaryHeadId(value: string | undefined) {
    (this.form as any).secondaryHeadId = value;
  }

  get hrResponsibleId(): string | undefined {
    return (this.form as any).hrResponsibleId;
  }

  set hrResponsibleId(value: string | undefined) {
    (this.form as any).hrResponsibleId = value;
  }

  // Map base class showActiveOnly to Section's showInactive (inverted logic)
  get showInactive(): boolean {
    return !this.showActiveOnly;
  }

  set showInactive(value: boolean) {
    this.showActiveOnly = !value;
  }

  // Override base class method to reload data from API instead of just filtering
  override onActiveFilterChange(): void {
    // Section management needs to reload from API with new activeOnly parameter
    if (this.selectedDepartmentId) {
      this.loadItems();
    }
  }

  constructor(
    private sectionService: SectionService,
    private departmentService: DepartmentService,
    private branchService: BranchService,
    authService: AuthService,
    logger: LoggerService,
    private router: Router
  ) {
    super(authService, logger);
    this.form = this.getEmptyForm();
  }

  override ngOnInit(): void {
    this.canManageSettings = this.authService.hasPermission('ManageSettings');
    this.loadBranches();
    this.logger.info('Section Management initialized', undefined, 'SectionManagementComponent');
  }

  protected override getEmptyForm(): CreateSectionRequest {
    return {
      departmentId: this.selectedDepartmentId || '',
      name: '',
      code: '',
      description: '',
      headId: undefined,
      secondaryHeadId: undefined,
      hrResponsibleId: undefined,
      isActive: true
    };
  }

  loadBranches(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'User not authenticated';
      this.logger.error('User not authenticated', undefined, 'SectionManagementComponent');
      return;
    }

    this.branchService.getBranches(currentUser.companyId, false).subscribe({
      next: (data) => {
        this.branches = data;
        if (this.branches.length > 0 && !this.selectedBranchId) {
          this.selectedBranchId = this.branches[0].id;
          this.onBranchChange();
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to load branches';
        this.logger.error('Error loading branches', error, 'SectionManagementComponent');
      }
    });
  }

  onBranchChange(): void {
    this.selectedDepartmentId = '';
    this.items = [];
    this.filteredItems = [];
    if (this.selectedBranchId) {
      this.loadDepartments();
    } else {
      this.departments = [];
    }
  }

  loadDepartments(): void {
    if (!this.selectedBranchId) return;

    this.departmentService.getDepartments(this.selectedBranchId, false).subscribe({
      next: (data) => {
        this.departments = data;
        if (this.departments.length > 0 && !this.selectedDepartmentId) {
          this.selectedDepartmentId = this.departments[0].id;
          this.onDepartmentChange();
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to load departments';
        this.logger.error('Error loading departments', error, 'SectionManagementComponent');
      }
    });
  }

  onDepartmentChange(): void {
    if (this.selectedDepartmentId) {
      this.loadItems();
    } else {
      this.items = [];
      this.filteredItems = [];
    }
  }

  protected override loadItems(): void {
    if (!this.selectedDepartmentId) {
      this.items = [];
      this.filteredItems = [];
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.sectionService.getSections(this.selectedDepartmentId, this.showActiveOnly).subscribe({
      next: (data) => {
        this.items = data;
        this.filterItems();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load sections. Please try again.';
        this.loading = false;
        this.logger.error('Error loading sections', error, 'SectionManagementComponent');
      }
    });
  }

  protected override filterItems(): void {
    let filtered = this.items;

    // Filter by active status (uses showActiveOnly from base class)
    if (this.showActiveOnly) {
      filtered = filtered.filter(s => s.isActive);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(s =>
        s.name.toLowerCase().includes(search) ||
        s.code.toLowerCase().includes(search) ||
        (s.description && s.description.toLowerCase().includes(search))
      );
    }

    this.filteredItems = filtered;
  }

  protected override createItem(request: CreateSectionRequest): Observable<ApiResponse<Section>> {
    return this.sectionService.createSection(request);
  }

  protected override updateItem(id: string, request: UpdateSectionRequest): Observable<ApiResponse<Section>> {
    return this.sectionService.updateSection(id, request);
  }

  protected override deleteItem(id: string): Observable<ApiResponse<boolean>> {
    return this.sectionService.deleteSection(id) as Observable<ApiResponse<boolean>>;
  }

  override openEditModal(item: Section): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to edit sections';
      return;
    }

    this.modalMode = 'edit';
    this.modalTitle = 'Edit Section';

    // Create edit form with all section-specific fields
    this.form = {
      id: item.id,
      departmentId: item.departmentId,
      name: item.name,
      code: item.code,
      description: item.description || '',
      headId: item.headId,
      secondaryHeadId: item.secondaryHeadId,
      hrResponsibleId: item.hrResponsibleId,
      isActive: item.isActive
    } as (CreateSectionRequest & { id?: string }) | (UpdateSectionRequest & { code?: string });

    // Set user display names
    this.headName = item.headName;
    this.secondaryHeadName = item.secondaryHeadName;
    this.hrResponsibleName = item.hrResponsibleName;

    this.showModal = true;
    this.errorMessage = '';
  }

  protected override validateForm(): boolean {
    // Call base validation first
    if (!super.validateForm()) {
      return false;
    }

    // Validate department ID is set
    if (!this.departmentId) {
      this.errorMessage = 'Department is required';
      return false;
    }

    // Validate section code length
    if (this.formCode && this.formCode.length > 20) {
      this.errorMessage = 'Section code must be 20 characters or less';
      return false;
    }

    return true;
  }

  override save(): void {
    if (!this.validateForm()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    if (this.modalMode === 'create') {
      this.createItem(this.form as CreateSectionRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Section created successfully';
            this.logger.info('Section created', undefined, 'SectionManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to create section';
            this.logger.error('Failed to create section', { message: response.message }, 'SectionManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create section. Please try again.';
          this.logger.error('Error creating section', error, 'SectionManagementComponent');
          this.loading = false;
        }
      });
    } else {
      // Build update request with all fields including section-specific ones
      const updateRequest = {
        name: this.formName,
        code: this.formCode,
        description: this.formDescription,
        headId: this.headId,
        secondaryHeadId: this.secondaryHeadId,
        hrResponsibleId: this.hrResponsibleId,
        isActive: this.formIsActive
      } as UpdateSectionRequest;

      this.updateItem(this.formId, updateRequest).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.successMessage = 'Section updated successfully';
            this.logger.info('Section updated', undefined, 'SectionManagementComponent');
            this.closeModal();
            this.loadItems();
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = response.message || 'Failed to update section';
            this.logger.error('Failed to update section', { message: response.message }, 'SectionManagementComponent');
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update section. Please try again.';
          this.logger.error('Error updating section', error, 'SectionManagementComponent');
          this.loading = false;
        }
      });
    }
  }

  // Alias methods and getters for template compatibility
  get sections(): Section[] {
    return this.items;
  }

  get filteredSections(): Section[] {
    return this.filteredItems;
  }

  get sectionForm(): (CreateSectionRequest & { id?: string }) | (UpdateSectionRequest & { code?: string }) {
    return this.form;
  }

  set sectionForm(value: (CreateSectionRequest & { id?: string }) | (UpdateSectionRequest & { code?: string })) {
    this.form = value;
  }

  get sectionToDelete(): Section | null {
    return this.itemToDelete;
  }

  set sectionToDelete(value: Section | null) {
    this.itemToDelete = value;
  }

  // Backward compatibility for old naming conventions
  get isEditMode(): boolean {
    return this.modalMode === 'edit';
  }

  get showDeleteModal(): boolean {
    return this.showDeleteConfirm;
  }

  override openCreateModal(): void {
    if (!this.selectedDepartmentId) {
      this.errorMessage = 'Please select a department first';
      return;
    }

    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to create sections';
      return;
    }

    super.openCreateModal();
    this.modalTitle = 'Create New Section';
    // Set departmentId after calling super which resets the form
    this.departmentId = this.selectedDepartmentId;
  }

  override closeModal(): void {
    super.closeModal();
  }

  saveSection(): void {
    this.save();
  }

  openDeleteModal(section: Section): void {
    if (!this.canManageSettings) {
      this.errorMessage = 'You do not have permission to delete sections';
      return;
    }
    super.confirmDelete(section);
  }

  closeDeleteModal(): void {
    this.cancelDelete();
  }

  override confirmDelete(): void {
    this.executeDelete();
  }

  loadSections(): void {
    this.loadItems();
  }

  // Section-specific utility methods
  toggleSectionStatus(section: Section): void {
    const updateRequest: UpdateSectionRequest = {
      name: section.name,
      code: section.code,
      description: section.description,
      headId: section.headId,
      secondaryHeadId: section.secondaryHeadId,
      hrResponsibleId: section.hrResponsibleId,
      isActive: !section.isActive
    };

    this.sectionService.updateSection(section.id, updateRequest).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          section.isActive = !section.isActive;
          this.successMessage = `Section ${section.isActive ? 'activated' : 'deactivated'} successfully`;
          this.logger.info('Section status toggled', { sectionId: section.id, isActive: section.isActive }, 'SectionManagementComponent');
          setTimeout(() => this.successMessage = '', 3000);
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to update section status';
        this.logger.error('Error toggling section status', error, 'SectionManagementComponent');
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  getSelectedDepartmentName(): string {
    const department = this.departments.find(d => d.id === this.selectedDepartmentId);
    return department ? department.name : '';
  }

  getActiveSectionsCount(): number {
    return this.sections.filter(s => s.isActive).length;
  }

  getInactiveSectionsCount(): number {
    return this.sections.filter(s => !s.isActive).length;
  }

  navigateBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // User selection handlers
  onHeadSelected(user: UserSearchResult | null): void {
    this.headId = user?.id;
    this.headName = user?.fullName;
  }

  onSecondaryHeadSelected(user: UserSearchResult | null): void {
    this.secondaryHeadId = user?.id;
    this.secondaryHeadName = user?.fullName;
  }

  onHrResponsibleSelected(user: UserSearchResult | null): void {
    this.hrResponsibleId = user?.id;
    this.hrResponsibleName = user?.fullName;
  }
}
