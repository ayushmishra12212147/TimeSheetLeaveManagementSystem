using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReportService.DTOs;
using ReportService.Exceptions;

namespace ReportService.Services
{
    public class ExportService : IExportService
    {
        public Task<ExportFileResult> ExportAttendanceAsync(AttendanceReportResponseDto report, string format, CancellationToken cancellationToken = default)
        {
            var headers = new[]
            {
                "Employee Id", "Employee Name", "Date", "Status", "Clock In", "Clock Out", "Duration (Min)", "Clock-In By", "Clock-Out By", "Holiday", "Leave Type"
            };

            var rows = report.Rows.Select(x => new[]
            {
                x.EmployeeId,
                x.EmployeeName,
                x.Date.ToString("yyyy-MM-dd"),
                x.Status,
                x.ClockInAtUtc?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                x.ClockOutAtUtc?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                x.DurationMinutes?.ToString() ?? string.Empty,
                x.ScannedInByManagerName ?? string.Empty,
                x.ScannedOutByManagerName ?? string.Empty,
                x.HolidayName ?? string.Empty,
                x.LeaveTypeName ?? string.Empty
            });

            return Task.FromResult(ExportTable("Attendance Report", $"attendance-report-{DateTime.UtcNow:yyyyMMddHHmmss}", format, headers, rows));
        }

        public Task<ExportFileResult> ExportLeaveAsync(LeaveReportResponseDto report, string format, CancellationToken cancellationToken = default)
        {
            var headers = new[]
            {
                "Employee Id", "Employee Name", "Leave Type", "Start Date", "End Date", "Days", "Status", "Pending Role", "Approved By", "Rejected By"
            };

            var rows = report.Rows.Select(x => new[]
            {
                x.EmployeeId,
                x.EmployeeName,
                x.LeaveTypeName,
                x.StartDate.ToString("yyyy-MM-dd"),
                x.EndDate.ToString("yyyy-MM-dd"),
                x.RequestedDays.ToString("0.##"),
                x.Status,
                x.PendingApprovalRole ?? string.Empty,
                x.ApprovedByName ?? string.Empty,
                x.RejectedByName ?? string.Empty
            });

            return Task.FromResult(ExportTable("Leave Report", $"leave-report-{DateTime.UtcNow:yyyyMMddHHmmss}", format, headers, rows));
        }

        public Task<ExportFileResult> ExportTimesheetAsync(TimesheetReportResponseDto report, string format, CancellationToken cancellationToken = default)
        {
            var headers = new[]
            {
                "Employee Id", "Employee Name", "Week Start", "Week End", "Hours", "Entries", "Status", "Late", "Min Hours Met", "Approved By"
            };

            var rows = report.Rows.Select(x => new[]
            {
                x.EmployeeId,
                x.EmployeeName,
                x.WeekStartDate.ToString("yyyy-MM-dd"),
                x.WeekEndDate.ToString("yyyy-MM-dd"),
                x.TotalHours.ToString("0.##"),
                x.EntryCount.ToString(),
                x.Status,
                x.IsLateSubmission ? "Yes" : "No",
                x.MeetsMinimumWeeklyHours ? "Yes" : "No",
                x.ApprovedByName ?? string.Empty
            });

            return Task.FromResult(ExportTable("Timesheet Report", $"timesheet-report-{DateTime.UtcNow:yyyyMMddHHmmss}", format, headers, rows));
        }

        private static ExportFileResult ExportTable(string title, string baseFileName, string format, string[] headers, IEnumerable<string[]> rows)
        {
            if (string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase))
            {
                return BuildExcel(title, baseFileName, headers, rows);
            }

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BuildPdf(title, baseFileName, headers, rows);
            }

            throw new ApiException(StatusCodes.Status400BadRequest, "Unsupported export format.");
        }

        private static ExportFileResult BuildExcel(string title, string baseFileName, string[] headers, IEnumerable<string[]> rows)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Report");

            worksheet.Cells[1, 1].Value = title;
            worksheet.Cells[1, 1, 1, headers.Length].Merge = true;
            worksheet.Cells[1, 1, 1, headers.Length].Style.Font.Bold = true;
            worksheet.Cells[1, 1, 1, headers.Length].Style.Font.Size = 14;

            for (var i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
            }

            var rowIndex = 4;
            foreach (var row in rows)
            {
                for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
                {
                    worksheet.Cells[rowIndex, columnIndex + 1].Value = row[columnIndex];
                }

                rowIndex++;
            }

            worksheet.Cells.AutoFitColumns();

            return new ExportFileResult
            {
                Content = package.GetAsByteArray(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"{baseFileName}.xlsx"
            };
        }

        private static ExportFileResult BuildPdf(string title, string baseFileName, string[] headers, IEnumerable<string[]> rows)
        {
            var rowList = rows.ToList();
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Text(title).Bold().FontSize(16);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in headers)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var headerText in headers)
                            {
                                header.Cell().Element(CellStyle).Text(headerText).Bold();
                            }
                        });

                        foreach (var row in rowList)
                        {
                            foreach (var cell in row)
                            {
                                table.Cell().Element(CellStyle).Text(cell);
                            }
                        }
                    });
                });
            }).GeneratePdf();

            return new ExportFileResult
            {
                Content = pdfBytes,
                ContentType = "application/pdf",
                FileName = $"{baseFileName}.pdf"
            };
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
        }
    }
}
