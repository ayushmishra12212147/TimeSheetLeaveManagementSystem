using HolidayService.Data;
using HolidayService.DTOs;
using HolidayService.Exceptions;
using HolidayService.Models;
using Microsoft.EntityFrameworkCore;

namespace HolidayService.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly HolidayDbContext _dbContext;

        public HolidayService(HolidayDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<HolidayResponseDto>> GetAllAsync(int? year, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Holidays.AsNoTracking();

            if (year.HasValue)
            {
                query = query.Where(x => x.HolidayDate.Year == year.Value);
            }

            var holidays = await query
                .OrderBy(x => x.HolidayDate)
                .ToListAsync(cancellationToken);

            return holidays.Select(MapHoliday).ToList();
        }

        public async Task<HolidayResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var holiday = await GetHolidayEntityAsync(id, cancellationToken);
            return MapHoliday(holiday);
        }

        public async Task<HolidayCheckResponseDto> CheckAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            var holiday = await _dbContext.Holidays
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.HolidayDate == date, cancellationToken);

            return new HolidayCheckResponseDto
            {
                Date = date,
                IsHoliday = holiday != null,
                HolidayId = holiday?.Id,
                HolidayName = holiday?.Name
            };
        }

        public async Task<HolidayResponseDto> CreateAsync(CreateHolidayDto dto, CancellationToken cancellationToken = default)
        {
            await EnsureHolidayDateIsAvailableAsync(dto.HolidayDate, null, cancellationToken);

            var now = DateTime.UtcNow;
            var holiday = new Holiday
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                HolidayDate = dto.HolidayDate,
                Description = NormalizeOptionalText(dto.Description),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _dbContext.Holidays.Add(holiday);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapHoliday(holiday);
        }

        public async Task<HolidayResponseDto> UpdateAsync(Guid id, UpdateHolidayDto dto, CancellationToken cancellationToken = default)
        {
            var holiday = await GetHolidayEntityAsync(id, cancellationToken);

            await EnsureHolidayDateIsAvailableAsync(dto.HolidayDate, holiday.Id, cancellationToken);

            holiday.Name = dto.Name.Trim();
            holiday.HolidayDate = dto.HolidayDate;
            holiday.Description = NormalizeOptionalText(dto.Description);
            holiday.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapHoliday(holiday);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var holiday = await GetHolidayEntityAsync(id, cancellationToken);
            _dbContext.Holidays.Remove(holiday);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<HolidayResponseDto>> CopyYearAsync(CopyHolidayYearDto dto, CancellationToken cancellationToken = default)
        {
            var sourceHolidays = await _dbContext.Holidays
                .Where(x => x.HolidayDate.Year == dto.SourceYear)
                .OrderBy(x => x.HolidayDate)
                .ToListAsync(cancellationToken);

            if (sourceHolidays.Count == 0)
            {
                throw new ApiException(StatusCodes.Status404NotFound, $"No holidays were found for source year {dto.SourceYear}.");
            }

            var targetDates = await _dbContext.Holidays
                .Where(x => x.HolidayDate.Year == dto.TargetYear)
                .Select(x => x.HolidayDate)
                .ToListAsync(cancellationToken);

            var targetDateSet = targetDates.ToHashSet();
            var copiedHolidays = new List<Holiday>();
            var now = DateTime.UtcNow;

            foreach (var sourceHoliday in sourceHolidays)
            {
                var targetDate = new DateOnly(dto.TargetYear, sourceHoliday.HolidayDate.Month, sourceHoliday.HolidayDate.Day);

                if (targetDateSet.Contains(targetDate))
                {
                    if (dto.SkipExistingDates)
                    {
                        continue;
                    }

                    throw new ApiException(StatusCodes.Status409Conflict, $"A holiday already exists on {targetDate:yyyy-MM-dd} in target year {dto.TargetYear}.");
                }

                var holiday = new Holiday
                {
                    Id = Guid.NewGuid(),
                    Name = sourceHoliday.Name,
                    HolidayDate = targetDate,
                    Description = sourceHoliday.Description,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };

                copiedHolidays.Add(holiday);
                targetDateSet.Add(targetDate);
            }

            if (copiedHolidays.Count == 0)
            {
                return Array.Empty<HolidayResponseDto>();
            }

            _dbContext.Holidays.AddRange(copiedHolidays);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return copiedHolidays
                .OrderBy(x => x.HolidayDate)
                .Select(MapHoliday)
                .ToList();
        }

        private async Task<Holiday> GetHolidayEntityAsync(Guid id, CancellationToken cancellationToken)
        {
            var holiday = await _dbContext.Holidays.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (holiday == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Holiday not found.");
            }

            return holiday;
        }

        private async Task EnsureHolidayDateIsAvailableAsync(
            DateOnly holidayDate,
            Guid? currentHolidayId,
            CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Holidays.AnyAsync(
                x => x.HolidayDate == holidayDate && (!currentHolidayId.HasValue || x.Id != currentHolidayId.Value),
                cancellationToken);

            if (exists)
            {
                throw new ApiException(StatusCodes.Status409Conflict, $"A holiday already exists on {holidayDate:yyyy-MM-dd}.");
            }
        }

        private static string? NormalizeOptionalText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private static HolidayResponseDto MapHoliday(Holiday holiday)
        {
            return new HolidayResponseDto
            {
                Id = holiday.Id,
                Name = holiday.Name,
                HolidayDate = holiday.HolidayDate,
                Description = holiday.Description,
                Year = holiday.HolidayDate.Year,
                DayOfWeek = holiday.HolidayDate.DayOfWeek.ToString(),
                CreatedAtUtc = holiday.CreatedAtUtc,
                UpdatedAtUtc = holiday.UpdatedAtUtc
            };
        }
    }
}
