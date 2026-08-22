using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReportService.DTOs;
using ReportService.Exceptions;
using ReportService.Options;

namespace ReportService.Clients
{
    public class EmployeeDirectoryClient : IEmployeeDirectoryClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceEndpointsOptions _endpoints;

        public EmployeeDirectoryClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ServiceEndpointsOptions> endpoints)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _endpoints = endpoints.Value;
        }

        public async Task<EmployeeDirectoryUserDto> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoints.EmployeeServiceBaseUrl.TrimEnd('/')}/api/users/{userId}");
            ForwardAuthorization(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "EmployeeService lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<EmployeeDirectoryUserDto>>(cancellationToken: cancellationToken);
            if (envelope?.Data == null)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "EmployeeService returned an invalid user payload.");
            }

            return envelope.Data;
        }

        public async Task<IReadOnlyCollection<EmployeeDirectoryUserDto>> GetUsersAsync(
            string? role = null,
            Guid? managerId = null,
            string? employeeId = null,
            CancellationToken cancellationToken = default)
        {
            var queryParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(role))
            {
                queryParts.Add($"role={Uri.EscapeDataString(role)}");
            }

            if (managerId.HasValue)
            {
                queryParts.Add($"managerId={managerId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                queryParts.Add($"employeeId={Uri.EscapeDataString(employeeId)}");
            }

            var query = queryParts.Count == 0 ? string.Empty : $"?{string.Join("&", queryParts)}";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_endpoints.EmployeeServiceBaseUrl.TrimEnd('/')}/api/users{query}");
            ForwardAuthorization(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "EmployeeService lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<EmployeeDirectoryUserDto>>>(cancellationToken: cancellationToken);
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
