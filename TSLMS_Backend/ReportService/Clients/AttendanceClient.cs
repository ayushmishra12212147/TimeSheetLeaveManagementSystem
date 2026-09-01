using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReportService.DTOs;
using ReportService.Exceptions;
using ReportService.Options;

namespace ReportService.Clients
{
    public class AttendanceClient : IAttendanceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceEndpointsOptions _endpoints;

        public AttendanceClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ServiceEndpointsOptions> endpoints)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _endpoints = endpoints.Value;
        }

        public async Task<IReadOnlyCollection<DownstreamAttendanceRecordDto>> GetAttendanceRecordsAsync(
            DateOnly? dateFrom,
            DateOnly? dateTo,
            string? employeeId = null,
            CancellationToken cancellationToken = default)
        {
            var queryParts = new List<string>();

            if (dateFrom.HasValue)
            {
                queryParts.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
            }

            if (dateTo.HasValue)
            {
                queryParts.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
            }

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                queryParts.Add($"employeeId={Uri.EscapeDataString(employeeId)}");
            }

            var query = queryParts.Count == 0 ? string.Empty : $"?{string.Join("&", queryParts)}";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoints.EmployeeServiceBaseUrl.TrimEnd('/')}/api/attendance/records{query}");
            ForwardAuthorization(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "EmployeeService attendance lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<DownstreamAttendanceRecordDto>>>(cancellationToken: cancellationToken);
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
