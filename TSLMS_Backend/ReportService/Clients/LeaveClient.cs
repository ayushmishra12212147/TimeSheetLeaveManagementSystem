using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReportService.DTOs;
using ReportService.Exceptions;
using ReportService.Options;

namespace ReportService.Clients
{
    public class LeaveClient : ILeaveClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceEndpointsOptions _endpoints;

        public LeaveClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ServiceEndpointsOptions> endpoints)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _endpoints = endpoints.Value;
        }

        public async Task<IReadOnlyCollection<DownstreamLeaveRequestResponseDto>> GetLeaveRequestsAsync(
            string? employeeId = null,
            CancellationToken cancellationToken = default)
        {
            var query = string.IsNullOrWhiteSpace(employeeId)
                ? string.Empty
                : $"?employeeId={Uri.EscapeDataString(employeeId)}";

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoints.LeaveServiceBaseUrl.TrimEnd('/')}/api/v1/leaves{query}");
            ForwardAuthorization(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "LeaveService request lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<DownstreamLeaveRequestResponseDto>>>(cancellationToken: cancellationToken);
            return envelope?.Data ?? [];
        }

        public async Task<IReadOnlyCollection<DownstreamLeaveBalanceResponseDto>> GetLeaveBalancesAsync(
            string employeeId,
            int? year = null,
            CancellationToken cancellationToken = default)
        {
            var query = year.HasValue ? $"?year={year.Value}" : string.Empty;
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoints.LeaveServiceBaseUrl.TrimEnd('/')}/api/v1/leave-balances/{Uri.EscapeDataString(employeeId)}{query}");
            ForwardAuthorization(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "LeaveService balance lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<DownstreamLeaveBalanceResponseDto>>>(cancellationToken: cancellationToken);
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
