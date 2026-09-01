import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { AttendanceRecord, GenerateQrResponse, ScanQrRequest } from '../models/attendance';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  constructor(private http: HttpClient) {}

  generateQr(type: string): Observable<ApiResponse<GenerateQrResponse>> {
    return this.http.post<ApiResponse<GenerateQrResponse>>(API_ENDPOINTS.attendance.generateQr, { type: type });
  }

  scanIn(dto: ScanQrRequest): Observable<ApiResponse<AttendanceRecord>> {
    return this.http.post<ApiResponse<AttendanceRecord>>(API_ENDPOINTS.attendance.scanIn, dto);
  }

  scanOut(dto: ScanQrRequest): Observable<ApiResponse<AttendanceRecord>> {
    return this.http.post<ApiResponse<AttendanceRecord>>(API_ENDPOINTS.attendance.scanOut, dto);
  }

  getMyAttendance(month?: string): Observable<ApiResponse<AttendanceRecord[]>> {
    let params = new HttpParams();
    if (month) {
      // month is in YYYY-MM format
      const [yearStr, monthStr] = month.split('-');
      const year = parseInt(yearStr, 10);
      const monthNum = parseInt(monthStr, 10);
      const lastDay = new Date(year, monthNum, 0).getDate();
      
      params = params.set('dateFrom', `${year}-${monthStr}-01`);
      params = params.set('dateTo', `${year}-${monthStr}-${lastDay.toString().padStart(2, '0')}`);
    }
    return this.http.get<ApiResponse<AttendanceRecord[]>>(API_ENDPOINTS.attendance.my, { params });
  }

  getTeamAttendance(date?: string): Observable<ApiResponse<AttendanceRecord[]>> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    return this.http.get<ApiResponse<AttendanceRecord[]>>(API_ENDPOINTS.attendance.team, { params });
  }
}
