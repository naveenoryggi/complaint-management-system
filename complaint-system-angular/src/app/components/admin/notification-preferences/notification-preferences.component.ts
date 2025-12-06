import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

interface NotificationChannel {
  id: string;
  name: string;
  description: string;
  enabled: boolean;
}

interface EventNotification {
  id: string;
  event: string;
  description: string;
  inApp: boolean;
  email: boolean;
}

interface QuietHoursSettings {
  enabled: boolean;
  startTime: string;
  endTime: string;
}

interface NotificationPreferences {
  channels: NotificationChannel[];
  eventNotifications: EventNotification[];
  quietHours: QuietHoursSettings;
}

@Component({
  selector: 'app-notification-preferences',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notification-preferences.component.html',
  styleUrls: ['./notification-preferences.component.scss']
})
export class NotificationPreferencesComponent implements OnInit {
  loading = false;
  saving = false;
  successMessage = '';
  errorMessage = '';
  hasChanges = false;

  // Notification Channels
  channels: NotificationChannel[] = [
    {
      id: 'inApp',
      name: 'In-app Notifications',
      description: 'Receive alerts directly within the application.',
      enabled: true
    },
    {
      id: 'email',
      name: 'Email Notifications',
      description: 'Receive alerts directly to your registered email address.',
      enabled: false
    }
  ];

  // Event-Specific Notifications
  eventNotifications: EventNotification[] = [
    {
      id: 'complaint_assigned',
      event: 'When a new complaint is assigned to you',
      description: 'Get notified when a complaint is assigned to you for handling.',
      inApp: false,
      email: true
    },
    {
      id: 'status_changed',
      event: "When a complaint's status is changed",
      description: 'Get notified when a complaint status is updated.',
      inApp: true,
      email: true
    },
    {
      id: 'comment_added',
      event: 'When a new comment is added to a complaint',
      description: 'Get notified when someone comments on a complaint you are involved in.',
      inApp: true,
      email: false
    },
    {
      id: 'escalated',
      event: 'When a complaint is escalated',
      description: 'Get notified when a complaint is escalated to a higher level.',
      inApp: true,
      email: true
    },
    {
      id: 'resolved',
      event: 'When a complaint is resolved',
      description: 'Get notified when a complaint you submitted or are involved in is resolved.',
      inApp: true,
      email: true
    },
    {
      id: 'sla_breach',
      event: 'When an SLA is about to breach',
      description: 'Get notified when a complaint is approaching its SLA deadline.',
      inApp: true,
      email: true
    }
  ];

  // Quiet Hours Settings
  quietHours: QuietHoursSettings = {
    enabled: false,
    startTime: '22:00',
    endTime: '07:00'
  };

  // Time options for dropdowns
  timeOptions: string[] = [];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.generateTimeOptions();
  }

  ngOnInit(): void {
    this.loadPreferences();
  }

  generateTimeOptions(): void {
    for (let hour = 0; hour < 24; hour++) {
      const hourStr = hour.toString().padStart(2, '0');
      this.timeOptions.push(`${hourStr}:00`);
      this.timeOptions.push(`${hourStr}:30`);
    }
  }

  loadPreferences(): void {
    this.loading = true;

    // Load from localStorage for now (can be replaced with API call)
    const savedPrefs = localStorage.getItem('notificationPreferences');
    if (savedPrefs) {
      try {
        const prefs: NotificationPreferences = JSON.parse(savedPrefs);
        this.channels = prefs.channels || this.channels;
        this.eventNotifications = prefs.eventNotifications || this.eventNotifications;
        this.quietHours = prefs.quietHours || this.quietHours;
      } catch (error) {
        console.error('Error parsing saved preferences:', error);
      }
    }

    this.loading = false;
  }

  onChannelToggle(channel: NotificationChannel): void {
    channel.enabled = !channel.enabled;
    this.hasChanges = true;
  }

  onEventNotificationChange(): void {
    this.hasChanges = true;
  }

  onQuietHoursToggle(): void {
    this.quietHours.enabled = !this.quietHours.enabled;
    this.hasChanges = true;
  }

  onQuietHoursTimeChange(): void {
    this.hasChanges = true;
  }

  savePreferences(): void {
    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const preferences: NotificationPreferences = {
      channels: this.channels,
      eventNotifications: this.eventNotifications,
      quietHours: this.quietHours
    };

    // Save to localStorage (can be replaced with API call)
    try {
      localStorage.setItem('notificationPreferences', JSON.stringify(preferences));

      // Simulate API call delay
      setTimeout(() => {
        this.saving = false;
        this.hasChanges = false;
        this.successMessage = 'Preferences saved successfully!';

        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      }, 500);
    } catch (error) {
      this.saving = false;
      this.errorMessage = 'Failed to save preferences. Please try again.';
      console.error('Error saving preferences:', error);
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // Check if email channel is enabled
  isEmailEnabled(): boolean {
    return this.channels.find(c => c.id === 'email')?.enabled || false;
  }

  // Check if in-app channel is enabled
  isInAppEnabled(): boolean {
    return this.channels.find(c => c.id === 'inApp')?.enabled || false;
  }
}
