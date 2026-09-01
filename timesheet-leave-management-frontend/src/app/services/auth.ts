import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import {
  LoginRequest,
  LoginResponse,
  UserProfile,
  TokenPayload,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  FirstLoginResetRequest
} from '../models/auth';

interface BackendUserProfile {
  id: string;
  employeeId: string;
  fullName: string;
  email: string;
  role: string;
  gender?: string | null;
  departmentId?: string | null;
  managerId?: string | null;
  isFirstLogin?: boolean;
  mustResetPassword?: boolean;
}

interface BackendLoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: BackendUserProfile;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly TOKEN_KEY = 'access_token';

  private currentUser$ = new BehaviorSubject<UserProfile | null>(null);

  constructor(private http: HttpClient) {
    this.loadUserFromToken();
  }

  // ─── Auth API Calls ──────────────────────────────

  login(payload: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<BackendLoginResponse>>(
      API_ENDPOINTS.auth.login,
      payload
    ).pipe(
      map(res => this.normalizeLoginResponse(res)),
      tap(res => {
        if (res.success && res.data?.accessToken) {
          this.setToken(res.data.accessToken);
          if (res.data.user) {
            this.currentUser$.next(res.data.user);
          } else {
            this.loadUserFromToken();
          }
        }
      })
    );
  }

  logout(): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(
      API_ENDPOINTS.auth.logout,
      {}
    ).pipe(
      tap(() => this.clearSession())
    );
  }

  forgotPassword(payload: ForgotPasswordRequest): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(
      API_ENDPOINTS.auth.forgotPassword,
      payload
    );
  }

  resetPassword(payload: ResetPasswordRequest): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(
      API_ENDPOINTS.auth.resetPassword,
      payload
    );
  }

  firstLoginReset(payload: FirstLoginResetRequest): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(
      API_ENDPOINTS.auth.firstLoginReset,
      payload
    );
  }

  refresh(): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<BackendLoginResponse>>(
      API_ENDPOINTS.auth.refresh,
      {}
    ).pipe(
      map(res => this.normalizeLoginResponse(res)),
      tap(res => {
        if (res.success && res.data?.accessToken) {
          this.setToken(res.data.accessToken);
          if (res.data.user) {
            this.currentUser$.next(res.data.user);
          } else {
            this.loadUserFromToken();
          }
        }
      })
    );
  }

  getMe(): Observable<ApiResponse<UserProfile>> {
    return this.http.get<ApiResponse<BackendUserProfile>>(API_ENDPOINTS.auth.me).pipe(
      map(res => ({
        ...res,
        data: this.mapUserProfile(res.data),
      })),
      tap(res => {
        if (res.success && res.data) {
          this.currentUser$.next(res.data);
        }
      })
    );
  }

  // ─── Token Management ────────────────────────────

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  clearSession(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.currentUser$.next(null);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;

    const payload = this.decodeToken(token);
    if (!payload) return false;

    return payload.exp * 1000 > Date.now();
  }

  isFirstLogin(): boolean {
    const token = this.getToken();
    if (!token) return false;

    const payload = this.decodeToken(token);
    return payload?.first_login === 'true';
  }

  getUserRole(): string | null {
    const token = this.getToken();
    if (!token) return null;

    const payload = this.decodeToken(token);
    return payload?.role ?? null;
  }

  getUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;

    const payload = this.decodeToken(token);
    return payload?.sub ?? null;
  }

  getEmployeeId(): string | null {
    const token = this.getToken();
    if (!token) return null;

    const payload = this.decodeToken(token);
    return payload?.employee_id ?? null;
  }

  getCurrentUser(): Observable<UserProfile | null> {
    return this.currentUser$.asObservable();
  }

  getCurrentUserValue(): UserProfile | null {
    return this.currentUser$.getValue();
  }

  // ─── Private Helpers ─────────────────────────────

  private loadUserFromToken(): void {
    const token = this.getToken();
    if (!token) {
      this.currentUser$.next(null);
      return;
    }

    const payload = this.decodeToken(token);
    if (!payload) {
      this.currentUser$.next(null);
      return;
    }

    this.currentUser$.next({
      userId: payload.sub,
      employeeId: payload.employee_id,
      fullName: '',
      email: payload.email,
      role: payload.role,
      gender: payload.gender,
      departmentName: '',
      isFirstLogin: payload.first_login === 'true',
      mustResetPassword: false,
    });
  }

  private decodeToken(token: string): TokenPayload | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;

      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      const parsed = JSON.parse(decoded);

      // .NET ClaimTypes.Role serializes as the long URI in JWT
      // Normalize it to a simple 'role' field
      const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
      if (!parsed.role && parsed[ROLE_CLAIM]) {
        parsed.role = parsed[ROLE_CLAIM];
      }

      return parsed as TokenPayload;
    } catch {
      return null;
    }
  }

  private normalizeLoginResponse(res: ApiResponse<BackendLoginResponse>): ApiResponse<LoginResponse> {
    const expiresAt = res.data?.expiresAtUtc ? Date.parse(res.data.expiresAtUtc) : Date.now();
    const user = res.data?.user ? this.mapUserProfile(res.data.user) : undefined;

    return {
      ...res,
      data: {
        accessToken: res.data?.accessToken ?? '',
        expiresIn: Math.max(0, expiresAt - Date.now()),
        isFirstLogin: user?.isFirstLogin ?? false,
        mustResetPassword: user?.mustResetPassword ?? false,
        user,
      },
    };
  }

  private mapUserProfile(user: BackendUserProfile): UserProfile {
    return {
      userId: user.id,
      employeeId: user.employeeId,
      fullName: user.fullName,
      email: user.email,
      role: user.role,
      gender: user.gender ?? 'Unspecified',
      departmentName: '',
      isFirstLogin: !!user.isFirstLogin,
      mustResetPassword: !!user.mustResetPassword,
    };
  }
}
