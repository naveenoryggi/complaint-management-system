import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotificationService } from '../../../services/notification.service';
import { AuthService } from '../../../services/auth.service';
import {
  AppNotification,
  NotificationType,
  NotificationPriority,
  NotificationQueryParams,
  getNotificationIcon,
  getNotificationColor,
  getTypeLabel,
  getPriorityLabel
} from '../../../models/notification.model';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notification-center.component.html',
  styleUrls: ['./notification-center.component.scss']
})
export class NotificationCenterComponent implements OnInit, OnDestroy {
  notifications: AppNotification[] = [];
  filteredNotifications: AppNotification[] = [];
  isLoading = true;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalPages = 1;
  totalCount = 0;

  // Filters
  selectedFilter: 'all' | 'unread' | 'read' = 'all';
  selectedType: NotificationType | '' = '';
  selectedPriority: NotificationPriority | '' = '';
  searchTerm = '';

  // For dropdown options
  notificationTypes = Object.values(NotificationType);
  notificationPriorities = Object.values(NotificationPriority).filter(v => typeof v === 'number') as NotificationPriority[];

  // Stats
  unreadCount = 0;

  private destroy$ = new Subject<void>();

  constructor(
    private notificationService: NotificationService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Initialize notification service and subscribe to updates
    this.authService.currentUser
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        if (user && user.id) {
          this.notificationService.initialize(user.id);
          this.loadNotifications();
          this.subscribeToNotifications();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private subscribeToNotifications(): void {
    // Subscribe to unread count
    this.notificationService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadCount = count;
      });

    // Subscribe to notifications list
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
        this.applyFilters();
        this.isLoading = false;
      });
  }

  loadNotifications(): void {
    this.isLoading = true;

    const params: NotificationQueryParams = {
      page: this.currentPage,
      pageSize: this.pageSize
    };

    // Apply read filter
    if (this.selectedFilter === 'unread') {
      params.isRead = false;
    } else if (this.selectedFilter === 'read') {
      params.isRead = true;
    }

    // Apply type filter
    if (this.selectedType) {
      params.type = this.selectedType;
    }

    // Apply priority filter
    if (this.selectedPriority !== '') {
      params.priority = this.selectedPriority;
    }

    this.notificationService.loadNotifications(params);
  }

  applyFilters(): void {
    let filtered = [...this.notifications];

    // Apply read/unread filter (for local filtering when API doesn't support it)
    if (this.selectedFilter === 'unread') {
      filtered = filtered.filter(n => !n.isRead);
    } else if (this.selectedFilter === 'read') {
      filtered = filtered.filter(n => n.isRead);
    }

    // Apply type filter locally
    if (this.selectedType) {
      filtered = filtered.filter(n => n.type === this.selectedType);
    }

    // Apply priority filter locally
    if (this.selectedPriority !== '') {
      filtered = filtered.filter(n => n.priority === this.selectedPriority);
    }

    // Apply search
    if (this.searchTerm.trim()) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(n =>
        n.title.toLowerCase().includes(search) ||
        n.message.toLowerCase().includes(search)
      );
    }

    this.filteredNotifications = filtered;
    this.totalCount = filtered.length;
    this.totalPages = Math.ceil(this.totalCount / this.pageSize);
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadNotifications();
  }

  onSearch(): void {
    this.applyFilters();
  }

  clearFilters(): void {
    this.selectedFilter = 'all';
    this.selectedType = '';
    this.selectedPriority = '';
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadNotifications();
  }

  // Notification actions
  onNotificationClick(notification: AppNotification): void {
    // Mark as read if unread
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }

    // Navigate to related entity
    const url = this.notificationService.getNotificationUrl(notification);
    this.router.navigate([url]);
  }

  markAsRead(notification: AppNotification, event: Event): void {
    event.stopPropagation();
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe(() => {
        notification.isRead = true;
        notification.readAt = new Date();
      });
    }
  }

  markAsUnread(notification: AppNotification, event: Event): void {
    event.stopPropagation();
    if (notification.isRead) {
      this.notificationService.markAsUnread(notification.id).subscribe(() => {
        notification.isRead = false;
        notification.readAt = undefined;
      });
    }
  }

  deleteNotification(notification: AppNotification, event: Event): void {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this notification?')) {
      this.notificationService.deleteNotification(notification.id).subscribe(() => {
        this.notifications = this.notifications.filter(n => n.id !== notification.id);
        this.applyFilters();
      });
    }
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe(() => {
      this.notifications.forEach(n => {
        n.isRead = true;
        n.readAt = new Date();
      });
      this.applyFilters();
    });
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadNotifications();
    }
  }

  previousPage(): void {
    this.goToPage(this.currentPage - 1);
  }

  nextPage(): void {
    this.goToPage(this.currentPage + 1);
  }

  // Helper methods
  getNotificationIcon(type: NotificationType): string {
    return getNotificationIcon(type);
  }

  getNotificationColor(type: NotificationType): string {
    return getNotificationColor(type);
  }

  getTypeLabel(type: NotificationType): string {
    return getTypeLabel(type);
  }

  getPriorityLabel(priority: NotificationPriority): string {
    return getPriorityLabel(priority);
  }

  getTimeAgo(date: Date | string): string {
    const now = new Date();
    const notificationDate = new Date(date);
    const diffMs = now.getTime() - notificationDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return notificationDate.toLocaleDateString();
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  }

  getPriorityClass(priority: NotificationPriority): string {
    switch (priority) {
      case NotificationPriority.URGENT: return 'priority-urgent';
      case NotificationPriority.HIGH: return 'priority-high';
      case NotificationPriority.NORMAL: return 'priority-normal';
      case NotificationPriority.LOW: return 'priority-low';
      default: return 'priority-normal';
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  getEndItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }
}
