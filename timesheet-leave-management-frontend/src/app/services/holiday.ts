import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { Holiday, CreateHolidayDto, UpdateHolidayDto, CopyHolidayYearDto } from '../models/holiday';

@Injectable({ providedIn: 'root' })
export class HolidayService {
  constructor(private http: HttpClient) {}

  getHolidays(year?: number): Observable<ApiResponse<Holiday[]>> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    return this.http.get<ApiResponse<Holiday[]>>(API_ENDPOINTS.holidays.base, { params });
  }

  createHoliday(dto: CreateHolidayDto): Observable<ApiResponse<Holiday>> {
    return this.http.post<ApiResponse<Holiday>>(API_ENDPOINTS.holidays.base, dto);
  }

  updateHoliday(id: string, dto: UpdateHolidayDto): Observable<ApiResponse<Holiday>> {
    return this.http.put<ApiResponse<Holiday>>(API_ENDPOINTS.holidays.byId(id), dto);
  }

  deleteHoliday(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(API_ENDPOINTS.holidays.byId(id));
  }

  copyYear(dto: CopyHolidayYearDto): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(API_ENDPOINTS.holidays.copyYear, dto);
  }
}
