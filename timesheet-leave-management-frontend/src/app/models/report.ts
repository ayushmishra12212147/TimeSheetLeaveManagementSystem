export type ReportTypeId = 1 | 2 | 4;
export type ReportExportFormat = 'excel' | 'pdf';

export interface ReportRequest {
  id: string;
  reportType: string;
  status: string;
  dateFrom: string;
  dateTo: string;
  requestedByEmployeeId: string;
  scopeEmployeeId: string | null;
  requestedByName: string;
  approvedByName: string | null;
  approvedAtUtc: string | null;
  rejectedByName: string | null;
  rejectedAtUtc: string | null;
  rejectionReason: string | null;
  createdAtUtc: string;
}

export interface CreateReportRequestDto {
  reportType: ReportTypeId;
  dateFrom: string;
  dateTo: string;
  employeeId?: string;
}

export interface RejectReportRequestDto {
  reason: string;
}
