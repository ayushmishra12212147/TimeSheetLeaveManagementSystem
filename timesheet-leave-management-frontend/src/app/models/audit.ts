export interface AuditLog {
  id: string;
  occurredAtUtc: string;
  serviceName: string;
  eventKey: string;
  action: string;
  entityType: string;
  entityId: string;
  actorUserId: string | null;
  actorEmployeeId: string | null;
  actorName: string | null;
  subjectUserId: string | null;
  subjectEmployeeId: string | null;
  subjectName: string | null;
  outcome: string;
  description: string;
  metadataJson: string;
}

export interface AuditLogPage {
  items: AuditLog[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface AuditLogFilter {
  serviceName?: string;
  eventKey?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
  actorUserId?: string;
  subjectUserId?: string;
  dateFromUtc?: string;
  dateToUtc?: string;
  page: number;
  pageSize: number;
}
