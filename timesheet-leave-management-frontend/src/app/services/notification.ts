import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, map, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { Notification, NotificationListResponse, NotificationPreference, UnreadCount } from '../models/notification';

interface BackendNotificationPreference {
  emailNotificationsEnabled: boolean;
  inAppNotificationsEnabled: boolean;
  updatedAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  getNotifications(): Observable<ApiResponse<NotificationListResponse>> {
    return this.http.get<ApiResponse<NotificationListResponse>>(API_ENDPOINTS.notifications.base);
  }

  markRead(id: string): Observable<ApiResponse<null>> {
    return this.http.patch<ApiResponse<null>>(API_ENDPOINTS.notifications.markRead(id), {}).pipe(
      tap(res => {
        if (res.success) {
          const currentCount = this.unreadCountSubject.value;
          if (currentCount > 0) {
            this.unreadCountSubject.next(currentCount - 1);
          }
        }
      })
    );
  }

  markAllRead(): Observable<ApiResponse<null>> {
    return this.http.patch<ApiResponse<null>>(API_ENDPOINTS.notifications.markAllRead, {}).pipe(
      tap(res => {
        if (res.success) {
          this.unreadCountSubject.next(0);
        }
      })
    );
  }

  getUnreadCount(): Observable<ApiResponse<UnreadCount>> {
    return this.http.get<ApiResponse<UnreadCount>>(API_ENDPOINTS.notifications.unreadCount).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.unreadCountSubject.next(res.data.unreadCount);
        }
      })
    );
  }

  getPreferences(): Observable<ApiResponse<NotificationPreference>> {
    return this.http
      .get<ApiResponse<BackendNotificationPreference>>(API_ENDPOINTS.notifications.preferences)
      .pipe(
        map(res => ({
          ...res,
          data: res.data
            ? this.mapPreferences(res.data)
            : { inAppEnabled: true, emailEnabled: true },
        }))
      );
  }

  updatePreferences(dto: Partial<NotificationPreference>): Observable<ApiResponse<NotificationPreference>> {
    return this.http
      .put<ApiResponse<BackendNotificationPreference>>(API_ENDPOINTS.notifications.preferences, {
        inAppNotificationsEnabled: !!dto.inAppEnabled,
        emailNotificationsEnabled: !!dto.emailEnabled,
      })
      .pipe(
        map(res => ({
          ...res,
          data: res.data
            ? this.mapPreferences(res.data)
            : { inAppEnabled: !!dto.inAppEnabled, emailEnabled: !!dto.emailEnabled },
        }))
      );
  }

  private mapPreferences(preference: BackendNotificationPreference): NotificationPreference {
    return {
      inAppEnabled: preference.inAppNotificationsEnabled,
      emailEnabled: preference.emailNotificationsEnabled,
      updatedAtUtc: preference.updatedAtUtc,
    };
  }
}
