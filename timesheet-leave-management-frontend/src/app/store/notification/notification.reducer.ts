import { createReducer, on } from '@ngrx/store';
import {
  loadUnreadCount,
  loadUnreadCountSuccess,
  loadUnreadCountFailure,
  decrementUnreadCount,
  resetUnreadCount,
} from './notification.actions';

export interface NotificationState {
  unreadCount: number;
  loading: boolean;
  error: string | null;
}

export const initialNotificationState: NotificationState = {
  unreadCount: 0,
  loading: false,
  error: null,
};

export const notificationReducer = createReducer(
  initialNotificationState,
  on(loadUnreadCount, (state) => ({ ...state, loading: true })),
  on(loadUnreadCountSuccess, (state, { count }) => ({
    ...state,
    unreadCount: count,
    loading: false,
    error: null,
  })),
  on(loadUnreadCountFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
  on(decrementUnreadCount, (state) => ({
    ...state,
    unreadCount: Math.max(0, state.unreadCount - 1),
  })),
  on(resetUnreadCount, (state) => ({ ...state, unreadCount: 0 }))
);
