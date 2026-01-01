import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { PortalAuthService } from '../../../services/portal-auth.service';

@Component({
  selector: 'app-portal-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './portal-layout.component.html',
  styleUrls: ['./portal-layout.component.scss']
})
export class PortalLayoutComponent {
  isSidebarOpen = signal(true);
  isUserMenuOpen = signal(false);

  user = computed(() => this.authService.user());

  menuItems = [
    { path: '/portal/dashboard', label: 'Dashboard', icon: 'dashboard' },
    { path: '/portal/tickets', label: 'My Tickets', icon: 'tickets' },
    { path: '/portal/tickets/new', label: 'Create Ticket', icon: 'add' },
    { path: '/portal/profile', label: 'Profile', icon: 'profile' }
  ];

  constructor(
    private authService: PortalAuthService,
    private router: Router
  ) {}

  toggleSidebar(): void {
    this.isSidebarOpen.update(v => !v);
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen.update(v => !v);
  }

  closeUserMenu(): void {
    this.isUserMenuOpen.set(false);
  }

  logout(): void {
    this.authService.logout();
  }
}
