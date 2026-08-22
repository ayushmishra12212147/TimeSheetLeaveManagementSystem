import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';

interface DashboardCard {
  title: string;
  value: string;
  icon: string;
  color: string;
  route: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [NgFor, NgClass, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {

  userName = '';
  userRole = '';
  employeeId = '';
  greeting = '';
  cards: DashboardCard[] = [];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.userRole = this.authService.getUserRole() || 'Employee';
    this.employeeId = this.authService.getEmployeeId() || '';
    this.greeting = this.getGreeting();
    this.buildCards();

    this.authService.getMe().subscribe({
      next: (res) => {
        if (res.success) {
          this.userName = res.data.fullName;
        }
      },
      error: () => {
        this.userName = this.employeeId;
      }
    });
  }

  private getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    return 'Good Evening';
  }

  private buildCards(): void {
    // Cards visible to all roles
    this.cards = [
      {
        title: 'My Attendance',
        value: 'View',
        icon: '',
        color: 'bg-blue-50 border-blue-200 text-blue-700',
        route: '/dashboard/attendance'
      },
      {
        title: 'My Leaves',
        value: 'View',
        icon: '',
        color: 'bg-green-50 border-green-200 text-green-700',
        route: '/dashboard/leaves'
      },
      {
        title: 'My Timesheet',
        value: 'View',
        icon: '',
        color: 'bg-purple-50 border-purple-200 text-purple-700',
        route: '/dashboard/timesheets'
      },
      {
        title: 'Notifications',
        value: 'View',
        icon: '',
        color: 'bg-yellow-50 border-yellow-200 text-yellow-700',
        route: '/dashboard/notifications'
      },
    ];

    // Manager/HRAdmin cards
    if (this.userRole === 'Manager' || this.userRole === 'HRAdmin') {
      this.cards.push(
        {
          title: 'Pending Leaves',
          value: 'Review',
          icon: '',
          color: 'bg-orange-50 border-orange-200 text-orange-700',
          route: '/dashboard/leaves/pending'
        },
        {
          title: 'Pending Timesheets',
          value: 'Review',
          icon: '',
          color: 'bg-teal-50 border-teal-200 text-teal-700',
          route: '/dashboard/timesheets/pending'
        }
      );
    }

    // HRAdmin cards
    if (this.userRole === 'HRAdmin') {
      this.cards.push(
        {
          title: 'Manage Users',
          value: 'Manage',
          icon: '',
          color: 'bg-indigo-50 border-indigo-200 text-indigo-700',
          route: '/dashboard/users'
        },
        {
          title: 'Reports',
          value: 'Generate',
          icon: '',
          color: 'bg-pink-50 border-pink-200 text-pink-700',
          route: '/dashboard/reports'
        }
      );
    }
  }
}
