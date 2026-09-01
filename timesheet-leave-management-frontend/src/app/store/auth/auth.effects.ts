import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { AuthService } from '../../services/auth';
import {
  loadCurrentUser,
  loadCurrentUserSuccess,
  loadCurrentUserFailure,
  logout,
  logoutSuccess,
} from './auth.actions';

export const loadCurrentUserEffect = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) =>
    actions$.pipe(
      ofType(loadCurrentUser),
      switchMap(() =>
        authService.getMe().pipe(
          map((res) =>
            res.success
              ? loadCurrentUserSuccess({ user: res.data })
              : loadCurrentUserFailure({ error: res.message })
          ),
          catchError((err) =>
            of(loadCurrentUserFailure({ error: err?.message ?? 'Failed to load user' }))
          )
        )
      )
    ),
  { functional: true }
);

export const logoutEffect = createEffect(
  (actions$ = inject(Actions), authService = inject(AuthService)) =>
    actions$.pipe(
      ofType(logout),
      switchMap(() =>
        authService.logout().pipe(
          map(() => logoutSuccess()),
          catchError(() => {
            authService.clearSession();
            return of(logoutSuccess());
          })
        )
      )
    ),
  { functional: true }
);

export const logoutSuccessEffect = createEffect(
  (
    actions$ = inject(Actions),
    authService = inject(AuthService),
    router = inject(Router)
  ) =>
    actions$.pipe(
      ofType(logoutSuccess),
      tap(() => {
        authService.clearSession();
        router.navigate(['/login']);
      })
    ),
  { functional: true, dispatch: false }
);
