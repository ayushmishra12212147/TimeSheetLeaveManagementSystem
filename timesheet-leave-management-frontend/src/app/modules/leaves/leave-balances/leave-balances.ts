import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass } from '@angular/common';
import { LeaveBalanceService } from '../../../services/leave-balance';
import { LeaveBalance } from '../../../models/leave';

@Component({
  selector: 'app-leave-balances',
  imports: [NgFor, NgIf, NgClass],
  templateUrl: './leave-balances.html',
  styleUrl: './leave-balances.css'
})
export class LeaveBalances implements OnInit {

  balances: LeaveBalance[] = [];
  loading = true;
  errorMessage = '';

  constructor(private leaveBalanceService: LeaveBalanceService) {}

  ngOnInit(): void {
    this.loadBalances();
  }

  loadBalances(): void {
    this.loading = true;
    this.leaveBalanceService.getMyBalances().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.balances = res.data || [];
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load balances.';
      }
    });
  }

  getUsagePercent(balance: LeaveBalance): number {
    const total = balance.allocatedDays + balance.carriedForwardDays + balance.manualAdjustmentDays;
    if (total <= 0) return 0;
    return Math.round((balance.usedDays / total) * 100);
  }

  getProgressColor(balance: LeaveBalance): string {
    const percent = this.getUsagePercent(balance);
    if (percent >= 90) return 'bg-red-500';
    if (percent >= 70) return 'bg-yellow-500';
    return 'bg-green-500';
  }
}
