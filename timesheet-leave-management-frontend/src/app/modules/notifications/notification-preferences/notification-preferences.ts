import { Component, OnInit } from '@angular/core';
import { NgIf } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { NotificationService } from '../../../services/notification';
import { NotificationPreference } from '../../../models/notification';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-notification-preferences',
  imports: [NgIf, ReactiveFormsModule],
  templateUrl: './notification-preferences.html',
  styleUrl: './notification-preferences.css'
})
export class NotificationPreferences implements OnInit {
  preference: NotificationPreference | null = null;
  prefForm!: FormGroup;
  loading = true;
  saving = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private notifService: NotificationService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.prefForm = this.fb.group({
      inAppEnabled: [true],
      emailEnabled: [true],
    });
    this.loadPreferences();
  }

  loadPreferences(): void {
    this.loading = true;
    this.notifService.getPreferences().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.preference = res.data;
          this.prefForm.patchValue({
            inAppEnabled: res.data.inAppEnabled,
            emailEnabled: res.data.emailEnabled,
          });
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load preferences.';
      }
    });
  }

  save(): void {
    this.saving = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.notifService.updatePreferences(this.prefForm.value).subscribe({
      next: (res) => {
        this.saving = false;
        if (res.success) {
          this.successMessage = 'Preferences saved successfully.';
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err.error?.message || 'Failed to save preferences.';
      }
    });
  }
}
