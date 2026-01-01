import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login';
import { authGuard } from './guards/auth.guard';
import { portalAuthGuard } from './guards/portal-auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },

  // Portal Routes
  {
    path: 'portal/login',
    loadComponent: () => import('./components/portal/portal-login/portal-login.component').then(m => m.PortalLoginComponent)
  },
  {
    path: 'portal',
    loadComponent: () => import('./components/portal/portal-layout/portal-layout.component').then(m => m.PortalLayoutComponent),
    canActivate: [portalAuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./components/portal/portal-dashboard/portal-dashboard.component').then(m => m.PortalDashboardComponent)
      },
      {
        path: 'tickets',
        loadComponent: () => import('./components/portal/portal-ticket-list/portal-ticket-list.component').then(m => m.PortalTicketListComponent)
      },
      {
        path: 'tickets/new',
        loadComponent: () => import('./components/portal/portal-ticket-create/portal-ticket-create.component').then(m => m.PortalTicketCreateComponent)
      }
    ]
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./components/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./components/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/dashboard').then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'complaints',
    loadComponent: () => import('./components/complaints/complaint-list/complaint-list.component').then(m => m.ComplaintListComponent),
    canActivate: [authGuard]
  },
  {
    path: 'complaints/new',
    loadComponent: () => import('./components/complaints/complaint-form/complaint-form.component').then(m => m.ComplaintFormComponent),
    canActivate: [authGuard]
  },
  {
    path: 'complaints/:id',
    loadComponent: () => import('./components/complaints/complaint-detail/complaint-detail.component').then(m => m.ComplaintDetailComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/company-settings',
    loadComponent: () => import('./components/admin/company-settings/company-settings').then(m => m.CompanySettings),
    canActivate: [authGuard]
  },
  {
    path: 'admin/escalation-matrix',
    loadComponent: () => import('./components/admin/escalation-matrix/escalation-matrix.component').then(m => m.EscalationMatrixComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/escalation-policy',
    loadComponent: () => import('./components/admin/escalation-policy/escalation-policy.component').then(m => m.EscalationPolicyComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/escalation-wizard',
    loadComponent: () => import('./components/admin/escalation-wizard/escalation-wizard.component').then(m => m.EscalationWizardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/categories',
    loadComponent: () => import('./components/admin/category-management/category-management.component').then(m => m.CategoryManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/status-masters',
    loadComponent: () => import('./components/admin/status-master-management/status-master-management.component').then(m => m.StatusMasterManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/priority-masters',
    loadComponent: () => import('./components/admin/priority-master-management/priority-master-management.component').then(m => m.PriorityMasterManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/branches',
    loadComponent: () => import('./components/admin/branch-management/branch-management.component').then(m => m.BranchManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/departments',
    loadComponent: () => import('./components/admin/department-management/department-management.component').then(m => m.DepartmentManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/sections',
    loadComponent: () => import('./components/admin/section-management/section-management.component').then(m => m.SectionManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/employee-types',
    loadComponent: () => import('./components/admin/employee-type-management/employee-type-management.component').then(m => m.EmployeeTypeManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/users',
    loadComponent: () => import('./components/admin/user-management/user-management.component').then(m => m.UserManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/roles',
    loadComponent: () => import('./components/admin/role-management/role-management.component').then(m => m.RoleManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/oryggi-sync',
    loadComponent: () => import('./components/admin/oryggi-sync/oryggi-sync.component').then(m => m.OryggiSyncComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/complaint-info-settings',
    loadComponent: () => import('./components/admin/complaint-info-settings/complaint-info-settings.component').then(m => m.ComplaintInfoSettingsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/email-settings',
    loadComponent: () => import('./components/admin/email-settings/email-settings-management.component').then(m => m.EmailSettingsManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/email-ticketing-config',
    loadComponent: () => import('./components/admin/email-ticketing-config/email-ticketing-config.component').then(m => m.EmailTicketingConfigComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/templates',
    loadComponent: () => import('./components/admin/template-management/template-management.component').then(m => m.TemplateManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/notification-rules',
    loadComponent: () => import('./components/admin/notification-rule-management/notification-rule-management.component').then(m => m.NotificationRuleManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/event-types',
    loadComponent: () => import('./components/admin/event-type-management/event-type-management.component').then(m => m.EventTypeManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/sms-gateway',
    loadComponent: () => import('./components/admin/sms-gateway-management/sms-gateway-management.component').then(m => m.SmsGatewayManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/whatsapp-settings',
    loadComponent: () => import('./components/admin/whatsapp-settings-management/whatsapp-settings-management.component').then(m => m.WhatsAppSettingsManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/resource-pools',
    loadComponent: () => import('./components/admin/resource-pool-management/resource-pool-management.component').then(m => m.ResourcePoolManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/sla-management',
    loadComponent: () => import('./components/admin/sla-management/sla-management.component').then(m => m.SLAManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/workflow-management',
    loadComponent: () => import('./components/admin/workflow-management/workflow-management.component').then(m => m.WorkflowManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/password-management',
    loadComponent: () => import('./components/admin/password-management/password-management.component').then(m => m.PasswordManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/license',
    loadComponent: () => import('./components/admin/license-management/license-management.component').then(m => m.LicenseManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/crm',
    loadComponent: () => import('./components/crm/customer-management/customer-management.component').then(m => m.CustomerManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'crm/customers',
    loadComponent: () => import('./components/crm/customer-management/customer-management.component').then(m => m.CustomerManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/products',
    loadComponent: () => import('./components/crm/product-management/product-management.component').then(m => m.ProductManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/products/categories',
    loadComponent: () => import('./components/crm/product-category/product-category.component').then(m => m.ProductCategoryComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/products/brands',
    loadComponent: () => import('./components/crm/brand-management/brand-management.component').then(m => m.BrandManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/products/subtypes',
    loadComponent: () => import('./components/crm/product-subtype-management/product-subtype-management.component').then(m => m.ProductSubTypeManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'crm/products',
    loadComponent: () => import('./components/crm/product-management/product-management.component').then(m => m.ProductManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/contracts',
    loadComponent: () => import('./components/crm/contract-management/contract-management.component').then(m => m.ContractManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'crm/contracts',
    loadComponent: () => import('./components/crm/contract-management/contract-management.component').then(m => m.ContractManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/warranties',
    loadComponent: () => import('./components/crm/warranty-management/warranty-management.component').then(m => m.WarrantyManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'crm/warranties',
    loadComponent: () => import('./components/crm/warranty-management/warranty-management.component').then(m => m.WarrantyManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/projects',
    loadComponent: () => import('./components/crm/project-management/project-management.component').then(m => m.ProjectManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'crm/projects',
    loadComponent: () => import('./components/crm/project-management/project-management.component').then(m => m.ProjectManagementComponent),
    canActivate: [authGuard]
  },
  {
    path: 'change-password',
    loadComponent: () => import('./components/shared/change-password/change-password.component').then(m => m.ChangePasswordComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/notification-preferences',
    loadComponent: () => import('./components/admin/notification-preferences/notification-preferences.component').then(m => m.NotificationPreferencesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'notifications',
    loadComponent: () => import('./components/notifications/notification-center/notification-center.component').then(m => m.NotificationCenterComponent),
    canActivate: [authGuard]
  },
  {
    path: 'team-performance',
    loadComponent: () => import('./components/team-performance/team-performance.component').then(m => m.TeamPerformanceComponent),
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: '/login' }
];
