namespace LeaveService.DTOs
{
    public class AdjustBalanceDto
    {
        public decimal Days { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
