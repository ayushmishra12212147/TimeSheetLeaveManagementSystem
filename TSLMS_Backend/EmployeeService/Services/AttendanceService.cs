using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EmployeeService.Data;
using EmployeeService.DTOs;
using EmployeeService.Enums;
using EmployeeService.Events;
using EmployeeService.Exceptions;
using EmployeeService.Messaging;
using EmployeeService.Models;
using EmployeeService.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EmployeeService.Services
{
    public class AttendanceService : IAttendanceService
    {
        private static readonly TimeZoneInfo IndiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        private readonly AppDbContext _appDbContext;
        private readonly AttendanceDbContext _attendanceDbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRabbitMQPublisher _publisher;
        private readonly AttendanceOptions _options;

        public AttendanceService(
            AppDbContext appDbContext,
            AttendanceDbContext attendanceDbContext,
            ICurrentUserService currentUserService,
            IRabbitMQPublisher publisher,
            IOptions<AttendanceOptions> options)
        {
            _appDbContext = appDbContext;
            _attendanceDbContext = attendanceDbContext;
            _currentUserService = currentUserService;
            _publisher = publisher;
            _options = options.Value;
        }

        public async Task<GenerateQrResponseDto> GenerateQrAsync(GenerateQrDto dto, CancellationToken cancellationToken = default)
        {
            var qrType = NormalizeQrType(dto.Type);
            var currentUser = await GetCurrentUserAsync(cancellationToken);
            var today = GetBusinessDate();
            var nowUtc = DateTime.UtcNow;
            var expiresAtUtc = nowUtc.AddMinutes(_options.QrExpiryMinutes);

            var attendanceRecord = await _attendanceDbContext.AttendanceRecords
                .FirstOrDefaultAsync(x => x.EmployeeUserId == currentUser.Id && x.AttendanceDate == today, cancellationToken);

            if (qrType == "clock-in")
            {
                if (attendanceRecord?.ClockInAtUtc != null && attendanceRecord.ClockOutAtUtc == null)
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "You are already clocked in. Generate a clock-out QR instead.");
                }

                if (attendanceRecord?.ClockOutAtUtc != null)
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Attendance is already completed for today.");
                }
            }
            else
            {
                if (attendanceRecord?.ClockInAtUtc == null)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, "Clock-in must be completed before generating a clock-out QR.");
                }

                if (attendanceRecord.ClockOutAtUtc != null)
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Clock-out is already completed for today.");
                }
            }

            attendanceRecord ??= new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                EmployeeUserId = currentUser.Id,
                EmployeeId = currentUser.EmployeeId,
                EmployeeName = currentUser.FullName,
                AttendanceDate = today,
                Status = AttendanceStatus.Absent,
                CreatedAtUtc = nowUtc
            };

            attendanceRecord.EmployeeId = currentUser.EmployeeId;
            attendanceRecord.EmployeeName = currentUser.FullName;
            attendanceRecord.QrNonce = Guid.NewGuid().ToString("N");
            attendanceRecord.QrExpiresAt = expiresAtUtc;
            attendanceRecord.PendingQrType = qrType;
            attendanceRecord.IsNonceConsumed = false;
            attendanceRecord.IsQrExpired = false;
            attendanceRecord.UpdatedAtUtc = nowUtc;

            if (_attendanceDbContext.Entry(attendanceRecord).State == EntityState.Detached)
            {
                _attendanceDbContext.AttendanceRecords.Add(attendanceRecord);
            }

            await _attendanceDbContext.SaveChangesAsync(cancellationToken);

            var payload = BuildPayload(attendanceRecord, qrType, expiresAtUtc);

            return new GenerateQrResponseDto
            {
                Type = qrType,
                AttendanceDate = today,
                ExpiresAtUtc = expiresAtUtc,
                QrPayload = JsonSerializer.Serialize(payload)
            };
        }

        public Task<AttendanceResponseDto> ScanInAsync(ScanQrDto dto, CancellationToken cancellationToken = default)
        {
            return ScanAsync(dto, "clock-in", cancellationToken);
        }

        public Task<AttendanceResponseDto> ScanOutAsync(ScanQrDto dto, CancellationToken cancellationToken = default)
        {
            return ScanAsync(dto, "clock-out", cancellationToken);
        }

        public async Task<IReadOnlyCollection<AttendanceResponseDto>> GetMyAsync(DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            var (from, to) = NormalizeRange(dateFrom, dateTo);

            var records = await _attendanceDbContext.AttendanceRecords
                .AsNoTracking()
                .Where(x => x.EmployeeUserId == currentUserId && x.AttendanceDate >= from && x.AttendanceDate <= to)
                .OrderByDescending(x => x.AttendanceDate)
                .ToListAsync(cancellationToken);

            return records.Select(Map).ToList();
        }

        public async Task<IReadOnlyCollection<AttendanceResponseDto>> GetTeamAsync(DateOnly? date, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetUserId();
            var targetDate = date ?? GetBusinessDate();

            var teamMembers = await _appDbContext.Users
                .AsNoTracking()
                .Where(x => x.ManagerId == currentUserId)
                .OrderBy(x => x.FullName)
                .ToListAsync(cancellationToken);

            var teamUserIds = teamMembers.Select(x => x.Id).ToList();
            var records = await _attendanceDbContext.AttendanceRecords
                .AsNoTracking()
                .Where(x => x.AttendanceDate == targetDate && teamUserIds.Contains(x.EmployeeUserId))
                .ToListAsync(cancellationToken);

            var recordsByUserId = records.ToDictionary(x => x.EmployeeUserId);
            return teamMembers.Select(user =>
            {
                if (recordsByUserId.TryGetValue(user.Id, out var record))
                {
                    return Map(record);
                }

                return new AttendanceResponseDto
                {
                    Id = Guid.Empty,
                    EmployeeUserId = user.Id,
                    EmployeeId = user.EmployeeId,
                    EmployeeName = user.FullName,
                    AttendanceDate = targetDate,
                    Status = AttendanceStatus.Absent.ToString()
                };
            }).ToList();
        }

        public async Task<IReadOnlyCollection<AttendanceResponseDto>> GetHistoryAsync(Guid employeeUserId, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken = default)
        {
            await EnsureEmployeeVisibleAsync(employeeUserId, cancellationToken);
            var (from, to) = NormalizeRange(dateFrom, dateTo);

            var records = await _attendanceDbContext.AttendanceRecords
                .AsNoTracking()
                .Where(x => x.EmployeeUserId == employeeUserId && x.AttendanceDate >= from && x.AttendanceDate <= to)
                .OrderByDescending(x => x.AttendanceDate)
                .ToListAsync(cancellationToken);

            return records.Select(Map).ToList();
        }

        public async Task<IReadOnlyCollection<AttendanceResponseDto>> GetReportAsync(DateOnly? dateFrom, DateOnly? dateTo, string? employeeId, CancellationToken cancellationToken = default)
        {
            var (from, to) = NormalizeRange(dateFrom, dateTo);
            var visibleUsers = await GetVisibleUsersAsync(employeeId, cancellationToken);
            var visibleUserIds = visibleUsers.Select(x => x.Id).ToList();

            var records = await _attendanceDbContext.AttendanceRecords
                .AsNoTracking()
                .Where(x => visibleUserIds.Contains(x.EmployeeUserId) && x.AttendanceDate >= from && x.AttendanceDate <= to)
                .OrderByDescending(x => x.AttendanceDate)
                .ThenBy(x => x.EmployeeName)
                .ToListAsync(cancellationToken);

            return records.Select(Map).ToList();
        }

        private async Task<AttendanceResponseDto> ScanAsync(ScanQrDto dto, string expectedType, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.QrPayload))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "QR payload is required.");
            }

            var payload = DeserializePayload(dto.QrPayload);
            ValidateSignature(payload);

            if (!string.Equals(payload.Type, expectedType, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, $"QR is not valid for {expectedType}.");
            }

            var manager = await GetCurrentUserAsync(cancellationToken);
            if (!string.Equals(manager.Role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only managers can scan attendance QR codes.");
            }

            var employee = await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == payload.EmployeeUserId, cancellationToken);

            if (employee == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Employee not found for the QR code.");
            }

            if (employee.ManagerId != manager.Id)
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only the employee's assigned manager can scan this QR.");
            }

            var attendanceRecord = await _attendanceDbContext.AttendanceRecords
                .FirstOrDefaultAsync(x => x.EmployeeUserId == employee.Id && x.AttendanceDate == payload.AttendanceDate, cancellationToken);

            if (attendanceRecord == null ||
                !string.Equals(attendanceRecord.QrNonce, payload.Nonce, StringComparison.Ordinal) ||
                !string.Equals(attendanceRecord.PendingQrType, expectedType, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "QR code is invalid or no longer active.");
            }

            var nowUtc = DateTime.UtcNow;
            if (payload.ExpiresAtUtc < nowUtc || attendanceRecord.QrExpiresAt < nowUtc)
            {
                attendanceRecord.IsQrExpired = true;
                attendanceRecord.UpdatedAtUtc = nowUtc;
                await _attendanceDbContext.SaveChangesAsync(cancellationToken);
                throw new ApiException(StatusCodes.Status400BadRequest, "QR code has expired.");
            }

            if (attendanceRecord.IsNonceConsumed)
            {
                throw new ApiException(StatusCodes.Status409Conflict, "QR code has already been consumed.");
            }

            attendanceRecord.IsNonceConsumed = true;
            attendanceRecord.IsQrExpired = false;
            attendanceRecord.UpdatedAtUtc = nowUtc;

            if (expectedType == "clock-in")
            {
                if (attendanceRecord.ClockInAtUtc != null)
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Clock-in has already been completed.");
                }

                attendanceRecord.ClockInAtUtc = nowUtc;
                attendanceRecord.ScannedInByManagerId = manager.Id;
                attendanceRecord.ScannedInByManagerName = manager.FullName;
                attendanceRecord.Status = AttendanceStatus.PendingClockOut;
            }
            else
            {
                if (attendanceRecord.ClockInAtUtc == null)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, "Clock-in must be completed before clock-out.");
                }

                if (attendanceRecord.ClockOutAtUtc != null)
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Clock-out has already been completed.");
                }

                attendanceRecord.ClockOutAtUtc = nowUtc;
                attendanceRecord.ScannedOutByManagerId = manager.Id;
                attendanceRecord.ScannedOutByManagerName = manager.FullName;
                attendanceRecord.DurationMinutes = Math.Max(0, (int)Math.Round((attendanceRecord.ClockOutAtUtc.Value - attendanceRecord.ClockInAtUtc.Value).TotalMinutes));
                attendanceRecord.Status = attendanceRecord.DurationMinutes < _options.HalfDayThresholdMinutes
                    ? AttendanceStatus.HalfDay
                    : AttendanceStatus.Present;
            }

            attendanceRecord.PendingQrType = null;
            await _attendanceDbContext.SaveChangesAsync(cancellationToken);

            if (expectedType == "clock-in")
            {
                _publisher.Publish(new AttendanceClockInEvent
                {
                    RecipientUserId = employee.Id,
                    RecipientEmail = employee.Email,
                    RecipientName = employee.FullName,
                    AttendanceRecordId = attendanceRecord.Id,
                    EmployeeUserId = employee.Id,
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.FullName,
                    AttendanceDate = attendanceRecord.AttendanceDate,
                    ClockInAtUtc = attendanceRecord.ClockInAtUtc!.Value,
                    ScannedByManagerId = manager.Id,
                    ScannedByManagerName = manager.FullName
                }, "attendance.clockin");
            }
            else
            {
                _publisher.Publish(new AttendanceClockOutEvent
                {
                    RecipientUserId = employee.Id,
                    RecipientEmail = employee.Email,
                    RecipientName = employee.FullName,
                    AttendanceRecordId = attendanceRecord.Id,
                    EmployeeUserId = employee.Id,
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.FullName,
                    AttendanceDate = attendanceRecord.AttendanceDate,
                    ClockOutAtUtc = attendanceRecord.ClockOutAtUtc!.Value,
                    DurationMinutes = attendanceRecord.DurationMinutes ?? 0,
                    ScannedByManagerId = manager.Id,
                    ScannedByManagerName = manager.FullName
                }, "attendance.clockout");
            }

            return Map(attendanceRecord);
        }

        private AttendanceQrPayload BuildPayload(AttendanceRecord attendanceRecord, string qrType, DateTime expiresAtUtc)
        {
            var payload = new AttendanceQrPayload
            {
                EmployeeUserId = attendanceRecord.EmployeeUserId,
                EmployeeId = attendanceRecord.EmployeeId,
                AttendanceDate = attendanceRecord.AttendanceDate,
                Type = qrType,
                Nonce = attendanceRecord.QrNonce ?? string.Empty,
                ExpiresAtUtc = expiresAtUtc
            };

            payload.Signature = ComputeSignature(payload);
            return payload;
        }

        private string ComputeSignature(AttendanceQrPayload payload)
        {
            var canonical = $"{payload.EmployeeUserId:D}|{payload.EmployeeId}|{payload.AttendanceDate:yyyy-MM-dd}|{payload.Type}|{payload.Nonce}|{payload.ExpiresAtUtc:O}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.QrSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical));
            return Convert.ToBase64String(hash);
        }

        private void ValidateSignature(AttendanceQrPayload payload)
        {
            var expected = ComputeSignature(payload);
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(payload.Signature)))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "QR signature is invalid.");
            }
        }

        private static AttendanceQrPayload DeserializePayload(string qrPayload)
        {
            try
            {
                var payload = JsonSerializer.Deserialize<AttendanceQrPayload>(qrPayload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return payload ?? throw new ApiException(StatusCodes.Status400BadRequest, "QR payload could not be read.");
            }
            catch (JsonException)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "QR payload is malformed.");
            }
        }

        private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(x => x.Id == _currentUserService.GetUserId(), cancellationToken);
            if (user == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "User not found.");
            }

            return user;
        }

        private async Task EnsureEmployeeVisibleAsync(Guid employeeUserId, CancellationToken cancellationToken)
        {
            var role = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();

            if (string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase) || employeeUserId == currentUserId)
            {
                return;
            }

            if (!string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "You are not allowed to view this attendance history.");
            }

            var visible = await _appDbContext.Users.AnyAsync(x => x.Id == employeeUserId && x.ManagerId == currentUserId, cancellationToken);
            if (!visible)
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "You are not allowed to view this attendance history.");
            }
        }

        private async Task<List<User>> GetVisibleUsersAsync(string? employeeId, CancellationToken cancellationToken)
        {
            var role = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();
            var normalizedEmployeeId = string.IsNullOrWhiteSpace(employeeId) ? null : employeeId.Trim().ToUpperInvariant();

            IQueryable<User> query = _appDbContext.Users.AsNoTracking();

            if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Id == currentUserId || x.ManagerId == currentUserId);
            }
            else if (!string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Id == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(normalizedEmployeeId))
            {
                query = query.Where(x => x.EmployeeId == normalizedEmployeeId);
            }

            return await query.OrderBy(x => x.FullName).ToListAsync(cancellationToken);
        }

        private static string NormalizeQrType(string type)
        {
            var normalized = type.Trim().ToLowerInvariant();
            return normalized switch
            {
                "clock-in" or "clockin" => "clock-in",
                "clock-out" or "clockout" => "clock-out",
                _ => throw new ApiException(StatusCodes.Status400BadRequest, "Attendance QR type must be either clock-in or clock-out.")
            };
        }

        private static (DateOnly From, DateOnly To) NormalizeRange(DateOnly? dateFrom, DateOnly? dateTo)
        {
            var today = GetBusinessDate();
            var from = dateFrom ?? new DateOnly(today.Year, today.Month, 1);
            var to = dateTo ?? today;
            return from <= to ? (from, to) : (to, from);
        }

        private static DateOnly GetBusinessDate()
        {
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndiaTimeZone));
        }

        private static AttendanceResponseDto Map(AttendanceRecord record)
        {
            return new AttendanceResponseDto
            {
                Id = record.Id,
                EmployeeUserId = record.EmployeeUserId,
                EmployeeId = record.EmployeeId,
                EmployeeName = record.EmployeeName,
                AttendanceDate = record.AttendanceDate,
                ClockInAtUtc = record.ClockInAtUtc,
                ClockOutAtUtc = record.ClockOutAtUtc,
                DurationMinutes = record.DurationMinutes,
                Status = record.Status.ToString(),
                ScannedInByManagerId = record.ScannedInByManagerId,
                ScannedInByManagerName = record.ScannedInByManagerName,
                ScannedOutByManagerId = record.ScannedOutByManagerId,
                ScannedOutByManagerName = record.ScannedOutByManagerName
            };
        }

        private sealed class AttendanceQrPayload
        {
            public Guid EmployeeUserId { get; set; }
            public string EmployeeId { get; set; } = string.Empty;
            public DateOnly AttendanceDate { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Nonce { get; set; } = string.Empty;
            public DateTime ExpiresAtUtc { get; set; }
            public string Signature { get; set; } = string.Empty;
        }
    }
}
