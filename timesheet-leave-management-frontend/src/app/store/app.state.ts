import { AuthState } from './auth/auth.reducer';
import { NotificationState } from './notification/notification.reducer';

export interface AppState {
  auth: AuthState;
  notifications: NotificationState;
}
