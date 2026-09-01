import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, DatePipe, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LeaveService } from '../../../services/leave';
import { LeaveRequest } from '../../../models/leave';
import { LEAVE_STATUS_LABELS } from '../../../constants/enums';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-my-leaves',
  imports: [NgFor, NgIf, NgClass, DatePipe, RouterLink, FormsModule],
  templateUrl: './my-leaves.html',
  styleUrl: './my-leaves.css'
})
export class MyLeaves implements OnInit {

  leaves: LeaveRequest[] = [];
  filteredLeaves: LeaveRequest[] = [];
  loading = true;
  errorMessage = '';
  statusFilter = '';
  statusLabels = LEAVE_STATUS_LABELS;

  statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'PendingManagerApproval', label: 'Pending Manager' },
    { value: 'PendingHrApproval', label: 'Pending HR' },
    { value: 'Approved', label: 'Approved' },
    { value: 'Rejected', label: 'Rejected' },
    { value: 'Withdrawn', label: 'Withdrawn' },
    { value: 'Cancelled', label: 'Cancelled' },
  ];

  constructor(
    private leaveService: LeaveService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.loadLeaves();
  }

  loadLeaves(): void {
    this.loading = true;
    this.errorMessage = '';

    this.leaveService.getMyLeaves().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.leaves = res.data || [];
          this.applyFilter();
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load leaves.';
      }
    });
  }

  applyFilter(): void {
    if (this.statusFilter) {
      this.filteredLeaves = this.leaves.filter(l => l.status === this.statusFilter);
    } else {
      this.filteredLeaves = [...this.leaves];
    }
  }

  onFilterChange(): void {
    this.applyFilter();
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Approved': return 'badge-success';
      case 'Rejected': return 'badge-danger';
      case 'PendingManagerApproval':
      case 'PendingHrApproval': return 'badge-warning';
      case 'Withdrawn':
      case 'Cancelled': return 'badge-gray';
      default: return 'badge-info';
    }
  }

  getStatusLabel(status: string): string {
    return this.statusLabels[status] || status;
  }

  canWithdraw(leave: LeaveRequest): boolean {
    return leave.status === 'PendingManagerApproval' || leave.status === 'PendingHrApproval';
  }

  canCancel(leave: LeaveRequest): boolean {
    if (leave.status !== 'Approved') return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const startDate = new Date(leave.startDate);
    startDate.setHours(0, 0, 0, 0);
    return startDate >= today;
  }

  async withdrawLeave(leave: LeaveRequest): Promise<void> {
    const confirmed = await this.alertService.confirm('Are you sure you want to withdraw this leave request?', 'Withdraw Leave');
    if (!confirmed) return;

    this.leaveService.withdrawLeave(leave.id).subscribe({
      next: () => {
        this.alertService.success('Leave request withdrawn.');
        this.loadLeaves();
      },
      error: (err) => this.alertService.error(err.error?.message || 'Failed to withdraw.')
    });
  }

  async cancelLeave(leave: LeaveRequest): Promise<void> {
    const reason = await this.alertService.prompt('Cancel Leave', 'Please provide a reason for cancellation:', 'e.g. Plans changed');
    if (!reason) return;

    this.leaveService.cancelLeave(leave.id, reason).subscribe({
      next: () => {
        this.alertService.success('Leave cancelled successfully.');
        this.loadLeaves();
      },
      error: (err) => this.alertService.error(err.error?.message || 'Failed to cancel.')
    });
  }
}