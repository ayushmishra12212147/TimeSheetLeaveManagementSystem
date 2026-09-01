import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NotificationTemplate } from '../../../models/notification';
import { AlertService } from '../../../services/alert.service';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../../../constants/api-endpoints';
import { ApiResponse } from '../../../models/common';

@Component({
  selector: 'app-notification-templates',
  imports: [NgFor, NgIf, DatePipe, ReactiveFormsModule],
  templateUrl: './notification-templates.html',
  styleUrl: './notification-templates.css',
})
export class NotificationTemplates implements OnInit {
  templates: NotificationTemplate[] = [];
  loading = true;
  errorMessage = '';
  showForm = false;
  editingId: string | null = null;
  editingTemplate: NotificationTemplate | null = null;
  templateForm!: FormGroup;
  formLoading = false;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.templateForm = this.fb.group({
      eventKey: ['', [Validators.required]],
      name: ['', [Validators.required]],
      subjectTemplate: ['', [Validators.required]],
      bodyTemplate: ['', [Validators.required]],
      description: [''],
      isActive: [true],
    });
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.loading = true;
    this.http
      .get<ApiResponse<NotificationTemplate[]>>(API_ENDPOINTS.notifications.templates)
      .subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) {
            this.templates = (res.data || []).map(template => ({
              ...template,
              name: template.name || template.eventKey,
              isActive: template.isActive ?? true,
            }));
          } else {
            this.errorMessage = res.message;
          }
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = err.error?.message || 'Failed to load templates.';
        },
      });
  }

  openEdit(t: NotificationTemplate): void {
    this.editingId = t.id;
    this.editingTemplate = t;
    this.templateForm.patchValue({
      eventKey: t.eventKey,
      name: t.name,
      subjectTemplate: t.subjectTemplate,
      bodyTemplate: t.bodyTemplate,
      description: t.description || '',
      isActive: t.isActive,
    });
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editingId = null;
    this.editingTemplate = null;
    this.templateForm.reset({ isActive: true });
  }

  submit(): void {
    if (this.templateForm.invalid) {
      this.templateForm.markAllAsTouched();
      return;
    }
    if (!this.editingId) return;

    this.formLoading = true;
    const formValue = this.templateForm.getRawValue();
    this.http
      .put<ApiResponse<NotificationTemplate>>(
        `${API_ENDPOINTS.notifications.templates}/${this.editingId}`,
        {
          name: formValue.name.trim(),
          subjectTemplate: formValue.subjectTemplate.trim(),
          bodyTemplate: formValue.bodyTemplate.trim(),
          description: formValue.description?.trim() || null,
          isActive: !!formValue.isActive,
        }
      )
      .subscribe({
        next: (res) => {
          this.formLoading = false;
          if (res.success) {
            this.closeForm();
            this.loadTemplates();
            this.alertService.success('Template updated.');
          } else {
            this.alertService.error(res.message || 'Failed to update.');
          }
        },
        error: (err) => {
          this.formLoading = false;
          this.alertService.error(err.error?.message || 'Failed to update.');
        },
      });
  }
}
