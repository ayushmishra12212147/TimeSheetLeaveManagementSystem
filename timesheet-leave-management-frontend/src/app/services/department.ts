import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { Department } from '../models/department';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  constructor(private http: HttpClient) {}

  getDepartments(): Observable<ApiResponse<Department[]>> {
    return this.http.get<Department[]>(API_ENDPOINTS.departments.base).pipe(
      map(data => ({ success: true, message: 'Departments fetched', data }))
    );
  }

  createDepartment(dto: { name: string }): Observable<ApiResponse<Department>> {
    return this.http.post<Department>(API_ENDPOINTS.departments.base, dto).pipe(
      map(data => ({ success: true, message: 'Department created', data }))
    );
  }

  updateDepartment(id: string, dto: { name: string }): Observable<ApiResponse<Department>> {
    return this.http.put<Department>(API_ENDPOINTS.departments.byId(id), dto).pipe(
      map(data => ({ success: true, message: 'Department updated', data }))
    );
  }

  deleteDepartment(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<any>(API_ENDPOINTS.departments.byId(id)).pipe(
      map(() => ({ success: true, message: 'Department deleted', data: null }))
    );
  }
}
