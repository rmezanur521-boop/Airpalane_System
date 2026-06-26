using AirplaneSystem.Domain.Entities.Audit;
using AirplaneSystem.Infrastructure.Persistence;

namespace AirplaneSystem.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        await _next(context);

        // Only audit mutating requests with successful responses
        if (IsMutatingMethod(context.Request.Method) && context.Response.StatusCode < 400)
        {
            var userId = context.User?.FindFirst("sub")?.Value;
            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault() ?? string.Empty;

            dbContext.AuditLogs.Add(new AuditLog
            {
                EntityName = "HttpRequest",
                EntityId = context.TraceIdentifier,
                Action = context.Request.Method,
                NewValues = $"{{\"path\":\"{context.Request.Path}\",\"status\":{context.Response.StatusCode}}}",
                UserId = Guid.TryParse(userId, out var uid) ? uid : null,
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId
            });

            try { await dbContext.SaveChangesAsync(); }
            catch { /* Don't fail the request if audit logging fails */ }
        }
    }

    private static bool IsMutatingMethod(string method) =>
        method is "POST" or "PUT" or "PATCH" or "DELETE";
}
