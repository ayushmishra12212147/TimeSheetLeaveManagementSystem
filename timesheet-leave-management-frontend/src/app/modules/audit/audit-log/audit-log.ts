import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditService } from '../../../services/audit';
import { AuditLog, AuditLogFilter } from '../../../models/audit';

@Component({
  selector: 'app-audit-log',
  imports: [NgFor, NgIf, DatePipe, FormsModule],
  templateUrl: './audit-log.html',
  styleUrl: './audit-log.css'
})
export class AuditLogComponent implements OnInit {
  logs: AuditLog[] = []; loading = true; errorMessage = '';
  page = 1; pageSize = 20; totalCount = 0;
  filterService = ''; filterAction = '';

  constructor(private auditService: AuditService) {}

  ngOnInit(): void { this.loadLogs(); }

  loadLogs(): void {
    this.loading = true;
    const filter: AuditLogFilter = { page: this.page, pageSize: this.pageSize, serviceName: this.filterService || undefined, action: this.filterAction || undefined };
    this.auditService.getLogs(filter).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) { this.logs = res.data.items; this.totalCount = res.data.totalCount; }
        else this.errorMessage = res.message;
      },
      error: (err) => { this.loading = false; this.errorMessage = err.error?.message || 'Failed.'; }
    });
  }

  applyFilter(): void { this.page = 1; this.loadLogs(); }
  nextPage(): void { if (this.page * this.pageSize < this.totalCount) { this.page++; this.loadLogs(); } }
  prevPage(): void { if (this.page > 1) { this.page--; this.loadLogs(); } }

  get totalPages(): number { return Math.ceil(this.totalCount / this.pageSize); }
}
