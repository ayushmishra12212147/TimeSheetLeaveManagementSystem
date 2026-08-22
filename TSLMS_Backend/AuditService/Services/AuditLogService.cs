using System.Text.Json;
using AuditService.Data;
using AuditService.DTOs;
using AuditService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AuditDbContext _dbContext;

        public AuditLogService(AuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task RecordEventAsync(string eventKey, string payloadJson, CancellationToken cancellationToken = default)
        {
            using var document = JsonDocument.Parse(payloadJson);
            var root = document.RootElement;
            var auditLog = BuildAuditLog(eventKey, payloadJson, root);

            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<AuditLogPageDto> GetAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default)
        {
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 50 : Math.Min(filter.PageSize, 200);

            IQueryable<AuditLog> query = _dbContext.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.ServiceName))
            {
                query = query.Where(x => x.ServiceName == filter.ServiceName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filter.EventKey))
            {
                query = query.Where(x => x.EventKey == filter.EventKey.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                query = query.Where(x => x.Action.Contains(filter.Action.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
            {
                query = query.Where(x => x.EntityType == filter.EntityType.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityId))
            {
                query = query.Where(x => x.EntityId == filter.EntityId.Trim());
            }

            if (filter.ActorUserId.HasValue)
            {
                query = query.Where(x => x.ActorUserId == filter.ActorUserId.Value);
            }

            if (filter.SubjectUserId.HasValue)
            {
                query = query.Where(x => x.SubjectUserId == filter.SubjectUserId.Value);
            }

            if (filter.DateFromUtc.HasValue)
            {
                query = query.Where(x => x.OccurredAtUtc >= filter.DateFromUtc.Value);
            }

            if (filter.DateToUtc.HasValue)
            {
                query = query.Where(x => x.OccurredAtUtc <= filter.DateToUtc.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.OccurredAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AuditLogResponseDto
                {
                    Id = x.Id,
                    OccurredAtUtc = x.OccurredAtUtc,
                    ServiceName = x.ServiceName,
                    EventKey = x.EventKey,
                    Action = x.Action,
                    EntityType = x.EntityType,
                    EntityId = x.EntityId,
                    ActorUserId = x.ActorUserId,
                    ActorEmployeeId = x.ActorEmployeeId,
                    ActorName = x.ActorName,
                    SubjectUserId = x.SubjectUserId,
                    SubjectEmployeeId = x.SubjectEmployeeId,
                    SubjectName = x.SubjectName,
                    Outcome = x.Outcome,
                    Description = x.Description,
                    MetadataJson = x.MetadataJson
                })
                .ToListAsync(cancellationToken);

            return new AuditLogPageDto
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddYears(-3);
            await _dbContext.AuditLogs
                .Where(x => x.OccurredAtUtc < cutoff)
                .ExecuteDeleteAsync(cancellationToken);
        }

        private static AuditLog BuildAuditLog(string eventKey, string payloadJson, JsonElement root)
        {
            return eventKey switch
            {
                "user.created" => CreateLog("EmployeeService", eventKey, "UserCreated", "User", GetGuidString(root, "UserId"), null, null, "HRAdmin", GetGuid(root, "UserId"), GetString(root, "EmployeeId"), GetString(root, "FullName"), $"{GetString(root, "FullName")} ({GetString(root, "EmployeeId")}) account created.", payloadJson),
                "user.manager-assignment.changed" => CreateLog("EmployeeService", eventKey, "ManagerAssignmentChanged", "User", GetGuidString(root, "EmployeeUserId"), GetGuid(root, "CurrentManagerUserId"), null, GetString(root, "CurrentManagerName"), GetGuid(root, "EmployeeUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "Action")} manager assignment for {GetString(root, "EmployeeName")} ({GetString(root, "EmployeeId")}).", payloadJson),
                "password.reset.requested" => CreateLog("AuthService", eventKey, "PasswordResetRequested", "User", GetGuidString(root, "UserId"), GetGuid(root, "UserId"), GetString(root, "EmployeeId"), GetString(root, "FullName"), GetGuid(root, "UserId"), GetString(root, "EmployeeId"), GetString(root, "FullName"), $"Password reset requested for {GetString(root, "FullName")} ({GetString(root, "EmployeeId")}).", payloadJson),
                "leave.submitted" => CreateLog("LeaveService", eventKey, "LeaveSubmitted", "LeaveRequest", GetGuidString(root, "LeaveRequestId"), GetGuid(root, "EmployeeUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), GetGuid(root, "EmployeeUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} submitted {GetString(root, "LeaveTypeName")} leave.", payloadJson),
                "leave.approved" => CreateLog("LeaveService", eventKey, "LeaveApproved", "LeaveRequest", GetGuidString(root, "LeaveRequestId"), null, null, GetString(root, "ApprovedByName"), GetGuid(root, "RecipientUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} leave approved by {GetString(root, "ApprovedByName")}.", payloadJson),
                "leave.rejected" => CreateLog("LeaveService", eventKey, "LeaveRejected", "LeaveRequest", GetGuidString(root, "LeaveRequestId"), null, null, GetString(root, "RejectedByName"), GetGuid(root, "RecipientUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} leave rejected by {GetString(root, "RejectedByName")}.", payloadJson),
                "leave.cancelled" => CreateLog("LeaveService", eventKey, "LeaveCancelled", "LeaveRequest", GetGuidString(root, "LeaveRequestId"), GetGuid(root, "RecipientUserId"), GetString(root, "EmployeeId"), GetString(root, "CancelledByName"), GetGuid(root, "RecipientUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} leave cancelled by {GetString(root, "CancelledByName")}.", payloadJson),
                "timesheet.submitted" => CreateLog("TimesheetService", eventKey, "TimesheetSubmitted", "TimesheetSummary", GetGuidString(root, "TimesheetSummaryId"), GetGuid(root, "EmployeeUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), GetGuid(root, "EmployeeUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} submitted timesheet for week {GetString(root, "WeekStartDate")}.", payloadJson),
                "timesheet.approved" => CreateLog("TimesheetService", eventKey, "TimesheetApproved", "TimesheetSummary", GetGuidString(root, "TimesheetSummaryId"), null, null, GetString(root, "ApprovedByName"), GetGuid(root, "RecipientUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} timesheet approved by {GetString(root, "ApprovedByName")}.", payloadJson),
                "timesheet.rejected" => CreateLog("TimesheetService", eventKey, "TimesheetRejected", "TimesheetSummary", GetGuidString(root, "TimesheetSummaryId"), null, null, GetString(root, "RejectedByName"), GetGuid(root, "RecipientUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} timesheet rejected by {GetString(root, "RejectedByName")}.", payloadJson),
                "report.requested" => CreateLog("ReportService", eventKey, "ReportRequested", "ReportRequest", GetGuidString(root, "ReportRequestId"), null, GetString(root, "RequestedByEmployeeId"), GetString(root, "RequestedByName"), null, GetString(root, "RequestedByEmployeeId"), GetString(root, "RequestedByName"), $"{GetString(root, "RequestedByName")} requested a {GetString(root, "ReportType")} report.", payloadJson),
                "report.approved" => CreateLog("ReportService", eventKey, "ReportApproved", "ReportRequest", GetGuidString(root, "ReportRequestId"), null, null, GetString(root, "ApprovedByName"), GetGuid(root, "RecipientUserId"), null, GetString(root, "RecipientName"), $"{GetString(root, "ReportType")} report approved by {GetString(root, "ApprovedByName")}.", payloadJson),
                "report.rejected" => CreateLog("ReportService", eventKey, "ReportRejected", "ReportRequest", GetGuidString(root, "ReportRequestId"), null, null, GetString(root, "RejectedByName"), GetGuid(root, "RecipientUserId"), null, GetString(root, "RecipientName"), $"{GetString(root, "ReportType")} report rejected by {GetString(root, "RejectedByName")}.", payloadJson),
                "attendance.clockin" => CreateLog("EmployeeService", eventKey, "AttendanceClockIn", "AttendanceRecord", GetGuidString(root, "AttendanceRecordId"), GetGuid(root, "ScannedByManagerId"), null, GetString(root, "ScannedByManagerName"), GetGuid(root, "EmployeeUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} clocked in, verified by {GetString(root, "ScannedByManagerName")}.", payloadJson),
                "attendance.clockout" => CreateLog("EmployeeService", eventKey, "AttendanceClockOut", "AttendanceRecord", GetGuidString(root, "AttendanceRecordId"), GetGuid(root, "ScannedByManagerId"), null, GetString(root, "ScannedByManagerName"), GetGuid(root, "EmployeeUserId"), GetString(root, "EmployeeId"), GetString(root, "EmployeeName"), $"{GetString(root, "EmployeeName")} clocked out, verified by {GetString(root, "ScannedByManagerName")}.", payloadJson),
                _ => CreateLog("Unknown", eventKey, "UnhandledEvent", "Unknown", string.Empty, null, null, null, null, null, null, $"Unhandled audit event received: {eventKey}", payloadJson)
            };
        }

        private static AuditLog CreateLog(string serviceName, string eventKey, string action, string entityType, string entityId, Guid? actorUserId, string? actorEmployeeId, string? actorName, Guid? subjectUserId, string? subjectEmployeeId, string? subjectName, string description, string payloadJson)
        {
            return new AuditLog
            {
                Id = Guid.NewGuid(),
                OccurredAtUtc = DateTime.UtcNow,
                ServiceName = serviceName,
                EventKey = eventKey,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                ActorUserId = actorUserId,
                ActorEmployeeId = actorEmployeeId,
                ActorName = actorName,
                SubjectUserId = subjectUserId,
                SubjectEmployeeId = subjectEmployeeId,
                SubjectName = subjectName,
                Outcome = "Success",
                Description = description,
                MetadataJson = payloadJson,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        private static string GetString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
                ? property.ToString()
                : string.Empty;
        }

        private static Guid? GetGuid(JsonElement root, string propertyName)
        {
            var value = GetString(root, propertyName);
            return Guid.TryParse(value, out var parsed) ? parsed : null;
        }

        private static string GetGuidString(JsonElement root, string propertyName)
        {
            return GetGuid(root, propertyName)?.ToString() ?? string.Empty;
        }
    }
}
