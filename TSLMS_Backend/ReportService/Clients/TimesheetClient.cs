using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReportService.DTOs;
using ReportService.Exceptions;
using ReportService.Options;

namespace ReportService.Clients
{
    public class TimesheetClient : ITimesheetClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceEndpointsOptions _endpoints;

        public TimesheetClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ServiceEndpointsOptions> endpoints)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _endpoints = endpoints.Value;
        }

        public async Task<IReadOnlyCollection<DownstreamWeeklyTimesheetSummaryResponseDto>> GetTeamTimesheetsAsync(
            DateOnly weekStartDate,
            string? employeeId = null,
            CancellationToken cancellationToken = default)
        {
            var queryParts = new List<string>
            {
                $"weekStartDate={weekStartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
            };

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                queryParts.Add($"employeeId={Uri.EscapeDataString(employeeId)}");
            }

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_endpoints.TimesheetServiceBaseUrl.TrimEnd('/')}/api/v1/timesheets/team?{string.Join("&", queryParts)}");

            ForwardAuthorization(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "TimesheetService lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<DownstreamWeeklyTimesheetSummaryResponseDto>>>(cancellationToken: cancellationToken);
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
