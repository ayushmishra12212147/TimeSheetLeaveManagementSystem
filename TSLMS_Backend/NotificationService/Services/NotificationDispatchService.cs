using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Enums;
using NotificationService.Events;
using NotificationService.Exceptions;
using NotificationService.Models;
using NotificationService.Options;

namespace NotificationService.Services
{
    public class NotificationDispatchService : INotificationDispatchService
    {
        private readonly NotificationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly ISignalRNotifier _signalRNotifier;
        private readonly FrontendUrlOptions _frontendUrls;

        public NotificationDispatchService(
            NotificationDbContext dbContext,
            IEmailService emailService,
            ISignalRNotifier signalRNotifier,
            IOptions<FrontendUrlOptions> frontendUrls)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _signalRNotifier = signalRNotifier;
            _frontendUrls = frontendUrls.Value;
        }

        public Task HandleUserCreatedAsync(UserCreatedEvent message, CancellationToken cancellationToken)
        {
            var tokens = new Dictionary<string, string?>
            {
                ["FullName"] = message.FullName,
                ["EmployeeId"] = message.EmployeeId,
                ["Email"] = message.Email,
                ["TempPassword"] = message.TempPassword,
                ["LoginUrl"] = _frontendUrls.WelcomeLoginUrl
            };

            return DispatchAsync(
                recipientUserId: message.UserId,
                recipientEmail: message.Email,
                recipientName: message.FullName,
                eventKey: "user.created",
                type: NotificationType.Welcome,
                tokens: tokens,
                actionUrl: _frontendUrls.WelcomeLoginUrl,
                entityType: "User",
                entityId: message.UserId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandlePasswordResetRequestedAsync(PasswordResetRequestedEvent message, CancellationToken cancellationToken)
        {
            var resetUrl = BuildResetUrl(message.ResetToken);
            var tokens = new Dictionary<string, string?>
            {
                ["FullName"] = message.FullName,
                ["EmployeeId"] = message.EmployeeId,
                ["Email"] = message.Email,
                ["ResetToken"] = message.ResetToken,
                ["ResetUrl"] = resetUrl,
                ["ExpiresAtUtc"] = message.ExpiresAtUtc.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return DispatchAsync(
                recipientUserId: message.UserId,
                recipientEmail: message.Email,
                recipientName: message.FullName,
                eventKey: "password.reset.requested",
                type: NotificationType.PasswordReset,
                tokens: tokens,
                actionUrl: resetUrl,
                entityType: "User",
                entityId: message.UserId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleLeaveSubmittedAsync(LeaveSubmittedEvent message, CancellationToken cancellationToken)
        {
            var leaveUrl = BuildLeaveUrl(message.LeaveRequestId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["LeaveTypeName"] = message.LeaveTypeName,
                ["StartDate"] = message.StartDate.ToString("yyyy-MM-dd"),
                ["EndDate"] = message.EndDate.ToString("yyyy-MM-dd"),
                ["RequestedDays"] = message.RequestedDays.ToString("0.##"),
                ["Reason"] = message.Reason,
                ["ApprovalRole"] = message.ApprovalRole,
                ["LeaveUrl"] = leaveUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "leave.submitted",
                type: NotificationType.LeaveSubmitted,
                tokens: tokens,
                actionUrl: leaveUrl,
                entityType: "LeaveRequest",
                entityId: message.LeaveRequestId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleLeaveApprovedAsync(LeaveApprovedEvent message, CancellationToken cancellationToken)
        {
            var leaveUrl = BuildLeaveUrl(message.LeaveRequestId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["LeaveTypeName"] = message.LeaveTypeName,
                ["StartDate"] = message.StartDate.ToString("yyyy-MM-dd"),
                ["EndDate"] = message.EndDate.ToString("yyyy-MM-dd"),
                ["RequestedDays"] = message.RequestedDays.ToString("0.##"),
                ["ApprovedByName"] = message.ApprovedByName,
                ["Comment"] = message.Comment,
                ["LeaveUrl"] = leaveUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "leave.approved",
                type: NotificationType.LeaveApproved,
                tokens: tokens,
                actionUrl: leaveUrl,
                entityType: "LeaveRequest",
                entityId: message.LeaveRequestId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleLeaveRejectedAsync(LeaveRejectedEvent message, CancellationToken cancellationToken)
        {
            var leaveUrl = BuildLeaveUrl(message.LeaveRequestId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["LeaveTypeName"] = message.LeaveTypeName,
                ["StartDate"] = message.StartDate.ToString("yyyy-MM-dd"),
                ["EndDate"] = message.EndDate.ToString("yyyy-MM-dd"),
                ["RequestedDays"] = message.RequestedDays.ToString("0.##"),
                ["RejectedByName"] = message.RejectedByName,
                ["Reason"] = message.Reason,
                ["LeaveUrl"] = leaveUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "leave.rejected",
                type: NotificationType.LeaveRejected,
                tokens: tokens,
                actionUrl: leaveUrl,
                entityType: "LeaveRequest",
                entityId: message.LeaveRequestId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleLeaveCancelledAsync(LeaveCancelledEvent message, CancellationToken cancellationToken)
        {
            var leaveUrl = BuildLeaveUrl(message.LeaveRequestId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["LeaveTypeName"] = message.LeaveTypeName,
                ["StartDate"] = message.StartDate.ToString("yyyy-MM-dd"),
                ["EndDate"] = message.EndDate.ToString("yyyy-MM-dd"),
                ["RequestedDays"] = message.RequestedDays.ToString("0.##"),
                ["CancelledByName"] = message.CancelledByName,
                ["Reason"] = message.Reason,
                ["LeaveUrl"] = leaveUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "leave.cancelled",
                type: NotificationType.LeaveCancelled,
                tokens: tokens,
                actionUrl: leaveUrl,
                entityType: "LeaveRequest",
                entityId: message.LeaveRequestId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleTimesheetSubmittedAsync(TimesheetSubmittedEvent message, CancellationToken cancellationToken)
        {
            var timesheetUrl = BuildTimesheetUrl(message.TimesheetSummaryId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["WeekStartDate"] = message.WeekStartDate.ToString("yyyy-MM-dd"),
                ["WeekEndDate"] = message.WeekEndDate.ToString("yyyy-MM-dd"),
                ["TotalHours"] = message.TotalHours.ToString("0.##"),
                ["IsLateSubmission"] = message.IsLateSubmission ? "Yes" : "No",
                ["TimesheetUrl"] = timesheetUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "timesheet.submitted",
                type: NotificationType.TimesheetSubmitted,
                tokens: tokens,
                actionUrl: timesheetUrl,
                entityType: "TimesheetSummary",
                entityId: message.TimesheetSummaryId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleTimesheetApprovedAsync(TimesheetApprovedEvent message, CancellationToken cancellationToken)
        {
            var timesheetUrl = BuildTimesheetUrl(message.TimesheetSummaryId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["WeekStartDate"] = message.WeekStartDate.ToString("yyyy-MM-dd"),
                ["WeekEndDate"] = message.WeekEndDate.ToString("yyyy-MM-dd"),
                ["TotalHours"] = message.TotalHours.ToString("0.##"),
                ["ApprovedByName"] = message.ApprovedByName,
                ["Comment"] = message.Comment,
                ["TimesheetUrl"] = timesheetUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "timesheet.approved",
                type: NotificationType.TimesheetApproved,
                tokens: tokens,
                actionUrl: timesheetUrl,
                entityType: "TimesheetSummary",
                entityId: message.TimesheetSummaryId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleTimesheetRejectedAsync(TimesheetRejectedEvent message, CancellationToken cancellationToken)
        {
            var timesheetUrl = BuildTimesheetUrl(message.TimesheetSummaryId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["WeekStartDate"] = message.WeekStartDate.ToString("yyyy-MM-dd"),
                ["WeekEndDate"] = message.WeekEndDate.ToString("yyyy-MM-dd"),
                ["TotalHours"] = message.TotalHours.ToString("0.##"),
                ["RejectedByName"] = message.RejectedByName,
                ["Reason"] = message.Reason,
                ["TimesheetUrl"] = timesheetUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "timesheet.rejected",
                type: NotificationType.TimesheetRejected,
                tokens: tokens,
                actionUrl: timesheetUrl,
                entityType: "TimesheetSummary",
                entityId: message.TimesheetSummaryId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleAttendanceClockInAsync(AttendanceClockInEvent message, CancellationToken cancellationToken)
        {
            var attendanceUrl = BuildAttendanceUrl(message.AttendanceRecordId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["AttendanceDate"] = message.AttendanceDate.ToString("yyyy-MM-dd"),
                ["ClockInAtUtc"] = message.ClockInAtUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                ["ScannedByManagerName"] = message.ScannedByManagerName,
                ["AttendanceUrl"] = attendanceUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "attendance.clockin",
                type: NotificationType.AttendanceClockIn,
                tokens: tokens,
                actionUrl: attendanceUrl,
                entityType: "AttendanceRecord",
                entityId: message.AttendanceRecordId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleAttendanceClockOutAsync(AttendanceClockOutEvent message, CancellationToken cancellationToken)
        {
            var attendanceUrl = BuildAttendanceUrl(message.AttendanceRecordId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["EmployeeName"] = message.EmployeeName,
                ["EmployeeId"] = message.EmployeeId,
                ["AttendanceDate"] = message.AttendanceDate.ToString("yyyy-MM-dd"),
                ["ClockOutAtUtc"] = message.ClockOutAtUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                ["DurationMinutes"] = message.DurationMinutes.ToString(),
                ["ScannedByManagerName"] = message.ScannedByManagerName,
                ["AttendanceUrl"] = attendanceUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "attendance.clockout",
                type: NotificationType.AttendanceClockOut,
                tokens: tokens,
                actionUrl: attendanceUrl,
                entityType: "AttendanceRecord",
                entityId: message.AttendanceRecordId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleReportRequestedAsync(ReportRequestedEvent message, CancellationToken cancellationToken)
        {
            var reportUrl = BuildReportUrl(message.ReportRequestId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["RequestedByName"] = message.RequestedByName,
                ["RequestedByEmployeeId"] = message.RequestedByEmployeeId,
                ["ReportType"] = message.ReportType,
                ["ScopeLabel"] = message.ScopeLabel,
                ["DateFrom"] = message.DateFrom.ToString("yyyy-MM-dd"),
                ["DateTo"] = message.DateTo.ToString("yyyy-MM-dd"),
                ["ReportUrl"] = reportUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "report.requested",
                type: NotificationType.ReportRequested,
                tokens: tokens,
                actionUrl: reportUrl,
                entityType: "ReportRequest",
                entityId: message.ReportRequestId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleReportApprovedAsync(ReportApprovedEvent message, CancellationToken cancellationToken)
        {
            var reportUrl = BuildReportUrl(message.ReportRequestId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["ReportType"] = message.ReportType,
                ["ApprovedByName"] = message.ApprovedByName,
                ["ScopeLabel"] = message.ScopeLabel,
                ["DateFrom"] = message.DateFrom.ToString("yyyy-MM-dd"),
                ["DateTo"] = message.DateTo.ToString("yyyy-MM-dd"),
                ["ReportUrl"] = reportUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "report.approved",
                type: NotificationType.ReportApproved,
                tokens: tokens,
                actionUrl: reportUrl,
                entityType: "ReportRequest",
                entityId: message.ReportRequestId.ToString(),
                cancellationToken: cancellationToken);
        }

        public Task HandleReportRejectedAsync(ReportRejectedEvent message, CancellationToken cancellationToken)
        {
            var reportUrl = BuildReportUrl(message.ReportRequestId);
            var tokens = new Dictionary<string, string?>
            {
                ["RecipientName"] = message.RecipientName,
                ["ReportType"] = message.ReportType,
                ["RejectedByName"] = message.RejectedByName,
                ["ScopeLabel"] = message.ScopeLabel,
                ["DateFrom"] = message.DateFrom.ToString("yyyy-MM-dd"),
                ["DateTo"] = message.DateTo.ToString("yyyy-MM-dd"),
                ["Reason"] = message.Reason,
                ["ReportUrl"] = reportUrl
            };

            return DispatchAsync(
                recipientUserId: message.RecipientUserId,
                recipientEmail: message.RecipientEmail,
                recipientName: message.RecipientName,
                eventKey: "report.rejected",
                type: NotificationType.ReportRejected,
                tokens: tokens,
                actionUrl: reportUrl,
                entityType: "ReportRequest",
                entityId: message.ReportRequestId.ToString(),
                cancellationToken: cancellationToken);
        }

        private async Task DispatchAsync(
            Guid recipientUserId,
            string recipientEmail,
            string recipientName,
            string eventKey,
            NotificationType type,
            IDictionary<string, string?> tokens,
            string? actionUrl,
            string? entityType,
            string? entityId,
            CancellationToken cancellationToken)
        {
            var emailTemplate = await GetTemplateAsync(eventKey, NotificationChannel.Email, cancellationToken);
            var inAppTemplate = await GetTemplateAsync(eventKey, NotificationChannel.InApp, cancellationToken);
            var preference = await GetOrCreatePreferenceAsync(recipientUserId, cancellationToken);

            var shouldSendEmail = emailTemplate.IsCritical || preference.EmailNotificationsEnabled;
            var shouldCreateInApp = inAppTemplate.IsCritical || preference.InAppNotificationsEnabled;

            if (shouldCreateInApp)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = recipientUserId,
                    Type = type,
                    Title = Render(inAppTemplate.SubjectTemplate, tokens),
                    Message = Render(inAppTemplate.BodyTemplate, tokens),
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow,
                    ActionUrl = actionUrl,
                    EntityType = entityType,
                    EntityId = entityId,
                    IsImportant = inAppTemplate.IsCritical
                };

                _dbContext.Notifications.Add(notification);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _signalRNotifier.PushToUserAsync(recipientUserId, new RealtimeNotificationDto
                {
                    Id = notification.Id,
                    Type = notification.Type.ToString(),
                    Title = notification.Title,
                    Message = notification.Message,
                    IsImportant = notification.IsImportant,
                    CreatedAtUtc = notification.CreatedAtUtc,
                    ActionUrl = notification.ActionUrl
                }, cancellationToken);
            }

            if (shouldSendEmail && !string.IsNullOrWhiteSpace(recipientEmail))
            {
                var subject = Render(emailTemplate.SubjectTemplate, tokens);
                var body = Render(emailTemplate.BodyTemplate, tokens);
                await _emailService.SendAsync(recipientEmail, recipientName, subject, body, cancellationToken);
            }
        }

        private async Task<NotificationTemplate> GetTemplateAsync(string eventKey, NotificationChannel channel, CancellationToken cancellationToken)
        {
            var template = await _dbContext.NotificationTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EventKey == eventKey && x.Channel == channel && x.IsActive, cancellationToken);

            if (template == null)
            {
                throw new ApiException(StatusCodes.Status500InternalServerError, $"Active notification template missing for {eventKey} ({channel}).");
            }

            return template;
        }

        private async Task<NotificationPreference> GetOrCreatePreferenceAsync(Guid userId, CancellationToken cancellationToken)
        {
            var preference = await _dbContext.NotificationPreferences
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (preference != null)
            {
                return preference;
            }

            preference = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EmailNotificationsEnabled = false,
                InAppNotificationsEnabled = true,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.NotificationPreferences.Add(preference);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return preference;
        }

        private string BuildResetUrl(string resetToken)
        {
            if (string.IsNullOrWhiteSpace(_frontendUrls.PasswordResetUrlTemplate))
            {
                return resetToken;
            }

            return _frontendUrls.PasswordResetUrlTemplate.Replace("{token}", Uri.EscapeDataString(resetToken), StringComparison.Ordinal);
        }

        private static string BuildLeaveUrl(Guid leaveRequestId)
        {
            return $"/leaves/{leaveRequestId}";
        }

        private static string BuildTimesheetUrl(Guid timesheetSummaryId)
        {
            return $"/timesheets/{timesheetSummaryId}";
        }

        private static string BuildAttendanceUrl(Guid attendanceRecordId)
        {
            return $"/attendance/{attendanceRecordId}";
        }

        private static string BuildReportUrl(Guid reportRequestId)
        {
            return $"/reports/requests/{reportRequestId}";
        }

        private static string Render(string template, IEnumerable<KeyValuePair<string, string?>> tokens)
        {
            var rendered = template;

            foreach (var token in tokens)
            {
                rendered = rendered.Replace($"{{{token.Key}}}", token.Value ?? string.Empty, StringComparison.Ordinal);
            }

            return rendered;
        }
    }
}
