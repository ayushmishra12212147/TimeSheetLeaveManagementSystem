namespace EmployeeService.Services
{
    public interface ICurrentUserService
    {
        Guid GetUserId();
        string GetEmployeeId();
        string GetRole();
    }
}
