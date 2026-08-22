import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../../services/report';
import { ReportExportFormat, ReportRequest, RejectReportRequestDto } from '../../../models/report';
import { AuthService } from '../../../services/auth';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-report-requests',
  imports: [NgFor, NgIf, NgClass, DatePipe, ReactiveFormsModule, FormsModule],
  templateUrl: './report-requests.html',
  styleUrl: './report-requests.css'
})
export class ReportRequests implements OnInit {
  reports: ReportRequest[] = [];
  loading = true;
  errorMessage = '';
  showForm = false;
  reportForm!: FormGroup;
  formLoading = false;
  actionLoading: Record<string, boolean> = {};
  rejectReasonMap: Record<string, string> = {};
  showRejectForm: Record<string, boolean> = {};
  detailsOpen: Record<string, boolean> = {};

  isManager = false;
  isHRAdmin = false;

  reportTypes = [
    { label: 'Attendance Report', value: 4 },
    { label: 'Leave Report', value: 1 },
    { label: 'Timesheet Report', value: 2 }
  ];

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private authService: AuthService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    const role = this.authService.getUserRole();
    const normalizedRole = this.normalizeRole(role);
    this.isManager = normalizedRole === 'manager';
    this.isHRAdmin = normalizedRole === 'hradmin';

    this.reportForm = this.fb.group({
      reportType: [4, [Validators.required]],
      dateFrom: ['', [Validators.required]],
      dateTo: ['', [Validators.required]],
    });
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.reportService.getReports().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) this.reports = res.data || [];
        else this.errorMessage = res.message;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load reports.';
      }
    });
  }

  openForm(): void {
    this.reportForm.reset({ reportType: 4, dateFrom: '', dateTo: '' });
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
  }

  submitForm(): void {
    if (this.reportForm.invalid) {
      this.reportForm.markAllAsTouched();
      return;
    }
    this.formLoading = true;
    this.reportService.createReport(this.reportForm.value).subscribe({
      next: () => {
        this.formLoading = false;
        this.closeForm();
        this.loadReports();
        this.alertService.success('Report request submitted.');
      },
      error: (err) => {
        this.formLoading = false;
        this.alertService.error(err.error?.message || 'Failed to submit.');
      }
    });
  }

  async approveReport(r: ReportRequest): Promise<void> {
    const confirmed = await this.alertService.confirm(`Approve report request from ${r.requestedByName}?`, 'Approve Report');
    if (!confirmed) return;

    this.actionLoading[r.id] = true;
    this.reportService.approveReport(r.id).subscribe({
      next: () => {
        this.actionLoading[r.id] = false;
        this.alertService.success('Report approved.');
        this.loadReports();
      },
      error: (err) => {
        this.actionLoading[r.id] = false;
        this.alertService.error(err.error?.message || 'Failed to approve.');
      }
    });
  }

  toggleRejectForm(r: ReportRequest): void {
    this.showRejectForm[r.id] = !this.showRejectForm[r.id];
  }

  toggleDetails(r: ReportRequest): void {
    this.detailsOpen[r.id] = !this.detailsOpen[r.id];
  }

  isDetailsOpen(r: ReportRequest): boolean {
    return !!this.detailsOpen[r.id];
  }

  rejectReport(r: ReportRequest): void {
    const reason = this.rejectReasonMap[r.id];
    if (!reason?.trim()) {
      this.alertService.error('Please provide a rejection reason.');
      return;
    }
    this.actionLoading[r.id] = true;
    const dto: RejectReportRequestDto = { reason };
    this.reportService.rejectReport(r.id, dto).subscribe({
      next: () => {
        this.actionLoading[r.id] = false;
        this.showRejectForm[r.id] = false;
        this.alertService.success('Report rejected.');
        this.loadReports();
      },
      error: (err) => {
        this.actionLoading[r.id] = false;
        this.alertService.error(err.error?.message || 'Failed to reject.');
      }
    });
  }

  exportReport(r: ReportRequest, format: ReportExportFormat): void {
    this.reportService.exportReport(r.id, format).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        const extension = format === 'excel' ? 'xlsx' : 'pdf';
        a.href = url;
        a.download = `${r.reportType}_${r.id}.${extension}`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.alertService.error('Export failed.')
    });
  }

  getStatusBadge(s: string): string {
    switch (this.normalizeStatus(s)) {
      case 'Approved': return 'badge-success';
      case 'Rejected': return 'badge-danger';
      case 'PendingHrApproval':
      case 'Pending': return 'badge-warning';
      default: return 'badge-gray';
    }
  }

  getStatusLabel(status: string): string {
    switch (this.normalizeStatus(status)) {
      case 'PendingHrApproval': return 'Pending HR Approval';
      case 'Approved': return 'Approved';
      case 'Rejected': return 'Rejected';
      default: return status || 'Unknown';
    }
  }

  isPending(r: ReportRequest): boolean {
    const status = this.normalizeStatus(r.status);
    return status === 'PendingHrApproval' || status === 'Pending';
  }

  isApproved(r: ReportRequest): boolean {
    return this.normalizeStatus(r.status) === 'Approved';
  }

  private normalizeStatus(status: string): string {
    return (status || '').replace(/\s+/g, '');
  }

  private normalizeRole(role: string | null): string {
    return (role || '').replace(/\s+/g, '').toLowerCase();
  }
}
