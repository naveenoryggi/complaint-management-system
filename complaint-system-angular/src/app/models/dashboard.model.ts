// Dashboard Preferences model
export interface DashboardPreferences {
  id: string;
  userId: string;
  statusWidgets: string[];
  layout: string;
  showTrends: boolean;
  showPercentages: boolean;
  autoRefreshInterval: number;
  dateRangeDays: number;
  theme?: string;
  widgetConfig?: string;
  createdAt: Date;
  updatedAt?: Date;
}

// Status Widget model
export interface StatusWidget {
  statusId: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  colorCode: string;
  iconClass: string;
  currentCount: number;
  previousCount: number;
  percentageChange: number;
  trend: 'up' | 'down' | 'stable';
  isFinal: boolean;
  averageTimeInStatus?: number;
}

// Dashboard Statistics model
export interface DashboardStatistics {
  statusWidgets: StatusWidget[];
  totalComplaints: number;
  activeComplaints: number;
  completedComplaints: number;
  overdueComplaints: number;
  todayComplaints: number;
  weekComplaints: number;
  monthComplaints: number;
  averageResolutionTime?: number;
  dateRangeDays: number;
  generatedAt: Date;
}

// Save Dashboard Preferences Request
export interface SaveDashboardPreferencesRequest {
  statusWidgets: string[];
  layout: string;
  showTrends: boolean;
  showPercentages: boolean;
  autoRefreshInterval: number;
  dateRangeDays: number;
  theme?: string;
  widgetConfig?: string;
}
