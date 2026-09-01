import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { HolidayService } from '../../../services/holiday';
import { Holiday } from '../../../models/holiday';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-holiday-list',
  imports: [NgFor, NgIf, DatePipe, ReactiveFormsModule, FormsModule],
  templateUrl: './holiday-list.html',
  styleUrl: './holiday-list.css'
})
export class HolidayList implements OnInit {
  holidays: Holiday[] = []; loading = true; errorMessage = '';
  selectedYear = new Date().getFullYear();
  showForm = false; editingId: string | null = null; holidayForm!: FormGroup; formLoading = false;

  constructor(private fb: FormBuilder, private holidayService: HolidayService, private alertService: AlertService) {}

  ngOnInit(): void {
    this.holidayForm = this.fb.group({
      name: ['', [Validators.required]], holidayDate: ['', [Validators.required]], description: ['']
    });
    this.loadHolidays();
  }

  loadHolidays(): void {
    this.loading = true;
    this.holidayService.getHolidays(this.selectedYear).subscribe({
      next: (res) => { this.loading = false; if (res.success) this.holidays = res.data || []; else this.errorMessage = res.message; },
      error: (err) => { this.loading = false; this.errorMessage = err.error?.message || 'Failed.'; }
    });
  }

  onYearChange(): void { this.loadHolidays(); }
  openCreate(): void { this.editingId = null; this.holidayForm.reset(); this.showForm = true; }
  openEdit(h: Holiday): void { this.editingId = h.id; this.holidayForm.patchValue({ name: h.name, holidayDate: h.holidayDate, description: h.description || '' }); this.showForm = true; }
  closeForm(): void { this.showForm = false; this.editingId = null; }

  submit(): void {
    if (this.holidayForm.invalid) { this.holidayForm.markAllAsTouched(); return; }
    this.formLoading = true;
    const req$ = this.editingId ? this.holidayService.updateHoliday(this.editingId, this.holidayForm.value) : this.holidayService.createHoliday(this.holidayForm.value);
    req$.subscribe({ next: () => { this.formLoading = false; this.closeForm(); this.loadHolidays(); }, error: (err) => { this.formLoading = false; this.alertService.error(err.error?.message || 'Failed.'); } });
  }

  async deleteHoliday(h: Holiday): Promise<void> {
    const confirmed = await this.alertService.confirm(`Delete holiday "${h.name}"?`, 'Delete Holiday');
    if (!confirmed) return;
    this.holidayService.deleteHoliday(h.id).subscribe({ next: () => this.loadHolidays(), error: (err) => this.alertService.error(err.error?.message || 'Failed.') });
  }
}
