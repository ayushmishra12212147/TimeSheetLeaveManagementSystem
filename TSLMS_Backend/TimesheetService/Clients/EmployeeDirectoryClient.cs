using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TimesheetService.DTOs;
using TimesheetService.Exceptions;
using TimesheetService.Options;

namespace TimesheetService.Clients
{
    public class EmployeeDirectoryClient : IEmployeeDirectoryClient
    {
        private readonly HttpClient _httpClient;
        private readonly ServiceEndpointsOptions _endpoints;

        public EmployeeDirectoryClient(HttpClient httpClient, IOptions<ServiceEndpointsOptions> endpoints)
        {
            _httpClient = httpClient;
            _endpoints = endpoints.Value;
        }

        public async Task<EmployeeDirectoryUserDto> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var url = $"{_endpoints.EmployeeServiceBaseUrl.TrimEnd('/')}/api/users/{userId}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

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
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(role))
            {
                queryParams.Add($"role={Uri.EscapeDataString(role)}");
            }

            if (managerId.HasValue)
            {
                queryParams.Add($"managerId={managerId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                queryParams.Add($"employeeId={Uri.EscapeDataString(employeeId)}");
            }

            var query = queryParams.Count == 0 ? string.Empty : $"?{string.Join("&", queryParams)}";
            var url = $"{_endpoints.EmployeeServiceBaseUrl.TrimEnd('/')}/api/users{query}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(StatusCodes.Status502BadGateway, "EmployeeService lookup failed.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<List<EmployeeDirectoryUserDto>>>(cancellationToken: cancellationToken);
            return envelope?.Data ?? new List<EmployeeDirectoryUserDto>();
        }
    }
}
