import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { TimesheetConfig, UpdateTimesheetConfigDto } from '../models/timesheet';

@Injectable({
  providedIn: 'root'
})
export class TimesheetConfigService {

  constructor(private http: HttpClient) {}

  getConfig(): Observable<ApiResponse<TimesheetConfig>> {
    return this.http.get<ApiResponse<TimesheetConfig>>(API_ENDPOINTS.timesheetConfig.base);
  }

  updateConfig(dto: UpdateTimesheetConfigDto): Observable<ApiResponse<TimesheetConfig>> {
    return this.http.put<ApiResponse<TimesheetConfig>>(API_ENDPOINTS.timesheetConfig.base, dto);
  }
}
