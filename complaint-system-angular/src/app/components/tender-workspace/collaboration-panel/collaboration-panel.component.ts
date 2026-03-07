import { Component, OnInit, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { signal, effect } from '@angular/core';
import { inject } from '@angular/core';
import { TenderService } from '../../../services/tender.service';

export interface TeamAssignment {
  user_id: string;
  user_name: string;
  role: string;
  assigned_at?: string;
}

export interface TenderComment {
  id: string;
  user_id: string;
  user_name: string;
  text: string;
  created_at: string;
  parent_id?: string | null;
}

export interface CommentsState {
  items: TenderComment[];
  total: number;
  page: number;
}

@Component({
  selector: 'app-collaboration-panel',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatDividerModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './collaboration-panel.component.html',
  styleUrls: ['./collaboration-panel.component.css'],
})
export class CollaborationPanelComponent implements OnInit {
  tenderId = input<string>('');

  private tenderService = inject(TenderService);
  private snackBar = inject(MatSnackBar);

  assignments = signal<TeamAssignment[]>([]);
  comments = signal<CommentsState>({ items: [], total: 0, page: 1 });
  loading = signal(false);
  newComment = signal('');
  newAssignment = signal<{ user_id: string; user_name: string; role: string }>({
    user_id: '',
    user_name: '',
    role: 'member',
  });
  showAssignForm = signal(false);

  roleOptions = [
    { value: 'lead', label: 'Lead' },
    { value: 'member', label: 'Member' },
    { value: 'reviewer', label: 'Reviewer' },
  ];

  constructor() {
    effect(() => {
      const id = this.tenderId();
      if (id) {
        this.loadAssignments();
        this.loadComments();
      }
    });
  }

  ngOnInit(): void {}

  loadAssignments(): void {
    const id = this.tenderId();
    if (!id) return;
    this.loading.set(true);
    (this.tenderService as any)
      .getTeamAssignments(id)
      .subscribe({
        next: (data: TeamAssignment[]) => {
          this.assignments.set(data || []);
          this.loading.set(false);
        },
        error: (err: any) => {
          console.error('Error loading team assignments:', err);
          this.assignments.set([]);
          this.loading.set(false);
        },
      });
  }

  loadComments(): void {
    const id = this.tenderId();
    if (!id) return;
    (this.tenderService as any)
      .getTenderComments(id, this.comments().page)
      .subscribe({
        next: (data: CommentsState) => {
          this.comments.set(data || { items: [], total: 0, page: 1 });
        },
        error: (err: any) => {
          console.error('Error loading comments:', err);
          this.comments.set({ items: [], total: 0, page: 1 });
        },
      });
  }

  addAssignment(): void {
    const id = this.tenderId();
    const form = this.newAssignment();
    if (!id || !form.user_id || !form.user_name) {
      this.snackBar.open('Please fill in all team member fields.', 'Close', { duration: 3000 });
      return;
    }
    this.loading.set(true);
    (this.tenderService as any)
      .addTeamAssignment(id, form)
      .subscribe({
        next: () => {
          this.snackBar.open('Team member added.', 'Close', { duration: 2500 });
          this.newAssignment.set({ user_id: '', user_name: '', role: 'member' });
          this.showAssignForm.set(false);
          this.loadAssignments();
        },
        error: (err: any) => {
          this.snackBar.open(err.error?.detail || 'Failed to add team member.', 'Close', { duration: 5000 });
          this.loading.set(false);
        },
      });
  }

  removeAssignment(userId: string): void {
    const id = this.tenderId();
    if (!id) return;
    this.loading.set(true);
    (this.tenderService as any)
      .removeTeamAssignment(id, userId)
      .subscribe({
        next: () => {
          this.snackBar.open('Team member removed.', 'Close', { duration: 2500 });
          this.loadAssignments();
        },
        error: (err: any) => {
          this.snackBar.open(err.error?.detail || 'Failed to remove team member.', 'Close', { duration: 5000 });
          this.loading.set(false);
        },
      });
  }

  addComment(): void {
    const id = this.tenderId();
    const text = this.newComment().trim();
    if (!id || !text) {
      this.snackBar.open('Comment cannot be empty.', 'Close', { duration: 3000 });
      return;
    }
    (this.tenderService as any)
      .addTenderComment(id, { text })
      .subscribe({
        next: () => {
          this.newComment.set('');
          this.loadComments();
        },
        error: (err: any) => {
          this.snackBar.open(err.error?.detail || 'Failed to post comment.', 'Close', { duration: 5000 });
        },
      });
  }

  deleteComment(commentId: string): void {
    const id = this.tenderId();
    if (!id) return;
    (this.tenderService as any)
      .deleteTenderComment(id, commentId)
      .subscribe({
        next: () => {
          this.snackBar.open('Comment deleted.', 'Close', { duration: 2500 });
          this.loadComments();
        },
        error: (err: any) => {
          this.snackBar.open(err.error?.detail || 'Failed to delete comment.', 'Close', { duration: 5000 });
        },
      });
  }

  formatDate(date: string): string {
    if (!date) return '';
    return new Date(date).toLocaleString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  getRoleLabel(value: string): string {
    return this.roleOptions.find(r => r.value === value)?.label ?? value;
  }

  getInitials(name: string): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  toggleAssignForm(): void {
    this.showAssignForm.update(v => !v);
  }

  updateNewAssignmentField(field: keyof { user_id: string; user_name: string; role: string }, value: string): void {
    this.newAssignment.update(f => ({ ...f, [field]: value }));
  }
}
