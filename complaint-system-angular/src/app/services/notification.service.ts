import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject, interval, of } from 'rxjs';
import { takeUntil, switchMap, catchError, tap, map, retry } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  AppNotification,
  NotificationType,
  NotificationResponse,
  SingleNotificationResponse,
  NotificationCountResponse,
  NotificationQueryParams,
  PaginatedNotificationResponse,
  MarkNotificationsRequest,
  NotificationEvent,
  NotificationPriority
} from '../models/notification.model';

// SignalR import (optional - will work without it)
declare var signalR: any;

@Injectable({
  providedIn: 'root'
})
export class NotificationService implements OnDestroy {
  private apiUrl = `${environment.apiUrl}/notifications`;
  private hubUrl = `${environment.apiUrl.replace('/api', '')}/notificationHub`;

  // State
  private notificationsSubject = new BehaviorSubject<AppNotification[]>([]);
  private unreadCountSubject = new BehaviorSubject<number>(0);
  private connectionStateSubject = new BehaviorSubject<'connected' | 'disconnected' | 'connecting'>('disconnected');
  private newNotificationSubject = new Subject<AppNotification>();

  // Observables
  public notifications$ = this.notificationsSubject.asObservable();
  public unreadCount$ = this.unreadCountSubject.asObservable();
  public connectionState$ = this.connectionStateSubject.asObservable();
  public newNotification$ = this.newNotificationSubject.asObservable();

  // Internal
  private destroy$ = new Subject<void>();
  private hubConnection: any = null;
  private pollingInterval = 30000; // 30 seconds fallback polling
  private isPolling = false;
  private userId: string | null = null;
  private localCache: Map<string, AppNotification> = new Map();

  constructor(private http: HttpClient) {
    console.log('NotificationService initialized');
  }

  /**
   * Initialize the notification service for a user
   */
  initialize(userId: string): void {
    this.userId = userId;
    console.log('Initializing notifications for user:', userId);

    // Try to establish SignalR connection
    this.connectSignalR();

    // Load real complaint IDs for mock notifications
    this.loadRealComplaintIds();

    // Load initial notifications
    this.loadNotifications();
    this.loadUnreadCount();

    // Start polling as fallback
    this.startPolling();
  }

  /**
   * Connect to SignalR hub for real-time notifications
   */
  private connectSignalR(): void {
    // Check if SignalR is available
    if (typeof signalR === 'undefined') {
      console.warn('SignalR not available, using polling only');
      return;
    }

    try {
      this.connectionStateSubject.next('connecting');

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          accessTokenFactory: () => localStorage.getItem('token') || ''
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      // Handle incoming notifications
      this.hubConnection.on('ReceiveNotification', (notification: AppNotification) => {
        this.handleNewNotification(notification);
      });

      // Handle notification read status updates
      this.hubConnection.on('NotificationRead', (notificationId: string) => {
        this.handleNotificationRead(notificationId);
      });

      // Handle unread count updates
      this.hubConnection.on('UnreadCountUpdate', (count: number) => {
        this.unreadCountSubject.next(count);
      });

      // Connection state handlers
      this.hubConnection.onreconnecting(() => {
        this.connectionStateSubject.next('connecting');
        console.log('SignalR reconnecting...');
      });

      this.hubConnection.onreconnected(() => {
        this.connectionStateSubject.next('connected');
        console.log('SignalR reconnected');
        // Refresh notifications after reconnection
        this.loadNotifications();
        this.loadUnreadCount();
      });

      this.hubConnection.onclose(() => {
        this.connectionStateSubject.next('disconnected');
        console.log('SignalR disconnected');
      });

      // Start connection
      this.hubConnection.start()
        .then(() => {
          this.connectionStateSubject.next('connected');
          console.log('SignalR connected successfully');
          // Join user's notification group
          if (this.userId) {
            this.hubConnection.invoke('JoinUserGroup', this.userId);
          }
        })
        .catch((err: any) => {
          console.error('SignalR connection failed:', err);
          this.connectionStateSubject.next('disconnected');
        });

    } catch (error) {
      console.error('Error setting up SignalR:', error);
      this.connectionStateSubject.next('disconnected');
    }
  }

  /**
   * Start polling for notifications (fallback mechanism)
   */
  private startPolling(): void {
    if (this.isPolling) return;
    this.isPolling = true;

    interval(this.pollingInterval)
      .pipe(
        takeUntil(this.destroy$),
        switchMap(() => {
          // Only poll if SignalR is disconnected
          if (this.connectionStateSubject.value !== 'connected') {
            return this.fetchUnreadCount();
          }
          return of(null);
        }),
        catchError(error => {
          console.error('Polling error:', error);
          return of(null);
        })
      )
      .subscribe();
  }

  /**
   * Load notifications from API
   */
  loadNotifications(params?: NotificationQueryParams): void {
    this.getNotifications(params).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          const notifications = response.data.items || response.data;
          this.notificationsSubject.next(notifications as AppNotification[]);
          // Update local cache
          (notifications as AppNotification[]).forEach(n => this.localCache.set(n.id, n));
        }
      },
      error: (error) => console.error('Error loading notifications:', error)
    });
  }

  /**
   * Load unread count
   */
  loadUnreadCount(): void {
    this.fetchUnreadCount().subscribe();
  }

  private fetchUnreadCount(): Observable<NotificationCountResponse | null> {
    return this.http.get<NotificationCountResponse>(`${this.apiUrl}/count`).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.unreadCountSubject.next(response.data.unread);
        }
      }),
      catchError(error => {
        console.error('Error fetching unread count:', error);
        return of(null);
      })
    );
  }

  /**
   * Handle incoming new notification
   */
  private handleNewNotification(notification: AppNotification): void {
    // Add to cache
    this.localCache.set(notification.id, notification);

    // Add to current notifications list
    const current = this.notificationsSubject.value;
    this.notificationsSubject.next([notification, ...current]);

    // Update unread count
    if (!notification.isRead) {
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
    }

    // Emit new notification event
    this.newNotificationSubject.next(notification);

    // Show browser notification if permitted
    this.showBrowserNotification(notification);
  }

  /**
   * Handle notification read status update
   */
  private handleNotificationRead(notificationId: string): void {
    const current = this.notificationsSubject.value;
    const updated = current.map(n =>
      n.id === notificationId ? { ...n, isRead: true, readAt: new Date() } : n
    );
    this.notificationsSubject.next(updated);

    // Update cache
    const cached = this.localCache.get(notificationId);
    if (cached) {
      this.localCache.set(notificationId, { ...cached, isRead: true, readAt: new Date() });
    }
  }

  /**
   * Show browser notification
   */
  private showBrowserNotification(notification: AppNotification): void {
    if (!('Notification' in window)) return;

    if (Notification.permission === 'granted') {
      new Notification(notification.title, {
        body: notification.message,
        icon: '/assets/icons/icon-192x192.png',
        tag: notification.id,
        data: { url: this.getNotificationUrl(notification) }
      });
    } else if (Notification.permission !== 'denied') {
      Notification.requestPermission().then(permission => {
        if (permission === 'granted') {
          this.showBrowserNotification(notification);
        }
      });
    }
  }

  /**
   * Get URL for notification navigation
   */
  getNotificationUrl(notification: AppNotification): string {
    switch (notification.referenceType) {
      case 'COMPLAINT':
        return `/complaints/${notification.referenceId}`;
      case 'COMMENT':
        return `/complaints/${notification.metadata?.['complaintId'] || notification.referenceId}`;
      case 'EMAIL_THREAD':
        return `/complaints/${notification.metadata?.['complaintId']}?tab=emails`;
      default:
        return '/notifications';
    }
  }

  // ==================== API Methods ====================

  /**
   * Get paginated notifications
   */
  getNotifications(params?: NotificationQueryParams): Observable<PaginatedNotificationResponse> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.page !== undefined) httpParams = httpParams.set('page', params.page.toString());
      if (params.pageSize !== undefined) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.isRead !== undefined) httpParams = httpParams.set('isRead', params.isRead.toString());
      if (params.type) httpParams = httpParams.set('type', params.type);
      if (params.referenceType) httpParams = httpParams.set('referenceType', params.referenceType);
      if (params.startDate) httpParams = httpParams.set('startDate', params.startDate);
      if (params.endDate) httpParams = httpParams.set('endDate', params.endDate);
      if (params.priority !== undefined) httpParams = httpParams.set('priority', params.priority.toString());
    }

    return this.http.get<PaginatedNotificationResponse>(this.apiUrl, { params: httpParams }).pipe(
      catchError(error => {
        console.error('Error fetching notifications:', error);
        // Return mock data for development/testing when API is not available
        return of(this.getMockNotificationsResponse());
      })
    );
  }

  /**
   * Get unread notifications only
   */
  getUnreadNotifications(limit: number = 10): Observable<NotificationResponse> {
    return this.http.get<NotificationResponse>(`${this.apiUrl}/unread`, {
      params: new HttpParams().set('limit', limit.toString())
    }).pipe(
      catchError(error => {
        console.error('Error fetching unread notifications:', error);
        return of(this.getMockUnreadResponse());
      })
    );
  }

  /**
   * Get notification count
   */
  getNotificationCount(): Observable<NotificationCountResponse> {
    return this.http.get<NotificationCountResponse>(`${this.apiUrl}/count`).pipe(
      catchError(error => {
        console.error('Error fetching notification count:', error);
        return of({ isSuccess: true, data: { total: 0, unread: 0, byType: {} } });
      })
    );
  }

  /**
   * Mark notification as read
   */
  markAsRead(notificationId: string): Observable<SingleNotificationResponse> {
    return this.http.put<SingleNotificationResponse>(`${this.apiUrl}/${notificationId}/read`, {}).pipe(
      tap(response => {
        if (response.isSuccess) {
          this.handleNotificationRead(notificationId);
          // Decrease unread count
          const currentCount = this.unreadCountSubject.value;
          if (currentCount > 0) {
            this.unreadCountSubject.next(currentCount - 1);
          }
        }
      }),
      catchError(error => {
        console.error('Error marking notification as read:', error);
        // Optimistically update UI anyway
        this.handleNotificationRead(notificationId);
        return of({ isSuccess: true });
      })
    );
  }

  /**
   * Mark notification as unread
   */
  markAsUnread(notificationId: string): Observable<SingleNotificationResponse> {
    return this.http.put<SingleNotificationResponse>(`${this.apiUrl}/${notificationId}/unread`, {}).pipe(
      tap(response => {
        if (response.isSuccess) {
          // Update local state
          const current = this.notificationsSubject.value;
          const updated = current.map(n =>
            n.id === notificationId ? { ...n, isRead: false, readAt: undefined } : n
          );
          this.notificationsSubject.next(updated);

          // Update cache
          const cached = this.localCache.get(notificationId);
          if (cached) {
            this.localCache.set(notificationId, { ...cached, isRead: false, readAt: undefined });
          }

          // Increase unread count
          this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
        }
      }),
      catchError(error => {
        console.error('Error marking notification as unread:', error);
        // Optimistically update UI anyway
        const current = this.notificationsSubject.value;
        const updated = current.map(n =>
          n.id === notificationId ? { ...n, isRead: false, readAt: undefined } : n
        );
        this.notificationsSubject.next(updated);
        return of({ isSuccess: true });
      })
    );
  }

  /**
   * Mark multiple notifications as read
   */
  markMultipleAsRead(request: MarkNotificationsRequest): Observable<NotificationResponse> {
    return this.http.put<NotificationResponse>(`${this.apiUrl}/mark-read`, request).pipe(
      tap(response => {
        if (response.isSuccess) {
          if (request.markAll) {
            // Mark all as read locally
            const current = this.notificationsSubject.value;
            const updated = current.map(n => ({ ...n, isRead: true, readAt: new Date() }));
            this.notificationsSubject.next(updated);
            this.unreadCountSubject.next(0);
          } else if (request.notificationIds) {
            request.notificationIds.forEach(id => this.handleNotificationRead(id));
          }
        }
      }),
      catchError(error => {
        console.error('Error marking notifications as read:', error);
        return of({ isSuccess: false, message: 'Failed to mark notifications as read' });
      })
    );
  }

  /**
   * Mark all notifications as read
   */
  markAllAsRead(): Observable<NotificationResponse> {
    return this.markMultipleAsRead({ markAll: true, isRead: true });
  }

  /**
   * Delete a notification
   */
  deleteNotification(notificationId: string): Observable<SingleNotificationResponse> {
    return this.http.delete<SingleNotificationResponse>(`${this.apiUrl}/${notificationId}`).pipe(
      tap(response => {
        if (response.isSuccess) {
          // Remove from local state
          const current = this.notificationsSubject.value;
          const notification = current.find(n => n.id === notificationId);
          this.notificationsSubject.next(current.filter(n => n.id !== notificationId));
          this.localCache.delete(notificationId);

          // Update unread count if notification was unread
          if (notification && !notification.isRead) {
            const currentCount = this.unreadCountSubject.value;
            if (currentCount > 0) {
              this.unreadCountSubject.next(currentCount - 1);
            }
          }
        }
      }),
      catchError(error => {
        console.error('Error deleting notification:', error);
        return of({ isSuccess: false, message: 'Failed to delete notification' });
      })
    );
  }

  /**
   * Clear all notifications
   */
  clearAllNotifications(): Observable<NotificationResponse> {
    return this.http.delete<NotificationResponse>(`${this.apiUrl}/clear-all`).pipe(
      tap(response => {
        if (response.isSuccess) {
          this.notificationsSubject.next([]);
          this.unreadCountSubject.next(0);
          this.localCache.clear();
        }
      }),
      catchError(error => {
        console.error('Error clearing notifications:', error);
        return of({ isSuccess: false, message: 'Failed to clear notifications' });
      })
    );
  }

  // ==================== Mock Data (for development) ====================

  private getMockNotificationsResponse(): PaginatedNotificationResponse {
    const mockNotifications = this.generateMockNotifications();
    return {
      isSuccess: true,
      data: {
        items: mockNotifications,
        totalCount: mockNotifications.length,
        page: 1,
        pageSize: 20,
        totalPages: 1
      }
    };
  }

  private getMockUnreadResponse(): NotificationResponse {
    const mockNotifications = this.generateMockNotifications().filter(n => !n.isRead);
    return {
      isSuccess: true,
      data: mockNotifications.slice(0, 10)
    };
  }

  // Cache for real complaint IDs fetched from API
  private cachedComplaintIds: string[] = [];

  /**
   * Load real complaint IDs for mock notifications
   */
  private loadRealComplaintIds(): void {
    this.http.get<any>(`${environment.apiUrl}/complaints?page=1&pageSize=5`).pipe(
      catchError(() => of(null))
    ).subscribe(response => {
      if (response?.isSuccess && response?.data?.items) {
        this.cachedComplaintIds = response.data.items.map((c: any) => c.id);
      }
    });
  }

  private generateMockNotifications(): AppNotification[] {
    const now = new Date();

    // Use cached real complaint IDs if available, otherwise use placeholder GUIDs
    const complaintIds = this.cachedComplaintIds.length >= 5
      ? this.cachedComplaintIds
      : [
          '7fcfaa62-8613-4ce2-9b8f-d295380f4067', // Real complaint ID from system
          'a86408a1-3111-4fa7-bdd2-2aa3bf5cc5b0', // Another known complaint
          '00000000-0000-0000-0000-000000000001',
          '00000000-0000-0000-0000-000000000002',
          '00000000-0000-0000-0000-000000000003'
        ];

    return [
      {
        id: '1',
        userId: this.userId || '',
        companyId: '1',
        type: NotificationType.COMPLAINT_ASSIGNED,
        title: 'New Complaint Assigned',
        message: 'You have been assigned complaint #CMP-20251207-0001',
        referenceType: 'COMPLAINT' as any,
        referenceId: complaintIds[0],
        isRead: false,
        createdAt: new Date(now.getTime() - 5 * 60000),
        priority: NotificationPriority.NORMAL,
        metadata: { complaintNumber: '#CMP-20251207-0001', complaintSubject: 'Product defect issue' }
      },
      {
        id: '2',
        userId: this.userId || '',
        companyId: '1',
        type: NotificationType.STATUS_CHANGED,
        title: 'Status Updated',
        message: 'Complaint #CMP-20251206-0525 status changed to "In Progress"',
        referenceType: 'COMPLAINT' as any,
        referenceId: complaintIds[1],
        isRead: false,
        createdAt: new Date(now.getTime() - 30 * 60000),
        priority: NotificationPriority.NORMAL,
        metadata: { complaintNumber: '#CMP-20251206-0525', oldStatus: 'Submitted', newStatus: 'In Progress' }
      },
      {
        id: '3',
        userId: this.userId || '',
        companyId: '1',
        type: NotificationType.SLA_WARNING,
        title: 'SLA Warning',
        message: 'Complaint #CMP-20251205-0123 is approaching SLA deadline (2 hours remaining)',
        referenceType: 'COMPLAINT' as any,
        referenceId: complaintIds[2],
        isRead: false,
        createdAt: new Date(now.getTime() - 2 * 3600000),
        priority: NotificationPriority.HIGH,
        metadata: { complaintNumber: '#CMP-20251205-0123', slaTimeRemaining: 7200 }
      },
      {
        id: '4',
        userId: this.userId || '',
        companyId: '1',
        type: NotificationType.COMMENT_ADDED,
        title: 'New Comment',
        message: 'John Doe commented on complaint #CMP-20251204-0089',
        referenceType: 'COMMENT' as any,
        referenceId: complaintIds[3],
        isRead: true,
        readAt: new Date(now.getTime() - 4 * 3600000),
        createdAt: new Date(now.getTime() - 5 * 3600000),
        priority: NotificationPriority.NORMAL,
        metadata: { complaintId: complaintIds[3], complaintNumber: '#CMP-20251204-0089', commentPreview: 'I have reviewed the issue and...' }
      },
      {
        id: '5',
        userId: this.userId || '',
        companyId: '1',
        type: NotificationType.ESCALATION,
        title: 'Complaint Escalated',
        message: 'Complaint #CMP-20251203-0456 has been escalated to Level 2',
        referenceType: 'COMPLAINT' as any,
        referenceId: complaintIds[4],
        isRead: true,
        readAt: new Date(now.getTime() - 24 * 3600000),
        createdAt: new Date(now.getTime() - 25 * 3600000),
        priority: NotificationPriority.HIGH,
        metadata: { complaintNumber: '#CMP-20251203-0456', escalationLevel: 2 }
      }
    ];
  }

  // ==================== Utility Methods ====================

  /**
   * Request browser notification permission
   */
  requestNotificationPermission(): Promise<NotificationPermission> {
    if (!('Notification' in window)) {
      return Promise.resolve('denied' as NotificationPermission);
    }
    return Notification.requestPermission();
  }

  /**
   * Check if browser notifications are supported and permitted
   */
  canShowBrowserNotifications(): boolean {
    return 'Notification' in window && Notification.permission === 'granted';
  }

  /**
   * Get current unread count synchronously
   */
  getUnreadCount(): number {
    return this.unreadCountSubject.value;
  }

  /**
   * Get cached notifications
   */
  getCachedNotifications(): AppNotification[] {
    return this.notificationsSubject.value;
  }

  /**
   * Disconnect and cleanup
   */
  disconnect(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
    this.connectionStateSubject.next('disconnected');
    this.userId = null;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.disconnect();
  }
}
