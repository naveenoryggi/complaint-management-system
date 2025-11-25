import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { NavigationService, Breadcrumb } from '../../../services/navigation.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './breadcrumb.component.html',
  styleUrl: './breadcrumb.component.scss'
})
export class BreadcrumbComponent implements OnInit {
  breadcrumbs$: Observable<Breadcrumb[]>;
  showBackButton$: Observable<boolean>;
  showHomeButton$: Observable<boolean>;
  pageTitle$: Observable<string>;

  constructor(
    private navigationService: NavigationService,
    private router: Router
  ) {
    this.breadcrumbs$ = this.navigationService.navigationConfig$.pipe(
      map(config => config.breadcrumbs)
    );

    this.showBackButton$ = this.navigationService.navigationConfig$.pipe(
      map(config => config.showBackButton)
    );

    this.showHomeButton$ = this.navigationService.navigationConfig$.pipe(
      map(config => config.showHomeButton)
    );

    this.pageTitle$ = this.navigationService.navigationConfig$.pipe(
      map(config => config.title)
    );
  }

  ngOnInit(): void {}

  navigateTo(url: string): void {
    this.router.navigate([url]);
  }

  goBack(): void {
    this.navigationService.goBack();
  }

  goHome(): void {
    this.navigationService.goHome();
  }
}
