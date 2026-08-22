using System.ComponentModel.DataAnnotations;
using EmployeeService.Enums;

namespace EmployeeService.Models
{
    public class AttendanceRecord
    {
        [Key]
        public Guid Id { get; set; }

        public Guid EmployeeUserId { get; set; }

        [MaxLength(20)]
        public string EmployeeId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

        public DateOnly AttendanceDate { get; set; }
        public Guid? ScannedInByManagerId { get; set; }

        [MaxLength(100)]
        public string? ScannedInByManagerName { get; set; }

        public Guid? ScannedOutByManagerId { get; set; }

        [MaxLength(100)]
        public string? ScannedOutByManagerName { get; set; }

        [MaxLength(100)]
        public string? QrNonce { get; set; }

        public DateTime? QrExpiresAt { get; set; }

        [MaxLength(20)]
        public string? PendingQrType { get; set; }

        public bool IsNonceConsumed { get; set; }
        public bool IsQrExpired { get; set; }
        public DateTime? ClockInAtUtc { get; set; }
        public DateTime? ClockOutAtUtc { get; set; }
        public int? DurationMinutes { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
