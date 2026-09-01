import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { API_ENDPOINTS } from '../../../constants/api-endpoints';
import { ApiResponse } from '../../../models/common';
import { LeaveRequest } from '../../../models/leave';
import { LEAVE_STATUS_LABELS } from '../../../constants/enums';

@Component({
  selector: 'app-team-calendar',
  imports: [NgFor, NgIf, NgClass, DatePipe, FormsModule],
  templateUrl: './team-calendar.html',
  styleUrl: './team-calendar.css',
})
export class TeamCalendar implements OnInit {
  leaves: LeaveRequest[] = [];
  loading = true;
  errorMessage = '';
  dateFrom = '';
  dateTo = '';
  statusLabels = LEAVE_STATUS_LABELS;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    this.dateFrom = this.fmt(firstDay);
    this.dateTo = this.fmt(lastDay);
    this.loadCalendar();
  }

  loadCalendar(): void {
    this.loading = true;
    let params = new HttpParams();
    if (this.dateFrom) params = params.set('dateFrom', this.dateFrom);
    if (this.dateTo) params = params.set('dateTo', this.dateTo);

    this.http
      .get<ApiResponse<LeaveRequest[]>>(API_ENDPOINTS.leaves.teamCalendar, { params })
      .subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) this.leaves = res.data || [];
          else this.errorMessage = res.message;
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = err.error?.message || 'Failed to load team calendar.';
        },
      });
  }

  onFilterChange(): void {
    this.loadCalendar();
  }

  getStatusBadge(status: string): string {
    switch (status) {
      case 'Approved': return 'badge-success';
      case 'Rejected': return 'badge-danger';
      case 'PendingManagerApproval':
      case 'PendingHrApproval': return 'badge-warning';
      default: return 'badge-gray';
    }
  }

  private fmt(d: Date): string {
    return d.toISOString().split('T')[0];
  }
}
