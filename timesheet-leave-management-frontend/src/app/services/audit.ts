import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { AuditLogPage, AuditLogFilter } from '../models/audit';

@Injectable({ providedIn: 'root' })
export class AuditService {
  constructor(private http: HttpClient) {}

  getLogs(filter: AuditLogFilter): Observable<ApiResponse<AuditLogPage>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.serviceName) params = params.set('serviceName', filter.serviceName);
    if (filter.action) params = params.set('action', filter.action);
    if (filter.entityType) params = params.set('entityType', filter.entityType);
    if (filter.actorUserId) params = params.set('actorUserId', filter.actorUserId);
    if (filter.dateFromUtc) params = params.set('dateFromUtc', filter.dateFromUtc);
    if (filter.dateToUtc) params = params.set('dateToUtc', filter.dateToUtc);

    return this.http.get<ApiResponse<AuditLogPage>>(API_ENDPOINTS.audit.base, { params });
  }
}
