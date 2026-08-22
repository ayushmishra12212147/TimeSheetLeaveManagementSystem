export interface LeaveType {
  id: string;
  name: string;
  code: string;
  description: string | null;
  defaultAnnualQuota: number;
  maxCarryForwardDays: number;
  requiresDocument: boolean;
  isAutoApprove: boolean;
  isActive: boolean;
  updatedAtUtc: string;
}

export interface CreateLeaveTypeDto {
  name: string;
  code: string;
  description?: string;
  defaultAnnualQuota: number;
  maxCarryForwardDays: number;
  requiresDocument: boolean;
  isAutoApprove: boolean;
  isActive: boolean;
}

export interface UpdateLeaveTypeDto extends CreateLeaveTypeDto {}

export interface LeaveRequest {
  id: string;
  employeeUserId: string;
  employeeId: string;
  employeeName: string;
  managerUserId: string | null;
  managerName: string | null;
  leaveTypeId: string;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  requestedDays: number;
  isHalfDay: boolean;
  halfDaySession: string;
  isUnpaid: boolean;
  reason: string;
  supportingDocumentUrl: string | null;
  status: string;
  pendingApprovalRole: string | null;
  approvedByName: string | null;
  approvedAtUtc: string | null;
  rejectedByName: string | null;
  rejectionReason: string | null;
  rejectedAtUtc: string | null;
  cancelledByName: string | null;
  cancellationReason: string | null;
  cancelledAtUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateLeaveRequestDto {
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  isHalfDay: boolean;
  halfDaySession: string | number;
  reason: string;
  supportingDocumentUrl?: string;
}

export interface UpdateLeaveRequestDto extends CreateLeaveRequestDto {}

export interface ApproveLeaveDto {
  comment?: string;
}

export interface RejectLeaveDto {
  reason: string;
}

export interface LeaveBalance {
  id: string;
  employeeUserId: string;
  employeeId: string;
  leaveTypeId: string;
  leaveTypeName: string;
  year: number;
  allocatedDays: number;
  carriedForwardDays: number;
  manualAdjustmentDays: number;
  pendingDays: number;
  usedDays: number;
  availableDays: number;
}

export interface AdjustBalanceDto {
  days: number;
  reason: string;
}

export interface CarryForwardBalanceDto {
  sourceYear: number;
  targetYear: number;
}
