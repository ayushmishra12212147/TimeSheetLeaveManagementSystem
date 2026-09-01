export interface User {
  id: string;
  employeeId: string;
  fullName: string;
  email: string;
  role: string;
  gender: string | null;
  departmentId: string | null;
  departmentName: string;
  managerId: string | null;
  managerName: string | null;
  isActive: boolean;
  createdAtUtc: string | null;
  updatedAtUtc: string | null;
}

export interface CreateUserDto {
  fullName: string;
  email: string;
  role: string;
  gender?: string;
  departmentId: string;
  managerId?: string;
}

export interface UpdateUserDto {
  fullName: string;
  email: string;
  role: string;
  gender?: string;
  departmentId: string;
}

export interface AssignManagerDto {
  userId: string;
  managerId: string;
}
