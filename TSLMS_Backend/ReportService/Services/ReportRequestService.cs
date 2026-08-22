using Microsoft.EntityFrameworkCore;
using ReportService.Clients;
using ReportService.Data;
using ReportService.DTOs;
using ReportService.Enums;
using ReportService.Events;
using ReportService.Exceptions;
using ReportService.Messaging;
using ReportService.Models;

namespace ReportService.Services
{
    public class ReportRequestService : IReportRequestService
    {
        private readonly ReportDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmployeeDirectoryClient _employeeDirectoryClient;
        private readonly IReportScopeResolver _reportScopeResolver;
        private readonly IAttendanceReportService _attendanceReportService;
        private readonly ILeaveReportService _leaveReportService;
        private readonly ITimesheetReportService _timesheetReportService;
        private readonly IExportService _exportService;
        private readonly IRabbitMQPublisher _publisher;

        public ReportRequestService(
            ReportDbContext dbContext,
            ICurrentUserService currentUserService,
            IEmployeeDirectoryClient employeeDirectoryClient,
            IReportScopeResolver reportScopeResolver,
            IAttendanceReportService attendanceReportService,
            ILeaveReportService leaveReportService,
            ITimesheetReportService timesheetReportService,
            IExportService exportService,
            IRabbitMQPublisher publisher)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _employeeDirectoryClient = employeeDirectoryClient;
            _reportScopeResolver = reportScopeResolver;
            _attendanceReportService = attendanceReportService;
            _leaveReportService = leaveReportService;
            _timesheetReportService = timesheetReportService;
            _exportService = exportService;
            _publisher = publisher;
        }

        public async Task<ReportRequestResponseDto> CreateAsync(CreateReportRequestDto dto, CancellationToken cancellationToken = default)
        {
            EnsureManager();
            EnsureRequestableType(dto.ReportType);

            var (dateFrom, dateTo) = NormalizeRange(dto.DateFrom, dto.DateTo);
            await _reportScopeResolver.ResolveEmployeesAsync(dto.EmployeeId, cancellationToken);
            var requester = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);

            var entity = new ReportRequest
            {
                Id = Guid.NewGuid(),
                ReportType = dto.ReportType,
                Status = ReportRequestStatus.PendingHrApproval,
                RequestedByUserId = requester.Id,
                RequestedByEmployeeId = requester.EmployeeId,
                RequestedByName = requester.FullName,
                ScopeEmployeeId = string.IsNullOrWhiteSpace(dto.EmployeeId) ? null : dto.EmployeeId.Trim().ToUpperInvariant(),
                DateFrom = dateFrom,
                DateTo = dateTo,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.ReportRequests.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var scopeLabel = await _reportScopeResolver.DescribeScopeAsync(dto.EmployeeId, cancellationToken);
            var hrAdmins = await _employeeDirectoryClient.GetUsersAsync(role: "HRAdmin", cancellationToken: cancellationToken);
            foreach (var hrAdmin in hrAdmins)
            {
                _publisher.Publish(new ReportRequestedEvent
                {
                    RecipientUserId = hrAdmin.Id,
                    RecipientEmail = hrAdmin.Email,
                    RecipientName = hrAdmin.FullName,
                    ReportRequestId = entity.Id,
                    ReportType = entity.ReportType.ToString(),
                    RequestedByEmployeeId = requester.EmployeeId,
                    RequestedByName = requester.FullName,
                    ScopeLabel = scopeLabel,
                    DateFrom = entity.DateFrom,
                    DateTo = entity.DateTo
                }, "report.requested");
            }

            return Map(entity);
        }

        public async Task<IReadOnlyCollection<ReportRequestResponseDto>> GetVisibleAsync(bool pendingOnly, CancellationToken cancellationToken = default)
        {
            var role = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();

            IQueryable<ReportRequest> query = _dbContext.ReportRequests.AsNoTracking();
            if (pendingOnly)
            {
                query = query.Where(x => x.Status == ReportRequestStatus.PendingHrApproval);
            }

            if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.RequestedByUserId == currentUserId);
            }
            else if (!string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only managers and HRAdmin can access report requests.");
            }

            var requests = await query.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
            return requests.Select(Map).ToList();
        }

        public async Task<ReportRequestResponseDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();
            var request = await GetRequestAsync(id, cancellationToken);
            if (request.Status != ReportRequestStatus.PendingHrApproval)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Only pending report requests can be approved.");
            }

            var approver = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            var requester = await _employeeDirectoryClient.GetUserAsync(request.RequestedByUserId, cancellationToken);
            var scopeLabel = await _reportScopeResolver.DescribeScopeAsync(request.ScopeEmployeeId, cancellationToken);
            request.Status = ReportRequestStatus.Approved;
            request.ApprovedByUserId = approver.Id;
            request.ApprovedByName = approver.FullName;
            request.ApprovedAtUtc = DateTime.UtcNow;
            request.RejectedByUserId = null;
            request.RejectedByName = null;
            request.RejectionReason = null;
            request.RejectedAtUtc = null;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _publisher.Publish(new ReportApprovedEvent
            {
                RecipientUserId = request.RequestedByUserId,
                RecipientEmail = requester.Email,
                RecipientName = requester.FullName,
                ReportRequestId = request.Id,
                ReportType = request.ReportType.ToString(),
                ApprovedByName = approver.FullName,
                ScopeLabel = scopeLabel,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo
            }, "report.approved");

            return Map(request);
        }

        public async Task<ReportRequestResponseDto> RejectAsync(Guid id, RejectReportRequestDto dto, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();
            var request = await GetRequestAsync(id, cancellationToken);
            if (request.Status != ReportRequestStatus.PendingHrApproval)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Only pending report requests can be rejected.");
            }

            var approver = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            var requester = await _employeeDirectoryClient.GetUserAsync(request.RequestedByUserId, cancellationToken);
            var scopeLabel = await _reportScopeResolver.DescribeScopeAsync(request.ScopeEmployeeId, cancellationToken);
            request.Status = ReportRequestStatus.Rejected;
            request.RejectedByUserId = approver.Id;
            request.RejectedByName = approver.FullName;
            request.RejectionReason = dto.Reason.Trim();
            request.RejectedAtUtc = DateTime.UtcNow;
            request.ApprovedByUserId = null;
            request.ApprovedByName = null;
            request.ApprovedAtUtc = null;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _publisher.Publish(new ReportRejectedEvent
            {
                RecipientUserId = request.RequestedByUserId,
                RecipientEmail = requester.Email,
                RecipientName = requester.FullName,
                ReportRequestId = request.Id,
                ReportType = request.ReportType.ToString(),
                RejectedByName = approver.FullName,
                ScopeLabel = scopeLabel,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                Reason = dto.Reason.Trim()
            }, "report.rejected");

            return Map(request);
        }

        public async Task<ExportFileResult> ExportAsync(Guid id, string format, CancellationToken cancellationToken = default)
        {
            var request = await GetRequestAsync(id, cancellationToken);
            if (request.Status != ReportRequestStatus.Approved)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Only approved report requests can be exported.");
            }

            var role = _currentUserService.GetRole();
            if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) &&
                request.RequestedByUserId != _currentUserService.GetUserId())
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Managers can only export their own approved reports.");
            }

            if (!string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "You are not allowed to export reports.");
            }

            return request.ReportType switch
            {
                ReportType.Leave => await _exportService.ExportLeaveAsync(
                    await _leaveReportService.GenerateAsync(new LeaveReportRequestDto
                    {
                        EmployeeId = request.ScopeEmployeeId,
                        DateFrom = request.DateFrom,
                        DateTo = request.DateTo
                    }, cancellationToken),
                    format,
                    cancellationToken),
                ReportType.Timesheet => await _exportService.ExportTimesheetAsync(
                    await _timesheetReportService.GenerateAsync(new TimesheetReportRequestDto
                    {
                        EmployeeId = request.ScopeEmployeeId,
                        DateFrom = request.DateFrom,
                        DateTo = request.DateTo
                    }, cancellationToken),
                    format,
                    cancellationToken),
                ReportType.Attendance => await _exportService.ExportAttendanceAsync(
                    await _attendanceReportService.GenerateAsync(new AttendanceReportRequestDto
                    {
                        EmployeeId = request.ScopeEmployeeId,
                        DateFrom = request.DateFrom,
                        DateTo = request.DateTo
                    }, cancellationToken),
                    format,
                    cancellationToken),
                _ => throw new ApiException(StatusCodes.Status400BadRequest, "Unsupported report request type.")
            };
        }

        private async Task<ReportRequest> GetRequestAsync(Guid id, CancellationToken cancellationToken)
        {
            var request = await _dbContext.ReportRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (request == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Report request not found.");
            }

            return request;
        }

        private static ReportRequestResponseDto Map(ReportRequest request)
        {
            return new ReportRequestResponseDto
            {
                Id = request.Id,
                ReportType = request.ReportType.ToString(),
                Status = request.Status.ToString(),
                RequestedByEmployeeId = request.RequestedByEmployeeId,
                RequestedByName = request.RequestedByName,
                ScopeEmployeeId = request.ScopeEmployeeId,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                CreatedAtUtc = request.CreatedAtUtc,
                ApprovedByName = request.ApprovedByName,
                ApprovedAtUtc = request.ApprovedAtUtc,
                RejectedByName = request.RejectedByName,
                RejectionReason = request.RejectionReason,
                RejectedAtUtc = request.RejectedAtUtc
            };
        }

        private void EnsureManager()
        {
            if (!string.Equals(_currentUserService.GetRole(), "Manager", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only managers can prepare report requests.");
            }
        }

        private void EnsureHrAdmin()
        {
            if (!string.Equals(_currentUserService.GetRole(), "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only HRAdmin can approve or reject report requests.");
            }
        }

        private static void EnsureRequestableType(ReportType reportType)
        {
            if (reportType is ReportType.Dashboard)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Dashboard reports do not require approval workflow.");
            }
        }

        private static (DateOnly DateFrom, DateOnly DateTo) NormalizeRange(DateOnly? dateFrom, DateOnly? dateTo)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var from = dateFrom ?? new DateOnly(today.Year, today.Month, 1);
            var to = dateTo ?? today;
            return from <= to ? (from, to) : (to, from);
        }
    }
}
