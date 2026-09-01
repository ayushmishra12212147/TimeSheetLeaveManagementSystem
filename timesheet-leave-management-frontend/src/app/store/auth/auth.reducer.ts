import { createReducer, on } from '@ngrx/store';
import { UserProfile } from '../../models/auth';
import {
  loadCurrentUser,
  loadCurrentUserSuccess,
  loadCurrentUserFailure,
  logout,
  logoutSuccess,
} from './auth.actions';

export interface AuthState {
  user: UserProfile | null;
  loading: boolean;
  error: string | null;
}

export const initialAuthState: AuthState = {
  user: null,
  loading: false,
  error: null,
};

export const authReducer = createReducer(
  initialAuthState,
  on(loadCurrentUser, (state) => ({ ...state, loading: true, error: null })),
  on(loadCurrentUserSuccess, (state, { user }) => ({
    ...state,
    user,
    loading: false,
    error: null,
  })),
  on(loadCurrentUserFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
  on(logout, (state) => ({ ...state, loading: true })),
  on(logoutSuccess, () => ({ ...initialAuthState }))
);
