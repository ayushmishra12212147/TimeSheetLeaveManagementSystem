namespace ReportService.Options
{
    public class ServiceEndpointsOptions
    {
        public const string SectionName = "ServiceEndpoints";

        public string EmployeeServiceBaseUrl { get; set; } = string.Empty;
        public string LeaveServiceBaseUrl { get; set; } = string.Empty;
        public string TimesheetServiceBaseUrl { get; set; } = string.Empty;
        public string HolidayServiceBaseUrl { get; set; } = string.Empty;
    }
}
