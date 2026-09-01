import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveService } from '../../../services/leave';
import { LeaveRequest } from '../../../models/leave';
import { LEAVE_STATUS_LABELS } from '../../../constants/enums';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-leave-approvals',
  imports: [NgFor, NgIf, DatePipe, FormsModule],
  templateUrl: './leave-approvals.html',
  styleUrl: './leave-approvals.css'
})
export class LeaveApprovals implements OnInit {

  pendingLeaves: LeaveRequest[] = [];
  loading = true;
  errorMessage = '';
  actionLoading: Record<string, boolean> = {};
  rejectReasonMap: Record<string, string> = {};
  showRejectForm: Record<string, boolean> = {};
  statusLabels = LEAVE_STATUS_LABELS;

  constructor(private leaveService: LeaveService, private alertService: AlertService) {}

  ngOnInit(): void {
    this.loadPending();
  }

  loadPending(): void {
    this.loading = true;
    this.errorMessage = '';

    this.leaveService.getPendingLeaves().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.pendingLeaves = res.data || [];
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load pending leaves.';
      }
    });
  }

  async approveLeave(leave: LeaveRequest): Promise<void> {
    const confirmed = await this.alertService.confirm(`Approve leave request from ${leave.employeeName}?`, 'Approve Leave');
    if (!confirmed) return;

    this.actionLoading[leave.id] = true;
    this.leaveService.approveLeave(leave.id).subscribe({
      next: () => {
        this.actionLoading[leave.id] = false;
        this.alertService.success('Leave approved successfully.');
        this.loadPending();
      },
      error: (err) => {
        this.actionLoading[leave.id] = false;
        this.alertService.error(err.error?.message || 'Failed to approve.');
      }
    });
  }

  toggleRejectForm(leave: LeaveRequest): void {
    this.showRejectForm[leave.id] = !this.showRejectForm[leave.id];
  }

  rejectLeave(leave: LeaveRequest): void {
    const reason = this.rejectReasonMap[leave.id];
    if (!reason || reason.trim().length === 0) {
      this.alertService.error('Please provide a rejection reason.', 'Validation Error');
      return;
    }

    this.actionLoading[leave.id] = true;
    this.leaveService.rejectLeave(leave.id, { reason }).subscribe({
      next: () => {
        this.actionLoading[leave.id] = false;
        this.showRejectForm[leave.id] = false;
        this.alertService.success('Leave request rejected.');
        this.loadPending();
      },
      error: (err) => {
        this.actionLoading[leave.id] = false;
        this.alertService.error(err.error?.message || 'Failed to reject.');
      }
    });
  }

  getStatusLabel(status: string): string {
    return this.statusLabels[status] || status;
  }
}
