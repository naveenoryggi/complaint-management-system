import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PortalService } from '../../../services/portal.service';
import { PortalAuthService } from '../../../services/portal-auth.service';
import { PortalDashboard, PortalTicketSummary } from '../../../models/portal.model';

@Component({
  selector: 'app-portal-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './portal-dashboard.component.html',
  styleUrls: ['./portal-dashboard.component.scss']
})
export class PortalDashboardComponent implements OnInit {
  isLoading = signal(true);

  get user() {
    return this.authService.user;
  }

  stats = signal({
    total: 0,
    open: 0,
    inProgress: 0,
    resolved: 0,
    closed: 0
  });

  recentTickets = signal<PortalTicketSummary[]>([]);

  constructor(
    private portalService: PortalService,
    private authService: PortalAuthService
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading.set(true);

    // Load tickets for stats
    this.portalService.getTickets(1, 100).subscribe({
      next: (result) => {
        const tickets = result.items || [];

        // Calculate stats
        this.stats.set({
          total: tickets.length,
          open: tickets.filter(t => t.status === 'Open' || t.status === 'New').length,
          inProgress: tickets.filter(t => t.status === 'In Progress' || t.status === 'Assigned').length,
          resolved: tickets.filter(t => t.status === 'Resolved').length,
          closed: tickets.filter(t => t.status === 'Closed').length
        });

        // Get recent 5 tickets
        this.recentTickets.set(tickets.slice(0, 5));
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading dashboard:', error);
        this.isLoading.set(false);
      }
    });
  }

  getStatusClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'New': 'status-new',
      'Open': 'status-open',
      'In Progress': 'status-progress',
      'Assigned': 'status-progress',
      'Resolved': 'status-resolved',
      'Closed': 'status-closed'
    };
    return statusMap[status] || 'status-default';
  }

  getPriorityClass(priority: string): string {
    const priorityMap: { [key: string]: string } = {
      'Critical': 'priority-critical',
      'High': 'priority-high',
      'Medium': 'priority-medium',
      'Low': 'priority-low'
    };
    return priorityMap[priority] || 'priority-default';
  }
}
