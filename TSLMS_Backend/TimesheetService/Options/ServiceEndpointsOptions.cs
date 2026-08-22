namespace TimesheetService.Options
{
    public class ServiceEndpointsOptions
    {
        public const string SectionName = "ServiceEndpoints";

        public string EmployeeServiceBaseUrl { get; set; } = string.Empty;
    }
}
