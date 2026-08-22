export interface LoginRequest {
  employeeId: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresIn: number;
  isFirstLogin: boolean;
  mustResetPassword: boolean;
  user?: UserProfile;
}

export interface TokenPayload {
  sub: string;
  email: string;
  role: string;
  employee_id: string;
  first_login: string;
  dept_id: string;
  gender: string;
  exp: number;
  iss: string;
  aud: string;
}

export interface UserProfile {
  userId: string;
  employeeId: string;
  fullName: string;
  email: string;
  role: string;
  gender: string;
  departmentName: string;
  isFirstLogin: boolean;
  mustResetPassword: boolean;
}

export interface ForgotPasswordRequest {
  employeeId: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface FirstLoginResetRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
