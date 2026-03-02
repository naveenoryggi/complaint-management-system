import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ReferenceBundleService, TenderCriteria, ScoringSimulation, BundleCriteriaAssignment, ReferenceBundle } from '../../../services/reference-bundle.service';

@Component({
  selector: 'app-criteria-tab',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './criteria-tab.component.html',
  styleUrls: ['./criteria-tab.component.css']
})
export class CriteriaTabComponent implements OnInit {
  @Input() tenderId!: string;

  private bundleService = inject(ReferenceBundleService);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  criteria = signal<TenderCriteria[]>([]);
  simulation = signal<ScoringSimulation | null>(null);
  bundles = signal<ReferenceBundle[]>([]);
  assignments = signal<BundleCriteriaAssignment[]>([]);
  loading = signal(false);
  simulating = signal(false);
  showCreateForm = signal(false);
  showAssignForm = signal(false);

  createForm!: FormGroup;
  assignForm!: FormGroup;

  displayedColumns: string[] = ['criteria_code', 'description', 'stage', 'max_marks', 'evidence_required', 'actions'];
  simColumns: string[] = ['criteria_code', 'max_marks', 'predicted_marks', 'gap_analysis'];

  stageOptions = [
    { value: 'technical', label: 'Technical' },
    { value: 'financial', label: 'Financial' },
    { value: 'combined', label: 'Combined' }
  ];

  ngOnInit() {
    this.createForm = this.fb.group({
      criteria_code: ['', Validators.required],
      description: ['', Validators.required],
      stage: ['technical', Validators.required],
      max_marks: [0, [Validators.required, Validators.min(0)]],
      evidence_required: ['']
    });

    this.assignForm = this.fb.group({
      criteria_id: ['', Validators.required],
      bundle_id: ['', Validators.required],
      predicted_marks: [null],
      notes: ['']
    });

    if (this.tenderId) {
      this.loadCriteria();
      this.loadBundles();
      this.loadAssignments();
    }
  }

  loadCriteria() {
    this.loading.set(true);
    this.bundleService.listTenderCriteria(this.tenderId).subscribe({
      next: (data) => {
        this.criteria.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading criteria:', error);
        this.snackBar.open('Failed to load criteria', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  onCreateCriteria() {
    if (this.createForm.invalid) return;

    this.bundleService.createTenderCriteria(this.tenderId, this.createForm.value).subscribe({
      next: () => {
        this.createForm.reset({ stage: 'technical', max_marks: 0 });
        this.showCreateForm.set(false);
        this.loadCriteria();
        this.snackBar.open('Criteria added', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error creating criteria:', error);
        this.snackBar.open('Failed to add criteria', 'Close', { duration: 3000 });
      }
    });
  }

  onSimulateScoring() {
    this.simulating.set(true);
    this.bundleService.simulateScoring(this.tenderId).subscribe({
      next: (result) => {
        this.simulation.set(result);
        this.simulating.set(false);
        this.snackBar.open('Scoring simulation complete', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Simulation error:', error);
        this.simulating.set(false);
        this.snackBar.open('Scoring simulation failed', 'Close', { duration: 3000 });
      }
    });
  }

  getTotalMaxMarks(): number {
    return this.criteria().reduce((sum, c) => sum + (c.max_marks || 0), 0);
  }

  getTechnicalMarks(): number {
    return this.criteria().filter(c => c.stage === 'technical').reduce((sum, c) => sum + (c.max_marks || 0), 0);
  }

  getFinancialMarks(): number {
    return this.criteria().filter(c => c.stage === 'financial').reduce((sum, c) => sum + (c.max_marks || 0), 0);
  }

  getScorePercent(): number {
    const sim = this.simulation();
    if (!sim || !sim.total_max_marks) return 0;
    return Math.round((sim.total_predicted_marks / sim.total_max_marks) * 100);
  }

  onDeleteCriteria(criteria: TenderCriteria) {
    if (confirm(`Delete criteria "${criteria.criteria_code}"?`)) {
      this.bundleService.deleteTenderCriteria(criteria.id).subscribe({
        next: () => {
          this.loadCriteria();
          this.loadAssignments();
          this.snackBar.open('Criteria deleted', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error deleting criteria:', error);
          this.snackBar.open('Failed to delete criteria', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getStageClass(stage: string): string {
    const map: { [key: string]: string } = {
      'technical': 'stage-technical',
      'financial': 'stage-financial',
      'combined': 'stage-combined'
    };
    return map[stage] || '';
  }

  loadBundles() {
    this.bundleService.listBundles(1, 100).subscribe({
      next: (response) => this.bundles.set(response.items),
      error: () => {}
    });
  }

  loadAssignments() {
    this.bundleService.listTenderAssignments(this.tenderId).subscribe({
      next: (data) => this.assignments.set(data),
      error: () => {}
    });
  }

  onAssignBundle() {
    if (this.assignForm.invalid) return;

    this.bundleService.assignBundleToCriteria(this.assignForm.value).subscribe({
      next: () => {
        this.assignForm.reset();
        this.showAssignForm.set(false);
        this.loadAssignments();
        this.snackBar.open('Bundle assigned to criteria', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error assigning bundle:', error);
        this.snackBar.open('Failed to assign bundle', 'Close', { duration: 3000 });
      }
    });
  }

  getAssignmentsForCriteria(criteriaId: string): BundleCriteriaAssignment[] {
    return this.assignments().filter(a => a.criteria_id === criteriaId);
  }

  getBundleName(bundleId: string): string {
    const bundle = this.bundles().find(b => b.id === bundleId);
    return bundle ? `${bundle.bundle_name} (${bundle.client_name})` : bundleId;
  }
}
