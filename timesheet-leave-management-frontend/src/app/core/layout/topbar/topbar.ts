import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from '../../../services/auth';
import { NotificationService } from '../../../services/notification';

@Component({
  selector: 'app-topbar',
  imports: [RouterLink, NgIf],
  templateUrl: './topbar.html',
  styleUrl: './topbar.css'
})
export class Topbar implements OnInit {

  @Input() sidebarCollapsed = false;
  @Output() toggleSidebar = new EventEmitter<void>();

  userName = '';
  userRole = '';
  employeeId = '';
  unreadCount = 0;

  constructor(
    private authService: AuthService,
    private notifService: NotificationService
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getUserRole() || '';
    this.employeeId = this.authService.getEmployeeId() || '';

    // Get full name from /me endpoint
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

    this.notifService.unreadCount$.subscribe(count => {
      this.unreadCount = count;
    });

    this.notifService.getUnreadCount().subscribe();
  }

  onToggleSidebar(): void {
    this.toggleSidebar.emit();
  }

  getRoleLabel(): string {
    switch (this.userRole) {
      case 'HRAdmin': return 'HR Admin';
      case 'Manager': return 'Manager';
      default: return 'Employee';
    }
  }

  getInitials(): string {
    if (!this.userName) return '?';
    return this.userName
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}