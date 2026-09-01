namespace LeaveService.Options
{
    public class ServiceEndpointsOptions
    {
        public const string SectionName = "ServiceEndpoints";

        public string EmployeeServiceBaseUrl { get; set; } = string.Empty;
        public string HolidayServiceBaseUrl { get; set; } = string.Empty;
    }
}
