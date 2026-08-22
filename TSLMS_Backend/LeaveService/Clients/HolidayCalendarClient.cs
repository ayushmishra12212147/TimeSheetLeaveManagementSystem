using System.Net.Http.Headers;
using System.Net.Http.Json;
using LeaveService.DTOs;
using LeaveService.Exceptions;
using LeaveService.Options;
using Microsoft.Extensions.Options;

namespace LeaveService.Clients
{
    public class HolidayCalendarClient : IHolidayCalendarClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceEndpointsOptions _endpoints;

        public HolidayCalendarClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ServiceEndpointsOptions> endpoints)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _endpoints = endpoints.Value;
        }

        public async Task<IReadOnlyCollection<HolidayResponseDto>> GetHolidaysAsync(int year, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_endpoints.HolidayServiceBaseUrl.TrimEnd('/')}/api/v1/holidays?year={year}");

            AttachBearerToken(request);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "HolidayService lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<HolidayResponseDto>>>(cancellationToken: cancellationToken);
            return envelope?.Data ?? new List<HolidayResponseDto>();
        }

        public async Task<HolidayCheckResponseDto> CheckAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_endpoints.HolidayServiceBaseUrl.TrimEnd('/')}/api/v1/holidays/check?date={date:yyyy-MM-dd}");

            AttachBearerToken(request);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "HolidayService lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<HolidayCheckResponseDto>>(cancellationToken: cancellationToken);
            if (envelope?.Data == null)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "HolidayService returned an invalid holiday payload.");
            }

            return envelope.Data;
        }

        private void AttachBearerToken(HttpRequestMessage request)
        {
            var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorization))
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "Authorization header is missing.");
            }

            if (!AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "Authorization header is invalid.");
            }

            request.Headers.Authorization = headerValue;
        }
    }
}
