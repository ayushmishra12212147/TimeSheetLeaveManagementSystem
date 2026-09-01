using ReportService.DTOs;

namespace ReportService.Clients
{
    public interface IHolidayClient
    {
        Task<IReadOnlyCollection<DownstreamHolidayResponseDto>> GetHolidaysAsync(int year, CancellationToken cancellationToken = default);
    }
}
