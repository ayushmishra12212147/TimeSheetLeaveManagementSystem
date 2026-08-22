import { createAction, props } from '@ngrx/store';

export const loadUnreadCount = createAction('[Notifications] Load Unread Count');

export const loadUnreadCountSuccess = createAction(
  '[Notifications] Load Unread Count Success',
  props<{ count: number }>()
);

export const loadUnreadCountFailure = createAction(
  '[Notifications] Load Unread Count Failure',
  props<{ error: string }>()
);

export const decrementUnreadCount = createAction('[Notifications] Decrement Unread Count');

export const resetUnreadCount = createAction('[Notifications] Reset Unread Count');
