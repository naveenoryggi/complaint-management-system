import { Injectable, signal, computed } from '@angular/core';

export interface SetupStep {
  id: string;
  label: string;
  description: string;
  category: SetupCategory;
  phase: number;
  required: boolean;
  completed: boolean;
  dependsOn: string[];
  route?: string;
  icon: string;
  estimatedMinutes: number;
}

export type SetupCategory = 'foundation' | 'core' | 'automation' | 'customization';

export interface SetupPhase {
  phase: number;
  name: string;
  description: string;
  steps: SetupStep[];
  completed: boolean;
  progress: number;
}

export interface DependencyValidationResult {
  canProceed: boolean;
  missingDependencies: SetupStep[];
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class SetupProgressService {
  private readonly STORAGE_KEY = 'complaint_system_setup_progress';

  private steps = signal<SetupStep[]>([
    // PHASE 1: FOUNDATION
    {
      id: 'company-settings',
      label: 'Company Settings',
      description: 'Configure company name, logo, and contact information',
      category: 'foundation',
      phase: 1,
      required: true,
      completed: false,
      dependsOn: [],
      route: '/admin/company-settings',
      icon: 'bi-building',
      estimatedMinutes: 5
    },
    {
      id: 'branches',
      label: 'Branches',
      description: 'Set up organizational branches/locations',
      category: 'foundation',
      phase: 1,
      required: true,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/branches',
      icon: 'bi-geo-alt',
      estimatedMinutes: 10
    },
    {
      id: 'departments',
      label: 'Departments',
      description: 'Create departments within branches',
      category: 'foundation',
      phase: 1,
      required: true,
      completed: false,
      dependsOn: ['branches'],
      route: '/admin/departments',
      icon: 'bi-building-gear',
      estimatedMinutes: 10
    },
    {
      id: 'sections',
      label: 'Sections',
      description: 'Define sections within departments',
      category: 'foundation',
      phase: 1,
      required: false,
      completed: false,
      dependsOn: ['departments'],
      route: '/admin/sections',
      icon: 'bi-boxes',
      estimatedMinutes: 10
    },
    {
      id: 'employee-types',
      label: 'Employee Types',
      description: 'Configure employee type classifications',
      category: 'foundation',
      phase: 1,
      required: true,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/employee-types',
      icon: 'bi-person-badge',
      estimatedMinutes: 5
    },
    {
      id: 'roles',
      label: 'Roles & Permissions',
      description: 'Set up user roles and access control',
      category: 'foundation',
      phase: 1,
      required: true,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/roles',
      icon: 'bi-shield-lock',
      estimatedMinutes: 10
    },
    {
      id: 'users',
      label: 'Users',
      description: 'Create user accounts and assign roles',
      category: 'foundation',
      phase: 1,
      required: true,
      completed: false,
      dependsOn: ['employee-types', 'roles', 'departments'],
      route: '/admin/users',
      icon: 'bi-people',
      estimatedMinutes: 15
    },

    // PHASE 2: CORE FEATURES
    {
      id: 'categories',
      label: 'Complaint Categories',
      description: 'Define complaint categories',
      category: 'core',
      phase: 2,
      required: true,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/categories',
      icon: 'bi-tags',
      estimatedMinutes: 10
    },
    {
      id: 'status-masters',
      label: 'Status Masters',
      description: 'Configure complaint status workflow',
      category: 'core',
      phase: 2,
      required: true,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/status-masters',
      icon: 'bi-circle',
      estimatedMinutes: 10
    },
    {
      id: 'priority-masters',
      label: 'Priority Masters',
      description: 'Set up priority levels',
      category: 'core',
      phase: 2,
      required: true,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/priority-masters',
      icon: 'bi-flag',
      estimatedMinutes: 10
    },
    {
      id: 'complaint-settings',
      label: 'Complaint Settings',
      description: 'Configure complaint system settings',
      category: 'core',
      phase: 2,
      required: true,
      completed: false,
      dependsOn: ['status-masters', 'priority-masters', 'categories'],
      route: '/admin/complaint-info-settings',
      icon: 'bi-sliders',
      estimatedMinutes: 5
    },
    {
      id: 'resource-pools',
      label: 'Resource Pools',
      description: 'Create resource pools for assignment',
      category: 'core',
      phase: 2,
      required: false,
      completed: false,
      dependsOn: ['users'],
      route: '/admin/resource-pools',
      icon: 'bi-people-fill',
      estimatedMinutes: 15
    },

    // PHASE 3: AUTOMATION
    {
      id: 'email-settings',
      label: 'Email Settings',
      description: 'Configure email server for notifications',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/email-settings',
      icon: 'bi-envelope-at',
      estimatedMinutes: 10
    },
    {
      id: 'sms-gateway',
      label: 'SMS Gateway',
      description: 'Set up SMS notifications',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/sms-gateway',
      icon: 'bi-phone',
      estimatedMinutes: 10
    },
    {
      id: 'whatsapp-settings',
      label: 'WhatsApp Settings',
      description: 'Configure WhatsApp integration',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/whatsapp-settings',
      icon: 'bi-whatsapp',
      estimatedMinutes: 10
    },
    {
      id: 'event-types',
      label: 'Event Types',
      description: 'Define event types for notifications',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/event-types',
      icon: 'bi-calendar-event',
      estimatedMinutes: 10
    },
    {
      id: 'templates',
      label: 'Communication Templates',
      description: 'Create email/SMS/WhatsApp templates',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['email-settings'],
      route: '/admin/templates',
      icon: 'bi-file-earmark-text',
      estimatedMinutes: 15
    },
    {
      id: 'notification-rules',
      label: 'Notification Rules',
      description: 'Set up automated notification rules',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['templates', 'event-types'],
      route: '/admin/notification-rules',
      icon: 'bi-bell',
      estimatedMinutes: 15
    },
    {
      id: 'global-sla-settings',
      label: 'Global SLA Settings',
      description: 'Configure global Service Level Agreement settings',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/global-sla-settings',
      icon: 'bi-clock-history',
      estimatedMinutes: 10
    },
    {
      id: 'sla-levels',
      label: 'SLA Levels',
      description: 'Create SLA level tiers (e.g., Standard, Premium, Enterprise)',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['global-sla-settings'],
      route: '/admin/sla-levels',
      icon: 'bi-bar-chart-steps',
      estimatedMinutes: 15
    },
    {
      id: 'category-sla',
      label: 'Category SLA Configuration',
      description: 'Set SLA timeframes for each complaint category',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['categories', 'sla-levels'],
      route: '/admin/category-sla',
      icon: 'bi-tags-fill',
      estimatedMinutes: 20
    },
    {
      id: 'priority-sla',
      label: 'Priority SLA Configuration',
      description: 'Define SLA timeframes for each priority level',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['priority-masters', 'sla-levels'],
      route: '/admin/priority-sla',
      icon: 'bi-flag-fill',
      estimatedMinutes: 15
    },
    {
      id: 'escalation-policy',
      label: 'Escalation Policy',
      description: 'Define escalation policies and rules',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['resource-pools', 'notification-rules', 'priority-sla', 'category-sla'],
      route: '/admin/escalation-policy',
      icon: 'bi-shield-check',
      estimatedMinutes: 20
    },
    {
      id: 'escalation-matrix',
      label: 'Escalation Matrix',
      description: 'Configure escalation matrix with SLA-based triggers',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['escalation-policy', 'sla-levels'],
      route: '/admin/escalation-matrix',
      icon: 'bi-diagram-3',
      estimatedMinutes: 25
    },
    {
      id: 'oryggi-sync',
      label: 'Oryggi Sync',
      description: 'Configure external Oryggi integration',
      category: 'automation',
      phase: 3,
      required: false,
      completed: false,
      dependsOn: ['company-settings'],
      route: '/admin/oryggi-sync',
      icon: 'bi-arrow-repeat',
      estimatedMinutes: 30
    },

    // PHASE 4: CUSTOMIZATION
    {
      id: 'dashboard-customization',
      label: 'Dashboard Customization',
      description: 'Customize dashboard widgets and preferences',
      category: 'customization',
      phase: 4,
      required: false,
      completed: false,
      dependsOn: ['complaint-settings'],
      route: '/admin/dashboard-settings',
      icon: 'bi-sliders',
      estimatedMinutes: 5
    }
  ]);

  // Computed values using Angular signals
  public allSteps = computed(() => this.steps());

  public totalSteps = computed(() => this.steps().length);

  public completedSteps = computed(() =>
    this.steps().filter(step => step.completed)
  );

  public requiredSteps = computed(() =>
    this.steps().filter(step => step.required)
  );

  public completedRequiredSteps = computed(() =>
    this.steps().filter(step => step.required && step.completed)
  );

  public overallProgress = computed(() => {
    const total = this.totalSteps();
    const completed = this.completedSteps().length;
    return total > 0 ? Math.round((completed / total) * 100) : 0;
  });

  public requiredProgress = computed(() => {
    const total = this.requiredSteps().length;
    const completed = this.completedRequiredSteps().length;
    return total > 0 ? Math.round((completed / total) * 100) : 0;
  });

  public isSetupComplete = computed(() =>
    this.requiredProgress() === 100
  );

  public phases = computed(() => {
    const phaseMap = new Map<number, SetupPhase>();
    const phaseNames = {
      1: { name: 'Foundation', description: 'Basic system configuration' },
      2: { name: 'Core Features', description: 'Complaint system setup' },
      3: { name: 'Automation', description: 'Advanced automation features' },
      4: { name: 'Customization', description: 'Optional customizations' }
    };

    this.steps().forEach(step => {
      if (!phaseMap.has(step.phase)) {
        phaseMap.set(step.phase, {
          phase: step.phase,
          name: phaseNames[step.phase as keyof typeof phaseNames]?.name || `Phase ${step.phase}`,
          description: phaseNames[step.phase as keyof typeof phaseNames]?.description || '',
          steps: [],
          completed: false,
          progress: 0
        });
      }

      const phase = phaseMap.get(step.phase)!;
      phase.steps.push(step);
    });

    // Calculate completion for each phase
    phaseMap.forEach(phase => {
      const completedInPhase = phase.steps.filter(s => s.completed).length;
      phase.progress = phase.steps.length > 0
        ? Math.round((completedInPhase / phase.steps.length) * 100)
        : 0;
      phase.completed = phase.progress === 100;
    });

    return Array.from(phaseMap.values()).sort((a, b) => a.phase - b.phase);
  });

  constructor() {
    this.loadProgress();
  }

  /**
   * Get all setup steps
   */
  getSteps(): SetupStep[] {
    return this.steps();
  }

  /**
   * Get a specific step by ID
   */
  getStepById(stepId: string): SetupStep | undefined {
    return this.steps().find(step => step.id === stepId);
  }

  /**
   * Check if a step's dependencies are met
   */
  validateDependencies(stepId: string): DependencyValidationResult {
    const step = this.getStepById(stepId);

    if (!step) {
      return {
        canProceed: false,
        missingDependencies: [],
        message: 'Step not found'
      };
    }

    if (step.dependsOn.length === 0) {
      return {
        canProceed: true,
        missingDependencies: [],
        message: 'No dependencies'
      };
    }

    const missingDeps = step.dependsOn
      .map(depId => this.getStepById(depId))
      .filter((dep): dep is SetupStep => dep !== undefined && !dep.completed);

    const canProceed = missingDeps.length === 0;

    return {
      canProceed,
      missingDependencies: missingDeps,
      message: canProceed
        ? 'All dependencies met'
        : `Please complete: ${missingDeps.map(d => d.label).join(', ')}`
    };
  }

  /**
   * Check if a step is available (dependencies met)
   */
  isStepAvailable(stepId: string): boolean {
    return this.validateDependencies(stepId).canProceed;
  }

  /**
   * Mark a step as completed
   */
  markStepCompleted(stepId: string): void {
    this.steps.update(steps => {
      const step = steps.find(s => s.id === stepId);
      if (step) {
        step.completed = true;
      }
      return [...steps];
    });
    this.saveProgress();
  }

  /**
   * Mark a step as incomplete
   */
  markStepIncomplete(stepId: string): void {
    this.steps.update(steps => {
      const step = steps.find(s => s.id === stepId);
      if (step) {
        step.completed = false;
      }
      return [...steps];
    });
    this.saveProgress();
  }

  /**
   * Get the next recommended step
   */
  getNextStep(): SetupStep | null {
    const incomplete = this.steps().filter(step => !step.completed);

    // First, try to find an incomplete required step with met dependencies
    for (const step of incomplete) {
      if (step.required && this.isStepAvailable(step.id)) {
        return step;
      }
    }

    // Then, try any incomplete step with met dependencies
    for (const step of incomplete) {
      if (this.isStepAvailable(step.id)) {
        return step;
      }
    }

    return null;
  }

  /**
   * Get steps by category
   */
  getStepsByCategory(category: SetupCategory): SetupStep[] {
    return this.steps().filter(step => step.category === category);
  }

  /**
   * Get steps by phase
   */
  getStepsByPhase(phase: number): SetupStep[] {
    return this.steps().filter(step => step.phase === phase);
  }

  /**
   * Reset all progress (for testing/demo)
   */
  resetProgress(): void {
    this.steps.update(steps => {
      steps.forEach(step => step.completed = false);
      return [...steps];
    });
    this.saveProgress();
  }

  /**
   * Auto-detect existing setup from API data
   */
  async detectExistingSetup(): Promise<void> {
    // This would make API calls to check if features have been configured
    // For now, we'll just mark it as a placeholder
    console.log('Detecting existing setup configuration...');

    // Example: Check if company settings exist
    // const hasCompany = await this.apiService.getCompanySettings().toPromise();
    // if (hasCompany) this.markStepCompleted('company-settings');

    // This would be implemented with actual API checks
  }

  /**
   * Get estimated time remaining
   */
  getEstimatedTimeRemaining(): number {
    return this.steps()
      .filter(step => !step.completed)
      .reduce((total, step) => total + step.estimatedMinutes, 0);
  }

  /**
   * Save progress to localStorage
   */
  private saveProgress(): void {
    const progress = this.steps().map(step => ({
      id: step.id,
      completed: step.completed
    }));

    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify({
        progress,
        lastUpdated: new Date().toISOString()
      }));
    } catch (error) {
      console.error('Failed to save setup progress:', error);
    }
  }

  /**
   * Load progress from localStorage
   */
  private loadProgress(): void {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;

      const { progress } = JSON.parse(stored);

      this.steps.update(steps => {
        progress.forEach((saved: { id: string; completed: boolean }) => {
          const step = steps.find(s => s.id === saved.id);
          if (step) {
            step.completed = saved.completed;
          }
        });
        return [...steps];
      });
    } catch (error) {
      console.error('Failed to load setup progress:', error);
    }
  }

  /**
   * Export progress as JSON (for debugging/reporting)
   */
  exportProgress(): string {
    return JSON.stringify({
      overall: {
        totalSteps: this.totalSteps(),
        completedSteps: this.completedSteps().length,
        progress: this.overallProgress(),
        requiredProgress: this.requiredProgress(),
        isComplete: this.isSetupComplete(),
        estimatedTimeRemaining: this.getEstimatedTimeRemaining()
      },
      phases: this.phases(),
      steps: this.steps()
    }, null, 2);
  }
}
