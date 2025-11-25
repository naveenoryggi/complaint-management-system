import { Component, Input, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SetupProgressService, SetupStep, SetupPhase } from '../../../services/setup-progress.service';

@Component({
  selector: 'app-dependency-matrix',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dependency-matrix.component.html',
  styleUrls: ['./dependency-matrix.component.scss']
})
export class DependencyMatrixComponent implements OnInit {
  @Input() showFullMatrix = false;
  @Input() highlightStep?: string;

  phases = computed(() => this.setupService.phases());
  selectedPhase = signal<number | null>(null);
  selectedStep = signal<SetupStep | null>(null);

  constructor(
    public setupService: SetupProgressService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.highlightStep) {
      const step = this.setupService.getStepById(this.highlightStep);
      if (step) {
        this.selectedStep.set(step);
        this.selectedPhase.set(step.phase);
      }
    }
  }

  /**
   * Select a phase to view its steps
   */
  selectPhase(phase: number): void {
    this.selectedPhase.set(this.selectedPhase() === phase ? null : phase);
  }

  /**
   * Select a step to view its dependencies
   */
  selectStep(step: SetupStep): void {
    this.selectedStep.set(this.selectedStep()?.id === step.id ? null : step);
  }

  /**
   * Get visual indicator for step status
   */
  getStepStatusIcon(step: SetupStep): string {
    if (step.completed) {
      return 'bi-check-circle-fill';
    }

    const validation = this.setupService.validateDependencies(step.id);
    if (!validation.canProceed) {
      return 'bi-lock-fill';
    }

    return 'bi-circle';
  }

  /**
   * Get color class for step status
   */
  getStepStatusClass(step: SetupStep): string {
    if (step.completed) {
      return 'status-completed';
    }

    const validation = this.setupService.validateDependencies(step.id);
    if (!validation.canProceed) {
      return 'status-locked';
    }

    return step.required ? 'status-required' : 'status-optional';
  }

  /**
   * Navigate to a step if available
   */
  navigateToStep(step: SetupStep): void {
    const validation = this.setupService.validateDependencies(step.id);

    if (!validation.canProceed) {
      alert(`Cannot proceed. Missing dependencies:\n${validation.message}`);
      return;
    }

    if (step.route) {
      this.router.navigate([step.route]);
    }
  }

  /**
   * Get dependency chain for a step
   */
  getDependencyChain(stepId: string): SetupStep[] {
    const chain: SetupStep[] = [];
    const visited = new Set<string>();

    const buildChain = (id: string) => {
      if (visited.has(id)) return;
      visited.add(id);

      const step = this.setupService.getStepById(id);
      if (!step) return;

      step.dependsOn.forEach(depId => {
        buildChain(depId);
      });

      chain.push(step);
    };

    buildChain(stepId);
    return chain;
  }

  /**
   * Get steps that depend on this step
   */
  getDependentSteps(stepId: string): SetupStep[] {
    return this.setupService.getSteps().filter(step =>
      step.dependsOn.includes(stepId)
    );
  }

  /**
   * Get phase color
   */
  getPhaseColor(phase: number): string {
    const colors = {
      1: '#4CAF50',  // Green - Foundation
      2: '#2196F3',  // Blue - Core
      3: '#FF9800',  // Orange - Automation
      4: '#9C27B0'   // Purple - Customization
    };
    return colors[phase as keyof typeof colors] || '#757575';
  }
}
