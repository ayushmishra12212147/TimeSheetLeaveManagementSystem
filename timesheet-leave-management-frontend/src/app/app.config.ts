import { ApplicationConfig, provideZoneChangeDetection, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { refreshInterceptor } from './core/interceptors/refresh.interceptor';
import { authReducer } from './store/auth/auth.reducer';
import {
  loadCurrentUserEffect,
  logoutEffect,
  logoutSuccessEffect,
} from './store/auth/auth.effects';
import { notificationReducer } from './store/notification/notification.reducer';
import { loadUnreadCountEffect } from './store/notification/notification.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor, refreshInterceptor, errorInterceptor])
    ),
    provideStore({
      auth: authReducer,
      notifications: notificationReducer,
    }),
    // Functional effects are passed as a plain object (not class instances)
    provideEffects({
      loadCurrentUserEffect,
      logoutEffect,
      logoutSuccessEffect,
      loadUnreadCountEffect,
    }),
    provideStoreDevtools({ maxAge: 25, logOnly: false }),
  ]
};
