using NotificationService.Events;

namespace NotificationService.Services
{
    public interface INotificationDispatchService
    {
        Task HandleUserCreatedAsync(UserCreatedEvent message, CancellationToken cancellationToken);
        Task HandlePasswordResetRequestedAsync(PasswordResetRequestedEvent message, CancellationToken cancellationToken);
        Task HandleLeaveSubmittedAsync(LeaveSubmittedEvent message, CancellationToken cancellationToken);
        Task HandleLeaveApprovedAsync(LeaveApprovedEvent message, CancellationToken cancellationToken);
        Task HandleLeaveRejectedAsync(LeaveRejectedEvent message, CancellationToken cancellationToken);
        Task HandleLeaveCancelledAsync(LeaveCancelledEvent message, CancellationToken cancellationToken);
        Task HandleTimesheetSubmittedAsync(TimesheetSubmittedEvent message, CancellationToken cancellationToken);
        Task HandleTimesheetApprovedAsync(TimesheetApprovedEvent message, CancellationToken cancellationToken);
        Task HandleTimesheetRejectedAsync(TimesheetRejectedEvent message, CancellationToken cancellationToken);
        Task HandleAttendanceClockInAsync(AttendanceClockInEvent message, CancellationToken cancellationToken);
        Task HandleAttendanceClockOutAsync(AttendanceClockOutEvent message, CancellationToken cancellationToken);
        Task HandleReportRequestedAsync(ReportRequestedEvent message, CancellationToken cancellationToken);
        Task HandleReportApprovedAsync(ReportApprovedEvent message, CancellationToken cancellationToken);
        Task HandleReportRejectedAsync(ReportRejectedEvent message, CancellationToken cancellationToken);
    }
}
