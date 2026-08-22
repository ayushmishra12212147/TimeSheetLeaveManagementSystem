import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../../services/auth';

export const firstLoginGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isFirstLogin()) {
    router.navigate(['/first-login']);
    return false;
  }

  return true;
};
