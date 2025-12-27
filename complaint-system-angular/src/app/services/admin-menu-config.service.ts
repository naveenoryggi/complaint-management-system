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
      icon: 'fa-gauge-high',
      color: '#4CAF50',
      order: 1,
      expanded: false,
      items: [
        { label: 'Company Settings', route: 'company-settings', icon: 'fa-building' }
      ]
    },
    {
      id: 'user-management',
      label: 'User Management',
      icon: 'fa-users',
      color: '#2196F3',
      order: 2,
      expanded: false,
      items: [
        { label: 'Users', route: 'users', icon: 'fa-users' },
        { label: 'Roles & Permissions', route: 'roles', icon: 'fa-shield-halved' },
        { label: 'Password Management', route: 'password-management', icon: 'fa-key', badge: 'New', permission: 'ManageUsers' },
        { label: 'Employee Types', route: 'employee-types', icon: 'fa-id-badge' },
        { label: 'Resource Pools', route: 'resource-pools', icon: 'fa-users-rectangle', badge: 'New' }
      ]
    },
    {
      id: 'org-structure',
      label: 'Organizational Structure',
      icon: 'fa-sitemap',
      color: '#FF9800',
      order: 3,
      expanded: false,
      items: [
        { label: 'Branches', route: 'branches', icon: 'fa-location-dot' },
        { label: 'Departments', route: 'departments', icon: 'fa-building-user' },
        { label: 'Sections', route: 'sections', icon: 'fa-cubes' }
      ]
    },
    {
      id: 'complaint-config',
      label: 'Complaint Configuration',
      icon: 'fa-gears',
      color: '#9C27B0',
      order: 4,
      expanded: false,
      items: [
        { label: 'Categories', route: 'categories', icon: 'fa-tags' },
        { label: 'Status Masters', route: 'status-masters', icon: 'fa-circle' },
        { label: 'Priority Masters', route: 'priority-masters', icon: 'fa-flag' },
        { label: 'SLA Management', route: 'sla-management', icon: 'fa-clock-rotate-left', badge: 'New', permission: 'ViewSLA' },
        { label: 'Workflow Management', route: 'workflow-management', icon: 'fa-diagram-project', badge: 'New' },
        { label: 'Complaint Settings', route: 'complaint-info-settings', icon: 'fa-sliders' }
      ]
    },
    {
      id: 'communication',
      label: 'Communication Settings',
      icon: 'fa-bullhorn',
      color: '#00BCD4',
      order: 5,
      expanded: false,
      items: [
        { label: 'Email Settings', route: 'email-settings', icon: 'fa-at' },
        { label: 'Email Ticketing', route: 'email-ticketing-config', icon: 'fa-envelope-open-text', badge: 'New', permission: 'ManageSettings' },
        { label: 'SMS Gateway', route: 'sms-gateway', icon: 'fa-mobile-screen' },
        { label: 'WhatsApp Settings', route: 'whatsapp-settings', icon: 'fa-brands fa-whatsapp' },
        { label: 'Templates', route: 'templates', icon: 'fa-file-lines' },
        { label: 'Event Types', route: 'event-types', icon: 'fa-calendar-days' },
        { label: 'Notification Rules', route: 'notification-rules', icon: 'fa-bell' },
        { label: 'Notification Preferences', route: 'notification-preferences', icon: 'fa-sliders', badge: 'New' }
      ]
    },
    {
      id: 'integrations',
      label: 'Integrations & Automation',
      icon: 'fa-rotate',
      color: '#F44336',
      order: 6,
      expanded: false,
      items: [
        { label: 'Oryggi Sync', route: 'oryggi-sync', icon: 'fa-rotate' },
        { label: 'Escalation Matrix', route: 'escalation-matrix', icon: 'fa-sitemap' },
        { label: 'Escalation Policy', route: 'escalation-policy', icon: 'fa-list-check' }
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
