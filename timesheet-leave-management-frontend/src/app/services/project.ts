import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ApiResponse } from '../models/common';
import { Project, CreateProjectDto, UpdateProjectDto } from '../models/project';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  constructor(private http: HttpClient) {}

  getProjects(): Observable<ApiResponse<Project[]>> {
    return this.http.get<ApiResponse<Project[]>>(API_ENDPOINTS.projects.base).pipe(
      map(res => ({
        ...res,
        data: (res.data || []).map(project => this.mapProject(project)),
      }))
    );
  }

  getProjectById(id: string): Observable<ApiResponse<Project | null>> {
    return this.getProjects().pipe(
      map(res => {
        if (!res.success) {
          return { ...res, data: null };
        }

        const project = (res.data || []).find(candidate => candidate.id === id) ?? null;
        if (!project) {
          return {
            success: false,
            message: 'Project not found.',
            data: null,
          };
        }

        return {
          ...res,
          data: project,
        };
      })
    );
  }

  createProject(dto: CreateProjectDto): Observable<ApiResponse<Project>> {
    return this.http.post<ApiResponse<Project>>(API_ENDPOINTS.projects.base, this.toProjectPayload(dto)).pipe(
      map(res => ({
        ...res,
        data: res.data ? this.mapProject(res.data) : (null as unknown as Project),
      }))
    );
  }

  updateProject(id: string, dto: UpdateProjectDto): Observable<ApiResponse<Project>> {
    return this.http.put<ApiResponse<Project>>(API_ENDPOINTS.projects.byId(id), this.toProjectPayload(dto)).pipe(
      map(res => ({
        ...res,
        data: res.data ? this.mapProject(res.data) : (null as unknown as Project),
      }))
    );
  }

  toggleProject(id: string): Observable<ApiResponse<Project>> {
    return this.http.post<ApiResponse<Project>>(API_ENDPOINTS.projects.toggle(id), {}).pipe(
      map(res => ({
        ...res,
        data: res.data ? this.mapProject(res.data) : (null as unknown as Project),
      }))
    );
  }

  private toProjectPayload(dto: CreateProjectDto | UpdateProjectDto) {
    return {
      name: dto.name.trim(),
      code: dto.code.trim().toUpperCase(),
      description: dto.description?.trim() || null,
      isActive: !!dto.isActive,
    };
  }

  private mapProject(project: Project): Project {
    return {
      id: project.id,
      name: project.name,
      code: project.code,
      description: project.description ?? null,
      isActive: project.isActive ?? true,
      updatedAtUtc: project.updatedAtUtc ?? '',
    };
  }
}
