import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LeaveTypeService } from '../../../services/leave-type';
import { LeaveType, CreateLeaveTypeDto } from '../../../models/leave';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-leave-types',
  imports: [NgFor, NgIf, NgClass, ReactiveFormsModule],
  templateUrl: './leave-types.html',
  styleUrl: './leave-types.css'
})
export class LeaveTypes implements OnInit {

  leaveTypes: LeaveType[] = [];
  loading = true;
  errorMessage = '';
  showForm = false;
  editingId: string | null = null;
  typeForm!: FormGroup;
  formLoading = false;

  constructor(
    private fb: FormBuilder,
    private leaveTypeService: LeaveTypeService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadLeaveTypes();
  }

  private initForm(): void {
    this.typeForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      code: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(10)]],
      description: [''],
      defaultAnnualQuota: [12, [Validators.required, Validators.min(0)]],
      maxCarryForwardDays: [0, [Validators.required, Validators.min(0)]],
      requiresDocument: [false],
      isAutoApprove: [false],
      isActive: [true]
    });
  }

  loadLeaveTypes(): void {
    this.loading = true;
    this.leaveTypeService.getLeaveTypes().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) this.leaveTypes = res.data || [];
        else this.errorMessage = res.message;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load leave types.';
      }
    });
  }

  openCreateForm(): void {
    this.editingId = null;
    this.typeForm.reset({ defaultAnnualQuota: 12, maxCarryForwardDays: 0, isActive: true });
    this.showForm = true;
  }

  openEditForm(type: LeaveType): void {
    this.editingId = type.id;
    this.typeForm.patchValue({
      name: type.name,
      code: type.code,
      description: type.description || '',
      defaultAnnualQuota: type.defaultAnnualQuota,
      maxCarryForwardDays: type.maxCarryForwardDays,
      requiresDocument: type.requiresDocument,
      isAutoApprove: type.isAutoApprove,
      isActive: type.isActive
    });
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editingId = null;
  }

  submitForm(): void {
    if (this.typeForm.invalid) {
      this.typeForm.markAllAsTouched();
      return;
    }

    this.formLoading = true;
    const rawValue = this.typeForm.value;
    const dto: CreateLeaveTypeDto = {
      name: rawValue.name.trim(),
      code: rawValue.code.trim().toUpperCase(),
      description: rawValue.description?.trim() || null,
      defaultAnnualQuota: Number(rawValue.defaultAnnualQuota) || 0,
      maxCarryForwardDays: Number(rawValue.maxCarryForwardDays) || 0,
      requiresDocument: !!rawValue.requiresDocument,
      isAutoApprove: !!rawValue.isAutoApprove,
      isActive: !!rawValue.isActive
    };

    const request$ = this.editingId
      ? this.leaveTypeService.updateLeaveType(this.editingId, dto)
      : this.leaveTypeService.createLeaveType(dto);

    request$.subscribe({
      next: () => {
        this.formLoading = false;
        this.closeForm();
        this.loadLeaveTypes();
      },
      error: (err) => {
        this.formLoading = false;
        this.alertService.error(err.error?.message || 'Operation failed.');
      }
    });
  }



  isFieldInvalid(field: string): boolean {
    const control = this.typeForm.get(field);
    return !!(control && control.invalid && control.touched);
  }
}
