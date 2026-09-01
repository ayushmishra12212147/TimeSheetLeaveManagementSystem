using LeaveService.Clients;
using LeaveService.Data;
using LeaveService.DTOs;
using LeaveService.Enums;
using LeaveService.Events;
using LeaveService.Exceptions;
using LeaveService.Messaging;
using LeaveService.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private static readonly LeaveStatus[] ActiveStatuses =
        [
            LeaveStatus.PendingManagerApproval,
            LeaveStatus.PendingHrApproval,
            LeaveStatus.Approved
        ];

        private readonly LeaveDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmployeeDirectoryClient _employeeDirectoryClient;
        private readonly IHolidayCalendarClient _holidayCalendarClient;
        private readonly IRabbitMQPublisher _publisher;

        public LeaveRequestService(
            LeaveDbContext dbContext,
            ICurrentUserService currentUserService,
            IEmployeeDirectoryClient employeeDirectoryClient,
            IHolidayCalendarClient holidayCalendarClient,
            IRabbitMQPublisher publisher)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _employeeDirectoryClient = employeeDirectoryClient;
            _holidayCalendarClient = holidayCalendarClient;
            _publisher = publisher;
        }

        public async Task<IReadOnlyCollection<LeaveRequestResponseDto>> GetVisibleAsync(string? employeeId, CancellationToken cancellationToken = default)
        {
            var currentRole = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();
            var currentEmployeeId = _currentUserService.GetEmployeeId();
            employeeId = NormalizeOptionalText(employeeId);

            IQueryable<LeaveRequest> query = _dbContext.LeaveRequests.AsNoTracking();

            if (string.Equals(currentRole, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(employeeId))
                {
                    query = query.Where(x => x.EmployeeId == employeeId);
                }
            }
            else if (string.Equals(currentRole, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(employeeId) && !string.Equals(employeeId, currentEmployeeId, StringComparison.OrdinalIgnoreCase))
                {
                    var directReport = (await _employeeDirectoryClient.GetUsersAsync(
                        managerId: currentUserId,
                        employeeId: employeeId,
                        cancellationToken: cancellationToken)).FirstOrDefault();

                    if (directReport == null)
                    {
                        throw new ApiException(StatusCodes.Status403Forbidden, "You can only view leave requests for your direct reports.");
                    }

                    query = query.Where(x => x.EmployeeUserId == directReport.Id);
                }
                else
                {
                    query = query.Where(x => x.EmployeeUserId == currentUserId);
                }
            }
            else
            {
                query = query.Where(x => x.EmployeeUserId == currentUserId);
            }

            var requests = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            return requests.Select(MapLeaveRequest).ToList();
        }

        public async Task<LeaveRequestResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var request = await GetLeaveRequestAsync(id, cancellationToken);
            EnsureCanView(request);
            return MapLeaveRequest(request);
        }

        public async Task<IReadOnlyCollection<LeaveRequestResponseDto>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            var currentRole = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();

            IQueryable<LeaveRequest> query = _dbContext.LeaveRequests.AsNoTracking();

            if (string.Equals(currentRole, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status == LeaveStatus.PendingManagerApproval && x.ManagerUserId == currentUserId);
            }
            else if (string.Equals(currentRole, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status == LeaveStatus.PendingHrApproval);
            }
            else
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only managers and HRAdmin can view pending leave requests.");
            }

            var requests = await query.OrderBy(x => x.StartDate).ToListAsync(cancellationToken);
            return requests.Select(MapLeaveRequest).ToList();
        }

        public async Task<IReadOnlyCollection<LeaveRequestResponseDto>> GetTeamCalendarAsync(
            DateOnly? dateFrom,
            DateOnly? dateTo,
            CancellationToken cancellationToken = default)
        {
            var currentRole = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();
            var start = dateFrom ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var end = dateTo ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(3));

            IQueryable<LeaveRequest> query = _dbContext.LeaveRequests
                .AsNoTracking()
                .Where(x =>
                    x.StartDate <= end &&
                    x.EndDate >= start &&
                    x.Status != LeaveStatus.Rejected &&
                    x.Status != LeaveStatus.Withdrawn &&
                    x.Status != LeaveStatus.Cancelled);

            if (string.Equals(currentRole, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.ManagerUserId == currentUserId);
            }
            else if (!string.Equals(currentRole, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only managers and HRAdmin can view team calendars.");
            }

            var requests = await query.OrderBy(x => x.StartDate).ToListAsync(cancellationToken);
            return requests.Select(MapLeaveRequest).ToList();
        }

        public async Task<LeaveRequestResponseDto> CreateAsync(CreateLeaveRequestDto dto, CancellationToken cancellationToken = default)
        {
            var employee = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            var leaveType = await GetActiveLeaveTypeAsync(dto.LeaveTypeId, cancellationToken);

            ValidateDocumentRequirement(leaveType, dto.SupportingDocumentUrl);
            await EnsureNoOverlapAsync(employee.Id, dto.StartDate, dto.EndDate, null, cancellationToken);

            var requestedDays = await CalculateRequestedDaysAsync(dto.StartDate, dto.EndDate, dto.IsHalfDay, dto.HalfDaySession, cancellationToken);
            var approvalRole = DetermineApprovalRole(employee.ManagerId, requestedDays);
            var manager = employee.ManagerId.HasValue
                ? await _employeeDirectoryClient.GetUserAsync(employee.ManagerId.Value, cancellationToken)
                : null;

            var balance = await GetOrCreateBalanceAsync(employee, leaveType, dto.StartDate.Year, cancellationToken);
            balance.PendingDays += requestedDays;
            balance.UpdatedAtUtc = DateTime.UtcNow;

            var now = DateTime.UtcNow;
            var request = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                EmployeeUserId = employee.Id,
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.FullName,
                EmployeeEmail = employee.Email,
                ManagerUserId = employee.ManagerId,
                ManagerName = manager?.FullName,
                ManagerEmail = manager?.Email,
                LeaveTypeId = leaveType.Id,
                LeaveTypeName = leaveType.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                RequestedDays = requestedDays,
                IsHalfDay = dto.IsHalfDay,
                HalfDaySession = dto.IsHalfDay ? dto.HalfDaySession : HalfDaySession.None,
                IsUnpaid = GetAvailableDays(balance) < requestedDays,
                Reason = dto.Reason.Trim(),
                SupportingDocumentUrl = NormalizeOptionalText(dto.SupportingDocumentUrl),
                Status = leaveType.IsAutoApprove ? LeaveStatus.Approved : approvalRole == ApprovalRole.Manager ? LeaveStatus.PendingManagerApproval : LeaveStatus.PendingHrApproval,
                PendingApprovalRole = leaveType.IsAutoApprove ? null : approvalRole,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            if (leaveType.IsAutoApprove)
            {
                request.ApprovedAtUtc = now;
                request.ApprovedByName = "System";
                balance.PendingDays -= requestedDays;
                balance.UsedDays += requestedDays;
            }

            _dbContext.LeaveRequests.Add(request);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (leaveType.IsAutoApprove)
            {
                PublishLeaveApproved(request, request.EmployeeUserId, request.EmployeeEmail, request.EmployeeName, "System", "Auto-approved");
            }
            else
            {
                await PublishLeaveSubmittedAsync(request, approvalRole, manager, cancellationToken);
            }

            return MapLeaveRequest(request);
        }

        public async Task<LeaveRequestResponseDto> UpdateAsync(Guid id, UpdateLeaveRequestDto dto, CancellationToken cancellationToken = default)
        {
            var request = await GetLeaveRequestAsync(id, cancellationToken);
            EnsureOwner(request);

            if (request.Status != LeaveStatus.PendingManagerApproval && request.Status != LeaveStatus.PendingHrApproval)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Only pending leave requests can be updated.");
            }

            var employee = await _employeeDirectoryClient.GetUserAsync(request.EmployeeUserId, cancellationToken);
            var leaveType = await GetActiveLeaveTypeAsync(dto.LeaveTypeId, cancellationToken);

            ValidateDocumentRequirement(leaveType, dto.SupportingDocumentUrl);
            await EnsureNoOverlapAsync(employee.Id, dto.StartDate, dto.EndDate, request.Id, cancellationToken);

            await ReleasePendingDaysAsync(request.EmployeeUserId, request.LeaveTypeId, request.StartDate.Year, request.RequestedDays, cancellationToken);

            var requestedDays = await CalculateRequestedDaysAsync(dto.StartDate, dto.EndDate, dto.IsHalfDay, dto.HalfDaySession, cancellationToken);
            var approvalRole = DetermineApprovalRole(employee.ManagerId, requestedDays);
            var manager = employee.ManagerId.HasValue
                ? await _employeeDirectoryClient.GetUserAsync(employee.ManagerId.Value, cancellationToken)
                : null;

            var balance = await GetOrCreateBalanceAsync(employee, leaveType, dto.StartDate.Year, cancellationToken);
            balance.PendingDays += requestedDays;
            balance.UpdatedAtUtc = DateTime.UtcNow;

            request.ManagerUserId = employee.ManagerId;
            request.ManagerName = manager?.FullName;
            request.ManagerEmail = manager?.Email;
            request.LeaveTypeId = leaveType.Id;
            request.LeaveTypeName = leaveType.Name;
            request.StartDate = dto.StartDate;
            request.EndDate = dto.EndDate;
            request.RequestedDays = requestedDays;
            request.IsHalfDay = dto.IsHalfDay;
            request.HalfDaySession = dto.IsHalfDay ? dto.HalfDaySession : HalfDaySession.None;
            request.IsUnpaid = GetAvailableDays(balance) < requestedDays;
            request.Reason = dto.Reason.Trim();
            request.SupportingDocumentUrl = NormalizeOptionalText(dto.SupportingDocumentUrl);
            request.Status = leaveType.IsAutoApprove ? LeaveStatus.Approved : approvalRole == ApprovalRole.Manager ? LeaveStatus.PendingManagerApproval : LeaveStatus.PendingHrApproval;
            request.PendingApprovalRole = leaveType.IsAutoApprove ? null : approvalRole;
            request.ApprovedByUserId = null;
            request.UpdatedAtUtc = DateTime.UtcNow;
            request.RejectedByUserId = null;
            request.RejectedByName = null;
            request.RejectedAtUtc = null;
            request.RejectionReason = null;
            request.CancelledByUserId = null;
            request.CancelledByName = null;
            request.CancelledAtUtc = null;
            request.CancellationReason = null;

            if (leaveType.IsAutoApprove)
            {
                request.ApprovedAtUtc = DateTime.UtcNow;
                request.ApprovedByName = "System";
                balance.PendingDays -= requestedDays;
                balance.UsedDays += requestedDays;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (leaveType.IsAutoApprove)
            {
                PublishLeaveApproved(request, request.EmployeeUserId, request.EmployeeEmail, request.EmployeeName, "System", "Auto-approved");
            }
            else
            {
                await PublishLeaveSubmittedAsync(request, approvalRole, manager, cancellationToken);
            }

            return MapLeaveRequest(request);
        }

        public async Task<LeaveRequestResponseDto> ApproveAsync(Guid id, ApproveLeaveDto dto, CancellationToken cancellationToken = default)
        {
            var request = await GetLeaveRequestAsync(id, cancellationToken);
            var approver = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            EnsureCanApprove(request, approver);

            request.Status = LeaveStatus.Approved;
            request.PendingApprovalRole = null;
            request.ApprovedAtUtc = DateTime.UtcNow;
            request.ApprovedByUserId = approver.Id;
            request.ApprovedByName = approver.FullName;
            request.UpdatedAtUtc = DateTime.UtcNow;

            var balance = await GetOrCreateBalanceAsync(
                new EmployeeDirectoryUserDto
                {
                    Id = request.EmployeeUserId,
                    EmployeeId = request.EmployeeId,
                    FullName = request.EmployeeName,
                    Email = request.EmployeeEmail
                },
                request.LeaveTypeId,
                request.StartDate.Year,
                cancellationToken);

            balance.PendingDays -= request.RequestedDays;
            balance.UsedDays += request.RequestedDays;
            balance.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            PublishLeaveApproved(request, request.EmployeeUserId, request.EmployeeEmail, request.EmployeeName, approver.FullName, dto.Comment);

            return MapLeaveRequest(request);
        }

        public async Task<LeaveRequestResponseDto> RejectAsync(Guid id, RejectLeaveDto dto, CancellationToken cancellationToken = default)
        {
            var request = await GetLeaveRequestAsync(id, cancellationToken);
            var approver = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            EnsureCanApprove(request, approver);

            request.Status = LeaveStatus.Rejected;
            request.PendingApprovalRole = null;
            request.RejectedAtUtc = DateTime.UtcNow;
            request.RejectedByUserId = approver.Id;
            request.RejectedByName = approver.FullName;
            request.RejectionReason = dto.Reason.Trim();
            request.UpdatedAtUtc = DateTime.UtcNow;

            await ReleasePendingDaysAsync(request.EmployeeUserId, request.LeaveTypeId, request.StartDate.Year, request.RequestedDays, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _publisher.Publish(new LeaveRejectedEvent
            {
                RecipientUserId = request.EmployeeUserId,
                RecipientEmail = request.EmployeeEmail,
                RecipientName = request.EmployeeName,
                LeaveRequestId = request.Id,
                EmployeeId = request.EmployeeId,
                EmployeeName = request.EmployeeName,
                LeaveTypeName = request.LeaveTypeName,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedDays = request.RequestedDays,
                RejectedByName = approver.FullName,
                Reason = dto.Reason.Trim()
            }, "leave.rejected");

            return MapLeaveRequest(request);
        }

        public async Task<LeaveRequestResponseDto> WithdrawAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var request = await GetLeaveRequestAsync(id, cancellationToken);
            EnsureOwner(request);

            if (request.Status != LeaveStatus.PendingManagerApproval && request.Status != LeaveStatus.PendingHrApproval)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Only pending leave requests can be withdrawn.");
            }

            request.Status = LeaveStatus.Withdrawn;
            request.PendingApprovalRole = null;
            request.CancelledAtUtc = DateTime.UtcNow;
            request.CancelledByUserId = request.EmployeeUserId;
            request.CancelledByName = request.EmployeeName;
            request.CancellationReason = "Withdrawn by employee.";
            request.UpdatedAtUtc = DateTime.UtcNow;

            await ReleasePendingDaysAsync(request.EmployeeUserId, request.LeaveTypeId, request.StartDate.Year, request.RequestedDays, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await PublishLeaveCancelledAsync(request, "Withdrawn by employee.", cancellationToken);

            return MapLeaveRequest(request);
        }

        public async Task<LeaveRequestResponseDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var request = await GetLeaveRequestAsync(id, cancellationToken);
            EnsureOwner(request);

            if (request.Status != LeaveStatus.Approved)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Only approved leave requests can be cancelled.");
            }

            request.Status = LeaveStatus.Cancelled;
            request.PendingApprovalRole = null;
            request.CancelledAtUtc = DateTime.UtcNow;
            request.CancelledByUserId = request.EmployeeUserId;
            request.CancelledByName = request.EmployeeName;
            request.CancellationReason = "Cancelled by employee.";
            request.UpdatedAtUtc = DateTime.UtcNow;

            await ReleaseApprovedDaysAsync(request.EmployeeUserId, request.LeaveTypeId, request.StartDate.Year, request.RequestedDays, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await PublishLeaveCancelledAsync(request, "Cancelled by employee.", cancellationToken);

            return MapLeaveRequest(request);
        }

        private async Task PublishLeaveSubmittedAsync(LeaveRequest request, ApprovalRole approvalRole, EmployeeDirectoryUserDto? manager, CancellationToken cancellationToken)
        {
            if (approvalRole == ApprovalRole.Manager)
            {
                if (manager == null)
                {
                    throw new ApiException(StatusCodes.Status500InternalServerError, "Manager information is missing for manager approval.");
                }

                _publisher.Publish(CreateSubmittedEvent(request, manager, approvalRole), "leave.submitted");
                return;
            }

            var hrAdmins = await _employeeDirectoryClient.GetUsersAsync(role: "HRAdmin", cancellationToken: cancellationToken);
            foreach (var hrAdmin in hrAdmins)
            {
                _publisher.Publish(CreateSubmittedEvent(request, hrAdmin, approvalRole), "leave.submitted");
            }
        }

        private async Task PublishLeaveCancelledAsync(LeaveRequest request, string reason, CancellationToken cancellationToken)
        {
            var approvalRole = DetermineApprovalRole(request.ManagerUserId, request.RequestedDays);

            if (approvalRole == ApprovalRole.Manager && request.ManagerUserId.HasValue)
            {
                var manager = await _employeeDirectoryClient.GetUserAsync(request.ManagerUserId.Value, cancellationToken);
                _publisher.Publish(CreateCancelledEvent(request, manager, reason), "leave.cancelled");
                return;
            }

            var hrAdmins = await _employeeDirectoryClient.GetUsersAsync(role: "HRAdmin", cancellationToken: cancellationToken);
            foreach (var hrAdmin in hrAdmins)
            {
                _publisher.Publish(CreateCancelledEvent(request, hrAdmin, reason), "leave.cancelled");
            }
        }

        private static LeaveSubmittedEvent CreateSubmittedEvent(LeaveRequest request, EmployeeDirectoryUserDto recipient, ApprovalRole role)
        {
            return new LeaveSubmittedEvent
            {
                RecipientUserId = recipient.Id,
                RecipientEmail = recipient.Email,
                RecipientName = recipient.FullName,
                LeaveRequestId = request.Id,
                EmployeeUserId = request.EmployeeUserId,
                EmployeeId = request.EmployeeId,
                EmployeeName = request.EmployeeName,
                LeaveTypeName = request.LeaveTypeName,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedDays = request.RequestedDays,
                Reason = request.Reason,
                ApprovalRole = role.ToString()
            };
        }

        private static LeaveCancelledEvent CreateCancelledEvent(LeaveRequest request, EmployeeDirectoryUserDto recipient, string reason)
        {
            return new LeaveCancelledEvent
            {
                RecipientUserId = recipient.Id,
                RecipientEmail = recipient.Email,
                RecipientName = recipient.FullName,
                LeaveRequestId = request.Id,
                EmployeeId = request.EmployeeId,
                EmployeeName = request.EmployeeName,
                LeaveTypeName = request.LeaveTypeName,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedDays = request.RequestedDays,
                CancelledByName = request.EmployeeName,
                Reason = reason
            };
        }

        private void PublishLeaveApproved(LeaveRequest request, Guid recipientUserId, string recipientEmail, string recipientName, string approvedByName, string? comment)
        {
            _publisher.Publish(new LeaveApprovedEvent
            {
                RecipientUserId = recipientUserId,
                RecipientEmail = recipientEmail,
                RecipientName = recipientName,
                LeaveRequestId = request.Id,
                EmployeeId = request.EmployeeId,
                EmployeeName = request.EmployeeName,
                LeaveTypeName = request.LeaveTypeName,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedDays = request.RequestedDays,
                ApprovedByName = approvedByName,
                Comment = NormalizeOptionalText(comment)
            }, "leave.approved");
        }

        private void EnsureOwner(LeaveRequest request)
        {
            if (request.EmployeeUserId != _currentUserService.GetUserId())
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "You can only manage your own leave requests.");
            }
        }

        private void EnsureCanView(LeaveRequest request)
        {
            var role = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();

            if (string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (request.EmployeeUserId == currentUserId)
            {
                return;
            }

            if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) && request.ManagerUserId == currentUserId)
            {
                return;
            }

            throw new ApiException(StatusCodes.Status403Forbidden, "You are not allowed to access this leave request.");
        }

        private static void EnsureCanApprove(LeaveRequest request, EmployeeDirectoryUserDto approver)
        {
            if (request.Status == LeaveStatus.PendingManagerApproval)
            {
                if (!string.Equals(approver.Role, "Manager", StringComparison.OrdinalIgnoreCase) || request.ManagerUserId != approver.Id)
                {
                    throw new ApiException(StatusCodes.Status403Forbidden, "Only the assigned manager can approve this leave request.");
                }

                return;
            }

            if (request.Status == LeaveStatus.PendingHrApproval)
            {
                if (!string.Equals(approver.Role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ApiException(StatusCodes.Status403Forbidden, "Only HRAdmin can approve this leave request.");
                }

                return;
            }

            throw new ApiException(StatusCodes.Status400BadRequest, "This leave request is not pending approval.");
        }

        private static ApprovalRole DetermineApprovalRole(Guid? managerUserId, decimal requestedDays)
        {
            return !managerUserId.HasValue || requestedDays >= 4m
                ? ApprovalRole.HRAdmin
                : ApprovalRole.Manager;
        }

        private async Task<LeaveRequest> GetLeaveRequestAsync(Guid id, CancellationToken cancellationToken)
        {
            var request = await _dbContext.LeaveRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (request == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Leave request not found.");
            }

            return request;
        }

        private async Task<LeaveType> GetActiveLeaveTypeAsync(Guid leaveTypeId, CancellationToken cancellationToken)
        {
            var leaveType = await _dbContext.LeaveTypes.FirstOrDefaultAsync(x => x.Id == leaveTypeId && x.IsActive, cancellationToken);
            if (leaveType == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Leave type not found or inactive.");
            }

            return leaveType;
        }

        private async Task EnsureNoOverlapAsync(Guid employeeUserId, DateOnly startDate, DateOnly endDate, Guid? currentRequestId, CancellationToken cancellationToken)
        {
            var overlapExists = await _dbContext.LeaveRequests.AnyAsync(
                x => x.EmployeeUserId == employeeUserId &&
                     ActiveStatuses.Contains(x.Status) &&
                     (!currentRequestId.HasValue || x.Id != currentRequestId.Value) &&
                     x.StartDate <= endDate &&
                     x.EndDate >= startDate,
                cancellationToken);

            if (overlapExists)
            {
                throw new ApiException(StatusCodes.Status409Conflict, "The selected dates overlap with an existing leave request.");
            }
        }

        private async Task<decimal> CalculateRequestedDaysAsync(
            DateOnly startDate,
            DateOnly endDate,
            bool isHalfDay,
            HalfDaySession halfDaySession,
            CancellationToken cancellationToken)
        {
            var holidayDates = await GetHolidayDatesAsync(startDate, endDate, cancellationToken);
            var workingDates = new List<DateOnly>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    continue;
                }

                if (holidayDates.Contains(date))
                {
                    continue;
                }

                workingDates.Add(date);
            }

            if (isHalfDay)
            {
                if (startDate != endDate)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, "Half-day leave must be for a single working day.");
                }

                if (halfDaySession == HalfDaySession.None)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, "Half-day session is required for half-day leave.");
                }

                if (workingDates.Count != 1)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, "You cannot apply half-day leave on a weekend or holiday.");
                }

                return 0.5m;
            }

            if (workingDates.Count == 0)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "The selected range contains no working days. Leave cannot be applied on holidays or weekends only.");
            }

            return workingDates.Count;
        }

        private async Task<HashSet<DateOnly>> GetHolidayDatesAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
        {
            var years = Enumerable.Range(startDate.Year, endDate.Year - startDate.Year + 1).Distinct();
            var holidayDates = new HashSet<DateOnly>();

            foreach (var year in years)
            {
                var holidays = await _holidayCalendarClient.GetHolidaysAsync(year, cancellationToken);
                foreach (var holiday in holidays)
                {
                    holidayDates.Add(holiday.HolidayDate);
                }
            }

            return holidayDates;
        }

        private async Task<LeaveBalance> GetOrCreateBalanceAsync(EmployeeDirectoryUserDto employee, LeaveType leaveType, int year, CancellationToken cancellationToken)
        {
            var balance = await _dbContext.LeaveBalances.FirstOrDefaultAsync(
                x => x.EmployeeUserId == employee.Id &&
                     x.LeaveTypeId == leaveType.Id &&
                     x.Year == year,
                cancellationToken);

            if (balance != null)
            {
                return balance;
            }

            balance = new LeaveBalance
            {
                Id = Guid.NewGuid(),
                EmployeeUserId = employee.Id,
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = leaveType.Id,
                LeaveType = leaveType,
                Year = year,
                AllocatedDays = leaveType.DefaultAnnualQuota,
                CarriedForwardDays = 0m,
                ManualAdjustmentDays = 0m,
                PendingDays = 0m,
                UsedDays = 0m,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.LeaveBalances.Add(balance);
            return balance;
        }

        private async Task<LeaveBalance> GetOrCreateBalanceAsync(EmployeeDirectoryUserDto employee, Guid leaveTypeId, int year, CancellationToken cancellationToken)
        {
            var leaveType = await _dbContext.LeaveTypes.FirstOrDefaultAsync(x => x.Id == leaveTypeId, cancellationToken);
            if (leaveType == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Leave type not found.");
            }

            return await GetOrCreateBalanceAsync(employee, leaveType, year, cancellationToken);
        }

        private async Task ReleasePendingDaysAsync(Guid employeeUserId, Guid leaveTypeId, int year, decimal days, CancellationToken cancellationToken)
        {
            var balance = await _dbContext.LeaveBalances.FirstOrDefaultAsync(
                x => x.EmployeeUserId == employeeUserId &&
                     x.LeaveTypeId == leaveTypeId &&
                     x.Year == year,
                cancellationToken);

            if (balance == null)
            {
                return;
            }

            balance.PendingDays = Math.Max(0m, balance.PendingDays - days);
            balance.UpdatedAtUtc = DateTime.UtcNow;
        }

        private async Task ReleaseApprovedDaysAsync(Guid employeeUserId, Guid leaveTypeId, int year, decimal days, CancellationToken cancellationToken)
        {
            var balance = await _dbContext.LeaveBalances.FirstOrDefaultAsync(
                x => x.EmployeeUserId == employeeUserId &&
                     x.LeaveTypeId == leaveTypeId &&
                     x.Year == year,
                cancellationToken);

            if (balance == null)
            {
                return;
            }

            balance.UsedDays = Math.Max(0m, balance.UsedDays - days);
            balance.UpdatedAtUtc = DateTime.UtcNow;
        }

        private static void ValidateDocumentRequirement(LeaveType leaveType, string? supportingDocumentUrl)
        {
            if (leaveType.RequiresDocument && string.IsNullOrWhiteSpace(supportingDocumentUrl))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"A supporting document is required for {leaveType.Name} leave.");
            }
        }

        private static decimal GetAvailableDays(LeaveBalance balance)
        {
            return balance.AllocatedDays + balance.CarriedForwardDays + balance.ManualAdjustmentDays - balance.PendingDays - balance.UsedDays;
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static LeaveRequestResponseDto MapLeaveRequest(LeaveRequest request)
        {
            return new LeaveRequestResponseDto
            {
                Id = request.Id,
                EmployeeUserId = request.EmployeeUserId,
                EmployeeId = request.EmployeeId,
                EmployeeName = request.EmployeeName,
                ManagerUserId = request.ManagerUserId,
                ManagerName = request.ManagerName,
                LeaveTypeId = request.LeaveTypeId,
                LeaveTypeName = request.LeaveTypeName,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedDays = request.RequestedDays,
                IsHalfDay = request.IsHalfDay,
                HalfDaySession = request.HalfDaySession.ToString(),
                IsUnpaid = request.IsUnpaid,
                Reason = request.Reason,
                SupportingDocumentUrl = request.SupportingDocumentUrl,
                Status = request.Status.ToString(),
                PendingApprovalRole = request.PendingApprovalRole?.ToString(),
                ApprovedByUserId = request.ApprovedByUserId,
                ApprovedByName = request.ApprovedByName,
                ApprovedAtUtc = request.ApprovedAtUtc,
                RejectedByUserId = request.RejectedByUserId,
                RejectedByName = request.RejectedByName,
                RejectionReason = request.RejectionReason,
                RejectedAtUtc = request.RejectedAtUtc,
                CreatedAtUtc = request.CreatedAtUtc,
                UpdatedAtUtc = request.UpdatedAtUtc
            };
        }
    }
}
