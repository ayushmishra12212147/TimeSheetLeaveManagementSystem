import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TimesheetService } from '../../../services/timesheet';
import { WeeklyTimesheetSummary, WeekTimesheet } from '../../../models/timesheet';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-timesheet-approvals',
  imports: [NgFor, NgIf, DatePipe, FormsModule],
  templateUrl: './timesheet-approvals.html',
  styleUrl: './timesheet-approvals.css'
})
export class TimesheetApprovals implements OnInit {

  pendingTimesheets: WeeklyTimesheetSummary[] = [];
  loading = true;
  errorMessage = '';
  actionLoading: Record<string, boolean> = {};
  rejectReasonMap: Record<string, string> = {};
  showRejectForm: Record<string, boolean> = {};
  timesheetDetailsMap: Record<string, WeekTimesheet> = {};
  loadingDetails: Record<string, boolean> = {};

  constructor(private timesheetService: TimesheetService, private alertService: AlertService) {}

  ngOnInit(): void {
    this.loadPending();
  }

  loadPending(): void {
    this.loading = true;
    this.errorMessage = '';

    this.timesheetService.getPendingTimesheets().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.pendingTimesheets = res.data || [];
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load pending timesheets.';
      }
    });
  }

  toggleDetails(ts: WeeklyTimesheetSummary): void {
    if (this.timesheetDetailsMap[ts.id]) {
      delete this.timesheetDetailsMap[ts.id];
      return;
    }

    this.loadingDetails[ts.id] = true;
    this.timesheetService.getWeekTimesheet(ts.weekStartDate, ts.employeeId).subscribe({
      next: (res) => {
        this.loadingDetails[ts.id] = false;
        if (res.success && res.data) {
          this.timesheetDetailsMap[ts.id] = res.data;
        } else {
          this.alertService.error(res.message);
        }
      },
      error: (err) => {
        this.loadingDetails[ts.id] = false;
        this.alertService.error(err.error?.message || 'Failed to load details.');
      }
    });
  }

  async approveTimesheet(ts: WeeklyTimesheetSummary): Promise<void> {
    const confirmed = await this.alertService.confirm(`Approve timesheet from ${ts.employeeName}?`, 'Approve Timesheet');
    if (!confirmed) return;

    this.actionLoading[ts.id] = true;
    this.timesheetService.approveTimesheet(ts.id).subscribe({
      next: () => {
        this.actionLoading[ts.id] = false;
        this.alertService.success('Timesheet approved successfully.');
        this.loadPending();
      },
      error: (err) => {
        this.actionLoading[ts.id] = false;
        this.alertService.error(err.error?.message || 'Failed to approve.');
      }
    });
  }

  toggleRejectForm(ts: WeeklyTimesheetSummary): void {
    this.showRejectForm[ts.id] = !this.showRejectForm[ts.id];
  }

  rejectTimesheet(ts: WeeklyTimesheetSummary): void {
    const reason = this.rejectReasonMap[ts.id];
    if (!reason || reason.trim().length === 0) {
      this.alertService.error('Please provide a rejection reason.', 'Validation Error');
      return;
    }

    this.actionLoading[ts.id] = true;
    this.timesheetService.rejectTimesheet(ts.id, { reason }).subscribe({
      next: () => {
        this.actionLoading[ts.id] = false;
        this.showRejectForm[ts.id] = false;
        this.alertService.success('Timesheet rejected.');
        this.loadPending();
      },
      error: (err) => {
        this.actionLoading[ts.id] = false;
        this.alertService.error(err.error?.message || 'Failed to reject.');
      }
    });
  }

  formatWeekRange(weekStart: string): string {
    const d = new Date(weekStart);
    const end = new Date(d);
    end.setDate(end.getDate() + 6);
    const fmt = (dt: Date) => dt.toLocaleDateString('en-US', { day: 'numeric', month: 'short' });
    return `${fmt(d)} – ${fmt(end)}`;
  }
}
