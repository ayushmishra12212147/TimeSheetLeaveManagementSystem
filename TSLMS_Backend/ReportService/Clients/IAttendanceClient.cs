using ReportService.DTOs;

namespace ReportService.Clients
{
    public interface IAttendanceClient
    {
        Task<IReadOnlyCollection<DownstreamAttendanceRecordDto>> GetAttendanceRecordsAsync(
            DateOnly? dateFrom,
            DateOnly? dateTo,
            string? employeeId = null,
            CancellationToken cancellationToken = default);
    }
}
