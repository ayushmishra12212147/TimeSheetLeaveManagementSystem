import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { ReportExportFormat, ReportRequest, CreateReportRequestDto, RejectReportRequestDto } from '../models/report';

type BackendReportRequest = ReportRequest & {
  requestedByEmployeeId?: string;
  scopeEmployeeId?: string | null;
};

@Injectable({ providedIn: 'root' })
export class ReportService {
  constructor(private http: HttpClient) {}

  getReports(): Observable<ApiResponse<ReportRequest[]>> {
    return this.http.get<ApiResponse<BackendReportRequest[]>>(API_ENDPOINTS.reports.requests).pipe(
      map(res => ({
        ...res,
        data: (res.data || []).map(report => this.mapReportRequest(report)),
      }))
    );
  }

  createReport(dto: CreateReportRequestDto): Observable<ApiResponse<ReportRequest>> {
    return this.http.post<ApiResponse<BackendReportRequest>>(API_ENDPOINTS.reports.requests, dto).pipe(
      map(res => ({
        ...res,
        data: res.data ? this.mapReportRequest(res.data) : (null as unknown as ReportRequest),
      }))
    );
  }

  approveReport(id: string): Observable<ApiResponse<ReportRequest>> {
    return this.http.patch<ApiResponse<BackendReportRequest>>(API_ENDPOINTS.reports.approve(id), {}).pipe(
      map(res => ({
        ...res,
        data: res.data ? this.mapReportRequest(res.data) : (null as unknown as ReportRequest),
      }))
    );
  }

  rejectReport(id: string, dto: RejectReportRequestDto): Observable<ApiResponse<ReportRequest>> {
    return this.http.patch<ApiResponse<BackendReportRequest>>(API_ENDPOINTS.reports.reject(id), dto).pipe(
      map(res => ({
        ...res,
        data: res.data ? this.mapReportRequest(res.data) : (null as unknown as ReportRequest),
      }))
    );
  }

  exportReport(id: string, format: ReportExportFormat): Observable<Blob> {
    return this.http.get(API_ENDPOINTS.reports.export(id, format), { responseType: 'blob' });
  }

  private mapReportRequest(report: BackendReportRequest): ReportRequest {
    return {
      id: report.id,
      reportType: report.reportType,
      status: report.status,
      dateFrom: report.dateFrom,
      dateTo: report.dateTo,
      requestedByEmployeeId: report.requestedByEmployeeId ?? '',
      scopeEmployeeId: report.scopeEmployeeId ?? null,
      requestedByName: report.requestedByName,
      approvedByName: report.approvedByName ?? null,
      approvedAtUtc: report.approvedAtUtc ?? null,
      rejectedByName: report.rejectedByName ?? null,
      rejectedAtUtc: report.rejectedAtUtc ?? null,
      rejectionReason: report.rejectionReason ?? null,
      createdAtUtc: report.createdAtUtc,
    };
  }
}
