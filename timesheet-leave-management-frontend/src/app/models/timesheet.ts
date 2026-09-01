export interface TimesheetEntry {
  id: string;
  weeklyTimesheetSummaryId: string;
  entryDate: string;
  projectId: string;
  projectName: string;
  hours: number;
  category: string;
  description: string | null;
  status: string;
  dailyTotalHours: number;
  isBelowDailyThresholdWarning: boolean;
  isAboveDailyThresholdWarning: boolean;
  updatedAtUtc: string;
}

export interface CreateTimesheetEntryDto {
  entryDate: string;
  projectId: string;
  hours: number;
  category: string | number;
  description?: string;
}

export interface UpdateTimesheetEntryDto extends CreateTimesheetEntryDto {}

export interface SubmitTimesheetDto {
  weekStartDate: string;
}

export interface ApproveTimesheetDto {
  comment?: string;
}

export interface RejectTimesheetDto {
  reason: string;
}

export interface WeeklyTimesheetSummary {
  id: string;
  employeeUserId: string;
  employeeId: string;
  employeeName: string;
  departmentName: string;
  managerUserId: string | null;
  managerName: string | null;
  weekStartDate: string;
  weekEndDate: string;
  totalHours: number;
  entryCount: number;
  status: string;
  isLateSubmission: boolean;
  submittedAtUtc: string | null;
  approvedByName: string | null;
  approvedAtUtc: string | null;
  rejectedByName: string | null;
  rejectionReason: string | null;
  rejectedAtUtc: string | null;
  projectBreakdown: { projectName: string; hours: number }[];
}

export interface WeekTimesheet {
  summaryId: string | null;
  employeeId: string;
  employeeName: string;
  weekStartDate: string;
  weekEndDate: string;
  totalHours: number;
  minimumWeeklyHours: number;
  meetsMinimumWeeklyHours: boolean;
  status: string;
  isLateSubmission: boolean;
  rejectionReason: string | null;
  entries: TimesheetEntry[];
}

export interface TimesheetConfig {
  id: string;
  minimumWeeklyHours: number;
  lowHoursWarningThreshold: number;
  highHoursWarningThreshold: number;
  autoApproveEnabled: boolean;
  autoApproveAfterHours: number;
  updatedAtUtc: string;
}

export interface UpdateTimesheetConfigDto {
  minimumWeeklyHours: number;
  lowHoursWarningThreshold: number;
  highHoursWarningThreshold: number;
  autoApproveEnabled: boolean;
  autoApproveAfterHours: number;
}
