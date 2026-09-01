import { Role } from './roles';

export interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles: Role[];
  section: string;
}

export const NAV_ITEMS: NavItem[] = [
  // ─── Main ─────────────────────────────────────────
  {
    label: 'Dashboard',
    icon: '🏠',
    route: '/dashboard',
    roles: [Role.Employee, Role.Manager, Role.HRAdmin],
    section: 'Main'
  },
  {
    label: 'My Attendance',
    icon: '📍',
    route: '/dashboard/attendance',
    roles: [Role.Employee, Role.Manager],
    section: 'Main'
  },
  {
    label: 'My Leaves',
    icon: '🏖️',
    route: '/dashboard/leaves',
    roles: [Role.Employee, Role.Manager],
    section: 'Main'
  },
  {
    label: 'My Timesheet',
    icon: '⏱️',
    route: '/dashboard/timesheets',
    roles: [Role.Employee, Role.Manager],
    section: 'Main'
  },
  {
    label: 'Leave Balances',
    icon: '📊',
    route: '/dashboard/leave-balances',
    roles: [Role.Employee, Role.Manager],
    section: 'Main'
  },

  // ─── Management (Manager + HRAdmin) ───────────────
  {
    label: 'Team Leaves',
    icon: '✅',
    route: '/dashboard/leaves/pending',
    roles: [Role.Manager, Role.HRAdmin],
    section: 'Management'
  },
  {
    label: 'Team Timesheets',
    icon: '📋',
    route: '/dashboard/timesheets/pending',
    roles: [Role.Manager, Role.HRAdmin],
    section: 'Management'
  },
  {
    label: 'Team Attendance',
    icon: '👥',
    route: '/dashboard/attendance/team',
    roles: [Role.Manager, Role.HRAdmin],
    section: 'Management'
  },
  {
    label: 'Team Calendar',
    icon: '📅',
    route: '/dashboard/leaves/team-calendar',
    roles: [Role.Manager, Role.HRAdmin],
    section: 'Management'
  },
  {
    label: 'Reports',
    icon: '📈',
    route: '/dashboard/reports',
    roles: [Role.Manager, Role.HRAdmin],
    section: 'Management'
  },

  // ─── Admin (HRAdmin only) ──────────────────────────
  {
    label: 'Users',
    icon: '👤',
    route: '/dashboard/users',
    roles: [Role.HRAdmin],
    section: 'Admin'
  },
  {
    label: 'Assign Manager',
    icon: '🔗',
    route: '/dashboard/assign-manager',
    roles: [Role.HRAdmin],
    section: 'Admin'
  },
  {
    label: 'Departments',
    icon: '🏢',
    route: '/dashboard/departments',
    roles: [Role.HRAdmin],
    section: 'Admin'
  },
  {
    label: 'Leave Types',
    icon: '🗂️',
    route: '/dashboard/leave-types',
    roles: [Role.HRAdmin],
    section: 'Admin'
  },
  {
    label: 'Projects',
    icon: '📁',
    route: '/dashboard/projects',
    roles: [Role.HRAdmin],
    section: 'Admin'
  },
  {
    label: 'Holidays',
    icon: '🎉',
    route: '/dashboard/holidays',
    roles: [Role.HRAdmin],
    section: 'Admin'
  },
  {
    label: 'TS Config',
    icon: '⚙️',
    route: '/dashboard/timesheet-config',
    roles: [Role.HRAdmin],
    section: 'Admin'
  },

  // ─── System (HRAdmin only) ─────────────────────────
  {
    label: 'Notif. Templates',
    icon: '📧',
    route: '/dashboard/notification-templates',
    roles: [Role.HRAdmin],
    section: 'System'
  },
  {
    label: 'Audit Logs',
    icon: '🔒',
    route: '/dashboard/audit',
    roles: [Role.HRAdmin],
    section: 'System'
  },
];
