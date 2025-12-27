import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, forkJoin, Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs/operators';
import { ComplaintService } from '../../../services/complaint.service';
import { UserService } from '../../../services/user.service';
import { BranchService } from '../../../services/branch.service';
import { DepartmentService } from '../../../services/department.service';
import { SectionService } from '../../../services/section.service';
import { AuthService } from '../../../services/auth.service';
import { DateService } from '../../../services/date.service';
import { MasterDataService } from '../../../services/master-data.service';
import { WorkflowService } from '../../../services/workflow.service';
import { CommentService, CreateCommentRequest, ApiResponse } from '../../../services/comment.service';
import { NavigationService } from '../../../services/navigation.service';
import { CategoryService } from '../../../services/category.service';
import { Comment } from '../../../models/comment.model';
import { Complaint, PreferredContactMethod, ComplaintHistory, ComplaintHistoryEvent, UpdateComplaintRequest } from '../../../models/complaint.model';
import { User } from '../../../models/user.model';
import { Branch } from '../../../models/branch.model';
import { Department } from '../../../models/department.model';
import { Section } from '../../../models/section.model';
import { Category } from '../../../models/category.model';
import { CategoryWorkflowTransition } from '../../../models/workflow.model';
import {
  AdvancedAssignmentMethod,
  AssignmentCriteria,
  AutoAssignRequest,
  AssignToPoolRequest,
  ResourcePoolCandidate,
  AssignmentCandidateOptions,
  UserCandidate
} from '../../../models/assignment.model';
import { SLAInfoPanelComponent } from '../../shared/sla-info-panel/sla-info-panel.component';
import { EmailThreadViewerComponent, EmailReplyEvent } from '../../shared/email-thread-viewer/email-thread-viewer.component';
import { EmailComposerModalComponent } from '../../shared/email-composer-modal/email-composer-modal.component';
import { EmailReplyComposerComponent } from '../../shared/email-reply-composer/email-reply-composer.component';
import { EmailThreadItemDto, EmailThreadService } from '../../../services/email-thread.service';
import { ReplyType } from '../../../models/communication.model';

// Interface for @mention users
interface MentionUser {
  id: string;
  displayName: string;
  email: string;
  role: string;
}

@Component({
  selector: 'app-complaint-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, SLAInfoPanelComponent, EmailThreadViewerComponent, EmailComposerModalComponent, EmailReplyComposerComponent],
  templateUrl: './complaint-detail.component.html',
  styleUrls: ['./complaint-detail.component.scss']
})
export class ComplaintDetailComponent implements OnInit, OnDestroy {
  complaint: Complaint | null = null;
  loading = false;
  error: string | null = null;
  actionLoading = false;
  actionError: string | null = null;
  successMessage: string | null = null;

  // For assign modal
  showAssignModal = false;
  selectedUserId = '';
  users: User[] = [];
  filteredUsers: User[] = [];
  branches: Branch[] = [];
  departments: Department[] = [];
  sections: Section[] = [];
  userSearchTerm = '';
  filterBranchId = '';
  filterDepartmentId = '';
  filterSectionId = '';
  loadingUsers = false;
  private userSearchSubject = new Subject<string>();
  minSearchLength = 3;

  // For escalate modal
  showEscalateModal = false;
  escalationReason = '';

  // For close modal
  showCloseModal = false;
  resolutionNotes = '';

  // For reopen modal
  showReopenModal = false;
  reopenReason = '';

  // For history
  complaintHistory: ComplaintHistory | null = null;
  loadingHistory = false;
  historyError: string | null = null;
  showHistory = false;

  // Advanced Assignment Properties
  assignmentType: 'auto' | 'pool' | 'user' = 'auto';
  loadingCandidates = false;
  hasCandidates = false;
  assignmentCandidates: ResourcePoolCandidate[] = [];

  // Auto assignment properties
  autoAssignReason = '';
  autoAssignPriority = 'normal';
  forceAssignment = false;
  bypassRules = false;

  // Resource pool properties
  selectedResourcePoolId = '';
  poolAssignmentMethod = AdvancedAssignmentMethod.BestFit;
  availableResourcePools: any[] = [];
  poolMembers: UserCandidate[] = [];
  selectedPoolMemberId = '';

  // Additional success message property
  actionSuccess: string | null = null;

  // Comment functionality
  comments: Comment[] = [];
  loadingComments = false;
  commentError: string | null = null;
  newComment = '';
  isInternalNote = false; // Toggle for internal notes
  submittingComment = false;
  showComments = true;

  // Workflow transition functionality
  availableTransitions: CategoryWorkflowTransition[] = [];
  loadingTransitions = false;
  showTransitionModal = false;
  selectedTransition: CategoryWorkflowTransition | null = null;
  transitionComment = '';

  // Master data arrays
  priorities: any[] = [];
  statuses: any[] = [];
  categories: Category[] = [];

  // Tab navigation
  activeTab: 'details' | 'complainant' | 'thread' | 'comments' = 'details';

  // Email thread tracking
  unreadEmailCount = 0;

  // Unread comments tracking (comments not yet seen by user)
  unreadCommentCount = 0;
  private seenCommentIds: Set<string> = new Set();

  // @Mention functionality
  showMentionDropdown = false;
  mentionSuggestions: MentionUser[] = [];
  mentionedUsers: MentionUser[] = [];
  selectedMentionIndex = 0;
  mentionSearchTerm = '';
  private mentionTriggerPosition = -1;
  private allMentionableUsers: MentionUser[] = [];

  // Edit mode properties
  isEditMode = false;
  editForm: {
    title: string;
    description: string;
    categoryId: string;
    priorityMasterId: string;
    statusMasterId: string;
    assignedToId: string;
    tags: string;
  } = {
    title: '',
    description: '',
    categoryId: '',
    priorityMasterId: '',
    statusMasterId: '',
    assignedToId: '',
    tags: ''
  };

  // Original complaint data for cancel functionality
  private originalComplaint: Complaint | null = null;

  // Email composer modal properties
  showEmailComposerModal = false;
  emailComposerDefaultTo = '';
  emailComposerDefaultSubject = '';
  emailComposerReplyToEmailId?: string;

  // Email reply composer properties
  showEmailReplyComposer = false;
  emailReplyTo?: EmailThreadItemDto;
  emailThread?: EmailThreadItemDto[]; // Full email thread for forwarding
  emailReplyType: ReplyType = ReplyType.Reply;
  emailInitialContent: string = ''; // Content from quick reply textbox
  ReplyType = ReplyType; // Expose enum to template

  // Destroy subject for subscription cleanup
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private complaintService: ComplaintService,
    private userService: UserService,
    private branchService: BranchService,
    private departmentService: DepartmentService,
    private sectionService: SectionService,
    private authService: AuthService,
    private dateService: DateService,
    private masterDataService: MasterDataService,
    private commentService: CommentService,
    private workflowService: WorkflowService,
    private navigationService: NavigationService,
    private categoryService: CategoryService,
    private emailThreadService: EmailThreadService
  ) {
    // Setup debounced user search
    this.userSearchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term || term.length < this.minSearchLength) {
          this.filteredUsers = [];
          this.loadingUsers = false;
          return [];
        }
        this.loadingUsers = true;
        return this.userService.searchUsers(term, undefined, 50);
      })
    ).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.filteredUsers = response.data;
        } else {
          this.filteredUsers = [];
        }
        this.loadingUsers = false;
      },
      error: (err) => {
        console.error('Error searching users:', err);
        this.filteredUsers = [];
        this.loadingUsers = false;
      }
    });
  }

  ngOnInit(): void {
    // Subscribe to route parameter changes to handle navigation between complaints
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const id = params.get('id');
        if (id) {
          // Reset component state for new complaint
          this.resetComponentState();

          // Load master data first, then load complaint
          this.loadMasterData()
            .then(() => {
              // Master data loaded successfully, now load complaint
              this.loadComplaint(id);
            })
            .catch((error: any) => {
              console.error('Failed to load master data:', error);
              // Still load complaint even if master data fails
              this.loadComplaint(id);
            });
        }
      });

    // Load mentionable users for @mention functionality
    this.loadMentionableUsers();
  }

  /**
   * Reset component state when navigating to a different complaint
   */
  private resetComponentState(): void {
    this.complaint = null;
    this.loading = false;
    this.error = null;
    this.comments = [];
    this.complaintHistory = null;
    this.showHistory = false;
    this.availableTransitions = [];
    this.unreadEmailCount = 0;
    this.unreadCommentCount = 0;
    this.activeTab = 'details';
    this.isEditMode = false;
    this.showEmailReplyComposer = false;

    // Reset modals
    this.showAssignModal = false;
    this.showEscalateModal = false;
    this.showCloseModal = false;
    this.showReopenModal = false;
    this.showTransitionModal = false;

    // Clear messages
    this.successMessage = null;
    this.actionError = null;
    this.actionSuccess = null;
  }

  ngOnDestroy(): void {
    this.userSearchSubject.complete();
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadComplaint(id: string): void {
    this.loading = true;
    this.error = null;

    this.complaintService.getComplaintById(id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaint = response.data;

          // Update breadcrumb with actual complaint number
          if (this.complaint.complaintNumber) {
            this.navigationService.setPageTitle(`Complaint ${this.complaint.complaintNumber}`);
            this.navigationService.updateLastBreadcrumb(
              `Complaint ${this.complaint.complaintNumber}`,
              'bi-file-text'
            );
          }

          // Load comments, workflow transitions, and email count after complaint is loaded
          this.loadComments();
          this.loadAllowedTransitions();
          this.loadUnreadEmailCount();
        } else {
          this.error = response.message || 'Failed to load complaint';
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load complaint details';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadAllowedTransitions(): void {
    if (!this.complaint?.categoryId || !this.complaint?.statusId) return;

    this.loadingTransitions = true;
    this.availableTransitions = [];

    this.workflowService.getAllowedTransitions(this.complaint.categoryId, this.complaint.statusId)
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.availableTransitions = response.data.transitions || [];
          }
          this.loadingTransitions = false;
        },
        error: (err) => {
          console.error('Error loading workflow transitions:', err);
          // Silently fail - this is optional functionality
          this.loadingTransitions = false;
        }
      });
  }

  getPreferredContactLabel(method: PreferredContactMethod): string {
    switch(method) {
      case PreferredContactMethod.Email: return 'Email Only';
      case PreferredContactMethod.Phone: return 'Phone Only';
      case PreferredContactMethod.Both: return 'Email & Phone';
      case PreferredContactMethod.SMS: return 'SMS';
      case PreferredContactMethod.InApp: return 'In-App Notification';
      default: return 'Not Specified';
    }
  }

  // Load master data for dynamic priority/status display
  private loadMasterData(): Promise<void> {
    // Load status, priority, and category options simultaneously
    return new Promise((resolve, reject) => {
      forkJoin({
        statusOptions: this.masterDataService.getStatusOptions(),
        priorityOptions: this.masterDataService.getPriorityOptions(),
        categories: this.categoryService.getCategories(true)
      }).subscribe({
        next: (result) => {
          console.log('Master data loaded successfully:', result);
          // Store the master data arrays
          this.statuses = result.statusOptions.map(s => ({
            id: s.value,
            name: s.label,
            description: s.description
          }));
          this.priorities = result.priorityOptions.map(p => ({
            id: p.value || p.id,
            name: p.label,
            description: p.description
          }));
          // Store categories
          if (result.categories.isSuccess && result.categories.data) {
            this.categories = result.categories.data;
          }
          resolve();
        },
        error: (error) => {
          console.error('Failed to load master data:', error);
          reject(error);
        }
      });
    });
  }

  // Updated getPriorityLabel and getPriorityClass methods are at the end of the class
// Updated getStatusLabel and getStatusClass methods are at the end of the class

  formatDate(dateString: string | null | undefined): string {
    if (!dateString) return '-';

    // Use DateService which respects user's configured timezone
    return this.dateService.formatDate(dateString);
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  goHome(): void {
    this.router.navigate(['/dashboard']);
  }

  assignComplaint(): void {
    if (!this.complaint || !this.selectedUserId) return;

    this.actionLoading = true;
    this.actionError = null;

    this.complaintService.assignComplaint(this.complaint.id, this.selectedUserId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaint = response.data;
          this.successMessage = 'Complaint assigned successfully';
          this.showAssignModal = false;
          this.selectedUserId = '';
          setTimeout(() => this.successMessage = null, 3000);
        }
        this.actionLoading = false;
      },
      error: (err) => {
        this.actionError = 'Failed to assign complaint';
        this.actionLoading = false;
        console.error(err);
      }
    });
  }

  escalateComplaint(): void {
    if (!this.complaint || !this.escalationReason.trim()) return;

    this.actionLoading = true;
    this.actionError = null;

    this.complaintService.escalateComplaint(this.complaint.id, this.escalationReason).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaint = response.data;
          this.successMessage = 'Complaint escalated successfully';
          this.showEscalateModal = false;
          this.escalationReason = '';
          setTimeout(() => this.successMessage = null, 3000);
        }
        this.actionLoading = false;
      },
      error: (err) => {
        this.actionError = 'Failed to escalate complaint';
        this.actionLoading = false;
        console.error(err);
      }
    });
  }

  closeComplaint(): void {
    if (!this.complaint || !this.resolutionNotes.trim()) return;

    this.actionLoading = true;
    this.actionError = null;

    this.complaintService.closeComplaint(this.complaint.id, this.resolutionNotes).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaint = response.data;
          this.successMessage = 'Complaint closed successfully';
          this.showCloseModal = false;
          this.resolutionNotes = '';
          setTimeout(() => this.successMessage = null, 3000);
        }
        this.actionLoading = false;
      },
      error: (err) => {
        this.actionError = 'Failed to close complaint';
        this.actionLoading = false;
        console.error(err);
      }
    });
  }

  reopenComplaint(): void {
    if (!this.complaint || !this.reopenReason.trim()) return;

    this.actionLoading = true;
    this.actionError = null;

    this.complaintService.reopenComplaint(this.complaint.id, this.reopenReason).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaint = response.data;
          this.successMessage = 'Complaint reopened successfully';
          this.showReopenModal = false;
          this.reopenReason = '';
          setTimeout(() => this.successMessage = null, 3000);
        }
        this.actionLoading = false;
      },
      error: (err) => {
        this.actionError = 'Failed to reopen complaint';
        this.actionLoading = false;
        console.error(err);
      }
    });
  }

  openAssignModal(): void {
    this.showAssignModal = true;
    this.actionError = null;
    this.loadAssignmentData();
  }

  loadAssignmentData(): void {
    // Don't load all users - use search instead
    this.filteredUsers = [];
    this.userSearchTerm = '';
    this.selectedUserId = '';
  }

  onUserSearchChange(): void {
    this.userSearchSubject.next(this.userSearchTerm);
  }

  getUserDisplayName(user: User): string {
    return `${user.fullName} (${user.email}${user.employeeCode ? ' - ' + user.employeeCode : ''})`;
  }

  openEscalateModal(): void {
    this.showEscalateModal = true;
    this.actionError = null;
  }

  openCloseModal(): void {
    this.showCloseModal = true;
    this.actionError = null;
  }

  openReopenModal(): void {
    this.showReopenModal = true;
    this.actionError = null;
  }

  closeModal(): void {
    this.showAssignModal = false;
    this.showEscalateModal = false;
    this.showCloseModal = false;
    this.showReopenModal = false;
    this.selectedUserId = '';
    this.escalationReason = '';
    this.resolutionNotes = '';
    this.reopenReason = '';
    this.actionError = null;
    this.userSearchTerm = '';
    this.filterBranchId = '';
    this.filterDepartmentId = '';
    this.filterSectionId = '';
  }

  canAssign(): boolean {
    if (!this.complaint?.status) return false;

    const status = typeof this.complaint.status === 'string'
      ? this.complaint.status.toLowerCase()
      : this.complaint.status;

    return status !== 'closed' && status !== 'resolved';
  }

  canEscalate(): boolean {
    if (!this.complaint?.status) return false;

    const status = typeof this.complaint.status === 'string'
      ? this.complaint.status.toLowerCase()
      : this.complaint.status;

    return status !== 'closed' && status !== 'resolved' && status !== 'escalated';
  }

  canClose(): boolean {
    if (!this.complaint?.status) return false;

    const status = typeof this.complaint.status === 'string'
      ? this.complaint.status.toLowerCase()
      : this.complaint.status;

    return status !== 'closed';
  }

  canReopen(): boolean {
    if (!this.complaint?.status) return false;

    const status = typeof this.complaint.status === 'string'
      ? this.complaint.status.toLowerCase()
      : this.complaint.status;

    return status === 'closed' || status === 'resolved';
  }

  toggleHistory(): void {
    this.showHistory = !this.showHistory;
    if (this.showHistory && !this.complaintHistory && this.complaint) {
      this.loadHistory(this.complaint.id);
    }
  }

  loadHistory(complaintId: string): void {
    this.loadingHistory = true;
    this.historyError = null;

    this.complaintService.getComplaintHistory(complaintId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaintHistory = response.data;
        } else {
          this.historyError = response.message || 'Failed to load complaint history';
        }
        this.loadingHistory = false;
      },
      error: (err) => {
        this.historyError = 'Failed to load complaint history';
        this.loadingHistory = false;
        console.error(err);
      }
    });
  }

  getEventIcon(eventType: string): string {
    switch (eventType.toLowerCase()) {
      case 'comment':
        return 'bi-chat-left-text';
      case 'statuschange':
        return 'bi-arrow-left-right';
      case 'assignment':
        return 'bi-person-check';
      case 'escalation':
        return 'bi-arrow-up-circle';
      case 'created':
        return 'bi-plus-circle';
      case 'reopened':
        return 'bi-arrow-counterclockwise';
      case 'closed':
        return 'bi-check-circle';
      default:
        return 'bi-record-circle';
    }
  }

  getEventClass(eventType: string): string {
    switch (eventType.toLowerCase()) {
      case 'comment':
        return 'text-info';
      case 'statuschange':
        return 'text-primary';
      case 'assignment':
        return 'text-success';
      case 'escalation':
        return 'text-danger';
      case 'created':
        return 'text-success';
      case 'reopened':
        return 'text-warning';
      case 'closed':
        return 'text-success';
      default:
        return 'text-secondary';
    }
  }

  // Advanced Assignment Methods

  showAssignmentCandidates(): void {
    if (!this.complaint) return;

    this.loadingCandidates = true;
    this.assignmentCandidates = [];

    const options: AssignmentCandidateOptions = {
      maxCandidates: 10,
      includeUserDetails: true,
      calculateSkillScores: true
    };

    this.complaintService.getAssignmentCandidates(this.complaint.id, options)
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.assignmentCandidates = response.data;
            this.hasCandidates = response.data.length > 0;
          } else {
            this.actionError = response.message || 'Failed to get assignment candidates';
          }
          this.loadingCandidates = false;
        },
        error: (err) => {
          console.error('Error getting assignment candidates:', err);
          this.actionError = 'Failed to get assignment candidates';
          this.loadingCandidates = false;
        }
      });
  }

  performAutoAssign(): void {
    if (!this.complaint) return;

    this.actionLoading = true;
    this.actionError = null;
    this.actionSuccess = null;

    const request: AutoAssignRequest = {
      forceAssignment: this.forceAssignment,
      assignmentReason: this.autoAssignReason || 'Auto Assignment',
      bypassRules: this.bypassRules,
      notes: 'Auto assigned from complaint detail'
    };

    this.complaintService.autoAssignComplaint(this.complaint.id, request)
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.complaint = response.data;
            this.actionSuccess = response.message || 'Complaint auto-assigned successfully';
            this.showAssignModal = false;
            setTimeout(() => this.actionSuccess = null, 3000);
          } else {
            this.actionError = response.message || 'Failed to auto-assign complaint';
          }
          this.actionLoading = false;
        },
        error: (err) => {
          console.error('Error auto-assigning complaint:', err);
          this.actionError = 'Failed to auto-assign complaint';
          this.actionLoading = false;
        }
      });
  }

  performPoolAssign(): void {
    if (!this.complaint || !this.selectedResourcePoolId) return;

    this.actionLoading = true;
    this.actionError = null;
    this.actionSuccess = null;

    const request: AssignToPoolRequest = {
      resourcePoolId: this.selectedResourcePoolId,
      assignmentMethod: this.poolAssignmentMethod,
      specificUserId: this.selectedPoolMemberId || undefined,
      assignmentReason: 'Pool Assignment from complaint detail',
      notes: 'Assigned from resource pool'
    };

    this.complaintService.assignComplaintToPool(this.complaint.id, request)
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.complaint = response.data;
            this.actionSuccess = response.message || 'Complaint assigned from pool successfully';
            this.showAssignModal = false;
            setTimeout(() => this.actionSuccess = null, 3000);
          } else {
            this.actionError = response.message || 'Failed to assign complaint from pool';
          }
          this.actionLoading = false;
        },
        error: (err) => {
          console.error('Error assigning complaint from pool:', err);
          this.actionError = 'Failed to assign complaint from pool';
          this.actionLoading = false;
        }
      });
  }

  performDirectAssign(): void {
    if (!this.complaint || !this.selectedUserId) return;

    this.actionLoading = true;
    this.actionError = null;
    this.actionSuccess = null;

    this.complaintService.assignComplaint(this.complaint.id, this.selectedUserId)
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.complaint = response.data;
            this.actionSuccess = response.message || 'Complaint assigned successfully';
            this.showAssignModal = false;
            setTimeout(() => this.actionSuccess = null, 3000);
          } else {
            this.actionError = response.message || 'Failed to assign complaint';
          }
          this.actionLoading = false;
        },
        error: (err) => {
          console.error('Error assigning complaint:', err);
          this.actionError = 'Failed to assign complaint';
          this.actionLoading = false;
        }
      });
  }

  onResourcePoolChange(): void {
    if (!this.selectedResourcePoolId) {
      this.poolMembers = [];
      return;
    }

    // For now, this is a placeholder - in a real implementation,
    // you would fetch pool members from the API
    this.poolMembers = [];
    this.selectedPoolMemberId = '';
  }

  // Updated type-safe methods to handle string types
  getPriorityClass(priority: string | undefined | null): string {
    // Add explicit null/undefined safety check
    if (priority === null || priority === undefined) return 'badge bg-secondary';

    // Handle string values from API
    if (typeof priority === 'string') {
      switch(priority.toLowerCase()) {
        case 'critical':
        case 'urgent':
        case 'emergency':
        case 'severe':
          return 'badge bg-danger';
        case 'high':
        case 'elevated':
          return 'badge bg-warning';
        case 'normal':
        case 'medium':
          return 'badge bg-primary';
        case 'low':
          return 'badge bg-secondary';
        default:
          return 'badge bg-secondary';
      }
    }

    // Handle numeric enum values
    return this.masterDataService.getPriorityClass(priority);
  }

  getPriorityLabel(priority: string | undefined | null): string {
    // If complaint has priorityId, fetch from master data
    if ((priority === null || priority === undefined) && this.complaint?.priorityId) {
      const priorityMaster = this.priorities.find(p => p.id === this.complaint!.priorityId);
      return priorityMaster?.name || 'Unknown Priority';
    }

    // Add explicit null/undefined safety check
    if (priority === null || priority === undefined) return 'Unknown Priority';

    // Handle string values from API
    if (typeof priority === 'string') {
      return priority; // Return the string value directly from API
    }

    // Handle numeric enum values
    return this.masterDataService.getPriorityName(priority);
  }

  getStatusClass(status: string | undefined | null): string {
    // Add explicit null/undefined safety check
    if (status === null || status === undefined) return 'badge bg-secondary';

    // Handle string values from API
    if (typeof status === 'string') {
      switch(status.toLowerCase()) {
        case 'closed':
        case 'resolved':
          return 'badge bg-success';
        case 'inprogress':
        case 'in progress':
          return 'badge bg-primary';
        case 'escalated':
          return 'badge bg-danger';
        case 'submitted':
        case 'underreview':
        case 'under review':
          return 'badge bg-info';
        case 'pendinginfo':
        case 'pending info':
          return 'badge bg-warning';
        case 'rejected':
          return 'badge bg-dark';
        case 'reopened':
          return 'badge bg-warning';
        default:
          return 'badge bg-secondary';
      }
    }

    // Handle numeric enum values
    return this.masterDataService.getStatusClass(status);
  }

  getStatusLabel(status: string | undefined | null): string {
    // If complaint has statusId, fetch from master data
    if ((status === null || status === undefined) && this.complaint?.statusId) {
      const statusMaster = this.statuses.find(s => s.id === this.complaint!.statusId);
      return statusMaster?.name || 'Unknown Status';
    }

    // Add explicit null/undefined safety check
    if (status === null || status === undefined) return 'Unknown Status';

    // Handle string values from API
    if (typeof status === 'string') {
      return status; // Return the string value directly from API
    }

    // Handle numeric enum values
    return this.masterDataService.getStatusName(status);
  }

  // Badge class helpers for new UI design
  getStatusBadgeClass(status: string | undefined | null): string {
    if (status === null || status === undefined) return 'status-default';
    if (typeof status === 'string') {
      switch(status.toLowerCase()) {
        case 'closed':
        case 'resolved':
          return 'status-success';
        case 'inprogress':
        case 'in progress':
          return 'status-primary';
        case 'escalated':
          return 'status-danger';
        case 'submitted':
        case 'underreview':
        case 'under review':
          return 'status-info';
        case 'pendinginfo':
        case 'pending info':
          return 'status-warning';
        case 'rejected':
          return 'status-dark';
        case 'reopened':
          return 'status-warning';
        default:
          return 'status-default';
      }
    }
    return 'status-default';
  }

  getPriorityBadgeClass(priority: string | undefined | null): string {
    if (priority === null || priority === undefined) return 'priority-default';
    if (typeof priority === 'string') {
      switch(priority.toLowerCase()) {
        case 'critical':
        case 'urgent':
        case 'emergency':
        case 'severe':
          return 'priority-critical';
        case 'high':
        case 'elevated':
          return 'priority-high';
        case 'normal':
        case 'medium':
          return 'priority-medium';
        case 'low':
          return 'priority-low';
        default:
          return 'priority-default';
      }
    }
    return 'priority-default';
  }

  // Email count for tab badge
  loadUnreadEmailCount(): void {
    if (!this.complaint) return;

    this.emailThreadService.getComplaintEmails(this.complaint.id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          // Count unread inbound emails (emails not sent by us that haven't been read)
          this.unreadEmailCount = response.data.filter(
            email => !email.isOutbound && !email.isRead
          ).length;
        }
      },
      error: (err) => {
        console.error('Error loading email count:', err);
        // Silently fail - this is for UI enhancement only
      }
    });
  }

  /**
   * Mark all emails as read when user views the Email Thread tab
   */
  markEmailsAsRead(): void {
    if (!this.complaint || this.unreadEmailCount === 0) return;

    this.emailThreadService.markAllAsRead(this.complaint.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.unreadEmailCount = 0;
        }
      },
      error: (err) => {
        console.error('Error marking emails as read:', err);
        // Silently fail - this is for UI enhancement only
      }
    });
  }

  /**
   * Get the localStorage key for storing seen comment IDs for this complaint
   */
  private getSeenCommentsStorageKey(): string {
    return `complaint_seen_comments_${this.complaint?.id || ''}`;
  }

  /**
   * Load seen comment IDs from localStorage
   */
  private loadSeenCommentIds(): void {
    if (!this.complaint) return;

    try {
      const stored = localStorage.getItem(this.getSeenCommentsStorageKey());
      if (stored) {
        const ids = JSON.parse(stored) as string[];
        this.seenCommentIds = new Set(ids);
      }
    } catch (e) {
      console.error('Error loading seen comment IDs:', e);
      this.seenCommentIds = new Set();
    }
  }

  /**
   * Save seen comment IDs to localStorage
   */
  private saveSeenCommentIds(): void {
    if (!this.complaint) return;

    try {
      const ids = Array.from(this.seenCommentIds);
      localStorage.setItem(this.getSeenCommentsStorageKey(), JSON.stringify(ids));
    } catch (e) {
      console.error('Error saving seen comment IDs:', e);
    }
  }

  /**
   * Mark all current comments as seen
   * Called when user views the Comments tab
   */
  markCommentsAsSeen(): void {
    this.comments.forEach(comment => {
      this.seenCommentIds.add(comment.id);
    });
    this.saveSeenCommentIds();
    this.unreadCommentCount = 0;
  }

  /**
   * Update the count of unread comments (comments not yet seen by user)
   * This helps users identify new activity on the complaint
   */
  updateUnreadCommentCount(): void {
    // Load seen comment IDs from localStorage
    this.loadSeenCommentIds();

    // Count comments that haven't been seen yet
    this.unreadCommentCount = this.comments.filter(comment =>
      !this.seenCommentIds.has(comment.id)
    ).length;
  }

  // Comment functionality methods
  loadComments(): void {
    if (!this.complaint) return;

    this.loadingComments = true;
    this.commentError = null;

    this.commentService.getComments(this.complaint.id, true).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.comments = response.data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
          // Calculate unread comments (not yet seen by user)
          this.updateUnreadCommentCount();
        } else {
          this.commentError = response.message || 'Failed to load comments';
        }
        this.loadingComments = false;
      },
      error: (err) => {
        this.commentError = 'Failed to load comments';
        this.loadingComments = false;
        console.error('Error loading comments:', err);
      }
    });
  }

  addComment(): void {
    if (!this.complaint || !this.newComment.trim()) return;

    this.submittingComment = true;
    this.commentError = null;

    // Collect mentioned user IDs for notification
    const mentionedUserIds = this.mentionedUsers.map(u => u.id);

    const request: CreateCommentRequest = {
      complaintId: this.complaint.id,
      comment: this.newComment.trim(),
      isInternal: this.isInternalNote,
      mentionedUserIds: mentionedUserIds.length > 0 ? mentionedUserIds : undefined
    };

    this.commentService.addComment(request).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          // Add new comment to the beginning of the array
          this.comments.unshift(response.data);
          const wasInternalNote = this.isInternalNote;
          const hadMentions = this.mentionedUsers.length > 0;
          this.newComment = '';
          this.isInternalNote = false; // Reset internal note toggle
          this.mentionedUsers = []; // Reset mentioned users

          // Build success message
          let successMsg = wasInternalNote ? 'Internal note added successfully' : 'Comment posted successfully';
          if (hadMentions) {
            successMsg += '. Notifications sent to mentioned users.';
          }
          this.actionSuccess = successMsg;

          // Mark the new comment as seen (user just added it)
          this.seenCommentIds.add(response.data.id);
          this.saveSeenCommentIds();
          setTimeout(() => this.actionSuccess = null, 4000);

          // Update complaint comment count
          if (this.complaint) {
            this.complaint.commentCount = (this.complaint.commentCount || 0) + 1;
          }
        } else {
          this.commentError = response.message || 'Failed to add comment';
        }
        this.submittingComment = false;
      },
      error: (err) => {
        this.commentError = 'Failed to add comment';
        this.submittingComment = false;
        console.error('Error adding comment:', err);
      }
    });
  }

  toggleComments(): void {
    this.showComments = !this.showComments;
    if (this.showComments && this.comments.length === 0) {
      this.loadComments();
    }
  }

  // ==========================================
  // @Mention Functionality Methods
  // ==========================================

  /**
   * Load mentionable users - staff members who can be mentioned
   */
  loadMentionableUsers(): void {
    // Load all users who can be mentioned (staff members)
    this.userService.getUsers().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.allMentionableUsers = response.data.map((user: User) => ({
            id: user.id,
            displayName: user.fullName || `${user.firstName} ${user.lastName}`,
            email: user.email,
            role: user.roles && user.roles.length > 0 ? user.roles[0].roleName : 'User'
          }));
        }
      },
      error: (err) => {
        console.error('Error loading mentionable users:', err);
      }
    });
  }

  /**
   * Get display name for role
   */
  private getRoleDisplayName(roleId: string): string {
    const roleMap: { [key: string]: string } = {
      'admin': 'Administrator',
      'manager': 'Manager',
      'supervisor': 'Supervisor',
      'agent': 'Support Agent',
      'technician': 'Technician',
      'user': 'User'
    };
    return roleMap[roleId?.toLowerCase()] || roleId || 'Staff';
  }

  /**
   * Get user initials for avatar
   */
  getUserInitials(displayName: string): string {
    if (!displayName) return '??';
    const names = displayName.trim().split(' ');
    if (names.length >= 2) {
      return (names[0][0] + names[names.length - 1][0]).toUpperCase();
    }
    return displayName.substring(0, 2).toUpperCase();
  }

  /**
   * Handle input in comment textarea - detect @mentions
   */
  onCommentInput(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    const cursorPos = textarea.selectionStart;
    const text = textarea.value;

    // Find if we're in a mention context (after @)
    const textBeforeCursor = text.substring(0, cursorPos);
    const lastAtIndex = textBeforeCursor.lastIndexOf('@');

    if (lastAtIndex !== -1) {
      const textAfterAt = textBeforeCursor.substring(lastAtIndex + 1);
      // Check if there's no space after @ (still in mention mode)
      if (!textAfterAt.includes(' ') && !textAfterAt.includes('\n')) {
        this.mentionTriggerPosition = lastAtIndex;
        this.mentionSearchTerm = textAfterAt.toLowerCase();
        this.filterMentionSuggestions();
        this.showMentionDropdown = true;
        this.selectedMentionIndex = 0;
        return;
      }
    }

    // Not in mention mode
    this.hideMentionDropdown();
  }

  /**
   * Handle keydown in comment textarea - navigate mention dropdown
   */
  onCommentKeydown(event: KeyboardEvent): void {
    if (!this.showMentionDropdown || this.mentionSuggestions.length === 0) {
      return;
    }

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.selectedMentionIndex = Math.min(
          this.selectedMentionIndex + 1,
          this.mentionSuggestions.length - 1
        );
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.selectedMentionIndex = Math.max(this.selectedMentionIndex - 1, 0);
        break;
      case 'Enter':
      case 'Tab':
        if (this.showMentionDropdown && this.mentionSuggestions.length > 0) {
          event.preventDefault();
          this.selectMention(this.mentionSuggestions[this.selectedMentionIndex]);
        }
        break;
      case 'Escape':
        event.preventDefault();
        this.hideMentionDropdown();
        break;
    }
  }

  /**
   * Filter mention suggestions based on search term
   */
  private filterMentionSuggestions(): void {
    if (!this.mentionSearchTerm) {
      this.mentionSuggestions = this.allMentionableUsers.slice(0, 5);
    } else {
      this.mentionSuggestions = this.allMentionableUsers
        .filter(user =>
          user.displayName.toLowerCase().includes(this.mentionSearchTerm) ||
          user.email.toLowerCase().includes(this.mentionSearchTerm) ||
          user.role.toLowerCase().includes(this.mentionSearchTerm)
        )
        .slice(0, 5);
    }
  }

  /**
   * Select a user from the mention dropdown
   */
  selectMention(user: MentionUser): void {
    // Get textarea reference
    const textarea = document.querySelector('.composer-textarea') as HTMLTextAreaElement;
    if (!textarea) return;

    const text = this.newComment;
    const beforeMention = text.substring(0, this.mentionTriggerPosition);
    const afterMention = text.substring(textarea.selectionStart);

    // Insert the mention
    this.newComment = beforeMention + '@' + user.displayName + ' ' + afterMention;

    // Add to mentioned users if not already there
    if (!this.mentionedUsers.find(u => u.id === user.id)) {
      this.mentionedUsers.push(user);
    }

    this.hideMentionDropdown();

    // Refocus textarea and set cursor position
    setTimeout(() => {
      const newCursorPos = beforeMention.length + user.displayName.length + 2;
      textarea.focus();
      textarea.setSelectionRange(newCursorPos, newCursorPos);
    }, 0);
  }

  /**
   * Remove a mentioned user
   */
  removeMention(user: MentionUser): void {
    this.mentionedUsers = this.mentionedUsers.filter(u => u.id !== user.id);
    // Also remove from the comment text
    const mentionPattern = new RegExp('@' + user.displayName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + '\\s?', 'g');
    this.newComment = this.newComment.replace(mentionPattern, '');
  }

  /**
   * Hide the mention dropdown
   */
  private hideMentionDropdown(): void {
    this.showMentionDropdown = false;
    this.mentionSuggestions = [];
    this.selectedMentionIndex = 0;
    this.mentionSearchTerm = '';
    this.mentionTriggerPosition = -1;
  }

  // Workflow transition methods
  openTransitionModal(transition: CategoryWorkflowTransition): void {
    this.selectedTransition = transition;
    this.transitionComment = '';
    this.showTransitionModal = true;
    this.actionError = null;
  }

  closeTransitionModal(): void {
    this.showTransitionModal = false;
    this.selectedTransition = null;
    this.transitionComment = '';
    this.actionError = null;
  }

  executeTransition(): void {
    if (!this.complaint || !this.selectedTransition) return;

    // Check if comment is required
    if (this.selectedTransition.requiresComment && !this.transitionComment.trim()) {
      this.actionError = 'A comment is required for this transition';
      return;
    }

    this.actionLoading = true;
    this.actionError = null;

    this.workflowService.transitionComplaint(
      this.complaint.id,
      this.selectedTransition.toStatusId,
      this.transitionComment.trim() || undefined
    ).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.complaint = response.data;
          this.successMessage = `Status changed to ${this.selectedTransition?.toStatusName || 'new status'}`;
          this.closeTransitionModal();
          // Reload allowed transitions for new status
          this.loadAllowedTransitions();
          setTimeout(() => this.successMessage = null, 3000);
        } else {
          this.actionError = response.message || 'Failed to transition complaint';
        }
        this.actionLoading = false;
      },
      error: (err) => {
        this.actionError = 'Failed to transition complaint';
        this.actionLoading = false;
        console.error('Error transitioning complaint:', err);
      }
    });
  }

  // Edit Mode Methods

  /**
   * Check if current user can edit the complaint
   * Handlers can edit complaints assigned to them
   * Admins can edit any complaint
   */
  canEditComplaint(): boolean {
    if (!this.complaint || !this.authService.currentUserValue) {
      return false;
    }

    const currentUser = this.authService.currentUserValue;
    const permissions = currentUser.permissions || [];

    // Admins can edit any complaint
    const isAdmin = permissions.includes('ManageUsers') ||
                    permissions.includes('ManageSettings') ||
                    permissions.includes('ManageCompany');

    if (isAdmin) {
      return true;
    }

    // Handlers can edit complaints assigned to them
    const isHandler = permissions.includes('AssignComplaint') ||
                      permissions.includes('EscalateComplaint');

    if (isHandler && this.complaint.assignedToId === currentUser.id) {
      return true;
    }

    return false;
  }

  /**
   * Enter edit mode - populate edit form with current complaint data
   */
  enterEditMode(): void {
    if (!this.complaint || !this.canEditComplaint()) {
      return;
    }

    // Store original complaint data for cancel functionality
    this.originalComplaint = { ...this.complaint };

    // Populate edit form
    this.editForm = {
      title: this.complaint.title || '',
      description: this.complaint.description || '',
      categoryId: this.complaint.categoryId || '',
      priorityMasterId: this.complaint.priorityId || '',
      statusMasterId: this.complaint.statusId || '',
      assignedToId: this.complaint.assignedToId || '',
      tags: this.complaint.tags || ''
    };

    this.isEditMode = true;
    this.actionError = null;
    this.successMessage = null;
  }

  /**
   * Cancel edit mode - restore original data
   */
  cancelEdit(): void {
    if (this.originalComplaint) {
      // Restore original complaint data
      this.complaint = { ...this.originalComplaint };
    }

    this.isEditMode = false;
    this.actionError = null;
    this.originalComplaint = null;

    // Reset form
    this.editForm = {
      title: '',
      description: '',
      categoryId: '',
      priorityMasterId: '',
      statusMasterId: '',
      assignedToId: '',
      tags: ''
    };
  }

  /**
   * Validate edit form before submission
   */
  private validateEditForm(): string | null {
    // Title is required
    if (!this.editForm.title || !this.editForm.title.trim()) {
      return 'Title is required';
    }

    // Description is read-only and cannot be edited, so no validation needed

    // Category is required
    if (!this.editForm.categoryId) {
      return 'Category is required';
    }

    // Priority is required
    if (!this.editForm.priorityMasterId) {
      return 'Priority is required';
    }

    // Status is required
    if (!this.editForm.statusMasterId) {
      return 'Status is required';
    }

    return null;
  }

  /**
   * Save edited complaint
   */
  saveEdit(): void {
    if (!this.complaint) {
      return;
    }

    // Validate form
    const validationError = this.validateEditForm();
    if (validationError) {
      this.actionError = validationError;
      return;
    }

    this.actionLoading = true;
    this.actionError = null;

    const updateRequest: UpdateComplaintRequest = {
      id: this.complaint.id,
      title: this.editForm.title.trim(),
      description: this.editForm.description.trim(),
      categoryId: this.editForm.categoryId,
      priorityMasterId: this.editForm.priorityMasterId,
      statusMasterId: this.editForm.statusMasterId,
      assignedToId: this.editForm.assignedToId || undefined,
      tags: this.editForm.tags?.trim() || undefined,
      resolutionNotes: this.complaint.resolutionNotes || undefined
    };

    this.complaintService.updateComplaint(this.complaint.id, updateRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.complaint = response.data;
            this.originalComplaint = null;
            this.successMessage = 'Complaint updated successfully';
            this.isEditMode = false;

            // Reload allowed transitions as status may have changed
            this.loadAllowedTransitions();

            setTimeout(() => {
              this.successMessage = null;
            }, 3000);
          } else {
            this.actionError = response.message || 'Failed to update complaint';
          }
          this.actionLoading = false;
        },
        error: (err) => {
          console.error('Error updating complaint:', err);
          this.actionError = err.error?.message || 'Failed to update complaint. Please try again.';
          this.actionLoading = false;
        }
      });
  }

  /**
   * Search for users to assign (for edit mode)
   */
  onAssignedUserSearchChange(): void {
    this.userSearchSubject.next(this.userSearchTerm);
  }

  /**
   * Get category name by ID
   */
  getCategoryName(categoryId: string): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category?.name || 'Unknown Category';
  }

  // Email composer methods - unified approach using same component for all email operations

  /**
   * Open composer for new email (not a reply)
   */
  openEmailComposer(): void {
    this.emailReplyTo = undefined; // No email to reply to
    this.emailThread = undefined; // No thread to include
    this.emailReplyType = ReplyType.NewEmail; // New email, not a reply/forward
    this.showEmailReplyComposer = true;
  }

  /**
   * Handle reply to existing email
   */
  onEmailReplyClicked(event: EmailReplyEvent): void {
    this.emailReplyTo = event.email;
    this.emailThread = undefined; // Reply doesn't need full thread
    this.emailReplyType = ReplyType.Reply;
    this.emailInitialContent = event.quickReplyContent || '';
    this.showEmailReplyComposer = true;
  }

  /**
   * Handle reply-all to existing email
   */
  onEmailReplyAllClicked(event: EmailReplyEvent): void {
    this.emailReplyTo = event.email;
    this.emailThread = undefined; // Reply All doesn't need full thread
    this.emailReplyType = ReplyType.ReplyAll;
    this.emailInitialContent = event.quickReplyContent || '';
    this.showEmailReplyComposer = true;
  }

  /**
   * Handle forward - includes full email thread
   */
  onEmailForwardClicked(event: EmailReplyEvent): void {
    this.emailReplyTo = event.email;
    this.emailReplyType = ReplyType.Forward;
    this.emailInitialContent = event.quickReplyContent || '';

    // Fetch the full email thread for forwarding the entire conversation
    if (this.complaint?.id) {
      this.emailThreadService.getComplaintEmails(this.complaint.id, false).subscribe({
        next: (response) => {
          if (response.isSuccess && response.data) {
            this.emailThread = response.data;
          } else {
            // Fallback: use just the single email if thread fetch fails
            this.emailThread = [event.email];
          }
          this.showEmailReplyComposer = true;
        },
        error: (err) => {
          console.error('Error loading email thread for forward:', err);
          // Fallback: use just the single email if thread fetch fails
          this.emailThread = [event.email];
          this.showEmailReplyComposer = true;
        }
      });
    } else {
      // No complaint ID available, use just the single email
      this.emailThread = [event.email];
      this.showEmailReplyComposer = true;
    }
  }

  /**
   * Handle Add Internal Note button click from email thread viewer
   * Switches to Comments & Activity tab where internal notes are managed
   */
  onAddInternalNoteClicked(): void {
    this.activeTab = 'comments';
    this.isInternalNote = true; // Pre-check the internal note toggle
    // Focus the comment input after a short delay to allow tab switch
    setTimeout(() => {
      const commentInput = document.querySelector('.comment-form textarea');
      if (commentInput) {
        (commentInput as HTMLTextAreaElement).focus();
      }
    }, 100);
  }

  onEmailReplySent(sentEmail: EmailThreadItemDto): void {
    this.showEmailReplyComposer = false;
    this.emailReplyTo = undefined;
    this.emailInitialContent = '';
    this.successMessage = 'Email sent successfully!';

    // Clear success message after 3 seconds
    setTimeout(() => {
      this.successMessage = null;
    }, 3000);

    // The email thread viewer will auto-refresh to show the new email
  }

  onEmailReplyCancelled(): void {
    this.showEmailReplyComposer = false;
    this.emailReplyTo = undefined;
    this.emailInitialContent = '';
  }
}
