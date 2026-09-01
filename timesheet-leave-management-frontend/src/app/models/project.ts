export interface Project {
  id: string;
  name: string;
  code: string;
  description: string | null;
  isActive: boolean;
  updatedAtUtc: string;
}

export interface CreateProjectDto {
  name: string;
  code: string;
  description?: string | null;
  isActive: boolean;
}

export interface UpdateProjectDto extends CreateProjectDto {}
