import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import {
  TimesheetEntry,
  WeekTimesheet,
  WeeklyTimesheetSummary,
  CreateTimesheetEntryDto,
  UpdateTimesheetEntryDto,
  SubmitTimesheetDto,
  ApproveTimesheetDto,
  RejectTimesheetDto
} from '../models/timesheet';

@Injectable({
  providedIn: 'root'
})
export class TimesheetService {

  constructor(private http: HttpClient) {}

  // ─── My Timesheet ────────────────────────────────

  getWeekTimesheet(weekStartDate: string, employeeId?: string): Observable<ApiResponse<WeekTimesheet>> {
    let params = new HttpParams().set('weekStartDate', weekStartDate);
    if (employeeId) {
      params = params.set('employeeId', employeeId);
    }
    return this.http.get<ApiResponse<WeekTimesheet>>(
      API_ENDPOINTS.timesheets.week, { params }
    );
  }

  createEntry(dto: CreateTimesheetEntryDto): Observable<ApiResponse<TimesheetEntry>> {
    return this.http.post<ApiResponse<TimesheetEntry>>(
      API_ENDPOINTS.timesheets.base, dto
    );
  }

  updateEntry(id: string, dto: UpdateTimesheetEntryDto): Observable<ApiResponse<TimesheetEntry>> {
    return this.http.put<ApiResponse<TimesheetEntry>>(
      API_ENDPOINTS.timesheets.byId(id), dto
    );
  }

  deleteEntry(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(
      API_ENDPOINTS.timesheets.byId(id)
    );
  }

  submitWeek(dto: SubmitTimesheetDto): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(
      API_ENDPOINTS.timesheets.submit, dto
    );
  }

  // ─── Approvals ────────────────────────────────────

  getPendingTimesheets(): Observable<ApiResponse<WeeklyTimesheetSummary[]>> {
    return this.http.get<ApiResponse<WeeklyTimesheetSummary[]>>(
      API_ENDPOINTS.timesheets.pending
    );
  }

  approveTimesheet(id: string, dto?: ApproveTimesheetDto): Observable<ApiResponse<null>> {
    return this.http.patch<ApiResponse<null>>(
      API_ENDPOINTS.timesheets.approve(id), dto || {}
    );
  }

  rejectTimesheet(id: string, dto: RejectTimesheetDto): Observable<ApiResponse<null>> {
    return this.http.patch<ApiResponse<null>>(
      API_ENDPOINTS.timesheets.reject(id), dto
    );
  }
}
