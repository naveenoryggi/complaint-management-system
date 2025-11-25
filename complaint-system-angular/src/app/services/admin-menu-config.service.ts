import { Injectable } from '@angular/core';

export interface MenuItem {
  label: string;
  route: string;
  icon: string;
  badge?: string;
  permission?: string;
}

export interface MenuCategory {
  id: string;
  label: string;
  icon: string;
  color: string;
  order: number;
  items: MenuItem[];
  expanded?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AdminMenuConfigService {
  private menuCategories: MenuCategory[] = [
    {
      id: 'dashboard-reports',
      label: 'Dashboard & Reports',
      icon: 'bi-speedometer2',
      color: '#4CAF50',
      order: 1,
      expanded: false,
      items: [
        { label: 'Company Settings', route: 'company-settings', icon: 'bi-building' }
      ]
    },
    {
      id: 'user-management',
      label: 'User Management',
      icon: 'bi-people-fill',
      color: '#2196F3',
      order: 2,
      expanded: false,
      items: [
        { label: 'Users', route: 'users', icon: 'bi-people' },
        { label: 'Roles & Permissions', route: 'roles', icon: 'bi-shield-lock' },
        { label: 'Password Management', route: 'password-management', icon: 'bi-key-fill', badge: 'New', permission: 'ManageUsers' },
        { label: 'Employee Types', route: 'employee-types', icon: 'bi-person-badge' },
        { label: 'Resource Pools', route: 'resource-pools', icon: 'bi-people-fill', badge: 'New' }
      ]
    },
    {
      id: 'org-structure',
      label: 'Organizational Structure',
      icon: 'bi-diagram-3-fill',
      color: '#FF9800',
      order: 3,
      expanded: false,
      items: [
        { label: 'Branches', route: 'branches', icon: 'bi-geo-alt' },
        { label: 'Departments', route: 'departments', icon: 'bi-building-gear' },
        { label: 'Sections', route: 'sections', icon: 'bi-boxes' }
      ]
    },
    {
      id: 'complaint-config',
      label: 'Complaint Configuration',
      icon: 'bi-gear-fill',
      color: '#9C27B0',
      order: 4,
      expanded: false,
      items: [
        { label: 'Categories', route: 'categories', icon: 'bi-tags' },
        { label: 'Status Masters', route: 'status-masters', icon: 'bi-circle' },
        { label: 'Priority Masters', route: 'priority-masters', icon: 'bi-flag' },
        { label: 'SLA Management', route: 'sla-management', icon: 'bi-clock-history', badge: 'New', permission: 'ViewSLA' },
        { label: 'Workflow Management', route: 'workflow-management', icon: 'bi-diagram-2', badge: 'New' },
        { label: 'Complaint Settings', route: 'complaint-info-settings', icon: 'bi-sliders' }
      ]
    },
    {
      id: 'communication',
      label: 'Communication Settings',
      icon: 'bi-megaphone-fill',
      color: '#00BCD4',
      order: 5,
      expanded: false,
      items: [
        { label: 'Email Settings', route: 'email-settings', icon: 'bi-envelope-at' },
        { label: 'Email Ticketing', route: 'email-ticketing-config', icon: 'bi-envelope-open-text', badge: 'New', permission: 'ManageSettings' },
        { label: 'SMS Gateway', route: 'sms-gateway', icon: 'bi-phone' },
        { label: 'WhatsApp Settings', route: 'whatsapp-settings', icon: 'bi-whatsapp' },
        { label: 'Templates', route: 'templates', icon: 'bi-file-earmark-text' },
        { label: 'Event Types', route: 'event-types', icon: 'bi-calendar-event' },
        { label: 'Notification Rules', route: 'notification-rules', icon: 'bi-bell' }
      ]
    },
    {
      id: 'integrations',
      label: 'Integrations & Automation',
      icon: 'bi-arrow-repeat',
      color: '#F44336',
      order: 6,
      expanded: false,
      items: [
        { label: 'Oryggi Sync', route: 'oryggi-sync', icon: 'bi-arrow-repeat' },
        { label: 'Escalation Matrix', route: 'escalation-matrix', icon: 'bi-diagram-3' },
        { label: 'Escalation Policy', route: 'escalation-policy', icon: 'bi-shield-check' }
      ]
    }
  ];

  constructor() { }

  /**
   * Get all menu categories
   */
  getMenuCategories(): MenuCategory[] {
    return this.menuCategories.sort((a, b) => a.order - b.order);
  }

  /**
   * Toggle expansion state of a category
   */
  toggleCategory(categoryId: string): void {
    const category = this.menuCategories.find(c => c.id === categoryId);
    if (category) {
      category.expanded = !category.expanded;
    }
  }

  /**
   * Collapse all categories
   */
  collapseAll(): void {
    this.menuCategories.forEach(c => c.expanded = false);
  }

  /**
   * Expand a specific category and collapse others
   */
  expandCategory(categoryId: string): void {
    this.menuCategories.forEach(c => {
      c.expanded = c.id === categoryId;
    });
  }

  /**
   * Get category by ID
   */
  getCategoryById(categoryId: string): MenuCategory | undefined {
    return this.menuCategories.find(c => c.id === categoryId);
  }

  /**
   * Search menu items
   */
  searchMenuItems(searchTerm: string): MenuItem[] {
    if (!searchTerm || searchTerm.trim() === '') {
      return [];
    }

    const term = searchTerm.toLowerCase();
    const results: MenuItem[] = [];

    this.menuCategories.forEach(category => {
      category.items.forEach(item => {
        if (item.label.toLowerCase().includes(term) ||
            item.route.toLowerCase().includes(term)) {
          results.push(item);
        }
      });
    });

    return results;
  }
}
