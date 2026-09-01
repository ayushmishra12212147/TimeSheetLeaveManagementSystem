export interface AttendanceRecord {
  id: string;
  employeeUserId: string;
  employeeId: string;
  employeeName: string;
  attendanceDate: string;
  clockInAtUtc: string | null;
  clockOutAtUtc: string | null;
  durationMinutes: number | null;
  status: string;
  scannedInByManagerName: string | null;
  scannedOutByManagerName: string | null;
  createdAtUtc: string;
}

export interface GenerateQrResponse {
  qrPayload: string;
  expiresAtUtc: string;
  type: string;
}

export interface ScanQrRequest {
  qrPayload: string;
}
