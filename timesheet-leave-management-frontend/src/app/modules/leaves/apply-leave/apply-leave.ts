import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LeaveService } from '../../../services/leave';
import { LeaveTypeService } from '../../../services/leave-type';
import { AuthService } from '../../../services/auth';
import { LeaveType } from '../../../models/leave';

@Component({
  selector: 'app-apply-leave',
  imports: [ReactiveFormsModule, FormsModule, NgFor, NgIf, NgClass],
  templateUrl: './apply-leave.html',
  styleUrl: './apply-leave.css'
})
export class ApplyLeave implements OnInit {

  leaveForm!: FormGroup;
  leaveTypes: LeaveType[] = [];
  loading = false;
  loadingTypes = true;
  errorMessage = '';
  successMessage = '';

  userGender: string = 'Unspecified';
  allLeaveTypes: LeaveType[] = [];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private leaveService: LeaveService,
    private leaveTypeService: LeaveTypeService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.userGender = this.authService.getCurrentUserValue()?.gender || 'Unspecified';
    this.initForm();
    this.loadLeaveTypes();
  }

  // ─── Form Initialization (Reactive Forms) ────────

  private initForm(): void {
    this.leaveForm = this.fb.group({
      leaveTypeId: ['', [Validators.required]],
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]],
      isHalfDay: [false],
      halfDaySession: [0],
      reason: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(500)]],
      supportingDocumentUrl: ['']
    }, {
      validators: [this.dateRangeValidator]
    });

    // When isHalfDay changes, update halfDaySession validation
    this.leaveForm.get('isHalfDay')?.valueChanges.subscribe(isHalfDay => {
      const sessionControl = this.leaveForm.get('halfDaySession');
      if (isHalfDay) {
        sessionControl?.setValidators([Validators.required, this.halfDaySessionValidator]);
        // Set endDate to startDate for half-day
        const startDate = this.leaveForm.get('startDate')?.value;
        if (startDate) {
          this.leaveForm.get('endDate')?.setValue(startDate);
        }
      } else {
        sessionControl?.clearValidators();
        sessionControl?.setValue(0);
      }
      sessionControl?.updateValueAndValidity();
    });
  }

  // ─── Custom Validators ────────────────────────────

  dateRangeValidator(group: FormGroup): { [key: string]: boolean } | null {
    const start = group.get('startDate')?.value;
    const end = group.get('endDate')?.value;
    if (start && end && new Date(start) > new Date(end)) {
      return { dateRangeInvalid: true };
    }
    return null;
  }

  halfDaySessionValidator(control: { value: string }): { [key: string]: boolean } | null {
    if (control.value === 'None') {
      return { invalidSession: true };
    }
    return null;
  }

  // ─── Data Loading ─────────────────────────────────

  private loadLeaveTypes(): void {
    this.leaveTypeService.getLeaveTypes().subscribe({
      next: (res) => {
        this.loadingTypes = false;
        if (res.success) {
          this.allLeaveTypes = (res.data || []).filter(t => t.isActive);
          this.filterLeaveTypes();
        }
      },
      error: () => {
        this.loadingTypes = false;
        this.errorMessage = 'Failed to load leave types.';
      }
    });
  }


  private filterLeaveTypes(): void {
    this.leaveTypes = this.allLeaveTypes.filter(t => {
      const nameLower = t.name.toLowerCase();
      
      // Maternity leave is for females only
      if (nameLower.includes('maternity')) {
        return this.userGender === 'Female';
      }
      
      // Paternity leave is for males only
      if (nameLower.includes('paternity')) {
        return this.userGender === 'Male';
      }
      
      return true;
    });

    // Reset selection if the currently selected leave type is no longer available
    const currentSelected = this.leaveForm.get('leaveTypeId')?.value;
    if (currentSelected && !this.leaveTypes.find(t => t.id === currentSelected)) {
      this.leaveForm.get('leaveTypeId')?.setValue('');
    }
  }

  // ─── Form Submission ──────────────────────────────

  submit(): void {
    if (this.leaveForm.invalid) {
      this.leaveForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.leaveForm.value;

    this.leaveService.createLeave({
      leaveTypeId: formValue.leaveTypeId,
      startDate: formValue.startDate,
      endDate: formValue.endDate,
      isHalfDay: formValue.isHalfDay,
      halfDaySession: Number(formValue.halfDaySession),
      reason: formValue.reason,
      supportingDocumentUrl: formValue.supportingDocumentUrl || undefined
    }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.successMessage = 'Leave request submitted successfully!';
          setTimeout(() => this.router.navigate(['/dashboard/leaves']), 1500);
        } else {
          this.errorMessage = res.message || 'Failed to submit leave request.';
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to submit leave request.';
      }
    });
  }

  // ─── Helpers ──────────────────────────────────────

  isFieldInvalid(field: string): boolean {
    const control = this.leaveForm.get(field);
    return !!(control && control.invalid && control.touched);
  }

  getSelectedLeaveType(): LeaveType | undefined {
    const id = this.leaveForm.get('leaveTypeId')?.value;
    return this.leaveTypes.find(t => t.id === id);
  }

  goBack(): void {
    this.router.navigate(['/dashboard/leaves']);
  }
}
