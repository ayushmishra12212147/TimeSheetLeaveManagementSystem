import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttendanceService } from '../../../services/attendance';
import { AttendanceRecord, GenerateQrResponse } from '../../../models/attendance';
import { AlertService } from '../../../services/alert.service';
import { AttendanceScan } from '../attendance-scan/attendance-scan';

@Component({
  selector: 'app-my-attendance',
  imports: [NgFor, NgIf, NgClass, DatePipe, FormsModule, AttendanceScan],
  templateUrl: './my-attendance.html',
  styleUrl: './my-attendance.css'
})
export class MyAttendance implements OnInit {
  records: AttendanceRecord[] = [];
  loading = true;
  errorMessage = '';
  selectedMonth = '';

  // QR modal state
  showScanModal = false;
  activeScanType: 'clock-in' | 'clock-out' = 'clock-in';

  // QR display state (for showing generated QR to manager)
  showQrDisplay = false;
  qrData: GenerateQrResponse | null = null;
  qrLoading = false;

  constructor(
    private attendanceService: AttendanceService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.selectedMonth = new Date().toISOString().slice(0, 7);
    this.loadRecords();
  }

  loadRecords(): void {
    this.loading = true;
    this.attendanceService.getMyAttendance(this.selectedMonth).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.records = this.fillMissingDates(res.data || [], this.selectedMonth);
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load attendance.';
      }
    });
  }

  private fillMissingDates(records: AttendanceRecord[], monthStr: string): AttendanceRecord[] {
    const [year, month] = monthStr.split('-').map(Number);
    const startDate = new Date(year, month - 1, 1);
    const endDate = new Date(year, month, 0); // Last day of the month

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Only show up to today's date if it's the current month
    const targetEndDate = endDate > today ? today : endDate;

    const recordMap = new Map<string, AttendanceRecord>();
    records.forEach(r => {
      // Backend returns YYYY-MM-DD string, so we can use it as a key
      const datePart = r.attendanceDate.split('T')[0];
      recordMap.set(datePart, r);
    });

    const filledRecords: AttendanceRecord[] = [];

    for (let d = new Date(startDate); d <= targetEndDate; d.setDate(d.getDate() + 1)) {
      const y = d.getFullYear();
      const m = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      const dateString = `${y}-${m}-${day}`;

      if (recordMap.has(dateString)) {
        filledRecords.push(recordMap.get(dateString)!);
      } else {
        filledRecords.push({
          id: '',
          employeeUserId: '',
          employeeId: '',
          employeeName: '',
          attendanceDate: dateString, // Backend DTO format is YYYY-MM-DD
          clockInAtUtc: null,
          clockOutAtUtc: null,
          durationMinutes: null,
          status: 'Absent',
          scannedInByManagerName: null,
          scannedOutByManagerName: null,
          createdAtUtc: new Date().toISOString()
        });
      }
    }

    // Sort descending by date (newest first)
    return filledRecords.sort((a, b) => new Date(b.attendanceDate).getTime() - new Date(a.attendanceDate).getTime());
  }

  onMonthChange(): void {
    this.loadRecords();
  }

  /** Generate a QR code for the employee to show to their manager */
  generateQr(type: 'clock-in' | 'clock-out'): void {
    this.qrLoading = true;
    this.qrData = null;
    this.attendanceService.generateQr(type).subscribe({
      next: (res) => {
        this.qrLoading = false;
        if (res.success) {
          this.qrData = res.data;
          this.showQrDisplay = true;
        } else {
          this.alertService.error(res.message || 'Failed to generate QR.');
        }
      },
      error: (err) => {
        this.qrLoading = false;
        this.alertService.error(err.error?.message || 'Failed to generate QR.');
      }
    });
  }

  /** Open the camera scanner (for managers scanning employee QR) */
  openScanner(type: 'clock-in' | 'clock-out'): void {
    this.activeScanType = type;
    this.showScanModal = true;
  }

  onScanSuccess(): void {
    this.showScanModal = false;
    this.alertService.success(
      this.activeScanType === 'clock-in' ? 'Clocked in successfully!' : 'Clocked out successfully!'
    );
    this.loadRecords();
  }

  onScanClose(): void {
    this.showScanModal = false;
  }

  closeQrDisplay(): void {
    this.showQrDisplay = false;
    this.qrData = null;
  }

  getStatusBadge(status: string): string {
    switch (status) {
      case 'Present': return 'badge-success';
      case 'HalfDay': return 'badge-warning';
      case 'Absent': return 'badge-danger';
      case 'PendingClockOut': return 'badge-info';
      default: return 'badge-info';
    }
  }

  formatDuration(mins: number | null): string {
    if (!mins) return '-';
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return `${h}h ${m}m`;
  }

  getQrExpiryCountdown(): string {
    if (!this.qrData) return '';
    const expiry = new Date(this.qrData.expiresAtUtc);
    const now = new Date();
    const diffMs = expiry.getTime() - now.getTime();
    if (diffMs <= 0) return 'Expired';
    const mins = Math.floor(diffMs / 60000);
    const secs = Math.floor((diffMs % 60000) / 1000);
    return `${mins}m ${secs}s`;
  }

  encodeURIComponent(str: string): string {
    return encodeURIComponent(str);
  }
}
