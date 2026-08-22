import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-first-login',
  imports: [FormsModule, NgIf],
  templateUrl: './first-login.html',
  styleUrl: './first-login.css'
})
export class FirstLogin {

  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  loading = false;
  errorMessage = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  submit(): void {
    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }
    if (this.newPassword.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.firstLoginReset({
      currentPassword: this.currentPassword,
      newPassword: this.newPassword,
      confirmPassword: this.confirmPassword
    }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.authService.clearSession();
          this.router.navigate(['/login']);
        } else {
          this.errorMessage = res.message || 'Failed to reset password.';
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to reset password.';
      }
    });
  }
}
