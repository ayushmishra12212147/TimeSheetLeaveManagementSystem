import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttendanceService } from '../../../services/attendance';
import { AttendanceRecord } from '../../../models/attendance';
import { AlertService } from '../../../services/alert.service';
import { AttendanceScan } from '../attendance-scan/attendance-scan';

@Component({
  selector: 'app-team-attendance',
  imports: [NgFor, NgIf, NgClass, DatePipe, FormsModule, AttendanceScan],
  templateUrl: './team-attendance.html',
  styleUrl: './team-attendance.css'
})
export class TeamAttendance implements OnInit {
  records: AttendanceRecord[] = [];
  loading = true;
  errorMessage = '';
  selectedDate = '';

  // Scan modal state
  showScanModal = false;
  activeScanType: 'clock-in' | 'clock-out' = 'clock-in';

  constructor(
    private attendanceService: AttendanceService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.selectedDate = new Date().toISOString().split('T')[0];
    this.loadRecords();
  }

  loadRecords(): void {
    this.loading = true;
    this.attendanceService.getTeamAttendance(this.selectedDate).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) this.records = res.data || [];
        else this.errorMessage = res.message;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load.';
      }
    });
  }

  onDateChange(): void { this.loadRecords(); }

  openScanner(type: 'clock-in' | 'clock-out'): void {
    this.activeScanType = type;
    this.showScanModal = true;
  }

  onScanSuccess(): void {
    this.showScanModal = false;
    this.alertService.success(
      this.activeScanType === 'clock-in'
        ? 'Employee clocked in successfully!'
        : 'Employee clocked out successfully!'
    );
    this.loadRecords();
  }

  onScanClose(): void {
    this.showScanModal = false;
  }

  getStatusBadge(s: string): string {
    switch (s) {
      case 'Present': return 'badge-success';
      case 'HalfDay': return 'badge-warning';
      case 'Absent': return 'badge-danger';
      case 'PendingClockOut': return 'badge-info';
      default: return 'badge-info';
    }
  }
}
