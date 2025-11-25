import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventTypeService } from '../../../services/event-type.service';
import { EventType, CreateEventTypeRequest, UpdateEventTypeRequest } from '../../../models/event-type.model';

@Component({
  selector: 'app-event-type-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './event-type-management.component.html',
  styleUrls: ['./event-type-management.component.css']
})
export class EventTypeManagementComponent implements OnInit {
  eventTypes: EventType[] = [];
  filteredEventTypes: EventType[] = [];
  loading = false;
  error: string | null = null;
  successMessage: string | null = null;

  // Filters
  filterEntityType = '';
  filterCategory = '';
  filterIncludeInactive = false;
  searchTerm = '';

  // Available options
  entityTypes: string[] = [];
  categories: string[] = [];

  // Modals
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;

  // Form data
  currentEventType: EventType | null = null;
  formData: CreateEventTypeRequest = {
    name: '',
    code: '',
    description: '',
    entityType: '',
    category: '',
    isActive: true,
    availableFields: '',
    iconClass: ''
  };

  constructor(private eventTypeService: EventTypeService) {}

  ngOnInit(): void {
    this.loadEventTypes();
    this.loadEntityTypes();
    this.loadCategories();
  }

  loadEventTypes(): void {
    this.loading = true;
    this.error = null;

    this.eventTypeService.getEventTypes({
      includeInactive: this.filterIncludeInactive,
      entityType: this.filterEntityType || undefined,
      category: this.filterCategory || undefined
    }).subscribe({
      next: (data) => {
        this.eventTypes = data;
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load event types';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadEntityTypes(): void {
    this.eventTypeService.getEntityTypes().subscribe({
      next: (data) => {
        this.entityTypes = data.entityTypes;
      },
      error: (err) => console.error('Failed to load entity types:', err)
    });
  }

  loadCategories(): void {
    this.eventTypeService.getCategories().subscribe({
      next: (data) => {
        this.categories = data.categories;
      },
      error: (err) => console.error('Failed to load categories:', err)
    });
  }

  applyFilters(): void {
    this.filteredEventTypes = this.eventTypes.filter(et => {
      const matchesSearch = !this.searchTerm ||
        et.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        et.code.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        (et.description && et.description.toLowerCase().includes(this.searchTerm.toLowerCase()));

      return matchesSearch;
    });
  }

  onFilterChange(): void {
    this.loadEventTypes();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  openCreateModal(): void {
    this.formData = {
      name: '',
      code: '',
      description: '',
      entityType: '',
      category: '',
      isActive: true,
      availableFields: '',
      iconClass: ''
    };
    this.showCreateModal = true;
    this.error = null;
  }

  openEditModal(eventType: EventType): void {
    this.currentEventType = eventType;
    this.formData = {
      name: eventType.name,
      code: eventType.code,
      description: eventType.description,
      entityType: eventType.entityType,
      category: eventType.category,
      isActive: eventType.isActive,
      availableFields: eventType.availableFields,
      iconClass: eventType.iconClass
    };
    this.showEditModal = true;
    this.error = null;
  }

  openDeleteModal(eventType: EventType): void {
    this.currentEventType = eventType;
    this.showDeleteModal = true;
    this.error = null;
  }

  closeModals(): void {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.showDeleteModal = false;
    this.currentEventType = null;
    this.error = null;
  }

  createEventType(): void {
    if (!this.validateForm()) return;

    this.loading = true;
    this.error = null;

    this.eventTypeService.createEventType(this.formData).subscribe({
      next: () => {
        this.successMessage = 'Event type created successfully';
        this.closeModals();
        this.loadEventTypes();
        this.loadEntityTypes();
        this.loadCategories();
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to create event type';
        this.loading = false;
      }
    });
  }

  updateEventType(): void {
    if (!this.validateForm() || !this.currentEventType) return;

    this.loading = true;
    this.error = null;

    const updateRequest: UpdateEventTypeRequest = {
      id: this.currentEventType.id,
      ...this.formData
    };

    this.eventTypeService.updateEventType(this.currentEventType.id, updateRequest).subscribe({
      next: () => {
        this.successMessage = 'Event type updated successfully';
        this.closeModals();
        this.loadEventTypes();
        this.loadEntityTypes();
        this.loadCategories();
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to update event type';
        this.loading = false;
      }
    });
  }

  deleteEventType(): void {
    if (!this.currentEventType) return;

    this.loading = true;
    this.error = null;

    this.eventTypeService.deleteEventType(this.currentEventType.id).subscribe({
      next: () => {
        this.successMessage = 'Event type deleted successfully';
        this.closeModals();
        this.loadEventTypes();
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to delete event type';
        this.loading = false;
      }
    });
  }

  validateForm(): boolean {
    if (!this.formData.name.trim()) {
      this.error = 'Name is required';
      return false;
    }
    if (!this.formData.code.trim()) {
      this.error = 'Code is required';
      return false;
    }
    if (!this.formData.entityType.trim()) {
      this.error = 'Entity Type is required';
      return false;
    }
    return true;
  }

  formatDate(dateString: string): string {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } catch {
      return 'Invalid Date';
    }
  }
}
