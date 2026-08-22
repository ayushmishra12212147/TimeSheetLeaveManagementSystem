using Microsoft.EntityFrameworkCore;
using NotificationService.Enums;
using NotificationService.Models;

namespace NotificationService.Data
{
    public static class NotificationDbSeeder
    {
        public static async Task SeedAsync(NotificationDbContext dbContext)
        {
            var now = DateTime.UtcNow;

            var defaults = new[]
            {
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc001"),
                    EventKey = "user.created",
                    Channel = NotificationChannel.Email,
                    Name = "Welcome Email",
                    SubjectTemplate = "Welcome to LTMS, {FullName}",
                    BodyTemplate = "<p>Hello {FullName},</p><p>Your LTMS account is ready.</p><p><strong>Employee ID:</strong> {EmployeeId}<br/><strong>Temporary Password:</strong> {TempPassword}</p><p>Login here: <a href=\"{LoginUrl}\">{LoginUrl}</a></p><p>Please reset your password on first login.</p>",
                    IsCritical = true,
                    Description = "Sent when HR creates a new employee account.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc002"),
                    EventKey = "user.created",
                    Channel = NotificationChannel.InApp,
                    Name = "Welcome In-App",
                    SubjectTemplate = "Welcome to LTMS",
                    BodyTemplate = "Your account is ready. Sign in with employee ID {EmployeeId} and reset your password at first login.",
                    IsCritical = true,
                    Description = "Stored as an in-app notification for the new employee.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc003"),
                    EventKey = "password.reset.requested",
                    Channel = NotificationChannel.Email,
                    Name = "Password Reset Email",
                    SubjectTemplate = "Reset your LTMS password",
                    BodyTemplate = "<p>Hello {FullName},</p><p>We received a password reset request for your LTMS account.</p><p>Reset link: <a href=\"{ResetUrl}\">{ResetUrl}</a></p><p>This link expires at {ExpiresAtUtc} UTC.</p>",
                    IsCritical = true,
                    Description = "Sent when a user requests a password reset.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc004"),
                    EventKey = "password.reset.requested",
                    Channel = NotificationChannel.InApp,
                    Name = "Password Reset In-App",
                    SubjectTemplate = "Password reset requested",
                    BodyTemplate = "A password reset was requested for your account. Use the reset link sent to your email before {ExpiresAtUtc} UTC.",
                    IsCritical = true,
                    Description = "Stored as an in-app notification when a password reset is requested.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc005"),
                    EventKey = "leave.submitted",
                    Channel = NotificationChannel.Email,
                    Name = "Leave Submitted Email",
                    SubjectTemplate = "New leave request from {EmployeeName}",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>{EmployeeName} ({EmployeeId}) submitted a {LeaveTypeName} request for {RequestedDays} day(s) from {StartDate} to {EndDate}.</p><p>Reason: {Reason}</p><p>Approval route: {ApprovalRole}</p><p>Open request: <a href=\"{LeaveUrl}\">{LeaveUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the approver when a leave request is submitted.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc006"),
                    EventKey = "leave.submitted",
                    Channel = NotificationChannel.InApp,
                    Name = "Leave Submitted In-App",
                    SubjectTemplate = "Leave request needs review",
                    BodyTemplate = "{EmployeeName} submitted {LeaveTypeName} leave from {StartDate} to {EndDate} for {RequestedDays} day(s).",
                    IsCritical = true,
                    Description = "Stored for managers or HR when a leave request is submitted.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc007"),
                    EventKey = "leave.approved",
                    Channel = NotificationChannel.Email,
                    Name = "Leave Approved Email",
                    SubjectTemplate = "Your leave request was approved",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>Your {LeaveTypeName} leave from {StartDate} to {EndDate} for {RequestedDays} day(s) was approved by {ApprovedByName}.</p><p>Comment: {Comment}</p><p>View request: <a href=\"{LeaveUrl}\">{LeaveUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the employee when leave is approved.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc008"),
                    EventKey = "leave.approved",
                    Channel = NotificationChannel.InApp,
                    Name = "Leave Approved In-App",
                    SubjectTemplate = "Leave approved",
                    BodyTemplate = "{LeaveTypeName} leave from {StartDate} to {EndDate} was approved by {ApprovedByName}.",
                    IsCritical = true,
                    Description = "Stored for employees when leave is approved.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc009"),
                    EventKey = "leave.rejected",
                    Channel = NotificationChannel.Email,
                    Name = "Leave Rejected Email",
                    SubjectTemplate = "Your leave request was rejected",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>Your {LeaveTypeName} leave from {StartDate} to {EndDate} for {RequestedDays} day(s) was rejected by {RejectedByName}.</p><p>Reason: {Reason}</p><p>View request: <a href=\"{LeaveUrl}\">{LeaveUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the employee when leave is rejected.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc010"),
                    EventKey = "leave.rejected",
                    Channel = NotificationChannel.InApp,
                    Name = "Leave Rejected In-App",
                    SubjectTemplate = "Leave rejected",
                    BodyTemplate = "{LeaveTypeName} leave from {StartDate} to {EndDate} was rejected by {RejectedByName}. Reason: {Reason}",
                    IsCritical = true,
                    Description = "Stored for employees when leave is rejected.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc011"),
                    EventKey = "leave.cancelled",
                    Channel = NotificationChannel.Email,
                    Name = "Leave Cancelled Email",
                    SubjectTemplate = "Leave request was cancelled",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>{EmployeeName} ({EmployeeId}) cancelled {LeaveTypeName} leave from {StartDate} to {EndDate} for {RequestedDays} day(s).</p><p>Reason: {Reason}</p><p>View request: <a href=\"{LeaveUrl}\">{LeaveUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the approver when an employee cancels or withdraws leave.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc012"),
                    EventKey = "leave.cancelled",
                    Channel = NotificationChannel.InApp,
                    Name = "Leave Cancelled In-App",
                    SubjectTemplate = "Leave cancelled",
                    BodyTemplate = "{EmployeeName} cancelled {LeaveTypeName} leave from {StartDate} to {EndDate}.",
                    IsCritical = true,
                    Description = "Stored for approvers when a leave request is cancelled or withdrawn.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc013"),
                    EventKey = "timesheet.submitted",
                    Channel = NotificationChannel.Email,
                    Name = "Timesheet Submitted Email",
                    SubjectTemplate = "New timesheet submitted by {EmployeeName}",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>{EmployeeName} ({EmployeeId}) submitted a timesheet for {WeekStartDate} to {WeekEndDate} totaling {TotalHours} hour(s).</p><p>Late submission: {IsLateSubmission}</p><p>Review it here: <a href=\"{TimesheetUrl}\">{TimesheetUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the approver when a timesheet is submitted.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc014"),
                    EventKey = "timesheet.submitted",
                    Channel = NotificationChannel.InApp,
                    Name = "Timesheet Submitted In-App",
                    SubjectTemplate = "Timesheet needs review",
                    BodyTemplate = "{EmployeeName} submitted a timesheet for {WeekStartDate} to {WeekEndDate} totaling {TotalHours} hour(s).",
                    IsCritical = true,
                    Description = "Stored for managers or HR when a timesheet is submitted.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc015"),
                    EventKey = "timesheet.approved",
                    Channel = NotificationChannel.Email,
                    Name = "Timesheet Approved Email",
                    SubjectTemplate = "Your timesheet was approved",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>Your timesheet for {WeekStartDate} to {WeekEndDate} totaling {TotalHours} hour(s) was approved by {ApprovedByName}.</p><p>Comment: {Comment}</p><p>View it here: <a href=\"{TimesheetUrl}\">{TimesheetUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the employee when a timesheet is approved.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc016"),
                    EventKey = "timesheet.approved",
                    Channel = NotificationChannel.InApp,
                    Name = "Timesheet Approved In-App",
                    SubjectTemplate = "Timesheet approved",
                    BodyTemplate = "Your timesheet for {WeekStartDate} to {WeekEndDate} was approved by {ApprovedByName}.",
                    IsCritical = true,
                    Description = "Stored for employees when a timesheet is approved.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc017"),
                    EventKey = "timesheet.rejected",
                    Channel = NotificationChannel.Email,
                    Name = "Timesheet Rejected Email",
                    SubjectTemplate = "Your timesheet was rejected",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>Your timesheet for {WeekStartDate} to {WeekEndDate} totaling {TotalHours} hour(s) was rejected by {RejectedByName}.</p><p>Reason: {Reason}</p><p>Update it here: <a href=\"{TimesheetUrl}\">{TimesheetUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the employee when a timesheet is rejected.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc018"),
                    EventKey = "timesheet.rejected",
                    Channel = NotificationChannel.InApp,
                    Name = "Timesheet Rejected In-App",
                    SubjectTemplate = "Timesheet rejected",
                    BodyTemplate = "Your timesheet for {WeekStartDate} to {WeekEndDate} was rejected by {RejectedByName}. Reason: {Reason}",
                    IsCritical = true,
                    Description = "Stored for employees when a timesheet is rejected.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc025"),
                    EventKey = "attendance.clockin",
                    Channel = NotificationChannel.Email,
                    Name = "Attendance Clock-In Email",
                    SubjectTemplate = "Attendance clock-in confirmed",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>Your attendance clock-in for {AttendanceDate} was verified by {ScannedByManagerName} at {ClockInAtUtc} UTC.</p><p>View details: <a href=\"{AttendanceUrl}\">{AttendanceUrl}</a></p>",
                    IsCritical = false,
                    Description = "Optional email sent when attendance clock-in is verified.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc026"),
                    EventKey = "attendance.clockin",
                    Channel = NotificationChannel.InApp,
                    Name = "Attendance Clock-In In-App",
                    SubjectTemplate = "Clock-in verified",
                    BodyTemplate = "Your clock-in for {AttendanceDate} was verified by {ScannedByManagerName} at {ClockInAtUtc} UTC.",
                    IsCritical = true,
                    Description = "Stored for employees when clock-in is verified.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc027"),
                    EventKey = "attendance.clockout",
                    Channel = NotificationChannel.Email,
                    Name = "Attendance Clock-Out Email",
                    SubjectTemplate = "Attendance clock-out confirmed",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>Your attendance clock-out for {AttendanceDate} was verified by {ScannedByManagerName} at {ClockOutAtUtc} UTC.</p><p>Total duration: {DurationMinutes} minute(s).</p><p>View details: <a href=\"{AttendanceUrl}\">{AttendanceUrl}</a></p>",
                    IsCritical = false,
                    Description = "Optional email sent when attendance clock-out is verified.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc028"),
                    EventKey = "attendance.clockout",
                    Channel = NotificationChannel.InApp,
                    Name = "Attendance Clock-Out In-App",
                    SubjectTemplate = "Clock-out verified",
                    BodyTemplate = "Your clock-out for {AttendanceDate} was verified by {ScannedByManagerName}. Total duration: {DurationMinutes} minute(s).",
                    IsCritical = true,
                    Description = "Stored for employees when clock-out is verified.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc019"),
                    EventKey = "report.requested",
                    Channel = NotificationChannel.Email,
                    Name = "Report Requested Email",
                    SubjectTemplate = "Report approval needed: {ReportType}",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>{RequestedByName} ({RequestedByEmployeeId}) submitted a {ReportType} report request for {ScopeLabel} covering {DateFrom} to {DateTo}.</p><p>Review it here: <a href=\"{ReportUrl}\">{ReportUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to HRAdmin when a manager submits a report request.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc020"),
                    EventKey = "report.requested",
                    Channel = NotificationChannel.InApp,
                    Name = "Report Requested In-App",
                    SubjectTemplate = "Report request needs approval",
                    BodyTemplate = "{RequestedByName} submitted a {ReportType} report request for {ScopeLabel} covering {DateFrom} to {DateTo}.",
                    IsCritical = true,
                    Description = "Stored for HRAdmin when a manager submits a report request.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc021"),
                    EventKey = "report.approved",
                    Channel = NotificationChannel.Email,
                    Name = "Report Approved Email",
                    SubjectTemplate = "Your report request was approved",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>Your {ReportType} report request for {ScopeLabel} covering {DateFrom} to {DateTo} was approved by {ApprovedByName}.</p><p>Export it here: <a href=\"{ReportUrl}\">{ReportUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the manager when HR approves a report request.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc022"),
                    EventKey = "report.approved",
                    Channel = NotificationChannel.InApp,
                    Name = "Report Approved In-App",
                    SubjectTemplate = "Report approved",
                    BodyTemplate = "Your {ReportType} report request for {ScopeLabel} covering {DateFrom} to {DateTo} was approved by {ApprovedByName}.",
                    IsCritical = true,
                    Description = "Stored for the manager when HR approves a report request.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc023"),
                    EventKey = "report.rejected",
                    Channel = NotificationChannel.Email,
                    Name = "Report Rejected Email",
                    SubjectTemplate = "Your report request was rejected",
                    BodyTemplate = "<p>Hello {RecipientName},</p><p>Your {ReportType} report request for {ScopeLabel} covering {DateFrom} to {DateTo} was rejected by {RejectedByName}.</p><p>Reason: {Reason}</p><p>View request: <a href=\"{ReportUrl}\">{ReportUrl}</a></p>",
                    IsCritical = true,
                    Description = "Sent to the manager when HR rejects a report request.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new NotificationTemplate
                {
                    Id = Guid.Parse("c6518d0a-2dfe-47f1-a886-b29ee93cc024"),
                    EventKey = "report.rejected",
                    Channel = NotificationChannel.InApp,
                    Name = "Report Rejected In-App",
                    SubjectTemplate = "Report rejected",
                    BodyTemplate = "Your {ReportType} report request for {ScopeLabel} covering {DateFrom} to {DateTo} was rejected by {RejectedByName}. Reason: {Reason}",
                    IsCritical = true,
                    Description = "Stored for the manager when HR rejects a report request.",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                }
            };

            var existingKeys = await dbContext.NotificationTemplates
                .Select(x => new { x.EventKey, x.Channel })
                .ToListAsync();

            var missingTemplates = defaults
                .Where(template => existingKeys.All(existing =>
                    existing.EventKey != template.EventKey || existing.Channel != template.Channel))
                .ToList();

            if (missingTemplates.Count == 0)
            {
                return;
            }

            dbContext.NotificationTemplates.AddRange(missingTemplates);
            await dbContext.SaveChangesAsync();
        }
    }
}
