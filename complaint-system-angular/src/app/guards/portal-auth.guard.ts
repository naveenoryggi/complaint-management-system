import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { PortalAuthService } from '../services/portal-auth.service';

export const portalAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(PortalAuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Redirect to portal login
  router.navigate(['/portal/login'], {
    queryParams: { returnUrl: state.url }
  });
  return false;
};
