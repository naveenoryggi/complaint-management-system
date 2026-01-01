import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PortalService, PagedResult } from '../../../services/portal.service';
import { PortalTicketSummary } from '../../../models/portal.model';

@Component({
  selector: 'app-portal-ticket-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './portal-ticket-list.component.html',
  styleUrls: ['./portal-ticket-list.component.scss']
})
export class PortalTicketListComponent implements OnInit {
  tickets = signal<PortalTicketSummary[]>([]);
  isLoading = signal(true);
  totalCount = signal(0);
  currentPage = signal(1);
  pageSize = signal(10);
  totalPages = signal(0);

  searchTerm = '';
  selectedStatus = '';

  constructor(private portalService: PortalService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading.set(true);

    this.portalService.getTickets(
      this.currentPage(),
      this.pageSize(),
      this.selectedStatus || undefined,
      undefined,
      this.searchTerm || undefined
    ).subscribe({
      next: (result: PagedResult<PortalTicketSummary>) => {
        this.tickets.set(result.items || []);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading tickets:', error);
        this.isLoading.set(false);
      }
    });
  }

  onSearch(): void {
    this.currentPage.set(1);
    this.loadTickets();
  }

  onStatusChange(): void {
    this.currentPage.set(1);
    this.loadTickets();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadTickets();
    }
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
