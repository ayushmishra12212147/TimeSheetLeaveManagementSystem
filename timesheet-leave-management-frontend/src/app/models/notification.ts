export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  isImportant: boolean;
  createdAtUtc: string;
  readAtUtc?: string;
  actionUrl?: string;
  entityType?: string;
  entityId?: string;
}

export interface NotificationListResponse {
  items: Notification[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface NotificationPreference {
  id?: string;
  userId?: string;
  inAppEnabled: boolean;
  emailEnabled: boolean;
  updatedAtUtc?: string;
}

export interface NotificationTemplate {
  id: string;
  eventKey: string;
  name: string;
  channel?: string;
  subjectTemplate: string;
  bodyTemplate: string;
  description?: string | null;
  isActive: boolean;
  isCritical?: boolean;
  updatedAtUtc: string;
}

export interface UnreadCount {
  unreadCount: number;
}
