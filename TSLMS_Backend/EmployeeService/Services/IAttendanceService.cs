using EmployeeService.DTOs;

namespace EmployeeService.Services
{
    public interface IAttendanceService
    {
        Task<GenerateQrResponseDto> GenerateQrAsync(GenerateQrDto dto, CancellationToken cancellationToken = default);
        Task<AttendanceResponseDto> ScanInAsync(ScanQrDto dto, CancellationToken cancellationToken = default);
        Task<AttendanceResponseDto> ScanOutAsync(ScanQrDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<AttendanceResponseDto>> GetMyAsync(DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<AttendanceResponseDto>> GetTeamAsync(DateOnly? date, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<AttendanceResponseDto>> GetHistoryAsync(Guid employeeUserId, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<AttendanceResponseDto>> GetReportAsync(DateOnly? dateFrom, DateOnly? dateTo, string? employeeId, CancellationToken cancellationToken = default);
    }
}
