import { environment } from '../environment';

const BASE = environment.apiBaseUrl;

export const API_ENDPOINTS = {
  auth: {
    login:                `${BASE}/auth/login`,
    refresh:              `${BASE}/auth/refresh`,
    logout:               `${BASE}/auth/logout`,
    forgotPassword:       `${BASE}/auth/forgot-password`,
    resetPassword:        `${BASE}/auth/reset-password`,
    firstLoginReset:      `${BASE}/auth/first-login/reset-password`,
    me:                   `${BASE}/auth/me`,
  },

  users: {
    base:                 `${BASE}/users`,
    byId:        (id: string) => `${BASE}/users/${id}`,
    assignManager:        `${BASE}/users/assign-manager`,
  },

  departments: {
    base:                 `${BASE}/departments`,
    byId:        (id: string) => `${BASE}/departments/${id}`,
  },

  attendance: {
    generateQr:           `${BASE}/attendance/generate-qr`,
    scanIn:               `${BASE}/attendance/scan-in`,
    scanOut:              `${BASE}/attendance/scan-out`,
    my:                   `${BASE}/attendance/my`,
    team:                 `${BASE}/attendance/team`,
    history:     (id: string) => `${BASE}/attendance/${id}/history`,
    records:              `${BASE}/attendance/records`,
  },

  leaves: {
    base:                 `${BASE}/leaves`,
    byId:        (id: string) => `${BASE}/leaves/${id}`,
    pending:              `${BASE}/leaves/pending`,
    teamCalendar:         `${BASE}/leaves/team-calendar`,
    approve:     (id: string) => `${BASE}/leaves/${id}/approve`,
    reject:      (id: string) => `${BASE}/leaves/${id}/reject`,
    withdraw:    (id: string) => `${BASE}/leaves/${id}/withdraw`,
    cancel:      (id: string) => `${BASE}/leaves/${id}/cancel`,
  },

  leaveTypes: {
    base:                 `${BASE}/leave-types`,
    byId:        (id: string) => `${BASE}/leave-types/${id}`,
    toggle:      (id: string) => `${BASE}/leave-types/${id}/toggle`,
  },

  leaveBalances: {
    my:                   `${BASE}/leave-balances/my`,
    byEmployee:  (empId: string) => `${BASE}/leave-balances/${empId}`,
    adjust:      (id: string) => `${BASE}/leave-balances/${id}/adjust`,
    carryForward:         `${BASE}/leave-balances/carry-forward`,
  },

  timesheets: {
    week:                 `${BASE}/timesheets/week`,
    base:                 `${BASE}/timesheets`,
    byId:        (id: string) => `${BASE}/timesheets/${id}`,
    submit:               `${BASE}/timesheets/submit`,
    pending:              `${BASE}/timesheets/pending`,
    team:                 `${BASE}/timesheets/team`,
    approve:     (id: string) => `${BASE}/timesheets/${id}/approve`,
    reject:      (id: string) => `${BASE}/timesheets/${id}/reject`,
  },

  timesheetConfig: {
    base:                 `${BASE}/timesheet-config`,
  },

  projects: {
    base:                 `${BASE}/projects`,
    byId:        (id: string) => `${BASE}/projects/${id}`,
    toggle:      (id: string) => `${BASE}/projects/${id}/toggle`,
  },

  holidays: {
    base:                 `${BASE}/holidays/`,
    byId:        (id: string) => `${BASE}/holidays/${id}`,
    check:                `${BASE}/holidays/check`,
    copyYear:             `${BASE}/holidays/copy-year`,
  },

  notifications: {
    base:                 `${BASE}/notifications/`,
    markRead:    (id: string) => `${BASE}/notifications/${id}/read`,
    markAllRead:          `${BASE}/notifications/read-all`,
    unreadCount:          `${BASE}/notifications/unread-count`,
    preferences:          `${BASE}/notifications/preferences`,
    templates:            `${BASE}/notifications/templates`,
  },

  audit: {
    base:                 `${BASE}/audit/`,
  },

  reports: {
    base:                 `${BASE}/reports`,
    requests:             `${BASE}/reports/requests`,
    byId:        (id: string) => `${BASE}/reports/${id}`,
    approve:     (id: string) => `${BASE}/reports/requests/${id}/approve`,
    reject:      (id: string) => `${BASE}/reports/requests/${id}/reject`,
    export:      (id: string, format: string) => `${BASE}/reports/requests/${id}/export?format=${format}`,
    dashboard:            `${BASE}/reports/dashboard`,
  },
} as const;
