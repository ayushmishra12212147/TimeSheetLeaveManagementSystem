import { createFeatureSelector, createSelector } from '@ngrx/store';
import { NotificationState } from './notification.reducer';

export const selectNotificationState =
  createFeatureSelector<NotificationState>('notifications');

export const selectUnreadCount = createSelector(
  selectNotificationState,
  (state) => state.unreadCount
);

export const selectNotificationLoading = createSelector(
  selectNotificationState,
  (state) => state.loading
);
