namespace ReportService.Services
{
    public interface ICurrentUserService
    {
        Guid GetUserId();
        string GetRole();
        string GetEmployeeId();
    }
}
