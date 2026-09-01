import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { LeaveBalance, AdjustBalanceDto, CarryForwardBalanceDto } from '../models/leave';

@Injectable({
  providedIn: 'root'
})
export class LeaveBalanceService {

  constructor(private http: HttpClient) {}

  getMyBalances(): Observable<ApiResponse<LeaveBalance[]>> {
    return this.http.get<ApiResponse<LeaveBalance[]>>(API_ENDPOINTS.leaveBalances.my);
  }

  getBalancesByEmployee(employeeId: string): Observable<ApiResponse<LeaveBalance[]>> {
    return this.http.get<ApiResponse<LeaveBalance[]>>(
      API_ENDPOINTS.leaveBalances.byEmployee(employeeId)
    );
  }

  adjustBalance(id: string, dto: AdjustBalanceDto): Observable<ApiResponse<LeaveBalance>> {
    return this.http.patch<ApiResponse<LeaveBalance>>(
      API_ENDPOINTS.leaveBalances.adjust(id), dto
    );
  }

  carryForward(dto: CarryForwardBalanceDto): Observable<ApiResponse<LeaveBalance[]>> {
    return this.http.post<ApiResponse<LeaveBalance[]>>(
      API_ENDPOINTS.leaveBalances.carryForward, dto
    );
  }
}
