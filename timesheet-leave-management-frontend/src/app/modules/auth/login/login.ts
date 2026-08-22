import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule, NgIf, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  employeeId = '';
  password = '';
  showPassword = false;
  loading = false;
  errorMessage = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  login(): void {
    if (!this.employeeId || !this.password) {
      this.errorMessage = 'Please enter Employee ID and Password.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.login({
      employeeId: this.employeeId,
      password: this.password
    }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          if (res.data.isFirstLogin || res.data.mustResetPassword) {
            this.router.navigate(['/first-login']);
          } else {
            this.router.navigate(['/dashboard']);
          }
        } else {
          this.errorMessage = res.message || 'Login failed.';
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Invalid credentials or server error.';
      }
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}