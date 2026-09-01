import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TimesheetService } from '../../../services/timesheet';
import { ProjectService } from '../../../services/project';
import { TimesheetEntry, WeekTimesheet } from '../../../models/timesheet';
import { Project } from '../../../models/project';
import { TimesheetCategory } from '../../../constants/enums';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-my-timesheet',
  imports: [NgFor, NgIf, NgClass, ReactiveFormsModule],
  templateUrl: './my-timesheet.html',
  styleUrl: './my-timesheet.css'
})
export class MyTimesheet implements OnInit {

  weekTimesheet: WeekTimesheet | null = null;
  projects: Project[] = [];
  loading = true;
  errorMessage = '';
  currentWeekStart = '';
  weekDays: string[] = [];

  // Entry form
  showEntryForm = false;
  editingEntryId: string | null = null;
  entryForm!: FormGroup;
  entryFormLoading = false;

  categories = Object.values(TimesheetCategory);

  constructor(
    private fb: FormBuilder,
    private timesheetService: TimesheetService,
    private projectService: ProjectService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.currentWeekStart = this.getMonday(new Date());
    this.initEntryForm();
    this.loadProjects();
    this.loadWeek();
  }

  // ─── Entry Form (Reactive Forms with sub-FormGroup) ───

  private initEntryForm(): void {
    this.entryForm = this.fb.group({
      // Date & Project sub-group
      dateProject: this.fb.group({
        entryDate: ['', [Validators.required]],
        projectId: ['', [Validators.required]],
      }),
      // Hours & Category sub-group
      hoursDetail: this.fb.group({
        hours: [0, [Validators.required, Validators.min(0.5), Validators.max(24)]],
        category: ['Development', [Validators.required]],
      }),
      // Description
      description: ['']
    });
  }

  // ─── Week Navigation ──────────────────────────────

  previousWeek(): void {
    const d = new Date(this.currentWeekStart);
    d.setDate(d.getDate() - 7);
    this.currentWeekStart = this.formatDate(d);
    this.loadWeek();
  }

  nextWeek(): void {
    const d = new Date(this.currentWeekStart);
    d.setDate(d.getDate() + 7);
    this.currentWeekStart = this.formatDate(d);
    this.loadWeek();
  }

  goToCurrentWeek(): void {
    this.currentWeekStart = this.getMonday(new Date());
    this.loadWeek();
  }

  // ─── Data Loading ─────────────────────────────────

  loadWeek(): void {
    this.loading = true;
    this.errorMessage = '';
    this.weekDays = this.getWeekDays(this.currentWeekStart);

    this.timesheetService.getWeekTimesheet(this.currentWeekStart).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.weekTimesheet = res.data;
        } else {
          this.weekTimesheet = null;
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.weekTimesheet = null;
        this.errorMessage = err.error?.message || 'Failed to load timesheet.';
      }
    });
  }

  private loadProjects(): void {
    this.projectService.getProjects().subscribe({
      next: (res) => {
        if (res.success) {
          this.projects = (res.data || []).filter(p => p.isActive);
        }
      }
    });
  }

  // ─── Entry CRUD ───────────────────────────────────

  openAddEntry(date?: string): void {
    this.editingEntryId = null;
    this.entryForm.reset({
      dateProject: { entryDate: date || '', projectId: '' },
      hoursDetail: { hours: 8, category: 'Development' },
      description: ''
    });
    this.showEntryForm = true;
  }

  openEditEntry(entry: TimesheetEntry): void {
    this.editingEntryId = entry.id;
    this.entryForm.patchValue({
      dateProject: { entryDate: entry.entryDate, projectId: entry.projectId },
      hoursDetail: { hours: entry.hours, category: entry.category },
      description: entry.description || ''
    });
    this.showEntryForm = true;
  }

  closeEntryForm(): void {
    this.showEntryForm = false;
    this.editingEntryId = null;
  }

  submitEntry(): void {
    if (this.entryForm.invalid) {
      this.entryForm.markAllAsTouched();
      return;
    }

    this.entryFormLoading = true;
    const val = this.entryForm.value;

    let categoryId = 1;
    if (val.hoursDetail.category === 'Overtime') categoryId = 2;
    if (val.hoursDetail.category === 'OnCall') categoryId = 3;

    const dto = {
      entryDate: val.dateProject.entryDate,
      projectId: val.dateProject.projectId,
      hours: val.hoursDetail.hours,
      category: categoryId,
      description: val.description || undefined
    };

    const request$ = this.editingEntryId
      ? this.timesheetService.updateEntry(this.editingEntryId, dto)
      : this.timesheetService.createEntry(dto);

    request$.subscribe({
      next: () => {
        this.entryFormLoading = false;
        this.closeEntryForm();
        this.loadWeek();
      },
      error: (err) => {
        this.entryFormLoading = false;
        this.alertService.error(err.error?.message || 'Failed to save entry.');
      }
    });
  }

  async deleteEntry(entry: TimesheetEntry): Promise<void> {
    const confirmed = await this.alertService.confirm('Delete this timesheet entry?', 'Delete Entry');
    if (!confirmed) return;
    this.timesheetService.deleteEntry(entry.id).subscribe({
      next: () => {
        this.alertService.success('Entry deleted.');
        this.loadWeek();
      },
      error: (err) => this.alertService.error(err.error?.message || 'Failed to delete.')
    });
  }

  // ─── Submit Week ──────────────────────────────────

  async submitWeek(): Promise<void> {
    if (!this.weekTimesheet) return;
    const confirmed = await this.alertService.confirm('Submit this week\'s timesheet? You won\'t be able to edit entries after submission.', 'Submit Week');
    if (!confirmed) return;

    this.timesheetService.submitWeek({ weekStartDate: this.currentWeekStart }).subscribe({
      next: () => {
        this.alertService.success('Timesheet submitted.');
        this.loadWeek();
      },
      error: (err) => this.alertService.error(err.error?.message || 'Failed to submit.')
    });
  }

  // ─── Helpers ──────────────────────────────────────

  getEntriesForDate(date: string): TimesheetEntry[] {
    if (!this.weekTimesheet) return [];
    return this.weekTimesheet.entries.filter(e => e.entryDate === date);
  }

  getDailyTotal(date: string): number {
    return this.getEntriesForDate(date).reduce((sum, e) => sum + e.hours, 0);
  }

  getDayName(date: string): string {
    return new Date(date).toLocaleDateString('en-US', { weekday: 'short' });
  }

  getFormattedDate(date: string): string {
    return new Date(date).toLocaleDateString('en-US', { day: 'numeric', month: 'short' });
  }

  isDraft(): boolean {
    return !this.weekTimesheet?.status || this.weekTimesheet.status === 'Draft' || this.weekTimesheet.status === 'Rejected';
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Approved': return 'badge-success';
      case 'Rejected': return 'badge-danger';
      case 'Submitted': return 'badge-warning';
      default: return 'badge-gray';
    }
  }

  isSubGroupInvalid(group: string, field: string): boolean {
    const control = this.entryForm.get(group)?.get(field);
    return !!(control && control.invalid && control.touched);
  }

  isFieldInvalid(field: string): boolean {
    const control = this.entryForm.get(field);
    return !!(control && control.invalid && control.touched);
  }

  getProjectName(id: string): string {
    return this.projects.find(p => p.id === id)?.name || id;
  }

  private getMonday(d: Date): string {
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);
    return this.formatDate(new Date(d.setDate(diff)));
  }

  private getWeekDays(startDate: string): string[] {
    const days: string[] = [];
    const d = new Date(startDate);
    for (let i = 0; i < 7; i++) {
      days.push(this.formatDate(new Date(d)));
      d.setDate(d.getDate() + 1);
    }
    return days;
  }

  private formatDate(d: Date): string {
    return d.toISOString().split('T')[0];
  }
}