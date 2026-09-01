using Microsoft.EntityFrameworkCore;
using TimesheetService.Clients;
using TimesheetService.Data;
using TimesheetService.DTOs;
using TimesheetService.Enums;
using TimesheetService.Events;
using TimesheetService.Exceptions;
using TimesheetService.Messaging;
using TimesheetService.Models;

namespace TimesheetService.Services
{
    public class TimesheetEntryService : ITimesheetEntryService
    {
        private readonly TimesheetDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmployeeDirectoryClient _employeeDirectoryClient;
        private readonly IRabbitMQPublisher _publisher;

        public TimesheetEntryService(
            TimesheetDbContext dbContext,
            ICurrentUserService currentUserService,
            IEmployeeDirectoryClient employeeDirectoryClient,
            IRabbitMQPublisher publisher)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _employeeDirectoryClient = employeeDirectoryClient;
            _publisher = publisher;
        }

        public async Task<WeekTimesheetResponseDto> GetWeekAsync(DateOnly? weekStartDate, string? employeeId, CancellationToken cancellationToken = default)
        {
            var employee = await ResolveEmployeeForWeekAsync(employeeId, cancellationToken);
            var (weekStart, weekEnd) = GetWeekWindow(weekStartDate);
            var config = await GetConfigAsync(cancellationToken);

            var summary = await _dbContext.WeeklyTimesheetSummaries
                .AsNoTracking()
                .Include(x => x.Entries)
                .FirstOrDefaultAsync(
                    x => x.EmployeeUserId == employee.Id &&
                         x.WeekStartDate == weekStart,
                    cancellationToken);

            if (summary == null)
            {
                return new WeekTimesheetResponseDto
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.FullName,
                    WeekStartDate = weekStart,
                    WeekEndDate = weekEnd,
                    TotalHours = 0m,
                    MinimumWeeklyHours = config.MinimumWeeklyHours,
                    MeetsMinimumWeeklyHours = false,
                    Status = TimesheetStatus.Draft.ToString(),
                    IsLateSubmission = false,
                    Entries = Array.Empty<TimesheetEntryResponseDto>()
                };
            }

            return MapWeek(summary, config);
        }

        public async Task<TimesheetEntryResponseDto> CreateAsync(CreateTimesheetEntryDto dto, CancellationToken cancellationToken = default)
        {
            ValidateEntryDate(dto.EntryDate);

            var employee = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            var project = await GetActiveProjectAsync(dto.ProjectId, cancellationToken);
            var config = await GetConfigAsync(cancellationToken);
            var (weekStart, weekEnd) = GetWeekWindow(dto.EntryDate);
            var summary = await GetOrCreateSummaryAsync(employee, weekStart, weekEnd, cancellationToken);

            PrepareSummaryForEditing(summary);
            await EnsureDailyHoursWithinLimitAsync(summary, dto.EntryDate, dto.Hours, null, cancellationToken);

            var now = DateTime.UtcNow;
            var entry = new TimesheetEntry
            {
                Id = Guid.NewGuid(),
                WeeklyTimesheetSummaryId = summary.Id,
                WeeklyTimesheetSummary = summary,
                EntryDate = dto.EntryDate,
                ProjectId = project.Id,
                Project = project,
                ProjectName = project.Name,
                Hours = dto.Hours,
                Category = dto.Category,
                Description = NormalizeOptionalText(dto.Description),
                Status = TimesheetStatus.Draft,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _dbContext.TimesheetEntries.Add(entry);
            summary.TotalHours += dto.Hours;
            summary.UpdatedAtUtc = now;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await MapEntryAsync(entry.Id, config, cancellationToken);
        }

        public async Task<TimesheetEntryResponseDto> UpdateAsync(Guid id, UpdateTimesheetEntryDto dto, CancellationToken cancellationToken = default)
        {
            ValidateEntryDate(dto.EntryDate);

            var entry = await _dbContext.TimesheetEntries
                .Include(x => x.WeeklyTimesheetSummary)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entry == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Timesheet entry not found.");
            }

            EnsureOwner(entry.WeeklyTimesheetSummary);
            PrepareSummaryForEditing(entry.WeeklyTimesheetSummary);

            var employee = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            var project = await GetActiveProjectAsync(dto.ProjectId, cancellationToken);
            var config = await GetConfigAsync(cancellationToken);
            var (targetWeekStart, targetWeekEnd) = GetWeekWindow(dto.EntryDate);

            var originalSummary = entry.WeeklyTimesheetSummary;
            var targetSummary = originalSummary;

            if (targetWeekStart != originalSummary.WeekStartDate)
            {
                targetSummary = await GetOrCreateSummaryAsync(employee, targetWeekStart, targetWeekEnd, cancellationToken);
                PrepareSummaryForEditing(targetSummary);
            }

            await EnsureDailyHoursWithinLimitAsync(targetSummary, dto.EntryDate, dto.Hours, entry.Id, cancellationToken);

            originalSummary.TotalHours = Math.Max(0m, originalSummary.TotalHours - entry.Hours);
            originalSummary.UpdatedAtUtc = DateTime.UtcNow;

            entry.WeeklyTimesheetSummaryId = targetSummary.Id;
            entry.WeeklyTimesheetSummary = targetSummary;
            entry.EntryDate = dto.EntryDate;
            entry.ProjectId = project.Id;
            entry.Project = project;
            entry.ProjectName = project.Name;
            entry.Hours = dto.Hours;
            entry.Category = dto.Category;
            entry.Description = NormalizeOptionalText(dto.Description);
            entry.Status = TimesheetStatus.Draft;
            entry.UpdatedAtUtc = DateTime.UtcNow;

            targetSummary.TotalHours += dto.Hours;
            targetSummary.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await RemoveSummaryIfEmptyAsync(originalSummary.Id, cancellationToken);

            return await MapEntryAsync(entry.Id, config, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entry = await _dbContext.TimesheetEntries
                .Include(x => x.WeeklyTimesheetSummary)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entry == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Timesheet entry not found.");
            }

            EnsureOwner(entry.WeeklyTimesheetSummary);
            PrepareSummaryForEditing(entry.WeeklyTimesheetSummary);

            entry.WeeklyTimesheetSummary.TotalHours = Math.Max(0m, entry.WeeklyTimesheetSummary.TotalHours - entry.Hours);
            entry.WeeklyTimesheetSummary.UpdatedAtUtc = DateTime.UtcNow;

            var summaryId = entry.WeeklyTimesheetSummaryId;
            _dbContext.TimesheetEntries.Remove(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await RemoveSummaryIfEmptyAsync(summaryId, cancellationToken);
        }

        public async Task<WeeklyTimesheetSummaryResponseDto> SubmitAsync(SubmitTimesheetDto dto, CancellationToken cancellationToken = default)
        {
            var employee = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            var (weekStart, _) = GetWeekWindow(dto.WeekStartDate);

            var summary = await _dbContext.WeeklyTimesheetSummaries
                .Include(x => x.Entries)
                .FirstOrDefaultAsync(
                    x => x.EmployeeUserId == employee.Id &&
                         x.WeekStartDate == weekStart,
                    cancellationToken);

            if (summary == null || summary.Entries.Count == 0)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "No draft timesheet entries found for the selected week.");
            }

            if (summary.Status == TimesheetStatus.Submitted)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "This week's timesheet is already submitted.");
            }

            if (summary.Status == TimesheetStatus.Approved)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Approved timesheets are locked.");
            }

            var config = await GetConfigAsync(cancellationToken);
            summary.TotalHours = summary.Entries.Sum(x => x.Hours);

            if (summary.TotalHours < config.MinimumWeeklyHours)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"Minimum weekly hours of {config.MinimumWeeklyHours:0.##} are required before submission.");
            }

            var submittedAtUtc = DateTime.UtcNow;
            summary.Status = TimesheetStatus.Submitted;
            summary.IsLateSubmission = IsLateSubmission(summary.WeekEndDate);
            summary.SubmittedAtUtc = submittedAtUtc;
            summary.ApprovedAtUtc = null;
            summary.ApprovedByUserId = null;
            summary.ApprovedByName = null;
            summary.RejectedAtUtc = null;
            summary.RejectedByUserId = null;
            summary.RejectedByName = null;
            summary.RejectionReason = null;
            summary.UpdatedAtUtc = submittedAtUtc;

            foreach (var entry in summary.Entries)
            {
                entry.Status = TimesheetStatus.Submitted;
                entry.UpdatedAtUtc = submittedAtUtc;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await PublishSubmittedAsync(summary, cancellationToken);

            return MapSummary(summary, config);
        }

        public async Task<IReadOnlyCollection<WeeklyTimesheetSummaryResponseDto>> GetPendingAsync(DateOnly? weekStartDate, CancellationToken cancellationToken = default)
        {
            var role = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();
            var config = await GetConfigAsync(cancellationToken);
            var normalizedWeekStart = weekStartDate.HasValue ? GetWeekWindow(weekStartDate).WeekStartDate : (DateOnly?)null;

            IQueryable<WeeklyTimesheetSummary> query = _dbContext.WeeklyTimesheetSummaries
                .AsNoTracking()
                .Include(x => x.Entries)
                .Where(x => x.Status == TimesheetStatus.Submitted);

            if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.ManagerUserId == currentUserId);
            }
            else if (!string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only managers and HRAdmin can view pending timesheets.");
            }

            if (normalizedWeekStart.HasValue)
            {
                query = query.Where(x => x.WeekStartDate == normalizedWeekStart.Value);
            }

            var summaries = await query
                .OrderByDescending(x => x.SubmittedAtUtc)
                .ToListAsync(cancellationToken);

            return summaries.Select(x => MapSummary(x, config)).ToList();
        }

        public async Task<WeeklyTimesheetSummaryResponseDto> ApproveAsync(Guid summaryId, ApproveTimesheetDto dto, CancellationToken cancellationToken = default)
        {
            var summary = await GetSummaryAsync(summaryId, cancellationToken);
            var approver = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            EnsureCanApprove(summary, approver);

            summary.Status = TimesheetStatus.Approved;
            summary.ApprovedAtUtc = DateTime.UtcNow;
            summary.ApprovedByUserId = approver.Id;
            summary.ApprovedByName = approver.FullName;
            summary.RejectedAtUtc = null;
            summary.RejectedByUserId = null;
            summary.RejectedByName = null;
            summary.RejectionReason = null;
            summary.UpdatedAtUtc = DateTime.UtcNow;

            foreach (var entry in summary.Entries)
            {
                entry.Status = TimesheetStatus.Approved;
                entry.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _publisher.Publish(new TimesheetApprovedEvent
            {
                RecipientUserId = summary.EmployeeUserId,
                RecipientEmail = summary.EmployeeEmail,
                RecipientName = summary.EmployeeName,
                TimesheetSummaryId = summary.Id,
                EmployeeId = summary.EmployeeId,
                EmployeeName = summary.EmployeeName,
                WeekStartDate = summary.WeekStartDate,
                WeekEndDate = summary.WeekEndDate,
                TotalHours = summary.TotalHours,
                ApprovedByName = approver.FullName,
                Comment = NormalizeOptionalText(dto.Comment)
            }, "timesheet.approved");

            return MapSummary(summary, await GetConfigAsync(cancellationToken));
        }

        public async Task<WeeklyTimesheetSummaryResponseDto> RejectAsync(Guid summaryId, RejectTimesheetDto dto, CancellationToken cancellationToken = default)
        {
            var summary = await GetSummaryAsync(summaryId, cancellationToken);
            var approver = await _employeeDirectoryClient.GetUserAsync(_currentUserService.GetUserId(), cancellationToken);
            EnsureCanApprove(summary, approver);

            summary.Status = TimesheetStatus.Rejected;
            summary.ApprovedAtUtc = null;
            summary.ApprovedByUserId = null;
            summary.ApprovedByName = null;
            summary.RejectedAtUtc = DateTime.UtcNow;
            summary.RejectedByUserId = approver.Id;
            summary.RejectedByName = approver.FullName;
            summary.RejectionReason = dto.Reason.Trim();
            summary.UpdatedAtUtc = DateTime.UtcNow;

            foreach (var entry in summary.Entries)
            {
                entry.Status = TimesheetStatus.Draft;
                entry.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _publisher.Publish(new TimesheetRejectedEvent
            {
                RecipientUserId = summary.EmployeeUserId,
                RecipientEmail = summary.EmployeeEmail,
                RecipientName = summary.EmployeeName,
                TimesheetSummaryId = summary.Id,
                EmployeeId = summary.EmployeeId,
                EmployeeName = summary.EmployeeName,
                WeekStartDate = summary.WeekStartDate,
                WeekEndDate = summary.WeekEndDate,
                TotalHours = summary.TotalHours,
                RejectedByName = approver.FullName,
                Reason = dto.Reason.Trim()
            }, "timesheet.rejected");

            return MapSummary(summary, await GetConfigAsync(cancellationToken));
        }

        public async Task<IReadOnlyCollection<WeeklyTimesheetSummaryResponseDto>> GetTeamAsync(DateOnly? weekStartDate, string? employeeId, CancellationToken cancellationToken = default)
        {
            var role = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();
            var config = await GetConfigAsync(cancellationToken);
            var normalizedWeekStart = weekStartDate.HasValue ? GetWeekWindow(weekStartDate).WeekStartDate : GetWeekWindow(DateOnly.FromDateTime(DateTime.Today)).WeekStartDate;

            IQueryable<WeeklyTimesheetSummary> query = _dbContext.WeeklyTimesheetSummaries
                .AsNoTracking()
                .Include(x => x.Entries)
                .Where(x => x.WeekStartDate == normalizedWeekStart);

            if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.ManagerUserId == currentUserId);
            }
            else if (!string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only managers and HRAdmin can view team timesheets.");
            }

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                query = query.Where(x => x.EmployeeId == employeeId);
            }

            var summaries = await query
                .OrderBy(x => x.EmployeeName)
                .ToListAsync(cancellationToken);

            return summaries.Select(x => MapSummary(x, config)).ToList();
        }

        public async Task<int> AutoApproveExpiredSubmittedAsync(CancellationToken cancellationToken = default)
        {
            var config = await GetConfigAsync(cancellationToken);
            if (!config.AutoApproveEnabled)
            {
                return 0;
            }

            var cutoff = DateTime.UtcNow.AddHours(-config.AutoApproveAfterHours);
            var summaries = await _dbContext.WeeklyTimesheetSummaries
                .Include(x => x.Entries)
                .Where(x => x.Status == TimesheetStatus.Submitted && x.SubmittedAtUtc != null && x.SubmittedAtUtc <= cutoff)
                .ToListAsync(cancellationToken);

            if (summaries.Count == 0)
            {
                return 0;
            }

            var events = new List<TimesheetApprovedEvent>();
            var now = DateTime.UtcNow;

            foreach (var summary in summaries)
            {
                summary.Status = TimesheetStatus.Approved;
                summary.ApprovedAtUtc = now;
                summary.ApprovedByUserId = null;
                summary.ApprovedByName = "System";
                summary.UpdatedAtUtc = now;
                summary.RejectedAtUtc = null;
                summary.RejectedByUserId = null;
                summary.RejectedByName = null;
                summary.RejectionReason = null;

                foreach (var entry in summary.Entries)
                {
                    entry.Status = TimesheetStatus.Approved;
                    entry.UpdatedAtUtc = now;
                }

                events.Add(new TimesheetApprovedEvent
                {
                    RecipientUserId = summary.EmployeeUserId,
                    RecipientEmail = summary.EmployeeEmail,
                    RecipientName = summary.EmployeeName,
                    TimesheetSummaryId = summary.Id,
                    EmployeeId = summary.EmployeeId,
                    EmployeeName = summary.EmployeeName,
                    WeekStartDate = summary.WeekStartDate,
                    WeekEndDate = summary.WeekEndDate,
                    TotalHours = summary.TotalHours,
                    ApprovedByName = "System",
                    Comment = "Auto-approved by system."
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var @event in events)
            {
                _publisher.Publish(@event, "timesheet.approved");
            }

            return summaries.Count;
        }

        private async Task PublishSubmittedAsync(WeeklyTimesheetSummary summary, CancellationToken cancellationToken)
        {
            if (summary.ManagerUserId.HasValue)
            {
                _publisher.Publish(new TimesheetSubmittedEvent
                {
                    RecipientUserId = summary.ManagerUserId.Value,
                    RecipientEmail = summary.ManagerEmail ?? string.Empty,
                    RecipientName = summary.ManagerName ?? "Manager",
                    TimesheetSummaryId = summary.Id,
                    EmployeeUserId = summary.EmployeeUserId,
                    EmployeeId = summary.EmployeeId,
                    EmployeeName = summary.EmployeeName,
                    WeekStartDate = summary.WeekStartDate,
                    WeekEndDate = summary.WeekEndDate,
                    TotalHours = summary.TotalHours,
                    IsLateSubmission = summary.IsLateSubmission
                }, "timesheet.submitted");

                return;
            }

            var hrAdmins = await _employeeDirectoryClient.GetUsersAsync(role: "HRAdmin", cancellationToken: cancellationToken);
            foreach (var hrAdmin in hrAdmins)
            {
                _publisher.Publish(new TimesheetSubmittedEvent
                {
                    RecipientUserId = hrAdmin.Id,
                    RecipientEmail = hrAdmin.Email,
                    RecipientName = hrAdmin.FullName,
                    TimesheetSummaryId = summary.Id,
                    EmployeeUserId = summary.EmployeeUserId,
                    EmployeeId = summary.EmployeeId,
                    EmployeeName = summary.EmployeeName,
                    WeekStartDate = summary.WeekStartDate,
                    WeekEndDate = summary.WeekEndDate,
                    TotalHours = summary.TotalHours,
                    IsLateSubmission = summary.IsLateSubmission
                }, "timesheet.submitted");
            }
        }

        private async Task<EmployeeDirectoryUserDto> ResolveEmployeeForWeekAsync(string? employeeId, CancellationToken cancellationToken)
        {
            var currentRole = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();
            var currentEmployeeId = _currentUserService.GetEmployeeId();

            if (string.IsNullOrWhiteSpace(employeeId) || string.Equals(employeeId, currentEmployeeId, StringComparison.OrdinalIgnoreCase))
            {
                return await _employeeDirectoryClient.GetUserAsync(currentUserId, cancellationToken);
            }

            if (string.Equals(currentRole, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                var hrTarget = (await _employeeDirectoryClient.GetUsersAsync(employeeId: employeeId, cancellationToken: cancellationToken)).FirstOrDefault();
                if (hrTarget == null)
                {
                    throw new ApiException(StatusCodes.Status404NotFound, "Employee not found.");
                }

                return hrTarget;
            }

            if (string.Equals(currentRole, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                var directReport = (await _employeeDirectoryClient.GetUsersAsync(
                    managerId: currentUserId,
                    employeeId: employeeId,
                    cancellationToken: cancellationToken)).FirstOrDefault();

                if (directReport == null)
                {
                    throw new ApiException(StatusCodes.Status403Forbidden, "You can only view timesheets for your direct reports.");
                }

                return directReport;
            }

            throw new ApiException(StatusCodes.Status403Forbidden, "You can only view your own timesheets.");
        }

        private async Task<Project> GetActiveProjectAsync(Guid projectId, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);
            if (project == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Project not found.");
            }

            if (!project.IsActive)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Inactive projects cannot be used for new timesheet entries.");
            }

            return project;
        }

        private async Task<TimesheetAutoApproveConfig> GetConfigAsync(CancellationToken cancellationToken)
        {
            var config = await _dbContext.TimesheetAutoApproveConfigs.FirstOrDefaultAsync(cancellationToken);
            if (config != null)
            {
                return config;
            }

            config = new TimesheetAutoApproveConfig
            {
                Id = Guid.NewGuid(),
                MinimumWeeklyHours = 40m,
                LowHoursWarningThreshold = 8m,
                HighHoursWarningThreshold = 12m,
                AutoApproveEnabled = false,
                AutoApproveAfterHours = 48,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.TimesheetAutoApproveConfigs.Add(config);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return config;
        }

        private async Task<WeeklyTimesheetSummary> GetOrCreateSummaryAsync(
            EmployeeDirectoryUserDto employee,
            DateOnly weekStartDate,
            DateOnly weekEndDate,
            CancellationToken cancellationToken)
        {
            var summary = await _dbContext.WeeklyTimesheetSummaries
                .Include(x => x.Entries)
                .FirstOrDefaultAsync(
                    x => x.EmployeeUserId == employee.Id &&
                         x.WeekStartDate == weekStartDate,
                    cancellationToken);

            if (summary != null)
            {
                return summary;
            }

            var manager = employee.ManagerId.HasValue
                ? await _employeeDirectoryClient.GetUserAsync(employee.ManagerId.Value, cancellationToken)
                : null;

            summary = new WeeklyTimesheetSummary
            {
                Id = Guid.NewGuid(),
                EmployeeUserId = employee.Id,
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.FullName,
                EmployeeEmail = employee.Email,
                ManagerUserId = employee.ManagerId,
                ManagerName = manager?.FullName,
                ManagerEmail = manager?.Email,
                WeekStartDate = weekStartDate,
                WeekEndDate = weekEndDate,
                TotalHours = 0m,
                Status = TimesheetStatus.Draft,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.WeeklyTimesheetSummaries.Add(summary);
            return summary;
        }

        private async Task<WeeklyTimesheetSummary> GetSummaryAsync(Guid summaryId, CancellationToken cancellationToken)
        {
            var summary = await _dbContext.WeeklyTimesheetSummaries
                .Include(x => x.Entries)
                .FirstOrDefaultAsync(x => x.Id == summaryId, cancellationToken);

            if (summary == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Timesheet summary not found.");
            }

            return summary;
        }

        private void EnsureOwner(WeeklyTimesheetSummary summary)
        {
            if (summary.EmployeeUserId != _currentUserService.GetUserId())
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "You can only manage your own timesheet entries.");
            }
        }

        private void EnsureCanApprove(WeeklyTimesheetSummary summary, EmployeeDirectoryUserDto approver)
        {
            if (summary.Status != TimesheetStatus.Submitted)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Only submitted timesheets can be approved or rejected.");
            }

            if (string.Equals(approver.Role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(approver.Role, "Manager", StringComparison.OrdinalIgnoreCase) && summary.ManagerUserId == approver.Id)
            {
                return;
            }

            throw new ApiException(StatusCodes.Status403Forbidden, "You are not allowed to approve this timesheet.");
        }

        private void PrepareSummaryForEditing(WeeklyTimesheetSummary summary)
        {
            if (summary.Status == TimesheetStatus.Submitted || summary.Status == TimesheetStatus.Approved)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Submitted or approved timesheet entries cannot be edited.");
            }

            if (summary.Status == TimesheetStatus.Rejected)
            {
                summary.Status = TimesheetStatus.Draft;
                summary.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        private async Task EnsureDailyHoursWithinLimitAsync(
            WeeklyTimesheetSummary summary,
            DateOnly entryDate,
            decimal newHours,
            Guid? currentEntryId,
            CancellationToken cancellationToken)
        {
            var existingHours = await _dbContext.TimesheetEntries
                .Where(x =>
                    x.WeeklyTimesheetSummaryId == summary.Id &&
                    x.EntryDate == entryDate &&
                    (!currentEntryId.HasValue || x.Id != currentEntryId.Value))
                .SumAsync(x => (decimal?)x.Hours, cancellationToken) ?? 0m;

            if (existingHours + newHours > 24m)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Daily hours cannot exceed 24.");
            }
        }

        private async Task RemoveSummaryIfEmptyAsync(Guid summaryId, CancellationToken cancellationToken)
        {
            var summary = await _dbContext.WeeklyTimesheetSummaries
                .Include(x => x.Entries)
                .FirstOrDefaultAsync(x => x.Id == summaryId, cancellationToken);

            if (summary == null || summary.Entries.Count > 0)
            {
                return;
            }

            if (summary.Status == TimesheetStatus.Submitted || summary.Status == TimesheetStatus.Approved)
            {
                return;
            }

            _dbContext.WeeklyTimesheetSummaries.Remove(summary);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<TimesheetEntryResponseDto> MapEntryAsync(Guid entryId, TimesheetAutoApproveConfig config, CancellationToken cancellationToken)
        {
            var entry = await _dbContext.TimesheetEntries
                .AsNoTracking()
                .Include(x => x.WeeklyTimesheetSummary)
                .FirstAsync(x => x.Id == entryId, cancellationToken);

            var dailyTotal = await _dbContext.TimesheetEntries
                .Where(x =>
                    x.WeeklyTimesheetSummary.EmployeeUserId == entry.WeeklyTimesheetSummary.EmployeeUserId &&
                    x.EntryDate == entry.EntryDate)
                .SumAsync(x => x.Hours, cancellationToken);

            return MapEntry(entry, dailyTotal, config);
        }

        private static TimesheetEntryResponseDto MapEntry(TimesheetEntry entry, decimal dailyTotal, TimesheetAutoApproveConfig config)
        {
            return new TimesheetEntryResponseDto
            {
                Id = entry.Id,
                WeeklyTimesheetSummaryId = entry.WeeklyTimesheetSummaryId,
                EntryDate = entry.EntryDate,
                ProjectId = entry.ProjectId,
                ProjectName = entry.ProjectName,
                Hours = entry.Hours,
                Category = entry.Category.ToString(),
                Description = entry.Description,
                Status = entry.Status.ToString(),
                DailyTotalHours = dailyTotal,
                IsBelowDailyThresholdWarning = dailyTotal < config.LowHoursWarningThreshold,
                IsAboveDailyThresholdWarning = dailyTotal > config.HighHoursWarningThreshold,
                UpdatedAtUtc = entry.UpdatedAtUtc
            };
        }

        private WeekTimesheetResponseDto MapWeek(WeeklyTimesheetSummary summary, TimesheetAutoApproveConfig config)
        {
            var dailyTotals = summary.Entries
                .GroupBy(x => x.EntryDate)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Hours));

            return new WeekTimesheetResponseDto
            {
                SummaryId = summary.Id,
                EmployeeId = summary.EmployeeId,
                EmployeeName = summary.EmployeeName,
                WeekStartDate = summary.WeekStartDate,
                WeekEndDate = summary.WeekEndDate,
                TotalHours = summary.TotalHours,
                MinimumWeeklyHours = config.MinimumWeeklyHours,
                MeetsMinimumWeeklyHours = summary.TotalHours >= config.MinimumWeeklyHours,
                Status = summary.Status.ToString(),
                IsLateSubmission = summary.IsLateSubmission,
                RejectionReason = summary.RejectionReason,
                Entries = summary.Entries
                    .OrderBy(x => x.EntryDate)
                    .ThenBy(x => x.ProjectName)
                    .Select(x => MapEntry(x, dailyTotals[x.EntryDate], config))
                    .ToList()
            };
        }

        private static WeeklyTimesheetSummaryResponseDto MapSummary(WeeklyTimesheetSummary summary, TimesheetAutoApproveConfig config)
        {
            return new WeeklyTimesheetSummaryResponseDto
            {
                Id = summary.Id,
                EmployeeUserId = summary.EmployeeUserId,
                EmployeeId = summary.EmployeeId,
                EmployeeName = summary.EmployeeName,
                ManagerUserId = summary.ManagerUserId,
                ManagerName = summary.ManagerName,
                WeekStartDate = summary.WeekStartDate,
                WeekEndDate = summary.WeekEndDate,
                TotalHours = summary.TotalHours,
                Status = summary.Status.ToString(),
                IsLateSubmission = summary.IsLateSubmission,
                MeetsMinimumWeeklyHours = summary.TotalHours >= config.MinimumWeeklyHours,
                EntryCount = summary.Entries.Count,
                SubmittedAtUtc = summary.SubmittedAtUtc,
                ApprovedByName = summary.ApprovedByName,
                ApprovedAtUtc = summary.ApprovedAtUtc,
                RejectedByName = summary.RejectedByName,
                RejectionReason = summary.RejectionReason,
                RejectedAtUtc = summary.RejectedAtUtc
            };
        }

        private static (DateOnly WeekStartDate, DateOnly WeekEndDate) GetWeekWindow(DateOnly? referenceDate)
        {
            var date = referenceDate ?? DateOnly.FromDateTime(DateTime.Today);

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                var saturday = date.AddDays(-1);
                return (saturday.AddDays(-5), saturday);
            }

            var delta = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var weekStart = date.AddDays(-delta);
            return (weekStart, weekStart.AddDays(5));
        }

        private static bool IsLateSubmission(DateOnly weekEndDate)
        {
            var dueAtLocal = weekEndDate.ToDateTime(new TimeOnly(23, 59, 59));
            return DateTime.Now > dueAtLocal;
        }

        private static void ValidateEntryDate(DateOnly entryDate)
        {
            if (entryDate == default)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Entry date is required.");
            }

            if (entryDate > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Future dates are not allowed in timesheets.");
            }

            if (entryDate.DayOfWeek == DayOfWeek.Sunday)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Sunday entries are not allowed for Monday-Saturday timesheets.");
            }
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
