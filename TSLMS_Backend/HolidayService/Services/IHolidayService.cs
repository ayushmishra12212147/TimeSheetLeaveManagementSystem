using HolidayService.DTOs;

namespace HolidayService.Services
{
    public interface IHolidayService
    {
        Task<IReadOnlyCollection<HolidayResponseDto>> GetAllAsync(int? year, CancellationToken cancellationToken = default);
        Task<HolidayResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<HolidayCheckResponseDto> CheckAsync(DateOnly date, CancellationToken cancellationToken = default);
        Task<HolidayResponseDto> CreateAsync(CreateHolidayDto dto, CancellationToken cancellationToken = default);
        Task<HolidayResponseDto> UpdateAsync(Guid id, UpdateHolidayDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<HolidayResponseDto>> CopyYearAsync(CopyHolidayYearDto dto, CancellationToken cancellationToken = default);
    }
}
