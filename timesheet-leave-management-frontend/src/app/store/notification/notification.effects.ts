import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { NotificationService } from '../../services/notification';
import {
  loadUnreadCount,
  loadUnreadCountSuccess,
  loadUnreadCountFailure,
} from './notification.actions';

export const loadUnreadCountEffect = createEffect(
  (actions$ = inject(Actions), notificationService = inject(NotificationService)) =>
    actions$.pipe(
      ofType(loadUnreadCount),
      switchMap(() =>
        notificationService.getUnreadCount().pipe(
          map((res) =>
            res.success && res.data
              ? loadUnreadCountSuccess({ count: res.data.unreadCount })
              : loadUnreadCountSuccess({ count: 0 })
          ),
          catchError((err) =>
            of(loadUnreadCountFailure({ error: err?.message ?? 'Failed' }))
          )
        )
      )
    ),
  { functional: true }
);
