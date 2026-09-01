import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { firstLoginGuard } from './core/guards/first-login-guard';

export const routes: Routes = [

  // ─── Public Auth Routes ───────────────────────────
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./modules/auth/login/login')
      .then(m => m.Login)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./modules/auth/forgot-password/forgot-password')
      .then(m => m.ForgotPassword)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./modules/auth/reset-password/reset-password')
      .then(m => m.ResetPassword)
  },
  {
    path: 'first-login',
    loadComponent: () => import('./modules/auth/first-login/first-login')
      .then(m => m.FirstLogin)
  },

  // ─── Protected Dashboard Routes ───────────────────
  {
    path: 'dashboard',
    loadComponent: () => import('./core/layout/app-shell/app-shell')
      .then(m => m.AppShell),
    canActivate: [authGuard, firstLoginGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./modules/dashboard/dashboard')
          .then(m => m.Dashboard)
      },

      // ─── Leave Module ─────────────────────────────
      {
        path: 'leaves',
        loadComponent: () => import('./modules/leaves/my-leaves/my-leaves')
          .then(m => m.MyLeaves)
      },
      {
        path: 'leaves/apply',
        loadComponent: () => import('./modules/leaves/apply-leave/apply-leave')
          .then(m => m.ApplyLeave)
      },
      {
        path: 'leaves/pending',
        loadComponent: () => import('./modules/leaves/leave-approvals/leave-approvals')
          .then(m => m.LeaveApprovals)
      },
      {
        path: 'leaves/team-calendar',
        loadComponent: () => import('./modules/leaves/team-calendar/team-calendar')
          .then(m => m.TeamCalendar)
      },
      {
        path: 'leave-balances',
        loadComponent: () => import('./modules/leaves/leave-balances/leave-balances')
          .then(m => m.LeaveBalances)
      },
      {
        path: 'leave-types',
        loadComponent: () => import('./modules/leaves/leave-types/leave-types')
          .then(m => m.LeaveTypes)
      },

      // ─── Attendance Module ────────────────────────
      {
        path: 'attendance',
        loadComponent: () => import('./modules/attendance/my-attendance/my-attendance')
          .then(m => m.MyAttendance)
      },
      {
        path: 'attendance/team',
        loadComponent: () => import('./modules/attendance/team-attendance/team-attendance')
          .then(m => m.TeamAttendance)
      },

      // ─── Timesheet Module ─────────────────────────
      {
        path: 'timesheets',
        loadComponent: () => import('./modules/timesheets/my-timesheet/my-timesheet')
          .then(m => m.MyTimesheet)
      },
      {
        path: 'timesheets/pending',
        loadComponent: () => import('./modules/timesheets/timesheet-approvals/timesheet-approvals')
          .then(m => m.TimesheetApprovals)
      },
      {
        path: 'timesheet-config',
        loadComponent: () => import('./modules/timesheets/timesheet-config/timesheet-config')
          .then(m => m.TimesheetConfigComponent)
      },

      // ─── Project Management ───────────────────────
      {
        path: 'projects',
        loadComponent: () => import('./modules/projects/project-list/project-list')
          .then(m => m.ProjectList)
      },

      // ─── User Management ──────────────────────────
      {
        path: 'users',
        loadComponent: () => import('./modules/users/user-list/user-list')
          .then(m => m.UserList)
      },
      {
        path: 'users/:id',
        loadComponent: () => import('./modules/users/user-detail/user-detail')
          .then(m => m.UserDetail)
      },
      {
        path: 'assign-manager',
        loadComponent: () => import('./modules/users/assign-manager/assign-manager')
          .then(m => m.AssignManager)
      },

      // ─── Department Management ────────────────────
      {
        path: 'departments',
        loadComponent: () => import('./modules/departments/department-list/department-list')
          .then(m => m.DepartmentList)
      },

      // ─── Holiday Management ───────────────────────
      {
        path: 'holidays',
        loadComponent: () => import('./modules/holidays/holiday-list/holiday-list')
          .then(m => m.HolidayList)
      },

      // ─── Notifications ────────────────────────────
      {
        path: 'notifications',
        loadComponent: () => import('./modules/notifications/notification-list/notification-list')
          .then(m => m.NotificationList)
      },
      {
        path: 'notification-preferences',
        loadComponent: () => import('./modules/notifications/notification-preferences/notification-preferences')
          .then(m => m.NotificationPreferences)
      },
      {
        path: 'notification-templates',
        loadComponent: () => import('./modules/notifications/notification-templates/notification-templates')
          .then(m => m.NotificationTemplates)
      },

      // ─── Reports ──────────────────────────────────
      {
        path: 'reports',
        loadComponent: () => import('./modules/reports/report-requests/report-requests')
          .then(m => m.ReportRequests)
      },

      // ─── Audit ────────────────────────────────────
      {
        path: 'audit',
        loadComponent: () => import('./modules/audit/audit-log/audit-log')
          .then(m => m.AuditLogComponent)
      },
    ]
  },

  // ─── Wildcard ─────────────────────────────────────
  {
    path: '**',
    redirectTo: 'login'
  }

];
