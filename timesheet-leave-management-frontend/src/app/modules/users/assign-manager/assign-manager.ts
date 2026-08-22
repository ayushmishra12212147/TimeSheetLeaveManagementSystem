import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../../../services/user';
import { User } from '../../../models/user';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-assign-manager',
  imports: [NgFor, NgIf, ReactiveFormsModule],
  templateUrl: './assign-manager.html',
  styleUrl: './assign-manager.css',
})
export class AssignManager implements OnInit {
  users: User[] = [];
  managers: User[] = [];
  loading = true;
  errorMessage = '';
  assignForm!: FormGroup;
  formLoading = false;
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.assignForm = this.fb.group({
      userId: ['', [Validators.required]],
      managerId: ['', [Validators.required]],
    });
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getUsers().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.users = (res.data || []).filter(u => u.isActive);
          this.managers = this.users.filter(u => u.role === 'Manager' || u.role === 'HRAdmin');
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load users.';
      },
    });
  }

  submit(): void {
    if (this.assignForm.invalid) {
      this.assignForm.markAllAsTouched();
      return;
    }

    this.formLoading = true;
    this.successMessage = '';

    const val = this.assignForm.value;
    this.userService
      .assignManager({
        userId: val.userId,
        managerId: val.managerId,
      })
      .subscribe({
        next: () => {
          this.formLoading = false;
          this.successMessage = 'Manager assigned successfully.';
          this.assignForm.reset();
          this.loadUsers();
        },
        error: (err) => {
          this.formLoading = false;
          this.alertService.error(err.error?.message || 'Failed to assign manager.');
        },
      });
  }

  getEmployees(): User[] {
    return this.users.filter(u => u.role === 'Employee');
  }
}
