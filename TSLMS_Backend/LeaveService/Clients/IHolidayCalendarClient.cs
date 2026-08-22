using LeaveService.DTOs;

namespace LeaveService.Clients
{
    public interface IHolidayCalendarClient
    {
        Task<IReadOnlyCollection<HolidayResponseDto>> GetHolidaysAsync(int year, CancellationToken cancellationToken = default);
        Task<HolidayCheckResponseDto> CheckAsync(DateOnly date, CancellationToken cancellationToken = default);
    }
}
