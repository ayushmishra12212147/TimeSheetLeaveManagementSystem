import { Component, OnInit } from '@angular/core';
import { NgIf } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TimesheetConfigService } from '../../../services/timesheet-config';
import { TimesheetConfig as ConfigModel } from '../../../models/timesheet';

@Component({
  selector: 'app-timesheet-config',
  imports: [NgIf, ReactiveFormsModule],
  templateUrl: './timesheet-config.html',
  styleUrl: './timesheet-config.css'
})
export class TimesheetConfigComponent implements OnInit {

  config: ConfigModel | null = null;
  configForm!: FormGroup;
  loading = true;
  saving = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private configService: TimesheetConfigService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadConfig();
  }

  private initForm(): void {
    this.configForm = this.fb.group({
      minimumWeeklyHours: [40, [Validators.required, Validators.min(1), Validators.max(168)]],
      lowHoursWarningThreshold: [6, [Validators.required, Validators.min(1), Validators.max(24)]],
      highHoursWarningThreshold: [12, [Validators.required, Validators.min(1), Validators.max(24)]],
      autoApproveEnabled: [false],
      autoApproveAfterHours: [72, [Validators.required, Validators.min(0)]],
    });
  }

  loadConfig(): void {
    this.loading = true;
    this.configService.getConfig().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.config = res.data;
          this.configForm.patchValue(res.data);
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load config.';
      }
    });
  }

  submit(): void {
    if (this.configForm.invalid) {
      this.configForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.configService.updateConfig(this.configForm.value).subscribe({
      next: () => {
        this.saving = false;
        this.successMessage = 'Configuration updated successfully!';
        this.loadConfig();
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err.error?.message || 'Failed to update config.';
      }
    });
  }

  isFieldInvalid(field: string): boolean {
    const control = this.configForm.get(field);
    return !!(control && control.invalid && control.touched);
  }
}
