import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  const role = authService.getUserRole();

  if (token) {
    const headers: Record<string, string> = {
      Authorization: `Bearer ${token}`
    };

    if (role) {
      headers['X-User-Role'] = role;
    }

    const cloned = req.clone({
      setHeaders: headers
    });
    return next(cloned);
  }

  return next(req);
};
