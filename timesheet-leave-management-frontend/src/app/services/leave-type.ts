import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { LeaveType, CreateLeaveTypeDto, UpdateLeaveTypeDto } from '../models/leave';

@Injectable({
  providedIn: 'root'
})
export class LeaveTypeService {

  constructor(private http: HttpClient) {}

  getLeaveTypes(): Observable<ApiResponse<LeaveType[]>> {
    return this.http.get<ApiResponse<LeaveType[]>>(API_ENDPOINTS.leaveTypes.base);
  }

  getLeaveTypeById(id: string): Observable<ApiResponse<LeaveType | null>> {
    return this.getLeaveTypes().pipe(
      map(res => {
        if (!res.success) {
          return { ...res, data: null };
        }

        const leaveType = (res.data || []).find(candidate => candidate.id === id) ?? null;
        if (!leaveType) {
          return {
            success: false,
            message: 'Leave type not found.',
            data: null,
          };
        }

        return {
          ...res,
          data: leaveType,
        };
      })
    );
  }

  createLeaveType(dto: CreateLeaveTypeDto): Observable<ApiResponse<LeaveType>> {
    return this.http.post<ApiResponse<LeaveType>>(API_ENDPOINTS.leaveTypes.base, dto);
  }

  updateLeaveType(id: string, dto: UpdateLeaveTypeDto): Observable<ApiResponse<LeaveType>> {
    return this.http.put<ApiResponse<LeaveType>>(API_ENDPOINTS.leaveTypes.byId(id), dto);
  }

  toggleLeaveType(id: string): Observable<ApiResponse<LeaveType>> {
    return this.http.post<ApiResponse<LeaveType>>(API_ENDPOINTS.leaveTypes.toggle(id), {});
  }
}
