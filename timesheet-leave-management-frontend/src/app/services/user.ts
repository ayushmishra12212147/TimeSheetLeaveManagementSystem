import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of, switchMap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { User, CreateUserDto, UpdateUserDto, AssignManagerDto } from '../models/user';

interface BackendDepartment {
  id: string;
  name: string;
}

interface BackendUser {
  id: string;
  employeeId: string;
  fullName: string;
  email: string;
  role: string;
  gender?: string | null;
  departmentId?: string | null;
  managerId?: string | null;
  department?: BackendDepartment | null;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private userDirectory = new Map<string, string>();

  constructor(private http: HttpClient) {}

  getUsers(): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<BackendUser[]>>(API_ENDPOINTS.users.base).pipe(
      map(res => ({
        ...res,
        data: this.mapUsers(res.data || []),
      }))
    );
  }

  getUserById(id: string): Observable<ApiResponse<User | null>> {
    return this.getUsers().pipe(
      map(res => {
        if (!res.success) {
          return { ...res, data: null };
        }

        const user = (res.data || []).find(candidate => candidate.id === id) ?? null;
        if (!user) {
          return {
            success: false,
            message: 'User not found.',
            data: null,
          };
        }

        return {
          ...res,
          data: user,
        };
      })
    );
  }

  createUser(dto: CreateUserDto): Observable<ApiResponse<User>> {
    const managerId = dto.managerId?.trim();

    return this.http.post<ApiResponse<BackendUser>>(API_ENDPOINTS.users.base, this.toUserPayload(dto)).pipe(
      map(res => this.mapUserApiResponse(res)),
      switchMap(res => {
        if (!res.success || !res.data || !managerId) {
          return of(res);
        }

        return this.assignManager({ userId: res.data.id, managerId }).pipe(
          map(() => ({
            ...res,
            data: {
              ...res.data,
              managerId,
              managerName: this.userDirectory.get(managerId) ?? null,
            },
          }))
        );
      })
    );
  }

  updateUser(id: string, dto: UpdateUserDto): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<BackendUser> | BackendUser>(
      API_ENDPOINTS.users.byId(id),
      this.toUserPayload(dto)
    ).pipe(
      map(response => this.mapUserMutationResponse(response))
    );
  }

  assignManager(dto: AssignManagerDto): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<unknown>>(API_ENDPOINTS.users.assignManager, dto).pipe(
      map(res => ({
        success: true,
        message: res.message || 'Manager assigned successfully.',
        data: null,
      }))
    );
  }

  private toUserPayload(dto: CreateUserDto | UpdateUserDto) {
    return {
      fullName: dto.fullName.trim(),
      email: dto.email.trim(),
      role: dto.role,
      gender: dto.gender?.trim() || null,
      departmentId: dto.departmentId || null,
    };
  }

  private mapUsers(users: BackendUser[]): User[] {
    this.userDirectory = new Map(users.map(user => [user.id, user.fullName]));
    return users.map(user => this.mapUser(user, this.userDirectory));
  }

  private mapUserApiResponse(res: ApiResponse<BackendUser>): ApiResponse<User> {
    return {
      ...res,
      data: res.data ? this.mapAndRememberUser(res.data) : (null as unknown as User),
    };
  }

  private mapUserMutationResponse(response: ApiResponse<BackendUser> | BackendUser): ApiResponse<User> {
    if (this.isApiResponse<BackendUser>(response)) {
      return this.mapUserApiResponse(response);
    }

    return {
      success: true,
      message: 'User updated successfully.',
      data: this.mapAndRememberUser(response),
    };
  }

  private mapAndRememberUser(user: BackendUser): User {
    const mapped = this.mapUser(user, this.userDirectory);
    this.userDirectory.set(mapped.id, mapped.fullName);
    return mapped;
  }

  private mapUser(user: BackendUser, directory: Map<string, string>): User {
    return {
      id: user.id,
      employeeId: user.employeeId,
      fullName: user.fullName,
      email: user.email,
      role: user.role,
      gender: user.gender ?? null,
      departmentId: user.departmentId ?? user.department?.id ?? null,
      departmentName: user.department?.name ?? '',
      managerId: user.managerId ?? null,
      managerName: user.managerId ? (directory.get(user.managerId) ?? null) : null,
      isActive: true,
      createdAtUtc: null,
      updatedAtUtc: null,
    };
  }

  private isApiResponse<T>(value: ApiResponse<T> | T): value is ApiResponse<T> {
    return typeof value === 'object' && value !== null && 'success' in value && 'message' in value;
  }
}
