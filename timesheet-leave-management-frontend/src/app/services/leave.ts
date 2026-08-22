import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import {
  LeaveRequest,
  CreateLeaveRequestDto,
  UpdateLeaveRequestDto,
  ApproveLeaveDto,
  RejectLeaveDto
} from '../models/leave';

@Injectable({
  providedIn: 'root'
})
export class LeaveService {

  constructor(private http: HttpClient) {}

  // ─── My Leaves ────────────────────────────────────

  getMyLeaves(status?: string): Observable<ApiResponse<LeaveRequest[]>> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<ApiResponse<LeaveRequest[]>>(
      API_ENDPOINTS.leaves.base, { params }
    );
  }

  getLeaveById(id: string): Observable<ApiResponse<LeaveRequest>> {
    return this.http.get<ApiResponse<LeaveRequest>>(
      API_ENDPOINTS.leaves.byId(id)
    );
  }

  createLeave(dto: CreateLeaveRequestDto): Observable<ApiResponse<LeaveRequest>> {
    return this.http.post<ApiResponse<LeaveRequest>>(
      API_ENDPOINTS.leaves.base, dto
    );
  }

  updateLeave(id: string, dto: UpdateLeaveRequestDto): Observable<ApiResponse<LeaveRequest>> {
    return this.http.put<ApiResponse<LeaveRequest>>(
      API_ENDPOINTS.leaves.byId(id), dto
    );
  }

  withdrawLeave(id: string): Observable<ApiResponse<null>> {
    return this.http.patch<ApiResponse<null>>(
      API_ENDPOINTS.leaves.withdraw(id), {}
    );
  }

  cancelLeave(id: string, reason: string): Observable<ApiResponse<null>> {
    return this.http.patch<ApiResponse<null>>(
      API_ENDPOINTS.leaves.cancel(id), { reason }
    );
  }

  // ─── Approvals (Manager / HRAdmin) ────────────────

  getPendingLeaves(): Observable<ApiResponse<LeaveRequest[]>> {
    return this.http.get<ApiResponse<LeaveRequest[]>>(
      API_ENDPOINTS.leaves.pending
    );
  }

  approveLeave(id: string, dto?: ApproveLeaveDto): Observable<ApiResponse<null>> {
    return this.http.patch<ApiResponse<null>>(
      API_ENDPOINTS.leaves.approve(id), dto || {}
    );
  }

  rejectLeave(id: string, dto: RejectLeaveDto): Observable<ApiResponse<null>> {
    return this.http.patch<ApiResponse<null>>(
      API_ENDPOINTS.leaves.reject(id), dto
    );
  }
}
