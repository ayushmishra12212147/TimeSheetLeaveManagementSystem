using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReportService.DTOs;
using ReportService.Exceptions;
using ReportService.Options;

namespace ReportService.Clients
{
    public class HolidayClient : IHolidayClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceEndpointsOptions _endpoints;

        public HolidayClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ServiceEndpointsOptions> endpoints)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _endpoints = endpoints.Value;
        }

        public async Task<IReadOnlyCollection<DownstreamHolidayResponseDto>> GetHolidaysAsync(int year, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoints.HolidayServiceBaseUrl.TrimEnd('/')}/api/v1/holidays?year={year}");
            ForwardAuthorization(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "HolidayService lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<DownstreamHolidayResponseDto>>>(cancellationToken: cancellationToken);
            return envelope?.Data ?? [];
        }

        private void ForwardAuthorization(HttpRequestMessage request)
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                request.Headers.TryAddWithoutValidation("Authorization", authHeader);
            }
        }
    }
}
