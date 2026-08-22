import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../../services/auth';
import { API_ENDPOINTS } from '../../constants/api-endpoints';

let isRefreshing = false;

export const refreshInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Skip refresh for auth endpoints themselves
  const isAuthEndpoint =
    req.url.includes(API_ENDPOINTS.auth.login) ||
    req.url.includes(API_ENDPOINTS.auth.refresh) ||
    req.url.includes(API_ENDPOINTS.auth.forgotPassword) ||
    req.url.includes(API_ENDPOINTS.auth.resetPassword) ||
    req.url.includes(API_ENDPOINTS.auth.firstLoginReset);

  if (isAuthEndpoint) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isRefreshing) {
        isRefreshing = true;

        return authService.refresh().pipe(
          switchMap((res) => {
            isRefreshing = false;
            if (res.success && res.data?.accessToken) {
              authService.setToken(res.data.accessToken);
              // Retry the original request with the new token
              const retried = req.clone({
                setHeaders: { Authorization: `Bearer ${res.data.accessToken}` },
              });
              return next(retried);
            }
            authService.clearSession();
            router.navigate(['/login']);
            return throwError(() => error);
          }),
          catchError((refreshError) => {
            isRefreshing = false;
            authService.clearSession();
            router.navigate(['/login']);
            return throwError(() => refreshError);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
