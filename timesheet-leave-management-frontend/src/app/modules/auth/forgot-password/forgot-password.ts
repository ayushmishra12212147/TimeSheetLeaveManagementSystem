import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-forgot-password',
  imports: [FormsModule, NgIf, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPassword {

  employeeId = '';
  loading = false;
  submitted = false;
  errorMessage = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  submit(): void {
    if (!this.employeeId) {
      this.errorMessage = 'Please enter your employee ID.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.forgotPassword({ employeeId: this.employeeId.trim() }).subscribe({
      next: () => {
        this.loading = false;
        this.submitted = true;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Something went wrong. Please try again.';
      }
    });
  }
}
