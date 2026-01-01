import { Component, OnInit, OnDestroy, HostListener, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';
import { LicenseService } from '../../../services/license.service';
import { ModuleConfig } from '../../../models/license.model';

@Component({
  selector: 'app-module-switcher',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './module-switcher.component.html',
  styleUrls: ['./module-switcher.component.scss']
})
export class ModuleSwitcherComponent implements OnInit, OnDestroy {
  @Output() moduleSelected = new EventEmitter<ModuleConfig>();

  modules: ModuleConfig[] = [];
  enabledModules: ModuleConfig[] = [];
  currentModule: ModuleConfig | null = null;
  showDropdown = false;
  isLoading = true;

  private destroy$ = new Subject<void>();

  constructor(
    private licenseService: LicenseService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadModules();

    // Listen for route changes to update current module
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntil(this.destroy$)
    ).subscribe((event: any) => {
      this.updateCurrentModule(event.urlAfterRedirects);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.module-switcher')) {
      this.showDropdown = false;
    }
  }

  loadModules(): void {
    this.isLoading = true;
    this.licenseService.getModules().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (modules) => {
        this.modules = modules;
        this.enabledModules = modules.filter(m => m.isEnabled);
        this.updateCurrentModule(this.router.url);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading modules:', error);
        this.isLoading = false;
      }
    });
  }

  updateCurrentModule(url: string): void {
    // Find the module that matches the current route
    const matchedModule = this.enabledModules.find(m => {
      if (!m.route) return false;
      return url.startsWith(m.route);
    });

    // Default to Core module for dashboard/complaints
    if (!matchedModule) {
      this.currentModule = this.enabledModules.find(m => m.moduleCode === 'CORE') || null;
    } else {
      this.currentModule = matchedModule;
    }
  }

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
  }

  selectModule(module: ModuleConfig): void {
    if (!module.isEnabled) return;

    this.currentModule = module;
    this.showDropdown = false;
    this.moduleSelected.emit(module);

    // Navigate to module route
    if (module.route) {
      this.router.navigate([module.route]);
    }
  }

  getModuleIcon(module: ModuleConfig): string {
    return module.icon || this.licenseService.getModuleIcon(module.moduleCode);
  }

  getModuleColor(module: ModuleConfig): string {
    return module.color || this.licenseService.getModuleColor(module.moduleCode);
  }

  isModuleActive(module: ModuleConfig): boolean {
    return this.currentModule?.moduleCode === module.moduleCode;
  }
}
