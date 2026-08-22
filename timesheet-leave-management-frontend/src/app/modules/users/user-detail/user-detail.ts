import { Component, OnInit } from '@angular/core';
import { NgIf, NgClass, NgFor, DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../services/user';
import { DepartmentService } from '../../../services/department';
import { LeaveBalanceService } from '../../../services/leave-balance';
import { User } from '../../../models/user';
import { LeaveBalance } from '../../../models/leave';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-user-detail',
  imports: [NgIf, NgClass, NgFor, DatePipe],
  templateUrl: './user-detail.html',
  styleUrl: './user-detail.css',
})
export class UserDetail implements OnInit {
  user: User | null = null;
  balances: LeaveBalance[] = [];
  loading = true;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private leaveBalanceService: LeaveBalanceService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadUser(id);
    } else {
      this.router.navigate(['/dashboard/users']);
    }
  }

  loadUser(id: string): void {
    this.loading = true;
    this.userService.getUserById(id).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.user = res.data;
          this.loadBalances(res.data.employeeId);
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load user.';
      },
    });
  }

  loadBalances(employeeId: string): void {
    this.leaveBalanceService.getBalancesByEmployee(employeeId).subscribe({
      next: (res) => {
        if (res.success) this.balances = res.data || [];
      },
    });
  }

  getRoleBadge(role: string): string {
    switch (role) {
      case 'HRAdmin': return 'badge-danger';
      case 'Manager': return 'badge-warning';
      default: return 'badge-info';
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard/users']);
  }
}
