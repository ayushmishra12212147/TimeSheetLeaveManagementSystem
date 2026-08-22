using HolidayService.DTOs;
using HolidayService.Helpers;
using HolidayService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HolidayService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/holidays")]
    public class HolidaysController : ControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidaysController(IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? year, CancellationToken cancellationToken)
        {
            var holidays = await _holidayService.GetAllAsync(year, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<HolidayResponseDto>>(holidays, "Holidays fetched successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var holiday = await _holidayService.GetByIdAsync(id, cancellationToken);
            return Ok(new ApiResponse<HolidayResponseDto>(holiday, "Holiday fetched successfully."));
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check([FromQuery] DateOnly date, CancellationToken cancellationToken)
        {
            var result = await _holidayService.CheckAsync(date, cancellationToken);
            return Ok(new ApiResponse<HolidayCheckResponseDto>(result, "Holiday check completed successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHolidayDto dto, CancellationToken cancellationToken)
        {
            var holiday = await _holidayService.CreateAsync(dto, cancellationToken);
            return Ok(new ApiResponse<HolidayResponseDto>(holiday, "Holiday created successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHolidayDto dto, CancellationToken cancellationToken)
        {
            var holiday = await _holidayService.UpdateAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<HolidayResponseDto>(holiday, "Holiday updated successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _holidayService.DeleteAsync(id, cancellationToken);
            return Ok(new ApiResponse<object>(null, "Holiday deleted successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost("copy-year")]
        public async Task<IActionResult> CopyYear([FromBody] CopyHolidayYearDto dto, CancellationToken cancellationToken)
        {
            var copiedHolidays = await _holidayService.CopyYearAsync(dto, cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<HolidayResponseDto>>(copiedHolidays, "Holiday calendar copied successfully."));
        }
    }
}
