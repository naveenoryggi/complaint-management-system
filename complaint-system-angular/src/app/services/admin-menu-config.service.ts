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
        { label: 'Company Settings', route: 'company-settings', icon: 'fa-building' },
        { label: 'License Management', route: 'license', icon: 'fa-key', badge: 'New', permission: 'ManageSettings' }
      ]
    },
    {
      id: 'enterprise-modules',
      label: 'Enterprise Modules',
      icon: 'fa-cubes',
      color: '#673AB7',
      order: 2,
      expanded: false,
      items: [
        { label: 'CRM / Customers', route: 'crm', icon: 'fa-address-book', badge: 'Pro', permission: 'ManageCRM' },
        { label: 'Product Catalog', route: 'products', icon: 'fa-box', badge: 'Pro', permission: 'ManageProducts' },
        { label: 'Product Categories', route: 'products/categories', icon: 'fa-folder-tree', badge: 'Pro', permission: 'ManageProducts' },
        { label: 'Product Brands', route: 'products/brands', icon: 'fa-award', badge: 'Pro', permission: 'ManageProducts' },
        { label: 'Product Sub-Types', route: 'products/subtypes', icon: 'fa-layer-group', badge: 'Pro', permission: 'ManageProducts' },
        { label: 'Contracts', route: 'contracts', icon: 'fa-file-contract', badge: 'Pro', permission: 'ManageContracts' },
        { label: 'Warranty Definitions', route: 'warranties', icon: 'fa-shield-halved', badge: 'Pro', permission: 'ManageContracts' },
        { label: 'Projects', route: 'projects', icon: 'fa-diagram-project', badge: 'Pro', permission: 'ManageProjects' },
        { label: 'Asset Management', route: 'assets', icon: 'fa-server', badge: 'Pro', permission: 'ManageAssets' },
        { label: 'Asset Assignments', route: 'asset-assignments', icon: 'fa-hand-holding-hand', badge: 'Pro', permission: 'ManageAssets' },
        { label: 'Stores', route: 'stores', icon: 'fa-store', badge: 'Pro', permission: 'ManageAssets' },
        { label: 'Asset Requests', route: 'asset-requests', icon: 'fa-clipboard-check', badge: 'Pro', permission: 'ManageAssets' },
        { label: 'Stock Categories', route: 'stock-categories', icon: 'fa-boxes-stacked', badge: 'Pro', permission: 'ManageAssets' },
        { label: 'Stock Items', route: 'stock-items', icon: 'fa-cubes-stacked', badge: 'Pro', permission: 'ManageAssets' },
        { label: 'Stock Movements', route: 'stock-movements', icon: 'fa-exchange-alt', badge: 'Pro', permission: 'ManageAssets' },
        { label: 'Customer Portal', route: 'portal-settings', icon: 'fa-globe', badge: 'Pro', permission: 'ManagePortal' }
      ]
    },
    {
      id: 'user-management',
      label: 'User Management',
      icon: 'fa-users',
      color: '#2196F3',
      order: 3,
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
      order: 4,
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
      order: 5,
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
      order: 6,
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
      id: 'tender-automation',
      label: 'Tender Automation',
      icon: 'fa-file-invoice',
      color: '#FF6F00',
      order: 7,
      expanded: false,
      items: [
        { label: 'Tender Dashboard', route: '/tender-dashboard', icon: 'fa-gauge-high', badge: 'New' },
        { label: 'All Tenders', route: '/tenders', icon: 'fa-file-lines' },
        { label: 'New Tender', route: '/tenders/new', icon: 'fa-plus' },
        { label: 'Company Profile', route: '/company-profile', icon: 'fa-building', badge: 'New' },
        { label: 'Reference Bundles', route: '/reference-bundles', icon: 'fa-folder-open', badge: 'New' },
        { label: 'OEM Management', route: '/oem-management', icon: 'fa-industry', badge: 'New' },
        { label: 'Portal Registrations', route: '/portal-registrations', icon: 'fa-globe', badge: 'New' },
        { label: 'EMD & Fees Tracker', route: '/emd-tracker', icon: 'fa-landmark', badge: 'New' },
        { label: 'Company Documents', route: '/company-documents', icon: 'fa-folder-open', badge: 'New' },
        { label: 'AI Generator', route: '/ai-generator', icon: 'fa-robot' },
        { label: 'AI Provider Settings', route: '/admin/ai-provider-settings', icon: 'fa-microchip', badge: 'New' }
      ]
    },
    {
      id: 'integrations',
      label: 'Integrations & Automation',
      icon: 'fa-rotate',
      color: '#F44336',
      order: 8,
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
