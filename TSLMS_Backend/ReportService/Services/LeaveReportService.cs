using ReportService.Clients;
using ReportService.DTOs;

namespace ReportService.Services
{
    public class LeaveReportService : ILeaveReportService
    {
        private readonly ILeaveClient _leaveClient;
        private readonly IReportScopeResolver _reportScopeResolver;

        public LeaveReportService(ILeaveClient leaveClient, IReportScopeResolver reportScopeResolver)
        {
            _leaveClient = leaveClient;
            _reportScopeResolver = reportScopeResolver;
        }

        public async Task<LeaveReportResponseDto> GenerateAsync(LeaveReportRequestDto request, CancellationToken cancellationToken = default)
        {
            var (dateFrom, dateTo) = NormalizeRange(request.DateFrom, request.DateTo);
            var employees = await _reportScopeResolver.ResolveEmployeesAsync(request.EmployeeId, cancellationToken);
            var rows = new List<LeaveReportRowDto>();
            var balances = new List<LeaveBalanceSnapshotDto>();
            var years = Enumerable.Range(dateFrom.Year, dateTo.Year - dateFrom.Year + 1).Distinct().ToList();

            foreach (var employee in employees)
            {
                var leaveRequests = await _leaveClient.GetLeaveRequestsAsync(employee.EmployeeId, cancellationToken);
                foreach (var item in leaveRequests.Where(x => OverlapsRange(x.StartDate, x.EndDate, dateFrom, dateTo)))
                {
                    if (!string.IsNullOrWhiteSpace(request.Status) &&
                        !string.Equals(item.Status, request.Status, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(request.LeaveType) &&
                        !string.Equals(item.LeaveTypeName, request.LeaveType, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    rows.Add(new LeaveReportRowDto
                    {
                        LeaveRequestId = item.Id,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmployeeName,
                        LeaveTypeName = item.LeaveTypeName,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        RequestedDays = item.RequestedDays,
                        IsUnpaid = item.IsUnpaid,
                        Status = item.Status,
                        PendingApprovalRole = item.PendingApprovalRole,
                        ApprovedByName = item.ApprovedByName,
                        ApprovedAtUtc = item.ApprovedAtUtc,
                        RejectedByName = item.RejectedByName,
                        RejectionReason = item.RejectionReason,
                        CreatedAtUtc = item.CreatedAtUtc
                    });
                }

                foreach (var year in years)
                {
                    var employeeBalances = await _leaveClient.GetLeaveBalancesAsync(employee.EmployeeId, year, cancellationToken);
                    balances.AddRange(employeeBalances.Select(x => new LeaveBalanceSnapshotDto
                    {
                        EmployeeId = x.EmployeeId,
                        LeaveTypeName = x.LeaveTypeName,
                        Year = x.Year,
                        AllocatedDays = x.AllocatedDays,
                        CarriedForwardDays = x.CarriedForwardDays,
                        ManualAdjustmentDays = x.ManualAdjustmentDays,
                        PendingDays = x.PendingDays,
                        UsedDays = x.UsedDays,
                        AvailableDays = x.AvailableDays
                    }));
                }
            }

            rows = rows
                .DistinctBy(x => x.LeaveRequestId)
                .OrderByDescending(x => x.StartDate)
                .ThenBy(x => x.EmployeeName)
                .ToList();

            var summary = new LeaveReportSummaryDto
            {
                TotalRequests = rows.Count,
                TotalRequestedDays = rows.Sum(x => x.RequestedDays),
                ApprovedDays = rows.Where(x => string.Equals(x.Status, "Approved", StringComparison.OrdinalIgnoreCase)).Sum(x => x.RequestedDays),
                PendingDays = rows.Where(x => x.Status.Contains("Pending", StringComparison.OrdinalIgnoreCase)).Sum(x => x.RequestedDays),
                RejectedDays = rows.Where(x => string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase)).Sum(x => x.RequestedDays),
                CancelledDays = rows.Where(x => string.Equals(x.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) || string.Equals(x.Status, "Withdrawn", StringComparison.OrdinalIgnoreCase)).Sum(x => x.RequestedDays),
                UnpaidDays = rows.Where(x => x.IsUnpaid).Sum(x => x.RequestedDays),
                StatusBreakdown = rows
                    .GroupBy(x => x.Status)
                    .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase)
            };

            return new LeaveReportResponseDto
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Scope = await _reportScopeResolver.DescribeScopeAsync(request.EmployeeId, cancellationToken),
                Rows = rows,
                Balances = balances
                    .OrderBy(x => x.EmployeeId)
                    .ThenBy(x => x.Year)
                    .ThenBy(x => x.LeaveTypeName)
                    .ToList(),
                Summary = summary
            };
        }

        private static (DateOnly DateFrom, DateOnly DateTo) NormalizeRange(DateOnly? dateFrom, DateOnly? dateTo)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var from = dateFrom ?? new DateOnly(today.Year, today.Month, 1);
            var to = dateTo ?? today;
            return from <= to ? (from, to) : (to, from);
        }

        private static bool OverlapsRange(DateOnly start, DateOnly end, DateOnly from, DateOnly to)
        {
            return start <= to && end >= from;
        }
    }
}
