export enum LeaveStatus {
  PendingManagerApproval = 'PendingManagerApproval',
  PendingHrApproval = 'PendingHrApproval',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Withdrawn = 'Withdrawn',
  Cancelled = 'Cancelled',
}

export enum HalfDaySession {
  None = 'None',
  Morning = 'Morning',
  Afternoon = 'Afternoon',
}

export enum ApprovalRole {
  Manager = 'Manager',
  HRAdmin = 'HRAdmin',
}

export enum TimesheetStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  Approved = 'Approved',
  Rejected = 'Rejected',
}

export enum TimesheetCategory {
  Development = 'Development',
  Testing = 'Testing',
  Design = 'Design',
  Meeting = 'Meeting',
  Training = 'Training',
  Documentation = 'Documentation',
  Support = 'Support',
  Other = 'Other',
}

export enum AttendanceStatus {
  Absent = 'Absent',
  PendingClockOut = 'PendingClockOut',
  Present = 'Present',
  HalfDay = 'HalfDay',
}

export const LEAVE_STATUS_LABELS: Record<string, string> = {
  PendingManagerApproval: 'Pending Manager',
  PendingHrApproval: 'Pending HR',
  Approved: 'Approved',
  Rejected: 'Rejected',
  Withdrawn: 'Withdrawn',
  Cancelled: 'Cancelled',
};

export const ATTENDANCE_STATUS_LABELS: Record<string, string> = {
  Absent: 'Absent',
  PendingClockOut: 'Pending Clock Out',
  Present: 'Present',
  HalfDay: 'Half Day',
};
