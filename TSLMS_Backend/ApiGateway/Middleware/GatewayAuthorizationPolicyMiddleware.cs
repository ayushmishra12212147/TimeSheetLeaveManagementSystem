using System.Security.Claims;

namespace ApiGateway.Middleware
{
    public class GatewayAuthorizationPolicyMiddleware
    {
        private readonly RequestDelegate _next;

        public GatewayAuthorizationPolicyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "/";
            var method = context.Request.Method.ToUpperInvariant();

            if (IsAlwaysAllowed(path, method))
            {
                await _next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated != true)
            {
                await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "Authentication is required.");
                return;
            }

            var role = context.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (IsHrOnly(path, method) && !string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                await WriteErrorAsync(context, StatusCodes.Status403Forbidden, "Only HRAdmin can access this endpoint.");
                return;
            }

            if (IsManagerOnly(path, method) && !string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                await WriteErrorAsync(context, StatusCodes.Status403Forbidden, "Only managers can access this endpoint.");
                return;
            }

            if (IsManagerOrHr(path, method) &&
                !string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                await WriteErrorAsync(context, StatusCodes.Status403Forbidden, "Only managers and HRAdmin can access this endpoint.");
                return;
            }

            await _next(context);
        }

        private static bool IsAlwaysAllowed(string path, string method)
        {
            if (path.StartsWith("/swagger") || path.StartsWith("/api/v1/") && path.Contains("/swagger/"))
            {
                return true;
            }

            if (path == "/api/v1/auth/reset-password" && method is "GET" or "POST")
            {
                return true;
            }

            return method == "POST" && path is
                "/api/v1/auth/login" or
                "/api/v1/auth/refresh" or
                "/api/v1/auth/forgot-password";
        }

        private static bool IsHrOnly(string path, string method)
        {
            return
                (path.StartsWith("/api/v1/holidays") && method is "POST" or "PUT" or "DELETE") ||
                path == "/api/v1/holidays/copy-year" ||
                (path.StartsWith("/api/v1/leave-types") && method is "POST" or "PUT" or "PATCH") ||
                (path.StartsWith("/api/v1/leave-balances") && (path.Contains("/adjust") || path.EndsWith("/carry-forward"))) ||
                (path.StartsWith("/api/v1/projects") && method is "POST" or "PUT" or "PATCH") ||
                (path == "/api/v1/timesheet-config" && method == "PUT") ||
                path.StartsWith("/api/v1/notifications/templates") ||
                path.StartsWith("/api/v1/audit") ||
                path == "/api/v1/reports/leave/export" ||
                path == "/api/v1/reports/attendance/export" ||
                path == "/api/v1/reports/timesheet/export" ||
                path.Contains("/api/v1/reports/requests/") && (path.EndsWith("/approve") || path.EndsWith("/reject")) ||
                (path.StartsWith("/api/v1/users") && method is "POST" or "PUT" or "DELETE") ||
                path == "/api/v1/users/assign-manager" ||
                (path.StartsWith("/api/v1/departments") && method is "POST" or "PUT");
        }

        private static bool IsManagerOnly(string path, string method)
        {
            return
                (path == "/api/v1/reports/requests" && method == "POST") ||
                ((path == "/api/v1/attendance/scan-in" || path == "/api/v1/attendance/scan-out") && method == "POST");
        }

        private static bool IsManagerOrHr(string path, string method)
        {
            return
                path == "/api/v1/leaves/pending" ||
                path == "/api/v1/leaves/team-calendar" ||
                (path.Contains("/api/v1/leaves/") && (path.EndsWith("/approve") || path.EndsWith("/reject"))) ||
                path == "/api/v1/timesheets/pending" ||
                path == "/api/v1/timesheets/team" ||
                (path.Contains("/api/v1/timesheets/") && (path.EndsWith("/approve") || path.EndsWith("/reject"))) ||
                path == "/api/v1/attendance/team" ||
                path == "/api/v1/attendance/records" ||
                (path.StartsWith("/api/v1/attendance/") && path.EndsWith("/history")) ||
                path.StartsWith("/api/v1/reports") ||
                (path.StartsWith("/api/v1/users") && method == "GET");
        }

        private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message
            });
        }
    }
}
