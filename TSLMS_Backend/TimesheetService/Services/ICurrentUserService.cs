namespace TimesheetService.Services
{
    public interface ICurrentUserService
    {
        Guid GetUserId();
        string GetRole();
        string GetEmployeeId();
    }
}
